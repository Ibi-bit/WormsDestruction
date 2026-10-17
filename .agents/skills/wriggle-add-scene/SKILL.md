---
name: wriggle-add-scene
description: Add a new game screen to Wriggle (title, pause, settings or menu) by subclassing Scene and registering it with SceneManager, then switching to it. Use when creating a new screen or changing between scenes.
---

## Steps

1. Create `Game/Scenes/<Name>Scene.cs`. Match the namespace and style of `Game/Scenes/GameScene.cs`.
2. Subclass `Scene` (`Game/Core/Scene.cs`). Override:
   - `Name`: required, shown in the debug window.
   - `OnEnter` / `OnExit`: load and release resources.
   - `Update(float dt)`: game state and input. Request switches here.
   - `Draw()`: world drawing only, inside `BeginDrawing`.
   - `DrawDebugUI()`: optional ImGui widgets for this scene.
3. Register it in the `Game` constructor in `Game/Core/Game.cs`:

   ```csharp
   _scenes.Register<PauseScene>(static () => new PauseScene());
   ```

4. Switch from inside `Update` with `Scenes.ChangeTo<PauseScene>()`. The switch applies after the current `Update`. Never switch from `Draw`.

## Rules to respect

- Lifecycle order is `OnEnter`, then `Update` and `Draw` each frame, then `OnExit`. See `specs/core.allium`.
- Scene restart goes through `SceneManager.RestartCurrent()`. Restart calls `OnExit` and builds a fresh instance on next activation.
- Scene-specific rules are only in the spec for that scene. The title, pause and settings scenes are not specified yet, so write a spec first (`wriggle-spec-driven`).
- Test scene registration and switching through the core spec tests (`CoreSpecTests.cs`).
