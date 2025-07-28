using Deck;
using SaL.Core;

namespace SaL.Gameplay.Commands
{
    public class LadderLockoutCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.LadderLockout;

        public void Execute(CommandContext context)
        {
            var card = CardRegistry.Instance.GetCardById(context.GameManager.GetPlayedCardId());
            if (card == null) return;
            
            // The actual lockout is handled by the GameManager state
            context.GameManager.AddEffect(card, context.TargetId);
        }
    }
}
