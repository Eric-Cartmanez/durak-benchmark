namespace DurakBenchmark;

internal class LowestCoverDefendStrategy : IDefendStrategy
{
    public bool Defend(List<SCard> hand, List<SCardPair> table, Suits trump)
    {
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
