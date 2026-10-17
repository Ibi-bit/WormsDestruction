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
    public Weapon SelectedWeapon => WeaponInventory.Keys.ElementAt(CurrentWeaponIndex);
    public int CurrentWeaponIndex { get; private set; } = 0;

    public Player(int id)
    {
        Id = id;
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= WeaponInventory.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        CurrentWeaponIndex = index;
    }

    public void CycleWeapon(int direction)
    {
        if (WeaponInventory.Count == 0)
            throw new InvalidOperationException("Cannot cycle weapons when the inventory is empty.");

        CurrentWeaponIndex = (CurrentWeaponIndex + Math.Sign(direction) + WeaponInventory.Count)
            % WeaponInventory.Count;
    }
}