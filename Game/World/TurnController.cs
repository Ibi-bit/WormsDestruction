using Wriggle.Players;

namespace Wriggle.World;

public sealed class TurnController
{
    private readonly GameWorld world;
    private int currentTurnIndex = 0;

    public int CurrentTurnIndex => currentTurnIndex;

    public TurnController(GameWorld world)
    {
        this.world = world;
    }

    public Player CurrentPlayer => world.Players[currentTurnIndex];

    public void AdvanceTurn()
    {
        if (world.Players.Count == 0)
            return;

        currentTurnIndex = (currentTurnIndex + 1) % world.Players.Count;
    }

    public void ResetTurnOrder()
    {
        currentTurnIndex = 0;
    }
}
