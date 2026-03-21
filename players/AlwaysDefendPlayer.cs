namespace DurakBenchmark;

internal class AlwaysDefendPlayer : IPlayer
{
    private List<SCard> hand = new();

    public string GetName() => "Always defend player";
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
        int i = Random.Shared.Next(hand.Count);
        var card = hand[i];
        hand.RemoveAt(i);
        return [card];
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

    public bool AddCards(List<SCardPair> table) => false;
}
