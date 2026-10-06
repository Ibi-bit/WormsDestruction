namespace My2DGame.Core;

/// <summary>
/// Base class for every game screen (title, gameplay, pause, settings...).
/// The lifecycle is: OnEnter -> Update/Draw per frame -> OnExit.
/// Use <see cref="Scenes"/> to switch to other scenes.
/// </summary>
public abstract class Scene
{
    /// <summary>Short human-readable name shown in the debug UI.</summary>
    public abstract string Name { get; }

    /// <summary>The manager this scene is attached to. Use it to switch scenes.</summary>
    protected SceneManager Scenes { get; private set; } = null!;

    internal void Attach(SceneManager manager) => Scenes = manager;

    public virtual void OnEnter() { }

    /// <summary>Frame update. <paramref name="dt"/> is elapsed seconds (Raylib.GetFrameTime).</summary>
    public virtual void Update(float dt) { }

    /// <summary>World rendering. Runs inside BeginDrawing/EndDrawing, before the ImGui pass.</summary>
    public virtual void Draw() { }

    /// <summary>Scene-specific ImGui widgets. Runs inside rlImGui.Begin/End.</summary>
    public virtual void DrawDebugUI() { }

    public virtual void OnExit() { }
}
