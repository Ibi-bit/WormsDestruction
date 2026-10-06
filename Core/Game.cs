using My2DGame.Scenes;
using My2DGame.UI;
using Raylib_cs;
using rlImGui_cs;

namespace My2DGame.Core;

/// <summary>
/// Owns the raylib window, the ImGui backend and the scene manager, and runs
/// the main loop: update -> draw world -> draw UI (ImGui) -> present.
/// </summary>
public sealed class Game : IDisposable
{
    private readonly GameConfig _config;
    private readonly SceneManager _scenes = new();
    private readonly DebugUI _debugUI;

    public Game(GameConfig config)
    {
        _config = config;

        ConfigFlags flags = ConfigFlags.Msaa4xHint;
        if (_config.EnableVsync) flags |= ConfigFlags.VSyncHint;
        if (_config.ResizableWindow || _config.StartMaximized) flags |= ConfigFlags.ResizableWindow;

        Raylib.SetConfigFlags(flags);
        Raylib.InitWindow(_config.WindowWidth, _config.WindowHeight, _config.WindowTitle);
        Raylib.InitAudioDevice();
        Raylib.SetTargetFPS(_config.TargetFps);

        // Let scenes decide what Escape does (title quits, gameplay returns to menu).
        Raylib.SetExitKey(KeyboardKey.Null);

        // Sets up Dear ImGui with the raylib backend and a dark theme.
        rlImGui.Setup(darkTheme: true);

        _scenes.Register<GameScene>(static () => new GameScene());

        _debugUI = new DebugUI(_scenes, config.ShowDebugWindow);
    }

    public void Run()
    {
        // Start in the editable starter scene.
        _scenes.ChangeTo<GameScene>();
        _scenes.ApplyPending();

        // Test/CI hook: GAME_SHOT_PATH=/path/to.png [GAME_SHOT_FRAME=120] makes
        // the game save a screenshot after N frames and keep running.
        string? shotPath = Environment.GetEnvironmentVariable("GAME_SHOT_PATH");
        int shotFrame = int.TryParse(Environment.GetEnvironmentVariable("GAME_SHOT_FRAME"), out int parsed)
            ? parsed
            : 120;

        bool firstFrame = true;
        int frameCount = 0;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            bool screenshotRequested = _debugUI.ProcessShortcuts();

            _scenes.Current?.Update(dt);

            // Scenes may request a switch/restart during Update — apply it now.
            _scenes.ApplyPending();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(15, 15, 20, 255));

            _scenes.Current?.Draw();

            rlImGui.Begin();
            _scenes.Current?.DrawDebugUI();
            _debugUI.Draw(_scenes.Current);
            rlImGui.End();

            int currentFrame = frameCount + 1;
            bool testScreenshotDue = shotPath is not null && currentFrame == shotFrame;
            if (testScreenshotDue || screenshotRequested)
            {
                string screenshotPath = testScreenshotDue
                    ? shotPath!
                    : Path.Combine(AppContext.BaseDirectory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Raylib.TakeScreenshot(screenshotPath);
            }

            Raylib.EndDrawing();
            frameCount = currentFrame;

            if (firstFrame)
            {
                firstFrame = false;

                // Maximize only after the window has been mapped/shown — some
                // window managers (incl. XWayland) fail or glitch if you ask
                // for a resize before the first presented frame.
                if (_config.StartMaximized) Raylib.MaximizeWindow();
            }
        }
    }

    public void Dispose()
    {
        rlImGui.Shutdown();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}
