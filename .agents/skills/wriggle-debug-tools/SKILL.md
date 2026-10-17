---
name: wriggle-debug-tools
description: Add ImGui debug widgets, scene debug controls, live tuning sliders and developer hotkeys to Wriggle. Use when adding debug UI, tuning values live, or adding a developer shortcut.
---

## Where things go

- Project-wide tools: `Game/UI/DebugUI.cs`. Its `Draw(scene)` shows FPS, window size, mouse position, the current scene name, and buttons for Restart scene and Quit.
- Scene-specific widgets: override `DrawDebugUI()` in the scene. It runs inside `rlImGui.Begin`/`End`.
- Physics tuning: `PhysicsWorld.DebugMenu()` in `Game/Physics/PhysicsWorld.cs`.

## Existing hotkeys

| Key | Action |
| --- | ------ |
| `F1` | ImGui demo window |
| `F2` | Debug window |
| `F10` | Screenshot |

Hotkeys are handled in `DebugUI.ProcessShortcuts()`. Add new ones there.

## ImGui rules

- Calls use ImGuiNET: `ImGui.Begin("Title")` ... `ImGui.End()`. Always pair them. Note that `DebugUI` already calls `ImGui.Begin("Debug")`.
- Use `ImGui.SameLine()` between buttons, and `ImGui.Separator()` between groups, as `DebugUI.cs` does.
- Debug tooling is not game logic. Keep it out of `specs/` and out of tests.
