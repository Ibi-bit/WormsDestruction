using Raylib_cs;
using System.Numerics;
using Wriggle.World;
using Wriggle.Physics;

namespace Wriggle.Characters;

public abstract class Character
{
    public virtual float Health { get; set; }
    public abstract Body Body { get; }

    public virtual void TakeDamage(float amount)
    {
        if (amount > 0f)
            Health = MathF.Max(0f, Health - amount);
    }

    public abstract void Draw();

}
