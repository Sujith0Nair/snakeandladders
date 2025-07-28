using Deck;

namespace SaL.Gameplay.Commands
{
    public class SwapPositionsCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.SwapPositions;

        public void Execute(CommandContext context)
        {
            var attackerPlayer = context.LobbyManager.GetPlayerData(context.AttackerId);
            var targetPlayer = context.LobbyManager.GetPlayerData(context.TargetId);

            if (attackerPlayer == null || targetPlayer == null) return;

            // We swap the values. The NetworkVariable synchronization will handle the rest.
            var attackerCell = attackerPlayer.CurrentCellIndex.Value;
            var targetCell = targetPlayer.CurrentCellIndex.Value;

            attackerPlayer.CurrentCellIndex.Value = targetCell;
            targetPlayer.CurrentCellIndex.Value = attackerCell;
        }
    }
}
