using System;
using System.Collections.Generic;
using Deck;
using SaL.Gameplay.Commands;

namespace SaL.Core
{
    /// <summary>
    /// Creates and provides instances of commands based on ActionCardType.
    /// This acts as a registry for all executable card commands.
    /// </summary>
    public class CommandFactory
    {
        private readonly Dictionary<CardSO.ActionCardType, ICommand> _commands = new()
        {
            { ForceRetreatCommand.Type, new ForceRetreatCommand() },
            { FreezeOpponentCommand.Type, new FreezeOpponentCommand() },
            { SwapPositionsCommand.Type, new SwapPositionsCommand() },
            { ForceToSnakeCommand.Type, new ForceToSnakeCommand() },
            { LadderVandalismCommand.Type, new LadderVandalismCommand() },
            { LadderLockoutCommand.Type, new LadderLockoutCommand() }
            // Add new commands here
        };

        public ICommand GetCommand(CardSO.ActionCardType actionType)
        {
            return _commands.GetValueOrDefault(actionType);
        }
    }
}
