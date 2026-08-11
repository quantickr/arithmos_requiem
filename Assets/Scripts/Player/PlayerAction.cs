using ArithmosRequiem.Cards;

namespace ArithmosRequiem.Player
{
    public enum PlayerActionType
    {
        PlayCard,
        DiscardCard,
        ExileCard,
        EndTurn
    }

    /// <summary>Одно решение игрока/ИИ в фазе розыгрыша.</summary>
    public readonly struct PlayerAction
    {
        public readonly PlayerActionType Type;
        public readonly Card Target; // для действий над картой

        public PlayerAction(PlayerActionType type, Card target = null)
        {
            Type = type;
            Target = target;
        }

        public static PlayerAction Play(Card card) => new PlayerAction(PlayerActionType.PlayCard, card);
        public static PlayerAction Discard(Card card) => new PlayerAction(PlayerActionType.DiscardCard, card);
        public static PlayerAction Exile(Card card) => new PlayerAction(PlayerActionType.ExileCard, card);
        public static PlayerAction End() => new PlayerAction(PlayerActionType.EndTurn);
    }
}
