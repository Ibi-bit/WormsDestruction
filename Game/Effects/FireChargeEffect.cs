using System.Numerics;
using Raylib_cs;

namespace Wriggle.VisualEffects;

public sealed class FireChargeEffect
{
    private const float ChargeDuration = 1.5f;

    public Vector2 Position { get; set; }
    public Vector2 AimDirection { get; set; }
    public float HeldTime { get; set; }
    public bool IsActive { get; set; }

    public float Charge => Math.Clamp(HeldTime / ChargeDuration, 0f, 1f);

    public void Draw()
    {
        if (!IsActive)
            return;

        float pulse = 1f + 0.08f * MathF.Sin(HeldTime * 18f);
        float radius = 18f + 18f * Charge;
        byte alpha = (byte)(100 + 125 * Charge);
        Color outer = new((byte)255, (byte)(80 + 120 * Charge), (byte)20, alpha);
        Color inner = new((byte)255, (byte)210, (byte)40, (byte)Math.Min(255, alpha + 25));
        Vector2 direction = AimDirection.LengthSquared() > 0.0001f
            ? Vector2.Normalize(AimDirection)
            : Vector2.UnitX;
        float indicatorLength = 60f + 100f * Charge;
        Vector2 indicatorEnd = Position + direction * indicatorLength;
        Vector2 perpendicular = new(-direction.Y, direction.X);
        Vector2 arrowLeft = indicatorEnd - direction * 12f + perpendicular * 7f;
        Vector2 arrowRight = indicatorEnd - direction * 12f - perpendicular * 7f;

        Raylib.DrawLineEx(Position, indicatorEnd, 2f + 2f * Charge, outer);
        Raylib.DrawLineEx(indicatorEnd, arrowLeft, 2f, outer);
        Raylib.DrawLineEx(indicatorEnd, arrowRight, 2f, outer);
        Raylib.DrawCircle((int)indicatorEnd.X, (int)indicatorEnd.Y, 4f + 3f * Charge, inner);
        Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, radius * pulse, outer);
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, (5f + 8f * Charge) * pulse, inner);
    }
}