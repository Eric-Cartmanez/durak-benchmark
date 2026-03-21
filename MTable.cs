namespace DurakBenchmark;

public struct SCard
{
    private Suits _suit;
    private int _rank;

    public SCard(Suits suit, int rank)
    {
        _suit = suit;
        _rank = rank;
    }
    public Suits Suit
    {
        get { return _suit; }
    }
    public int Rank
    {
        get { return _rank; }
    }
}

public struct SCardPair
{
    private SCard _down;
    private SCard _up;
    private bool _beaten;

    public SCard Down
    {
        get { return _down; }
        set { _down = value; _beaten = false; }
    }
    public bool Beaten
    {
        get { return _beaten; }
    }
    public SCard Up
    {
        get { return _up; }
    }

    public bool SetUp(SCard up, Suits trump)
    {
        if (_down.Suit == up.Suit)
        {
            if (_down.Rank < up.Rank)
            {
                _up = up;
                _beaten = true;
                return true;
            }
        }
        else if (up.Suit == trump)
        {
            _up = up;
            _beaten = true;
            return true;
        }
        return false;
    }

    public SCardPair(SCard down)
    {
        _down = down;
        _up = new SCard(0, 0);
        _beaten = false;
    }
}

public enum EndGame { First, Second, Draw }
public enum Suits { Hearts, Diamonds, Clubs, Spades }


internal class MTable
{
    public const int TotalCards = 6;
    public const string Separator = " | ";

    private static List<SCard> deck = new List<SCard>();
    private static IPlayer player1 = null!;
    private static IPlayer player2 = null!;
    private static List<SCard> plHand1 = new List<SCard>();
    private static List<SCard> plHand2 = new List<SCard>();
    private static SCard trump;
    private static List<SCardPair> table = null!;

    public static void Initialize(IPlayer p1, IPlayer p2)
    {
        deck.Clear();
        plHand1.Clear();
        plHand2.Clear();

        List<SCard> temp = new List<SCard>();
        Random rnd = new Random();

        for (int c = 0; c <= 3; c++)
            for (int d = 6; d <= 14; d++)
            {
                SCard card = new SCard((Suits)c, d);
                temp.Add(card);
            }

        for (int c = 0; c < 4 * 9; c++)
        {
            int num = rnd.Next(temp.Count);
            deck.Add(temp[num]);
            temp.RemoveAt(num);
        }

        player1 = p1;
        player2 = p2;

        for (int c = 0; c < TotalCards; c++)
        {
            player1.AddToHand(deck[0]); plHand1.Add(deck[0]); deck.RemoveAt(0);
            player2.AddToHand(deck[0]); plHand2.Add(deck[0]); deck.RemoveAt(0);
        }

        trump = deck[deck.Count - 1];
        Console.Write("Козырь "); ShowCard(trump);
        Console.WriteLine();
        player1.ShowHand();
        player2.ShowHand();
        Console.WriteLine();
    }

    public static SCard GetTrump()
    {
        return trump;
    }

