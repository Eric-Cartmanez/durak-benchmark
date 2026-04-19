namespace DurakBenchmark;

internal class SmartAttackStrategy : IAttackStrategy
{
    private readonly GameAnalytics _analytics;

    public SmartAttackStrategy(GameAnalytics analytics)
    {
        _analytics = analytics;
    }

    public List<SCard> LayCards(List<SCard> hand, Suits trump)
    {
        if (hand.Count == 0)
            return new List<SCard>();

        var nonTrumps = hand.Where(c => c.Suit != trump).ToList();
        var source = nonTrumps.Count > 0 ? nonTrumps : hand;

        var bestGroup = source
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => ScoreGroupForAttack(g, trump))
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

        bool defenderFailing = table.Any(p => !p.Beaten);

        if (defenderFailing)
        {
            var toAdd = CardHelper.SortByValue(matching, trump)
                .Where(c => c.Suit != trump)
                .ToList();

            if (toAdd.Count == 0)
                return false;

            foreach (var card in toAdd)
            {
                hand.Remove(card);
                table.Add(new SCardPair(card));
            }
            return true;
        }

        var cheapCards = CardHelper.SortByValue(matching, trump)
            .Where(c => c.Suit != trump && c.Rank <= 10)
            .Take(1)
            .ToList();

        if (cheapCards.Count == 0)
            return false;

        var chosen = cheapCards[0];
        hand.Remove(chosen);
        table.Add(new SCardPair(chosen));
        return true;
    }

    private int ScoreGroupForAttack(IEnumerable<SCard> group, Suits trump)
    {
        int total = 0;
        int count = 0;
        foreach (var card in group)
        {
            total += ScoreCardForAttack(card, trump);
            count++;
        }
        return total + count * 2;
    }

    private int ScoreCardForAttack(SCard card, Suits trump)
    {
        int seenHigherSameSuit = 0;
        for (int r = card.Rank + 1; r <= 14; r++)
        {
            if (_analytics.WasCardSeen(card.Suit, r))
                seenHigherSameSuit++;
        }

        int seenTrumps = 0;
        if (card.Suit != trump)
        {
            for (int r = 6; r <= 14; r++)
            {
                if (_analytics.WasCardSeen(trump, r))
                    seenTrumps++;
            }
        }

        return seenHigherSameSuit * 3 + seenTrumps - card.Rank;
    }
}
