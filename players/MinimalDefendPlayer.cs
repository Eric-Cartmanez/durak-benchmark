namespace DurakBenchmark;

internal class MinimalDefendPlayer : IPlayer
{
    private List<SCard> hand = new();

    public string GetName() => "Minimal defend player";
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
        var nonTrumps = hand.Where(c => c.Suit != trump).OrderBy(c => c.Rank).ToList();
        if (nonTrumps.Count == 0)
        {
            var c = hand.First(c => c.Suit == trump);
            hand.Remove(c);
            return [c];
        }
        int lowestRank = nonTrumps[0].Rank;
        var toPlay = nonTrumps.TakeWhile(c => c.Rank == lowestRank).ToList();
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

        SCard? nonTrump = hand.Where(c => c.Suit != trump && ranks.Contains(c.Rank))
            .OrderBy(c => c.Rank).Cast<SCard?>().FirstOrDefault();
        if (nonTrump != null) { table.Add(new SCardPair(nonTrump.Value)); hand.Remove(nonTrump.Value); return true; }

        SCard? trumpCard = hand.Where(c => c.Suit == trump && ranks.Contains(c.Rank))
            .OrderBy(c => c.Rank).Cast<SCard?>().FirstOrDefault();
        if (trumpCard != null) { table.Add(new SCardPair(trumpCard.Value)); hand.Remove(trumpCard.Value); return true; }

        return false;
    }
}
