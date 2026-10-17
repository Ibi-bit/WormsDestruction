using System.Numerics;
using Raylib_cs;

namespace Wriggle.VisualEffects;

public class BaseEffect : IDisposable
{
    public Vector2 Position { get; set; }
    public AnimatedTexture Animation { get; }
    public Color Tint { get; set; } = Color.White;
    public float Scale { get; set; } = 1f;
    public float Duration => Animation.Duration;
    public bool IsFinished => Animation.IsFinished;

    public BaseEffect(Vector2 position, AnimatedTexture animation)
    {
        Position = position;
        Animation = animation;
    }

    public virtual void Update(float dt)
    {
        Animation.Update(dt);
    }
    public virtual void Step()
    {
        Animation.Step();
    }

    public virtual void Draw()
    {
        Animation.Draw(Position, Tint, Scale);
    }

    public virtual void Reset()
    {
        Animation.Reset();
    }

    public virtual void Dispose()
    {
        Animation.Dispose();
    }
}