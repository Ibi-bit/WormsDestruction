using System.Numerics;
using ImGuiNET;
using My2DGame.Core;
using Raylib_cs;

namespace My2DGame.UI;

/// <summary>
/// The always-available ImGui overlay: global hotkeys, a scene-aware debug
/// window (FPS, timings, mouse/world position, scene-specific controls) and
/// the ImGui demo window. This is the natural place to add editor tools,
/// a console, or draw-call stats later.
/// </summary>
public sealed class DebugUI
{
    private readonly SceneManager _scenes;

    private bool _showDebugWindow;
    private bool _showDemoWindow;

    public DebugUI(SceneManager scenes, bool showDebugWindowOnStart)
    {
        _scenes = scenes;
        _showDebugWindow = showDebugWindowOnStart;
    }

    /// <summary>Global hotkeys, checked before a scene's Update.</summary>
    public bool ProcessShortcuts()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F1)) _showDemoWindow = !_showDemoWindow;
        if (Raylib.IsKeyPressed(KeyboardKey.F2)) _showDebugWindow = !_showDebugWindow;
        return Raylib.IsKeyPressed(KeyboardKey.F10);
    }

    public void Draw(Scene? scene)
    {
        if (_showDemoWindow) ImGui.ShowDemoWindow(ref _showDemoWindow);
        if (!_showDebugWindow) return;

        ImGui.SetNextWindowPos(new Vector2(12f, 12f), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Scene: {scene?.Name ?? "(none)"}");
            ImGui.Separator();

            ImGui.Text($"FPS: {Raylib.GetFPS()}   Frame: {Raylib.GetFrameTime() * 1000f:F2} ms");
            ImGui.Text($"Window: {Raylib.GetScreenWidth()} x {Raylib.GetScreenHeight()}   Time: {Raylib.GetTime():F1}s");
            ImGui.Text($"Mouse (screen): {Raylib.GetMousePosition()}");

            ImGui.Separator();

            if (ImGui.Button("Restart scene")) _scenes.RestartCurrent();
            ImGui.SameLine();
            if (ImGui.Button("Quit")) Raylib.CloseWindow();

            ImGui.Separator();

            float uiScale = ImGui.GetIO().FontGlobalScale;
            if (ImGui.SliderFloat("UI scale", ref uiScale, 0.75f, 2f))
                ImGui.GetIO().FontGlobalScale = uiScale;

            ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.6f, 1f), "F1 demo   F2 toggle this window");
        }
        ImGui.End();
    }

}
