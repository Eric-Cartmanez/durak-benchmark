namespace DurakBenchmark;

internal class SmartDefendStrategy : IDefendStrategy
{
    private readonly GameAnalytics _analytics;

    public SmartDefendStrategy(GameAnalytics analytics)
    {
        _analytics = analytics;
    }

    public bool Defend(List<SCard> hand, List<SCardPair> table, Suits trump)
    {
        _analytics.ObserveTable(table);

        var unbeatenIndices = new List<int>();
        for (int i = 0; i < table.Count; i++)
        {
            if (!table[i].Beaten)
                unbeatenIndices.Add(i);
        }
        unbeatenIndices.Sort((a, b) => table[b].Down.Rank.CompareTo(table[a].Down.Rank));

        var plan = new List<(int tableIndex, SCard card)>();
        var availableHand = new List<SCard>(hand);

        foreach (int idx in unbeatenIndices)
        {
            var target = table[idx].Down;

            var cover = CardHelper.FindLowestSameSuitCover(availableHand, target);
            cover ??= CardHelper.FindLowestTrumpCover(availableHand, target, trump);

            if (cover == null)
                return false;

            plan.Add((idx, cover.Value));
            availableHand.Remove(cover.Value);
        }

        int defenseCost = plan.Sum(p => CardHelper.CardValue(p.card, trump));
        int takeCost = 0;

        foreach (var pair in table)
        {
            takeCost += CardHelper.CardValue(pair.Down, trump);
            if (pair.Beaten)
                takeCost += CardHelper.CardValue(pair.Up, trump);
        }

        int trumpsInPlan = plan.Count(p => p.card.Suit == trump);
        bool lateGame = _analytics.TotalCardsSeen > 16;

        if (!lateGame && trumpsInPlan > 1 && defenseCost > takeCost * 0.8)
            return false;

        foreach (var (tableIndex, card) in plan)
        {
            var pair = table[tableIndex];
            pair.SetUp(card, trump);
            table[tableIndex] = pair;
            hand.Remove(card);
        }

        return true;
    }
}
