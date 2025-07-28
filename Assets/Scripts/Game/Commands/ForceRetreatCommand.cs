using Deck;
using SaL.Core;
using UnityEngine;

namespace SaL.Gameplay.Commands
{
    public class ForceRetreatCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.Retreat;

        public void Execute(CommandContext context)
        {
            var targetPlayer = context.LobbyManager.GetPlayerData(context.TargetId);
            if (targetPlayer == null) return;

            // Card data is fetched via the GameManager now
            var card = CardRegistry.Instance.GetCardById(context.GameManager.GetPlayedCardId());
            if (card == null) return;

            var targetCell = targetPlayer.CurrentCellIndex.Value - card.RetreatMoveTileCount;
            targetPlayer.CurrentCellIndex.Value = context.GameManager.ResolveFinalCellIndex(targetCell, context.TargetId);
        }
    }
}
