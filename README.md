# 2D Game Starter

A small C# game foundation using raylib for windowing, input, rendering, and audio, with Dear ImGui for development tools.

## Run

```bash
dotnet run
```

The default window settings are defined in `Core/GameConfig.cs`. On first run, `gameconfig.json` is written next to the executable; edit it to adjust the window size, title, VSync, and debug window visibility.

For automated screenshots, set `GAME_SHOT_PATH` and optionally `GAME_SHOT_FRAME` (defaults to frame 120). The game saves the screenshot after the selected frame and continues running.

## Start Building

`Scenes/GameScene.cs` is the empty starting scene. Put game state and frame updates in `Update`, world drawing in `Draw`, and optional scene-specific ImGui controls in `DrawDebugUI`. Use `OnEnter` and `OnExit` to load and release scene resources.

To add a scene, subclass `Scene` and register a factory in `Core/Game.cs`:

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

The debug window shows frame timing, window dimensions, mouse position, and scene-specific controls. Add project-wide development tools to `UI/DebugUI.cs`.

## Project Layout

```text
Program.cs            Entry point
Core/                 Game loop, settings, base scene, scene manager
Scenes/               Starting point for game scenes
UI/DebugUI.cs         ImGui development overlay and shortcuts
Content/              Runtime assets, copied to the build output
```

Files in `Content/` are copied beside the executable. Load them from `AppContext.BaseDirectory`, for example with `Path.Combine(AppContext.BaseDirectory, "Content", "textures", "background.png")`.

## Dependencies

`Raylib-cs` is pinned to 7.0.1 and `rlImgui-cs` to 3.2.0. Keep these versions compatible because the ImGui backend depends on raylib's native ABI.
