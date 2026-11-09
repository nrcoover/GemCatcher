# GemCatcher

A simple arcade-style game built with Godot 4.5 in C#

This game was created as a student project, expanding upon and building off of a game from the Udemy course ("Learn 2D Game Development: Godot 4 & C# from Scracth by Richard Allbert and Martyna Olivares)[https://www.udemy.com/course/learn-2d-game-development-godot-43-c-from-scratch/].

This game is the first game created in full by the game developer, Nicklefritz, and took roughly 80 hours spread across 6 months of work.

# Game Rules

- Survive all 12 stages by catching the required 25 gems per stage without losting all health.
- Missed gems will cause health to decay.
- Health is restored to full at the beginning of each stage.
- When player has incurred damage, health gems may be dropped.
- Power Up gems may be dropped.
- Power Up gems may be one of the following: Big Paddle power up, Triple "Ghost Paddle" power up, or the Black Hole power up.
- Game victory will unlock Infinite Mode, at which the game changes to surviving as many stages as possible.
- Game difficulty increases with every other stage advancement, including increased gem falling speed and incrementally increased number of gem spawners.

# Technical Features

This game was built with the following implemented features and techincal design points:

- Global singletons, including a Signal Manager and Game Manager
- Player Data save system
- Dynamic Main Menu with unlockable features
- Incremental difficulty system and game balancing
- Object Pooling system
- Tween utilization for creating "Game Juice" (game feel) and player experience
- Particles and Animations
- Dialogue and story lore

# Known Bugs

- Big Paddle Delay | Paddle Tweens occasionally interfere with one another. Most noticeable when a falling gem and a the "Big Paddle" power up are caught at near the same time; paddle size doesn't update until the next falling gem is caught, triggering a new size Tween.

# Desired Improvements

- Power Up Timeout Flash | Players have commented on the unforunate lack of notice that a power up is about to expire. There is a visible countdown, but has proven not noticable enough during gameplay. A flashing warning as time runs out would be beneficial, but an implementation that doesn't interfere with the current flashing warning system for the Paddle's boost function has yet to be discovered.
