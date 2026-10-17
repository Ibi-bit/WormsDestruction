using Raylib_cs;
using System.Numerics;

namespace Wriggle.VisualEffects;

public sealed class ExplosionEffect : IDisposable
{
    private const float DurationSeconds = 0.35f;
    private float elapsed;

    public Vector2 Position { get; }
    public float Radius { get; }
    public bool IsFinished => elapsed >= DurationSeconds;

    public ExplosionEffect(Vector2 position, float radius)
    {
        if (radius <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));

        Position = position;
        Radius = radius;
    }

    public void Update(float dt)
    {
        if (dt > 0f)
            elapsed = MathF.Min(elapsed + dt, DurationSeconds);
    }

    public void Draw()
    {
        float progress = elapsed / DurationSeconds;
        float growth = 0.12f + 0.88f * (1f - MathF.Pow(1f - progress, 2f));
        byte alpha = (byte)(255f * (1f - progress));

        Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius * growth, new Color((byte)255, (byte)125, (byte)0, alpha));
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius * growth * 0.58f, new Color((byte)235, (byte)35, (byte)15, alpha));
    }

    public void Dispose()
    {
    }
}