    public static EndGame Play(bool first)
    {
        bool playerFirst = first;
        bool defend, added = true;

        while (true)
        {
            List<SCard> cards;
            table = new List<SCardPair>();

            if (playerFirst)
                cards = player1.LayCards();
            else
                cards = player2.LayCards();

            while (cards.Count > 0)
            {
                table.Add(new SCardPair(cards[0]));
                cards.RemoveAt(0);
            }

            var attackerHand = playerFirst ? plHand1 : plHand2;
            foreach (var pair in table)
                attackerHand.Remove(pair.Down);

            Console.WriteLine("Делаем ход");
            ShowTable(table);

            while (true)
            {
                if (playerFirst)
                {
                    defend = player2.Defend(table);
                    Console.WriteLine("Отбивается " + player2.GetName());
                    ShowTable(table);

                    added = player1.AddCards(table);
                    Console.WriteLine("Подкидывает " + player1.GetName() + "  " + added);
                    ShowTable(table);

                    if (!defend)
                    {
                        while (table.Count > 0)
                        {
                            player2.AddToHand(table[0].Down);
                            plHand2.Add(table[0].Down);
                            if (table[0].Beaten)
                            {
                                player2.AddToHand(table[0].Up);
                                plHand2.Add(table[0].Up);
                            }
                            table.RemoveAt(0);
                        }
                        break;
                    }
                    if (!added) break;
                }
                else
                {
                    defend = player1.Defend(table);
                    Console.WriteLine("Отбивается " + player1.GetName());
                    ShowTable(table);

                    added = player2.AddCards(table);
                    Console.WriteLine("Подкидывает " + player2.GetName() + "  " + added);
                    ShowTable(table);

                    if (!defend)
                    {
                        while (table.Count > 0)
                        {
                            player1.AddToHand(table[0].Down);
                            plHand1.Add(table[0].Down);
                            if (table[0].Beaten)
                            {
                                player1.AddToHand(table[0].Up);
                                plHand1.Add(table[0].Up);
                            }
                            table.RemoveAt(0);
                        }
                        break;
                    }
                    if (!added) break;
                }
            }

            if (playerFirst)
            {
                if (defend) CheckHand(table, plHand2, false);
            }
            else
            {
                if (defend) CheckHand(table, plHand1, false);
            }

            AddCardsFromDeck(playerFirst);
            if (defend) playerFirst = !playerFirst;

            Console.WriteLine();
            player1.ShowHand();
            player2.ShowHand();

            EndGame? result = null;
            if (player1.GetCount() == 0 && player2.GetCount() == 0) result = EndGame.Draw;
            else if (player1.GetCount() == 0) result = EndGame.First;
            else if (player2.GetCount() == 0) result = EndGame.Second;

            if (result.HasValue)
            {
                return result.Value;
            }
        }
    }

    private static void AddCardsFromDeck(bool first)
    {
        if (first)
        {
            while (player1.GetCount() < TotalCards && deck.Count > 0)
            {
                player1.AddToHand(deck[0]); plHand1.Add(deck[0]); deck.RemoveAt(0);
            }
            while (player2.GetCount() < TotalCards && deck.Count > 0)
            {
                player2.AddToHand(deck[0]); plHand2.Add(deck[0]); deck.RemoveAt(0);
            }
        }
        else
        {
            while (player2.GetCount() < TotalCards && deck.Count > 0)
            {
                player2.AddToHand(deck[0]); plHand2.Add(deck[0]); deck.RemoveAt(0);
            }
            while (player1.GetCount() < TotalCards && deck.Count > 0)
            {
                player1.AddToHand(deck[0]); plHand1.Add(deck[0]); deck.RemoveAt(0);
            }
        }
    }

    private static void CheckHand(List<SCardPair> table, List<SCard> plHand, bool down)
    {
        if (down)
        {
            foreach (SCardPair cp in table)
            {
                if (plHand.Contains(cp.Down))
                    plHand.Remove(cp.Down);
                else
                    throw new Exception("Игрок сыграл картой, которой нет в руке");
            }
        }
        else
        {
            foreach (SCardPair cp in table)
            {
                if (cp.Beaten)
                {
                    if (plHand.Contains(cp.Up))
                        plHand.Remove(cp.Up);
                    else
                        throw new Exception("Игрок отбился картой, которой нет в руке");
                }
                else
                    throw new Exception("Не все карты покрыты");
            }
        }
    }

    public static void ShowCard(SCard card)
    {
        string msg = "";
        if ((int)card.Suit < 2) Console.ForegroundColor = ConsoleColor.Red;
        switch (card.Suit)
        {
            case Suits.Hearts:   msg = "ч"; break;
            case Suits.Diamonds: msg = "б"; break;
            case Suits.Clubs:    msg = "к"; break;
            case Suits.Spades:   msg = "п"; break;
        }
        switch (card.Rank)
        {
            case 6:  msg += "6"; break;
            case 7:  msg += "7"; break;
            case 8:  msg += "8"; break;
            case 9:  msg += "9"; break;
            case 10: msg += "0"; break;
            case 11: msg += "В"; break;
            case 12: msg += "Д"; break;
            case 13: msg += "К"; break;
            case 14: msg += "Т"; break;
        }
        Console.Write(msg);
        Console.ResetColor();
    }

    public static void ShowTable(List<SCardPair> table)
    {
        foreach (SCardPair pair in table)
        {
            if (pair.Beaten) ShowCard(pair.Up);
            else Console.Write("  ");
            Console.Write(Separator);
        }
        Console.WriteLine();
        foreach (SCardPair pair in table)
        {
            ShowCard(pair.Down);
            Console.Write(Separator);
        }
        Console.WriteLine();
    }
}
