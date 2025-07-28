namespace SaL.Gameplay.Managers
{
    /// <summary>
    /// A data class to hold all end-of-game statistics for a single player.
    /// This is not a NetworkBehaviour and will only exist on the host.
    /// </summary>
    public class PlayerStats
    {
        public ulong PlayerId;
        public string PlayerName;
        public int FinishPosition = -1; // -1 means DNF

        public int OffensiveCardsUsed;
        public int DefensiveCardsUsed;
        public int MovementCardsUsed;

        public int OffensiveCardsReceived;
        public int OffensiveCardsCountered;

        public bool WasDisconnected;
    }
}
