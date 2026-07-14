# GemCatcher

GemCatcher is a simple arcade-style game built with Godot 4.5 and C#.

Catch falling gems, keep your health up, and use boosters and power-ups to survive as the game speeds up.

## Requirements

- Godot 4.5.2 with .NET support
- .NET SDK 8.0

The repository includes a `global.json` file so local and CI builds use a stable .NET 8 SDK instead of whichever SDK happens to be installed globally.

## Running locally

1. Clone the repository.
2. Open the project folder in Godot 4.5.2 .NET.
3. Run the main scene from the editor.

You can also verify the C# project from a terminal:

```powershell
dotnet build GemCatcher.sln
```

## Controls

- Move left: `A` or left arrow
- Move right: `D` or right arrow
- Boost: up arrow
- Use action slot: `Spacebar`
- Pause or resume: `P`
- Return to menu: `Esc`
- The desktop window can be resized or maximized; gameplay scales proportionally with it.

## Gameplay

- Regular gems increase your score.
- Missed regular gems cost health and reset your catch streak.
- Heart gems restore health.
- Power-up gems randomly enlarge the paddle, add ghost paddles, or activate a gem-attracting magnetic field.
- Nuke pickups fill the single action slot when it is empty. Press `Spacebar` to consume the nuke and safely clear active falling objects; duplicate pickups do not stack.
- Every 10-gem catch streak awards a small bonus.
- Reaching a new stage grants bonus points and restores one health if damaged.
- Meteors begin falling at Stage 3. Dodge them for bonus points; a collision costs one health.
- Meteor frequency and speed increase as stages advance.
- Meteor Storms can occur from Stage 4 onward. For 14 seconds, normal collectibles pause while rapid meteors take over; every successful dodge remains worth two points.
- Stardust showers can occur on their own from Stage 2 onward, releasing harmless golden bonus gems worth three points each.
- Kid Play doubles friendly-object and paddle sizes, halves meteor sizes, and starts with ten lives instead of five.

## Project Notes

- GitHub Actions runs a .NET build on pushes and pull requests.
- Generated audio editor sidecar files (`*.sfk`) are ignored.
- Before publishing builds, confirm the project license and verify all third-party asset licenses/credits in `ASSET_CREDITS.md`.
