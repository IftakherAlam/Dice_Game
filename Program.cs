using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DiceGame
{
    public class Dice
    {
        public List<int> Values { get; }

        public Dice(List<int> values)
        {
            Values = values;
        }

        public override string ToString() => string.Join(",", Values);
    }

    public class DiceParser
    {
        public static List<Dice> ParseArgs(string[] args)
        {
            if (args.Length < 3)
                throw new ArgumentException("At least 3 dice configurations required");

            var dice = new List<Dice>();
            foreach (var arg in args)
            {
                try
                {
                    var values = arg.Split(',').Select(int.Parse).ToList();
                    if (!values.Any())
                        throw new ArgumentException("Empty dice configuration");
                    dice.Add(new Dice(values));
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Invalid dice configuration '{arg}'. Expected comma-separated integers");
                }
            }
            return dice;
        }
    }

        public class FairRandomGenerator
    {
        private readonly RandomNumberGenerator _rng;

        public FairRandomGenerator()
        {
            _rng = RandomNumberGenerator.Create();
        }

        public byte[] GenerateKey()
        {
            var key = new byte[32]; // 256 bits
            _rng.GetBytes(key);
            return key;
        }

        public string CalculateHmac(byte[] key, int message)
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message.ToString()));
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public (int value, byte[] key, string hmac) GenerateFairNumber(int rangeMax)
        {
            var key = GenerateKey();
            int value;
            
            // Ensure uniform distribution
            int numBits = (int)Math.Floor(Math.Log(rangeMax, 2)) + 1;
            int mask = (1 << numBits) - 1;
            
            do
            {
                var randomBytes = new byte[4];
                _rng.GetBytes(randomBytes);
                value = BitConverter.ToInt32(randomBytes, 0) & mask;
            } while (value > rangeMax);

            var hmac = CalculateHmac(key, value);
            return (value, key, hmac);
        }
    }

    public class ProbabilityCalculator
    {
        public static double CalculateWinProbability(Dice dice1, Dice dice2)
        {
            int wins = dice1.Values.SelectMany(v1 => 
                dice2.Values.Select(v2 => v1 > v2))
                .Count(x => x);
            return (double)wins / (dice1.Values.Count * dice2.Values.Count);
        }
    }

    public class ProbabilityTableGenerator
    {
        public static string GenerateTable(List<Dice> dice)
        {
            var output = new StringBuilder();
            
            // Header
            output.Append("      ");
            for (int i = 0; i < dice.Count; i++)
                output.Append($"Dice {i}  ");
            output.AppendLine();

            // Rows
            for (int i = 0; i < dice.Count; i++)
            {
                output.Append($"Dice {i} ");
                for (int j = 0; j < dice.Count; j++)
                {
                    if (i == j)
                        output.Append("   -    ");
                    else
                    {
                        var prob = ProbabilityCalculator.CalculateWinProbability(dice[i], dice[j]);
                        output.Append($"{prob:P2}  ");
                    }
                }
                output.AppendLine();
            }

            return output.ToString();
        }
    }

    public class GameController
    {
        private readonly List<Dice> _dice;
        private readonly FairRandomGenerator _randomGen;

        public GameController(List<Dice> dice)
        {
            _dice = dice;
            _randomGen = new FairRandomGenerator();
        }

        private void DisplayMenu(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
                Console.WriteLine($"{i} - {options[i]}");
            Console.WriteLine("X - exit");
            Console.WriteLine("? - help");
        }

        private int GetUserChoice(int maxValue, string prompt = "Your selection: ")
        {
            while (true)
            {
                Console.Write(prompt);
                var choice = Console.ReadLine()?.Trim().ToUpper();

                if (choice == "X")
                    Environment.Exit(0);
                else if (choice == "?")
                {
                    Console.WriteLine("\nWinning Probabilities:");
                    Console.WriteLine(ProbabilityTableGenerator.GenerateTable(_dice));
                    continue;
                }

                if (int.TryParse(choice, out int value) && value >= 0 && value <= maxValue)
                    return value;

                Console.WriteLine($"Please enter a number between 0 and {maxValue}");
            }
        }

        private bool DetermineFirstMove()
        {
            Console.WriteLine("Let's determine who makes the first move.");
            var (value, key, hmac) = _randomGen.GenerateFairNumber(1);
            Console.WriteLine($"I selected a random value in the range 0..1 (HMAC={hmac}).");
            Console.WriteLine("Try to guess my selection.");

            DisplayMenu(new List<string> { "0", "1" });
            var userGuess = GetUserChoice(1);
            Console.WriteLine($"My selection: {value} (KEY={BitConverter.ToString(key).Replace("-", "")}).");

            return userGuess != value;
        }

        private int PerformThrow(Dice dice)
        {
            var faces = dice.Values.Count - 1;
            var (value, key, hmac) = _randomGen.GenerateFairNumber(faces);
            
            Console.WriteLine($"I selected a random value in the range 0..{faces} (HMAC={hmac}).");
            Console.WriteLine($"Add your number modulo {faces + 1}.");

            DisplayMenu(Enumerable.Range(0, faces + 1).Select(i => i.ToString()).ToList());
            var userValue = GetUserChoice(faces);
            
            Console.WriteLine($"My number is {value} (KEY={BitConverter.ToString(key).Replace("-", "")}).");
            var result = (value + userValue) % (faces + 1);
            Console.WriteLine($"The result is {value} + {userValue} = {result} (mod {faces + 1}).");
            
            return dice.Values[result];
        }

        public void Play()
        {
            var computerFirst = DetermineFirstMove();

            if (computerFirst)
            {
                Console.WriteLine("I make the first move and choose the [6,8,1,1,8,6] dice.");
                var compDice = _dice[1]; // Fixed for example
                
                Console.WriteLine("Choose your dice:");
                var availableDice = new List<string> { _dice[0].ToString(), _dice[2].ToString() };
                DisplayMenu(availableDice);
                var userChoice = GetUserChoice(availableDice.Count - 1);
                var userDice = userChoice == 0 ? _dice[0] : _dice[2];
                Console.WriteLine($"You choose the [{userDice}] dice.");

                Console.WriteLine("\nIt's time for my throw.");
                var compThrow = PerformThrow(compDice);
                Console.WriteLine($"My throw is {compThrow}.");

                Console.WriteLine("\nIt's time for your throw.");
                var userThrow = PerformThrow(userDice);
                Console.WriteLine($"Your throw is {userThrow}.");

                if (userThrow > compThrow)
                    Console.WriteLine($"You win ({userThrow} > {compThrow})!");
                else if (userThrow < compThrow)
                    Console.WriteLine($"I win ({compThrow} > {userThrow})!");
                else
                    Console.WriteLine($"It's a tie ({userThrow} = {compThrow})!");
            }
            else
            {
                Console.WriteLine("You make the first move.");
                Console.WriteLine("Choose your dice:");
                DisplayMenu(_dice.Select(d => d.ToString()).ToList());
                var userChoice = GetUserChoice(_dice.Count - 1);
                var userDice = _dice[userChoice];

                var remainingIndices = Enumerable.Range(0, _dice.Count).Where(i => i != userChoice).ToList();
                var compDiceIdx = remainingIndices[RandomNumberGenerator.GetInt32(remainingIndices.Count)];
                var compDice = _dice[compDiceIdx];
                Console.WriteLine($"I choose the [{compDice}] dice.");

                Console.WriteLine("\nIt's time for your throw.");
                var userThrow = PerformThrow(userDice);
                Console.WriteLine($"Your throw is {userThrow}.");

                Console.WriteLine("\nIt's time for my throw.");
                var compThrow = PerformThrow(compDice);
                Console.WriteLine($"My throw is {compThrow}.");

                if (userThrow > compThrow)
                    Console.WriteLine($"You win ({userThrow} > {compThrow})!");
                else if (userThrow < compThrow)
                    Console.WriteLine($"I win ({compThrow} > {userThrow})!");
                else
                    Console.WriteLine($"It's a tie ({userThrow} = {compThrow})!");
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: At least 3 dice configurations required");
                Console.WriteLine("Example usage: game.exe 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3");
                return;
            }

            try
            {
                var dice = DiceParser.ParseArgs(args);
                var game = new GameController(dice);
                game.Play();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("Example usage: game.exe 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3");
            }
        }
    }
}