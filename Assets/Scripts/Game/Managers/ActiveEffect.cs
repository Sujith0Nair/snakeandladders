using Deck;

namespace SaL.Gameplay.Managers
{
    /// <summary>
    /// A data class to hold information about an ongoing status effect.
    /// </summary>
    public class ActiveEffect
    {
        public CardSO.ActionCardType EffectType;
        public CardSO.EffectDuration Duration;
        public ulong TargetId; // The player who is affected
        public int EffectValue; // e.g., The ID of the ladder being blocked
    }
}
