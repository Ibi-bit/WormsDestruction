using System.Numerics;
using System.Reflection;
using Raylib_cs;
using Wriggle.Characters;
using Wriggle.Physics;
using Wriggle.Players;
using Wriggle.Weapons;
using Wriggle.World;
using Xunit;

namespace Wriggle.Tests.Specs;

// Covers specs/world.allium. Obligations that need a Raylib texture (terrain loading, carving, painting), the Cell entity
// and the solid_alpha_threshold constant (inline in Terrain.FromImage) have no testable seam and are not covered here.
public class WorldSpecTests
{
    private sealed class TestWeapon : Weapon
    {
        public override void Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force) { }
        public override void Update(PhysicsWorld world, IReadOnlyList<Character> characters, float dt) { }
    }

    private static GameWorld NewWorld() => new(new PhysicsWorld(new Terrain(10, 10)));

    private static bool HasMember(Type type, string name) =>
        type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length > 0;

    [Fact]
    [Trait("Obligation", "value-equality.Colour")]
    public void Colour_HasStructuralEquality()
    {
        Assert.Equal(new Color(1, 2, 3, 4), new Color(1, 2, 3, 4));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.Colour")]
    public void Colour_HasRgbaComponents()
    {
        Assert.True(HasMember(typeof(Color), "R"));
        Assert.True(HasMember(typeof(Color), "G"));
        Assert.True(HasMember(typeof(Color), "B"));
        Assert.True(HasMember(typeof(Color), "A"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.Terrain")]
    public void Terrain_HasWidthHeightAndRestitution()
    {
        Assert.True(HasMember(typeof(Terrain), "W"));
        Assert.True(HasMember(typeof(Terrain), "H"));
        Assert.True(HasMember(typeof(Terrain), "Restitution"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.GameWorld")]
    public void GameWorld_HasSpecFields()
    {
        Assert.True(HasMember(typeof(GameWorld), "Terrain"));
        Assert.True(HasMember(typeof(GameWorld), "Players"));
        Assert.True(HasMember(typeof(GameWorld), "Weapons"));
        Assert.True(HasMember(typeof(RoundController), "CurrentRound"));
        Assert.True(HasMember(typeof(TurnController), "CurrentTurnIndex"));
        Assert.True(HasMember(typeof(GameWorld), "Gravity"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.Ammo")]
    public void Ammo_HasPlayerWeaponAndEnabled()
    {
        Assert.True(HasMember(typeof(Ammo), "IsEnabled"));
        Assert.True(HasMember(typeof(Ammo), "Player"));
        Assert.True(HasMember(typeof(Ammo), "Weapon"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.WeaponGrant")]
    public void WeaponGrant_TypeExists()
    {
        Assert.NotNull(typeof(GameWorld).Assembly.GetType("Wriggle.World.WeaponGrant"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.AmmoGrant")]
    public void AmmoGrant_TypeExists()
    {
        Assert.NotNull(typeof(GameWorld).Assembly.GetType("Wriggle.World.AmmoGrant"));
    }

    [Fact]
    [Trait("Obligation", "config-default.terrain_restitution_default")]
    public void TerrainRestitution_DefaultIsQuarter()
    {
        Assert.Equal(0.25f, new Terrain(1, 1).Restitution);
    }

    [Fact]
    [Trait("Obligation", "config-default.terrain_restitution_min")]
    public void TerrainRestitution_ClampsToMinimumZero()
    {
        var terrain = new Terrain(1, 1) { Restitution = -1f };
        Assert.Equal(0f, terrain.Restitution);
    }

    [Fact]
    [Trait("Obligation", "config-default.terrain_restitution_max")]
    public void TerrainRestitution_ClampsToMaximumOne()
    {
        var terrain = new Terrain(1, 1) { Restitution = 2f };
        Assert.Equal(1f, terrain.Restitution);
    }

    [Fact]
    [Trait("Obligation", "config-default.gravity_default")]
    public void Gravity_DefaultIs600()
    {
        var physics = new PhysicsWorld(new Terrain(20, 20));
        var body = new CircleBody(new Vector2(5, 5), 1f, 1f);
        physics.AddBody(body);

        physics.Step(0.1f);

        Assert.Equal(60f, body.Velocity.Y, 0.5f);
    }

    [Fact]
    [Trait("Obligation", "rule-success.RestitutionSet")]
    public void RestitutionSet_SetsTerrainRestitution()
    {
        var terrain = new Terrain(1, 1);
        terrain.Restitution = 0.5f;
        Assert.Equal(0.5f, terrain.Restitution);
    }

    [Fact]
    [Trait("Obligation", "rule-success.PlayerJoins")]
    public void PlayerJoins_AddsPlayerWithDisabledAmmoForEachWeapon()
    {
        var world = NewWorld();
        var weapon = new TestWeapon();
        world.AddWeapon(weapon);

        var player = new Player(1);
        world.AddPlayer(player);

        Assert.Contains(player, world.Players);
        Assert.False(player.WeaponInventory[weapon].IsEnabled);
    }

    [Fact]
    [Trait("Obligation", "rule-success.WeaponDefined")]
    public void WeaponDefined_AddsWeaponToWorld()
    {
        var world = NewWorld();
        world.AddWeapon(new TestWeapon());
        Assert.Single(world.Weapons);
    }

    [Fact]
    [Trait("Obligation", "rule-success.WeaponInventoryUpdated")]
    public void WeaponInventoryUpdated_GivesExistingPlayersMissingAmmo()
    {
        var world = NewWorld();
        var player = new Player(1);
        world.AddPlayer(player);

        var weapon = new TestWeapon();
        world.AddWeapon(weapon);
        Assert.False(player.WeaponInventory.ContainsKey(weapon));

        world.UpdatePlayersWeaponInventory();

        Assert.True(player.WeaponInventory.ContainsKey(weapon));
    }

    [Fact]
    [Trait("Obligation", "rule-success.RoundAmmoGranted")]
    public void RoundAmmoGranted_EnablesAmmoForThatRound()
    {
        var world = NewWorld();
        var weapon = new TestWeapon();
        world.AddWeapon(weapon, new Dictionary<int, int> { [1] = 5 });

        var player = new Player(1);
        world.AddPlayer(player);

        world.GiveRoundAmmo(player, 1);

        Assert.True(player.WeaponInventory[weapon].IsEnabled);
    }

    [Fact]
    [Trait("Obligation", "rule-success.StartNextRound")]
    public void StartNextRound_IncrementsRoundAndGrantsThatRound()
    {
        var world = NewWorld();
        var weapon = new TestWeapon();
        var player = new Player(1);
        world.AddPlayer(player);
        world.AddWeapon(weapon, new Dictionary<int, int> { [1] = 5 });
        world.UpdatePlayersWeaponInventory();

        world.RoundController.StartNextRound();

        Assert.Equal(1, world.RoundController.CurrentRound);
        Assert.True(player.WeaponInventory[weapon].AmmoByRound.ContainsKey(1));
    }

    [Fact]
    [Trait("Obligation", "rule-success.AdvanceTurn")]
    public void AdvanceTurn_MovesToNextPlayerAndWraps()
    {
        var world = NewWorld();
        world.AddPlayer(new Player(1));
        world.AddPlayer(new Player(2));

        world.TurnController.AdvanceTurn();
        Assert.Equal(1, world.TurnController.CurrentTurnIndex);

        world.TurnController.AdvanceTurn();
        Assert.Equal(0, world.TurnController.CurrentTurnIndex);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.AdvanceTurn.1")]
    public void AdvanceTurn_NoOpWithoutPlayers()
    {
        var world = NewWorld();

        world.TurnController.AdvanceTurn();

        Assert.Equal(0, world.TurnController.CurrentTurnIndex);
    }

    [Fact]
    [Trait("Obligation", "rule-success.ResetTurnOrder")]
    public void ResetTurnOrder_ReturnsToFirstPlayer()
    {
        var world = NewWorld();
        world.AddPlayer(new Player(1));
        world.AddPlayer(new Player(2));
        world.TurnController.AdvanceTurn();

        world.TurnController.ResetTurnOrder();

        Assert.Equal(0, world.TurnController.CurrentTurnIndex);
    }
}
