using Deck;

namespace SaL.Gameplay.Commands
{
    public class ForceToSnakeCommand : ICommand
    {
        public const CardSO.ActionCardType Type = CardSO.ActionCardType.ForceToSnake;

        public void Execute(CommandContext context)
        {
            var targetPlayer = context.LobbyManager.GetPlayerData(context.TargetId);
            if (targetPlayer == null) return;

            var currentCell = targetPlayer.CurrentCellIndex.Value;
            var closestSnakeHead = context.GameManager.GetClosestSnakeHead(currentCell);

            // Set the player's position to the snake head. The resolution logic will handle the slide.
            targetPlayer.CurrentCellIndex.Value = context.GameManager.ResolveFinalCellIndex(closestSnakeHead, context.TargetId);
        }
    }
}
