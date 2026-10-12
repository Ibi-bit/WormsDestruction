using Wriggle.Players;
using Wriggle.Weapons;
using Wriggle.Physics;

namespace Wriggle.World;

public class GameWorld
{
    public Terrain Terrain => PhysicsWorld?.Terrain ?? throw new InvalidOperationException("PhysicsWorld is not initialized.");
    public List<Player> Players { get; private set; } = [];
    public List<InventoryWeapon> Weapons { get; private set; } = [];
    public RoundController RoundController { get; }
    public TurnController TurnController { get; }
    public PhysicsWorld PhysicsWorld;

    public GameWorld(PhysicsWorld physicsWorld)
    {
        PhysicsWorld = physicsWorld;
        RoundController = new RoundController(this);
        TurnController = new TurnController(this);
    }
    

    public void AddWeapon(Weapon weapon, Dictionary<int, int> ammoByRound)
    {
        Weapons.Add(new InventoryWeapon(weapon, ammoByRound));
    }
    public void AddWeapon(Weapon weapon)
    {
        Weapons.Add(new InventoryWeapon(weapon));
    }
    public void UpdatePlayersWeaponInventory()
    {
        foreach (Player player in Players)
        {
            foreach (InventoryWeapon inventoryWeapon in Weapons)
            {
                if (!player.WeaponInventory.TryGetValue(inventoryWeapon.Weapon, out Ammo? ammo))
                {
                    ammo = new() { IsEnabled = false };
                    player.WeaponInventory[inventoryWeapon.Weapon] = ammo;
                }

                foreach (KeyValuePair<int, int> roundAmmo in inventoryWeapon.AmmoGrantedByRound)
                {
                    ammo.AddRoundAmmo(roundAmmo.Key, roundAmmo.Value);
                }
            }
        }
    }
    

    public void AddPlayer(Player player)
    {
        player.WeaponInventory.Clear();

        foreach (InventoryWeapon inventoryWeapon in Weapons)
        {
            Ammo ammo = new() { IsEnabled = false };

            foreach (KeyValuePair<int, int> roundAmmo in inventoryWeapon.AmmoGrantedByRound)
            {
                ammo.AddRoundAmmo(roundAmmo.Key, roundAmmo.Value);
            }

            player.WeaponInventory[inventoryWeapon.Weapon] = ammo;
        }

        Players.Add(player);
    }

    public void GiveRoundAmmo(Player player, int round)
    {
        foreach (InventoryWeapon inventoryWeapon in Weapons)
        {
            if (!inventoryWeapon.AmmoGrantedByRound.TryGetValue(round, out int amount))
                continue;

            if (!player.WeaponInventory.TryGetValue(inventoryWeapon.Weapon, out Ammo? ammo))
                continue;

            ammo.AddRoundAmmo(round, amount);
        }
    }
}
