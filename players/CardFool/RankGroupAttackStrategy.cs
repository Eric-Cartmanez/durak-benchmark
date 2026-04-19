namespace DurakBenchmark;

internal class RankGroupAttackStrategy : IAttackStrategy
{
    public List<SCard> LayCards(List<SCard> hand, Suits trump)
    {
        if (hand.Count == 0)
            return new List<SCard>();

        var bestGroup = hand
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Any(c => c.Suit == trump) ? 1 : 0)
            .ThenBy(g => g.Key)
            .First();

        var cards = bestGroup.ToList();

        foreach (var card in cards)
            hand.Remove(card);

        return cards;
    }

    public bool AddCards(List<SCard> hand, List<SCardPair> table, Suits trump)
    {
        var ranks = CardHelper.GetTableRanks(table);
        var matching = CardHelper.FindMatchingRanks(hand, ranks);

        if (matching.Count == 0)
            return false;

        var sorted = CardHelper.SortByValue(matching, trump);

        foreach (var card in sorted)
        {
            hand.Remove(card);
            table.Add(new SCardPair(card));
        }

        return true;
    }
}
