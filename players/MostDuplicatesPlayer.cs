namespace DurakBenchmark;

/// <summary>Атакует картами с наиболее часто встречающимся рангом в руке.</summary>
internal class MostDuplicatesPlayer : IPlayer
{
    private List<SCard> hand = new();

    public string GetName() => "Most duplicates player";
    public int GetCount() => hand.Count;
    public void AddToHand(SCard card) => hand.Add(card);
    public void ShowHand()
    {
        Console.WriteLine("Hand " + GetName());
        foreach (var c in hand) { MTable.ShowCard(c); Console.Write(MTable.Separator); }
        Console.WriteLine();
    }

    public List<SCard> LayCards()
    {
        Suits trump = MTable.GetTrump().Suit;
        var group = hand
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Any(c => c.Suit == trump) ? 1 : 0)
            .ThenBy(g => g.Key)
            .First();
        var toPlay = group.ToList();
        foreach (var c in toPlay) hand.Remove(c);
        return toPlay;
    }

    public bool Defend(List<SCardPair> table)
    {
        Suits trump = MTable.GetTrump().Suit;
        for (int i = 0; i < table.Count; i++)
        {
            SCardPair pair = table[i];
            if (pair.Beaten) continue;
            SCard attack = pair.Down;

            SCard? chosen = hand
                .Where(c => c.Suit == attack.Suit && c.Rank > attack.Rank)
                .OrderBy(c => c.Rank).Cast<SCard?>().FirstOrDefault();

            if (chosen == null && attack.Suit != trump)
                chosen = hand.Where(c => c.Suit == trump)
                    .OrderBy(c => c.Rank).Cast<SCard?>().FirstOrDefault();

            if (chosen == null) return false;

            pair.SetUp(chosen.Value, trump);
            table[i] = pair;
            hand.Remove(chosen.Value);
        }
        return true;
    }

    public bool AddCards(List<SCardPair> table)
    {
        Suits trump = MTable.GetTrump().Suit;
        var ranks = table.SelectMany(p => new[] { p.Down.Rank }.Concat(p.Beaten ? [p.Up.Rank] : [])).ToHashSet();

        SCard? chosen = hand.Where(c => ranks.Contains(c.Rank))
            .OrderByDescending(c => hand.Count(h => h.Rank == c.Rank))
            .ThenBy(c => c.Suit == trump ? 1 : 0)
            .Cast<SCard?>().FirstOrDefault();

        if (chosen == null) return false;
        table.Add(new SCardPair(chosen.Value));
        hand.Remove(chosen.Value);
        return true;
    }
}
