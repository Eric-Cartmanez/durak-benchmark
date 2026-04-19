namespace DurakBenchmark;

internal static class CardHelper
{
    public static bool CanBeat(SCard down, SCard up, Suits trump)
    {
        if (down.Suit == up.Suit)
            return up.Rank > down.Rank;
        return up.Suit == trump;
    }

    public static HashSet<int> GetTableRanks(List<SCardPair> table)
    {
        var ranks = new HashSet<int>();
        foreach (var pair in table)
        {
            ranks.Add(pair.Down.Rank);
            if (pair.Beaten)
                ranks.Add(pair.Up.Rank);
        }
        return ranks;
    }

    public static List<SCard> FindMatchingRanks(List<SCard> hand, HashSet<int> ranks)
        => hand.Where(c => ranks.Contains(c.Rank)).ToList();

    public static List<SCard> SortByValue(List<SCard> cards, Suits trump)
        => cards.OrderBy(c => c.Suit == trump ? 1 : 0).ThenBy(c => c.Rank).ToList();

    public static SCard? FindLowestCover(List<SCard> hand, SCard target, Suits trump)
        => FindLowestSameSuitCover(hand, target) ?? FindLowestTrumpCover(hand, target, trump);

    public static SCard? FindLowestSameSuitCover(List<SCard> hand, SCard target)
    {
        SCard? best = null;
        foreach (var card in hand)
        {
            if (card.Suit == target.Suit && card.Rank > target.Rank)
            {
                if (best == null || card.Rank < best.Value.Rank)
                    best = card;
            }
        }
        return best;
    }

    public static SCard? FindLowestTrumpCover(List<SCard> hand, SCard target, Suits trump)
    {
        if (target.Suit == trump)
            return FindLowestSameSuitCover(hand, target);

        SCard? best = null;
        foreach (var card in hand)
        {
            if (card.Suit == trump)
            {
                if (best == null || card.Rank < best.Value.Rank)
                    best = card;
            }
        }
        return best;
    }

    public static int CardValue(SCard card, Suits trump)
    {
        int value = card.Rank;
        if (card.Suit == trump)
            value += 15;
        return value;
    }
}
