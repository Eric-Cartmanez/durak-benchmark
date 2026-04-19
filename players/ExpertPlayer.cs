namespace DurakBenchmark;

// Порт MPlayer1 (Vasya) из проекта CardFool.
// Экспертная фабрика: SmartAttackStrategy + SmartDefendStrategy с аналитикой вышедших карт.
internal class ExpertPlayer : IPlayer
{
    private readonly List<SCard> hand = new();
    private IAttackStrategy? _attackStrategy;
    private IDefendStrategy? _defendStrategy;

    private void EnsureInitialized()
    {
        if (_attackStrategy == null)
        {
            IStrategyFactory factory = new ExpertStrategyFactory();
            _attackStrategy = factory.CreateAttackStrategy();
            _defendStrategy = factory.CreateDefendStrategy();
        }
    }

    public string GetName() => "Vasya (Expert)";
    public int GetCount() => hand.Count;
    public void AddToHand(SCard card) => hand.Add(card);

    public List<SCard> LayCards()
    {
        EnsureInitialized();
        return _attackStrategy!.LayCards(hand, MTable.GetTrump().Suit);
    }

    public bool Defend(List<SCardPair> table)
    {
        EnsureInitialized();
        return _defendStrategy!.Defend(hand, table, MTable.GetTrump().Suit);
    }

    public bool AddCards(List<SCardPair> table)
    {
        EnsureInitialized();
        return _attackStrategy!.AddCards(hand, table, MTable.GetTrump().Suit);
    }

    public void ShowHand()
    {
        Console.WriteLine("Hand " + GetName());
        foreach (var c in hand) { MTable.ShowCard(c); Console.Write(MTable.Separator); }
        Console.WriteLine();
    }
}
