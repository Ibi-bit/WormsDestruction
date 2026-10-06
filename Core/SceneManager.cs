namespace My2DGame.Core;

/// <summary>
/// Owns scene instances and pending transitions. Scenes are created lazily from
/// registered factories, so a scene restart (e.g. "play again") simply discards
/// the cached instance and rebuilds it.
/// </summary>
public sealed class SceneManager
{
    private readonly Dictionary<Type, Func<Scene>> _factories = new();
    private readonly Dictionary<Type, Scene> _instances = new();
    private Type? _pending;

    public Scene? Current { get; private set; }

    public SceneManager Register<T>(Func<Scene> factory) where T : Scene
    {
        _factories[typeof(T)] = factory;
        return this;
    }

    /// <summary>Queue a switch; it is applied by the game loop right after the current Update.</summary>
    public void ChangeTo<T>() where T : Scene => _pending = typeof(T);

    /// <summary>Runtime variant, e.g. for scene types resolved from config.</summary>
    public void ChangeTo(Type sceneType) => _pending = sceneType;

    /// <summary>Rebuild the current scene from scratch (same effect as a fresh start).</summary>
    public void RestartCurrent()
    {
        if (Current is null) return;
        _instances.Remove(Current.GetType());
        _pending = Current.GetType();
    }

    public void ApplyPending()
    {
        if (_pending is null) return;

        Type type = _pending;
        _pending = null;

        Current?.OnExit();

        if (!_instances.TryGetValue(type, out Scene? scene))
        {
            scene = _factories[type]();
            scene.Attach(this);
            _instances[type] = scene;
        }

        Current = scene;
        scene.OnEnter();
    }
}