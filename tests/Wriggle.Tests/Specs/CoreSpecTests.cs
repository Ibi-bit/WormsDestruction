using System.Reflection;
using Wriggle.Core;
using Xunit;

namespace Wriggle.Tests.Specs;

// Covers specs/core.allium. FrameProcessed runs inside Game's Raylib loop and has no testable seam, so it is not covered here.
public class CoreSpecTests
{
    private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    private sealed class SceneA : TestScene
    {
        public SceneA(List<string> log) : base("A", log) { }
    }

    private sealed class SceneB : TestScene
    {
        public SceneB(List<string> log) : base("B", log) { }
    }

    private class TestScene : Scene
    {
        private readonly string _name;
        private readonly List<string> _log;

        protected TestScene(string name, List<string> log)
        {
            _name = name;
            _log = log;
            _log.Add($"built:{name}");
        }

        public override string Name => _name;
        public override void OnEnter() => _log.Add($"enter:{_name}");
        public override void OnExit() => _log.Add($"exit:{_name}");
    }

    private static SceneManager NewManager(List<string> log) => new SceneManager()
        .Register<SceneA>(() => new SceneA(log))
        .Register<SceneB>(() => new SceneB(log));

    private static Type? Pending(SceneManager manager) =>
        typeof(SceneManager).GetField("_pending", Private)!.GetValue(manager) as Type;

    [Fact]
    [Trait("Obligation", "entity-fields.Scene")]
    public void Scene_HasStatus()
    {
        Assert.NotNull(typeof(Scene).GetProperty("Status", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.SceneManager")]
    public void SceneManager_HasCurrentAndPending()
    {
        Assert.NotNull(typeof(SceneManager).GetProperty("Current"));
        Assert.NotNull(typeof(SceneManager).GetField("_pending", Private));
    }

    [Fact]
    [Trait("Obligation", "entity-optional.SceneManager.current")]
    public void Current_IsNullUntilFirstSwitchThenSet()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        Assert.Null(manager.Current);

        manager.ChangeTo<SceneA>();
        manager.ApplyPending();
        Assert.NotNull(manager.Current);
    }

    [Fact]
    [Trait("Obligation", "entity-optional.SceneManager.pending")]
    public void Pending_IsNullWhenNothingQueued_AndSetWhenQueued()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        Assert.Null(Pending(manager));

        manager.ChangeTo<SceneA>();
        Assert.NotNull(Pending(manager));

        manager.ApplyPending();
        Assert.Null(Pending(manager));
    }

    [Fact]
    [Trait("Obligation", "rule-success.GameStarted")]
    public void GameStarted_ActivatesTheStartingScene()
    {
        var log = new List<string>();
        var manager = NewManager(log);

        manager.ChangeTo<SceneA>();
        manager.ApplyPending();

        Assert.IsType<SceneA>(manager.Current);
        Assert.Contains("enter:A", log);
    }

    [Fact]
    [Trait("Obligation", "rule-success.SceneSwitchRequested")]
    public void SceneSwitchRequested_QueuesTargetWithoutChangingCurrent()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        manager.ChangeTo<SceneA>();
        manager.ApplyPending();

        manager.ChangeTo<SceneB>();
        Assert.IsType<SceneA>(manager.Current);
        Assert.Equal(typeof(SceneB), Pending(manager));
    }

    [Fact]
    [Trait("Obligation", "rule-success.RestartCurrentScene")]
    public void RestartCurrentScene_RebuildsCurrentSceneOnNextApply()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        manager.ChangeTo<SceneA>();
        manager.ApplyPending();

        manager.RestartCurrent();
        manager.ApplyPending();

        Assert.Equal(2, log.Count(entry => entry == "built:A"));
        Assert.IsType<SceneA>(manager.Current);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.RestartCurrentScene.1")]
    public void RestartCurrentScene_IgnoredWhenNoCurrentScene()
    {
        var log = new List<string>();
        var manager = NewManager(log);

        manager.RestartCurrent();
        manager.ApplyPending();

        Assert.Null(manager.Current);
        Assert.DoesNotContain("built:A", log);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.RestartCurrentScene.2")]
    public void RestartCurrentScene_IgnoredWhenSwitchAlreadyQueued()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        manager.ChangeTo<SceneA>();
        manager.ApplyPending();

        manager.ChangeTo<SceneB>();
        manager.RestartCurrent();
        manager.ApplyPending();

        Assert.IsType<SceneB>(manager.Current);
    }

    [Fact]
    [Trait("Obligation", "rule-success.SwitchSceneApplied")]
    public void SwitchSceneApplied_ExitsOutgoingThenEntersTarget()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        manager.ChangeTo<SceneA>();
        manager.ApplyPending();
        log.Clear();

        manager.ChangeTo<SceneB>();
        manager.ApplyPending();

        Assert.Equal(new[] { "exit:A", "built:B", "enter:B" }, log);
        Assert.IsType<SceneB>(manager.Current);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.SwitchSceneApplied.1")]
    public void SwitchSceneApplied_NoOpWhenNothingPending()
    {
        var log = new List<string>();
        var manager = NewManager(log);
        manager.ChangeTo<SceneA>();
        manager.ApplyPending();
        log.Clear();

        manager.ApplyPending();

        Assert.Empty(log);
        Assert.IsType<SceneA>(manager.Current);
    }
}
