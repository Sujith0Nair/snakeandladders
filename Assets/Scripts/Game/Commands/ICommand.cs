using SaL.Gameplay.Managers;

namespace SaL.Gameplay.Commands
{
    /// <summary>
    /// Defines the contract for a command that can be executed on the host,
    /// representing the effect of a card.
    /// </summary>
    public interface ICommand
    {
        void Execute(CommandContext context);
    }

    /// <summary>
    /// A data structure to pass all necessary context to a command's Execute method.
    /// This avoids having a long list of parameters in the Execute method signature.
    /// </summary>
    public struct CommandContext
    {
        public GameManager GameManager;
        public LobbyManager LobbyManager;
        public DeckManager DeckManager;
        public ulong AttackerId;
        public ulong TargetId;
        public int SelectedLadderId;
        // We can add more context here later if needed.
    }
}
