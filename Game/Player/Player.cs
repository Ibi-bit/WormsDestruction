using Raylib_cs;
using Wriggle.Core;
using Wriggle.World;
using Wriggle.Physics;
using Wriggle.Characters;
using Wriggle.Weapons;
using System.Numerics;

namespace Wriggle.Players;

public class Player
{
    public int Id { get; }
    public List<Wriggler> Wrigglers { get; private set; } = [];
    public int CurrentWrigglerIndex { get; private set; } = 0;
    public Wriggler CurrentWriggler => Wrigglers[CurrentWrigglerIndex];
    public float Health => Wrigglers.Sum(w => w.Health);
    public Dictionary<Weapon, Ammo> WeaponInventory { get; private set; } = [];

    public Player(int id)
    {
        Id = id;
    }
}