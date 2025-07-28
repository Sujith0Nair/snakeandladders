using Deck;
using SaL.Core;

namespace SaL.Gameplay.Commands
{
    public class FreezeOpponentCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.FreezeOpponent;

        public void Execute(CommandContext context)
        {
            var card = CardRegistry.Instance.GetCardById(context.GameManager.GetPlayedCardId());
            if (card == null) return;
            
            context.GameManager.AddEffect(card, context.TargetId);
        }
    }
}
