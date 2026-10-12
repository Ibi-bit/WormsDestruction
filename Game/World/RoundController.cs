using Wriggle.Players;

namespace Wriggle.World;

public sealed class RoundController
{
    private readonly GameWorld world;
    private int currentRound = 0;

    public int CurrentRound => currentRound;

    public RoundController(GameWorld world)
    {
        this.world = world;
    }

    public void StartNextRound()
    {
        currentRound++;

        foreach (Player player in world.Players)
        {
            world.GiveRoundAmmo(player, currentRound);
        }
    }
}
