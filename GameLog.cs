namespace DurakBenchmark;

public enum ActionType { Attack, Defend, Throw }

/// <summary>Одно действие игрока — снапшот состояния до действия + само действие.</summary>
public struct ActionRecord
{
    public ActionType Type;
    public bool Player1Acts;

    // Состояние до действия
    public List<SCard> ActorHand;       // рука действующего игрока
    public List<SCard> SeenCards;       // карты в «бито»
    public List<SCardPair> Table;       // стол на момент действия
    public int OpponentCardCount;       // карт у соперника
    public int DeckSize;                // карт в колоде
    public SCard Trump;                 // козырь

    // Что сделал игрок
    public List<SCard> PlayedCards;     // атака / подброс (пусто если defend)
    public bool ActionResult;           // true = отбился / подкинул, false = взял / пас
}

/// <summary>Лог одной партии.</summary>
public struct GameLog
{
    public SCard Trump;
    public List<ActionRecord> Actions;
    public EndGame Outcome;             // итог с точки зрения игрока 1
    public int LoserCardCount;          // карт у проигравшего в конце
}
