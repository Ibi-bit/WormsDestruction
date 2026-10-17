---
name: wriggle-overview
description: Overview of the Wriggle C# raylib worms-style game, its folder layout, scenes, weapons, physics and world code. Start here for any question about how the Wriggle project is organised.
---

Wriggle is a C# (.NET 8) 2D worms-style game built on Raylib-cs with Dear ImGui debug tools.

## Layout

- `My2DGame.csproj`: project file, pinned NuGet versions (see `wriggle-dependencies`).
- `Game/Program.cs`: entry point.
- `Game/Core/`: game loop (`Game.cs`), settings (`GameConfig.cs`), base `Scene`, `SceneManager`.
- `Game/Scenes/GameScene.cs`: the current gameplay scene.
- `Game/World/`: `GameWorld`, `RoundController`, `TurnController`, `Terrain`.
- `Game/Physics/`: `PhysicsWorld`, `PhysicsBody`, `Ball`, `AreaEffect`.
- `Game/Weapons/`: `BaseWeapon.cs` (`Weapon`, `Ammo`, `InventoryWeapon`), `Grenades.cs`.
- `Game/Characters/`: `BaseCharacter`, `Wriggler`.
- `Game/Player/Player.cs`: player, wrigglers and weapon inventory.
- `Game/UI/DebugUI.cs`: ImGui overlay and hotkeys.
- `Game/Content/`: runtime assets, copied beside the executable.
- `specs/`: Allium behaviour specs (`core`, `physics`, `weapons`, `world`). These are the source of truth for rules.
- `tests/`: `WormsDestruction.sln` and `Wriggle.Tests` (xUnit).
- `.github/copilot-instructions.md`: the project's lazy-senior-dev rules. Follow them.

## Where to go next

- Build or run tests: `wriggle-build-and-test`, `wriggle-run-game`.
- New screen: `wriggle-add-scene`. New weapon: `wriggle-add-weapon`.
- Physics or terrain: `wriggle-physics-and-terrain`.
- Rules or specs: `wriggle-spec-driven`. Writing tests: `wriggle-write-tests`.
- Debug UI: `wriggle-debug-tools`. Assets: `wriggle-assets`. Packages: `wriggle-dependencies`.
