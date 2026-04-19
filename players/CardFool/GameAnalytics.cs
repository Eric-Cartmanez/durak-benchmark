namespace DurakBenchmark;

// Per-instance вместо singleton — иначе состояние утекает между партиями в бенчмарке.
internal class GameAnalytics
{
    private readonly HashSet<(Suits Suit, int Rank)> _seenCards = new();

    public void ObserveTable(List<SCardPair> table)
    {
        foreach (var pair in table)
        {
            _seenCards.Add((pair.Down.Suit, pair.Down.Rank));
            if (pair.Beaten)
                _seenCards.Add((pair.Up.Suit, pair.Up.Rank));
        }
    }

    public bool WasCardSeen(Suits suit, int rank) => _seenCards.Contains((suit, rank));

    public int TotalCardsSeen => _seenCards.Count;
}
