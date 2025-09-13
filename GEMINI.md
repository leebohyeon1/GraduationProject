# GraduationProject Gemini Context

This document provides a comprehensive overview of the GraduationProject, a 3D RPG game developed in Unity. It is intended to be used as a context file for the Gemini CLI to assist in development.

## Project Overview

This project is a 3D RPG game built with Unity. The core gameplay revolves around a player character with various combat abilities and enemy AI driven by behavior trees.

**Key Technologies:**

*   **Engine:** Unity 6000.2.1f1
*   **Programming Language:** C#
*   **Core Gameplay:**
    *   **Player:** A finite state machine (FSM) manages the player's actions, including movement, melee/ranged attacks, dodging, and defending.
    *   **Enemies:** A combination of a state machine and a behavior tree controls the enemy AI, enabling behaviors like chasing, patrolling, and attacking.
*   **Libraries & Packages:**
    *   **A\* Pathfinding Project:** Used for character navigation.
    *   **Unity Addressables:** For asset management.
    *   **Unity Input System:** For handling player input.
    *   **Universal Render Pipeline (URP):** For graphics.
    *   **BH\_Lib:** A custom library that appears to provide a dependency injection framework and other utilities.

## Building and Running

This is a standard Unity project. To build and run:

1.  Open the project in the Unity Editor (version 6000.2.1f1).
2.  Open one of the scenes from the `Assets/_GraduationProject/01_Scenes` directory.
3.  Press the "Play" button in the editor to run the game.

Builds for different platforms can be created using Unity's Build Settings (File > Build Settings).

## Development Conventions

*   **Folder Structure:** The project follows a feature-based organization within the `Assets/_GraduationProject` directory, with subfolders for scenes, features, art, and feels.
*   **Code Structure:**
    *   C# scripts are the primary language for development.
    *   The player character's logic is built around a finite state machine, with different states for various actions.
    *   Enemy AI is implemented using a behavior tree, managed by the `AiController` class.
    *   Interfaces like `IDamageable` and `IAttacker` are used to define contracts for combat interactions.
*   **Communication:** An `EventManager` is used for decoupled communication between different parts of the game.
*   **Dependency Management:** The project utilizes a custom dependency injection framework (`BH_Lib.DI`) to manage dependencies between classes.
