namespace DurakBenchmark;

internal class HighestCardPlayer : IPlayer
{
    private List<SCard> hand = new();

    public string GetName() => "Highest card player";
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
        var nonTrumps = hand.Where(c => c.Suit != trump).OrderByDescending(c => c.Rank).ToList();
        if (nonTrumps.Count == 0)
        {
            var c = hand.OrderByDescending(c => c.Rank).First();
            hand.Remove(c);
            return [c];
        }
        int highestRank = nonTrumps[0].Rank;
        var toPlay = nonTrumps.TakeWhile(c => c.Rank == highestRank).ToList();
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
            bool beaten = false;
            foreach (var card in hand)
            {
                if (pair.SetUp(card, trump))
                {
                    table[i] = pair;
                    hand.Remove(card);
                    beaten = true;
                    break;
                }
            }
            if (!beaten) return false;
        }
        return true;
    }

    public bool AddCards(List<SCardPair> table)
    {
        Suits trump = MTable.GetTrump().Suit;
        var ranks = table.SelectMany(p => new[] { p.Down.Rank }.Concat(p.Beaten ? [p.Up.Rank] : [])).ToHashSet();

        SCard? chosen = hand.Where(c => c.Suit != trump && ranks.Contains(c.Rank))
            .OrderByDescending(c => c.Rank).Cast<SCard?>().FirstOrDefault()
            ?? hand.Where(c => c.Suit == trump && ranks.Contains(c.Rank))
                .OrderByDescending(c => c.Rank).Cast<SCard?>().FirstOrDefault();

        if (chosen == null) return false;
        table.Add(new SCardPair(chosen.Value));
        hand.Remove(chosen.Value);
        return true;
    }
}
