using Deck;
using SaL.Core;

namespace SaL.Gameplay.Commands
{
    public class LadderVandalismCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.LadderVandalism;

        public void Execute(CommandContext context)
        {
            var card = CardRegistry.Instance.GetCardById(context.GameManager.GetPlayedCardId());
            if (card == null) return;

            // The actual blocking is done on the GameManager state
            context.GameManager.AddEffect(card, 0, context.SelectedLadderId); // No specific target player
        }
    }
}
