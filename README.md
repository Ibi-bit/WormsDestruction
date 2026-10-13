# 2D Game Starter

A small C# game foundation using raylib for windowing, input, rendering, and audio, with Dear ImGui for development tools.

## Run

```bash
dotnet run
```

The default window settings are defined in `Game/Core/GameConfig.cs`. On first run, `gameconfig.json` is written next to the executable; edit it to adjust the window size, title, VSync, and debug window visibility.

For automated screenshots, set `GAME_SHOT_PATH` and optionally `GAME_SHOT_FRAME` (defaults to frame 120). The game saves the screenshot after the selected frame and continues running.

## Start Building

`Game/Scenes/GameScene.cs` contains the current gameplay scene. Put game state and frame updates in `Update`, world drawing in `Draw`, and optional scene-specific ImGui controls in `DrawDebugUI`. Use `OnEnter` and `OnExit` to load and release scene resources.

To add a scene, subclass `Scene` and register a factory in `Game/Core/Game.cs`:

```csharp
_scenes.Register<PauseScene>(static () => new PauseScene());
```

Request transitions from a scene with `Scenes.ChangeTo<PauseScene>()`. Transitions are applied by the main loop after the current scene's update.

## Development Controls

| Input | Action |
|-------|--------|
| `F1` | Toggle the ImGui demo window |
| `F2` | Toggle the debug window |
| `F10` | Save a screenshot next to the executable |

The debug window shows frame timing, window dimensions, mouse position, and scene-specific controls. Add project-wide development tools to `Game/UI/DebugUI.cs`.

## Project Layout

```text
My2DGame.csproj        Project and dependency configuration
Game/                 All game source code and runtime assets
  Program.cs           Entry point
  Core/                Game loop, settings, base scene, scene manager
  Characters/          Character types and controllers
  Physics/              Bodies, world simulation, and area effects
  Scenes/               Gameplay scenes
  UI/DebugUI.cs         ImGui development overlay and shortcuts
  Weapons/              Weapons and projectiles
  World/                Terrain and world data
  Content/              Runtime assets, copied to build output
```

Files in `Game/Content/` are copied to `Content/` beside the executable. Load them from `AppContext.BaseDirectory`, for example with `Path.Combine(AppContext.BaseDirectory, "Content", "textures", "background.png")`.

## Dependencies

`Raylib-cs` is pinned to 7.0.1 and `rlImgui-cs` to 3.2.0. Keep these versions compatible because the ImGui backend depends on raylib's native ABI.

## Related Projects

- [jev-router](https://github.com/Ibi-bit/jev-router) - A prompt router for AI coding agents that selects appropriate skills and models based on prompt complexity.
