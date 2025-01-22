# Non-Transitive Dice Game

## Overview
This is a command-line implementation of a **generalized non-transitive dice game** with provable fair random generation. The game allows arbitrary dice configurations and ensures fairness through cryptographic techniques. The project is implemented in **C# (or the chosen language)** and follows object-oriented design principles with multiple classes handling different responsibilities.

## Features
- **Supports Arbitrary Dice**: The game accepts **3 or more** dice with arbitrary values as command-line arguments.
- **Provable Fair Random Generation**: Uses **HMAC-SHA3** to ensure fair random number generation.
- **Secure Random Number Generation**: Implements **cryptographically secure** random key generation (at least 256-bit) using a secure API.
- **Modular and Extensible Design**: Implements multiple classes for responsibilities such as dice parsing, probability calculation, fair random generation, and CLI interactions.
- **CLI-based Gameplay**: Provides an interactive menu-driven interface with options for help, dice selection, and exit.
- **Game Mechanics**:
  - The game determines the first player fairly using a **secure coin flip**.
  - Players (computer and user) select different dice and perform random throws.
  - The throws are **fair** using a cryptographic **HMAC-based verification system**.
  - The results of the game are displayed with proof of fair play.
- **Help Option**: Displays an **ASCII table of probabilities** for each dice pair.

## Requirements
- **.NET 6+ (if using C#)** or **Node.js/Python/Ruby/etc.**
- **HMAC-SHA3 support** (via built-in or third-party libraries)

## Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/your-username/non-transitive-dice-game.git
   cd non-transitive-dice-game
   ```
2. Build the project (for C#):
   ```sh
   dotnet build
   ```
3. Run the game with dice configurations:
   ```sh
   dotnet run -- 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3
   ```

## Usage
### Running the Game
Run the program with **at least 3 dice configurations** as arguments:
```sh
<executable> 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3
```

### Sample Gameplay Output
```plaintext
Let's determine who makes the first move.
I selected a random value in the range 0..1 (HMAC=C8E79615E637E6B14DDACA2309069A76D0882A4DD8102D9DEAD3FD6AC4AE289A).
Try to guess my selection.
0 - 0
1 - 1
X - exit
? - help
Your selection: 0
My selection: 1 (KEY=BD9BE48334BB9C5EC263953DA54727F707E95544739FCE7359C267E734E380A2).
I make the first move and choose the [6,8,1,1,8,6] dice.
...
You win (9 > 8)!
```

### Incorrect Usage
If the input arguments are incorrect (e.g., fewer than 3 dice, invalid characters, non-integer values), the program will display an error message with an example of valid input.

### Help Option
During the game, selecting the `?` option will display a probability table for all dice pairs.

## Design and Implementation
The project is structured using **object-oriented principles** and consists of the following main components:

1. **DiceParser**: Parses and validates dice input from the command line.
2. **FairRandomGenerator**: Implements cryptographic random number generation and HMAC verification.
3. **Dice**: Represents an individual dice and handles rolling mechanics.
4. **GameLogic**: Controls the flow of the game.
5. **ProbabilityCalculator**: Computes win probabilities between dice pairs.
6. **AsciiTableRenderer**: Renders the probability table in a user-friendly format.

## Security Considerations
- Uses **cryptographic randomness** for fair number generation.
- **HMAC validation** ensures that the computer does not alter numbers after user input.
- The user can verify fairness by recomputing HMAC using the revealed key.

## Contribution Guidelines
Contributions are welcome! To contribute:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request with detailed changes.

## License
MIT License.
---
This repository follows the **specified requirements** of the non-transitive dice game, ensuring fairness, modular design, and security through cryptographic methods.

