namespace DurakBenchmark;

internal class AlwaysDefendPlayer : IPlayer
{
    private List<SCard> hand = new List<SCard>();

    public string GetName() => "Always defend player";
    public int GetCount() => hand.Count;
    public void AddToHand(SCard card) => hand.Add(card);

    public void ShowHand()
    {
        Console.WriteLine("Hand " + GetName());
        foreach (SCard card in hand)
        {
            MTable.ShowCard(card);
            Console.Write(MTable.Separator);
        }
        Console.WriteLine();
    }

    public List<SCard> LayCards()
    {
        int randomIndex = Random.Shared.Next(hand.Count);
        SCard randomCard = hand[randomIndex];
        hand.RemoveAt(randomIndex);
        return [randomCard];
    }

    public bool Defend(List<SCardPair> table)
    {
        Suits trump = MTable.GetTrump().Suit;

        for (int i = 0; i < table.Count; i++)
        {
            SCardPair pair = table[i];
            if (pair.Beaten) continue;

            bool beaten = false;
            foreach (SCard card in hand)
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
        return false;
    }
}
