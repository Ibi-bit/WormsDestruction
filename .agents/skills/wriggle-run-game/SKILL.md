---
name: wriggle-run-game
description: Run the Wriggle game window, edit gameconfig.json window settings, and take screenshots, including automated screenshots with GAME_SHOT_PATH. Use when launching the game, checking visuals, or capturing a screenshot.
---

## Run

From the repository root:

```bash
dotnet run
```

The first run writes `gameconfig.json` next to the built executable. Edit it to change window size, title, VSync and debug window visibility. Defaults live in `Game/Core/GameConfig.cs`.

## Screenshots

- `F10` saves a screenshot next to the executable while playing.
- Automated: `GAME_SHOT_PATH=/absolute/path.png GAME_SHOT_FRAME=120 dotnet run`. The game saves after frame `GAME_SHOT_FRAME` (default 120) and keeps running. Use this to check visuals without clicking.

## Keys

| Key | Action |
| --- | ------ |
| `F1` | Toggle the ImGui demo window |
| `F2` | Toggle the debug window |
| `F10` | Screenshot |

Escape is not handled globally (`Game.cs` sets the exit key to none). Each scene decides what Escape does.
