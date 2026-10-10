using Raylib_cs;
using System.Numerics;
using Wriggle.World;
using Wriggle.Physics;

namespace Wriggle.Characters;

public class Wriggler
{
    public enum WrigglerState
    {
        Idle,
        Moving,
        Jumping,
        Falling
    }
    public CapsuleBody Body;
    public Color Color;
    public float MoveSpeed { get; set; } = 200f;
    public float JumpForce { get; set; } = 300f;
    public float GroundAcceleration { get; set; } = 1200f;
    public float GroundDeceleration { get; set; } = 1800f;
    public float AirAcceleration { get; set; } = 450f;
    public float AirDeceleration { get; set; } = 100f;
    public float JumpBufferDuration { get; set; } = 0.12f;
    public float CoyoteTimeDuration { get; set; } = 0.1f;
    public WrigglerState State { get; private set; } = WrigglerState.Idle;
    public bool IsOnGround => Body.isGrounded;
    private float jumpBufferRemaining;
    private float coyoteTimeRemaining;

    public Wriggler(Vector2 position, float height, float width, float mass, Color color)
    {
        Color = color;
        Body = new CapsuleBody(position, mass, height, width);
        Body.Restitution = 0f;
    }
    public void Update(float move, bool jumpPressed, float dt)
    {
        if (dt <= 0f)
            return;

        move = Math.Clamp(move, -1f, 1f);
        jumpBufferRemaining = jumpPressed
            ? JumpBufferDuration
            : MathF.Max(0f, jumpBufferRemaining - dt);
        coyoteTimeRemaining = IsOnGround
            ? CoyoteTimeDuration
            : MathF.Max(0f, coyoteTimeRemaining - dt);

        bool hasMoveInput = MathF.Abs(move) > 0.01f;
        float targetSpeed = move * MoveSpeed;
        float acceleration = IsOnGround
            ? (hasMoveInput ? GroundAcceleration : GroundDeceleration)
            : (hasMoveInput ? AirAcceleration : AirDeceleration);
        Body.Velocity.X = MoveTowards(Body.Velocity.X, targetSpeed, acceleration * dt);

        bool jumped = jumpBufferRemaining > 0f && coyoteTimeRemaining > 0f;
        if (jumped)
        {
            Body.Velocity.Y = -JumpForce;
            jumpBufferRemaining = 0f;
            coyoteTimeRemaining = 0f;
        }

        if (jumped || (!IsOnGround && Body.Velocity.Y < 0f))
            State = WrigglerState.Jumping;
        else if (!IsOnGround)
            State = WrigglerState.Falling;
        else if (hasMoveInput)
            State = WrigglerState.Moving;
        else
            State = WrigglerState.Idle;
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;

        return current + MathF.Sign(target - current) * maxDelta;
    }

    public void Draw()
    {
        Body.Draw();
    }
}
