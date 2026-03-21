namespace DurakBenchmark;

internal class RandomPlayer : IPlayer
{
    private List<SCard> hand = new List<SCard>();

    public string GetName() => "Simple bot";
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
        return false;
    }

    public bool AddCards(List<SCardPair> table)
    {
        return false;
    }
}
