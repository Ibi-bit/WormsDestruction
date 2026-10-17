using Raylib_cs;
using System.Numerics;
using Wriggle.World;
using Wriggle.Physics;
using Wriggle.Weapons;
using System.Collections.Generic;
namespace Wriggle.Characters;

public class Wriggler : Character
{
    public enum WrigglerState
    {
        Idle,
        Moving,
        Jumping,
        Falling
    }
    public override Body Body { get; }
    public override float Health { get; set; } = 100f;
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
    public Weapon SelectedWeapon { get; set; } = new Grenade();

    public Wriggler(Vector2 position, float height, float width, float mass, Color color)
    {
        Color = color;
        Body = new CapsuleBody(position, mass, height, width)
        {
            Restitution = 0f
        };
    }
    public void Update(
        PhysicsWorld world,
        IReadOnlyList<Character> characters,
        float move,
        bool jumpPressed,
        bool firePressed,
        Vector2 aimDirection,
        float dt)
    {
        if (dt <= 0f)
            return;


        if (Body.IsKnockedBack)
        {
            move = 0f;
            jumpPressed = false;
        }

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
        if (!Body.IsKnockedBack)
            Body.Velocity.X = MoveTowards(Body.Velocity.X, targetSpeed, acceleration * dt);

        bool jumped = !Body.IsKnockedBack && jumpBufferRemaining > 0f && coyoteTimeRemaining > 0f;
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

        if (firePressed)
        {
            Vector2 direction = aimDirection.LengthSquared() > 0.0001f
                ? Vector2.Normalize(aimDirection)
                : Vector2.UnitX;
            SelectedWeapon.Fire(world, Body.Position, direction, 0f, 0.5f, Body);
        }
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;

        return current + MathF.Sign(target - current) * maxDelta;
    }

    public override void Draw()
    {
        Body.Draw();
    }
}
