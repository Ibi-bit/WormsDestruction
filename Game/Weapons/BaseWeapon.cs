using Raylib_cs;
using System.Numerics;
using Wriggle.Characters;
using Wriggle.Physics;

namespace Wriggle.Weapons;

public abstract class Weapon
{
    public abstract void Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force, Body? owner = null);
    public abstract void Update(PhysicsWorld world, IReadOnlyList<Character> characters, float dt);
}

public sealed class Ammo
{
    public int Count { get; private set; }
    public bool IsEnabled { get; set; }
    public Dictionary<int, int> AmmoByRound { get; } = new();

    public void AddRoundAmmo(int round, int count)
    {
        if (count <= 0)
            return;

        AmmoByRound[round] = count;
        Count += count;
    }

    public bool TrySpendAmmo(int amount)
    {
        if (amount <= 0)
            return true;

        if (Count < amount)
            return false;

        Count -= amount;
        return true;
    }
}

public sealed class InventoryWeapon
{
    public Weapon Weapon { get; }
    public Dictionary<int, int> AmmoGrantedByRound { get; } = new();

    public InventoryWeapon(Weapon weapon)
    {
        Weapon = weapon;
    }
    public InventoryWeapon(Weapon weapon, Dictionary<int, int> ammoByRound)
    {
        Weapon = weapon;
        AmmoGrantedByRound = new Dictionary<int, int>(ammoByRound);
    }

    public void AddRoundAmmo(int round, int count)
    {
        AmmoGrantedByRound[round] = count;
    }
}