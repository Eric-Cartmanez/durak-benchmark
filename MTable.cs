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
    public Suits Suit => _suit;
    public int Rank => _rank;
}

public struct SCardPair
{
    private SCard _down;
    private SCard _up;
    private bool _beaten;

    public SCard Down
    {
        get => _down;
        set { _down = value; _beaten = false; }
    }
    public bool Beaten => _beaten;
    public SCard Up => _up;

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

    private static List<SCard> deck = new();
    private static IPlayer player1 = null!;
    private static IPlayer player2 = null!;
    private static List<SCard> plHand1 = new();
    private static List<SCard> plHand2 = new();
    private static SCard trump;
    private static List<SCardPair> table = null!;
    private static List<SCard> bito = new();
    public static GameLog CurrentLog;

    public static void Initialize(IPlayer p1, IPlayer p2)
    {
        deck.Clear();
        plHand1.Clear();
        plHand2.Clear();
        bito.Clear();
        CurrentLog = new GameLog { Actions = new List<ActionRecord>() };

        var temp = new List<SCard>();
        var rnd = new Random();

        for (int c = 0; c <= 3; c++)
            for (int d = 6; d <= 14; d++)
                temp.Add(new SCard((Suits)c, d));

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

    public static SCard GetTrump() => trump;

    public static EndGame Play(bool first)
    {
        bool playerFirst = first;
        bool defend, added = true;

        while (true)
        {
            table = new List<SCardPair>();

            var attackerHand = playerFirst ? plHand1 : plHand2;
            var defenderHand = playerFirst ? plHand2 : plHand1;
            var attackerHandSnapshot = new List<SCard>(attackerHand);

            var cards = playerFirst ? player1.LayCards() : player2.LayCards();

            while (cards.Count > 0)
            {
                table.Add(new SCardPair(cards[0]));
                cards.RemoveAt(0);
            }

            foreach (var pair in table)
                attackerHand.Remove(pair.Down);

            CurrentLog.Actions.Add(new ActionRecord {
                Type = ActionType.Attack,
                Player1Acts = playerFirst,
                ActorHand = attackerHandSnapshot,
                SeenCards = new List<SCard>(bito),
                Table = new List<SCardPair>(),
                OpponentCardCount = defenderHand.Count,
                DeckSize = deck.Count,
                Trump = trump,
                PlayedCards = table.Select(p => p.Down).ToList(),
                ActionResult = true
            });

            Console.WriteLine("Делаем ход");
            ShowTable(table);

            while (true)
            {
                if (playerFirst)
                {
                    var defSnap = new List<SCard>(plHand2);
                    var tableSnap = new List<SCardPair>(table);
                    defend = player2.Defend(table);

                    CurrentLog.Actions.Add(new ActionRecord {
                        Type = ActionType.Defend, Player1Acts = false,
                        ActorHand = defSnap, SeenCards = new List<SCard>(bito),
                        Table = tableSnap, OpponentCardCount = plHand1.Count,
                        DeckSize = deck.Count, Trump = trump,
                        PlayedCards = table.Where(p => p.Beaten).Select(p => p.Up).ToList(),
                        ActionResult = defend
                    });

                    Console.WriteLine("Отбивается " + player2.GetName());
                    ShowTable(table);

                    var atkSnap = new List<SCard>(plHand1);
                    var tableSnap2 = new List<SCardPair>(table);
                    added = player1.AddCards(table);

                    CurrentLog.Actions.Add(new ActionRecord {
                        Type = ActionType.Throw, Player1Acts = true,
                        ActorHand = atkSnap, SeenCards = new List<SCard>(bito),
                        Table = tableSnap2, OpponentCardCount = plHand2.Count,
                        DeckSize = deck.Count, Trump = trump,
                        PlayedCards = table.Skip(tableSnap2.Count).Select(p => p.Down).ToList(),
                        ActionResult = added
                    });

                    Console.WriteLine("Подкидывает " + player1.GetName() + "  " + added);
                    ShowTable(table);

                    if (!defend)
                    {
                        while (table.Count > 0)
                        {
                            player2.AddToHand(table[0].Down); plHand2.Add(table[0].Down);
                            if (table[0].Beaten) { player2.AddToHand(table[0].Up); plHand2.Add(table[0].Up); }
                            table.RemoveAt(0);
                        }
                        break;
                    }
                    if (!added) break;
                }
                else
                {
                    var defSnap = new List<SCard>(plHand1);
                    var tableSnap = new List<SCardPair>(table);
                    defend = player1.Defend(table);

                    CurrentLog.Actions.Add(new ActionRecord {
                        Type = ActionType.Defend, Player1Acts = true,
                        ActorHand = defSnap, SeenCards = new List<SCard>(bito),
                        Table = tableSnap, OpponentCardCount = plHand2.Count,
                        DeckSize = deck.Count, Trump = trump,
                        PlayedCards = table.Where(p => p.Beaten).Select(p => p.Up).ToList(),
                        ActionResult = defend
                    });

                    Console.WriteLine("Отбивается " + player1.GetName());
                    ShowTable(table);

                    var atkSnap = new List<SCard>(plHand2);
                    var tableSnap2 = new List<SCardPair>(table);
                    added = player2.AddCards(table);

                    CurrentLog.Actions.Add(new ActionRecord {
                        Type = ActionType.Throw, Player1Acts = false,
                        ActorHand = atkSnap, SeenCards = new List<SCard>(bito),
                        Table = tableSnap2, OpponentCardCount = plHand1.Count,
                        DeckSize = deck.Count, Trump = trump,
                        PlayedCards = table.Skip(tableSnap2.Count).Select(p => p.Down).ToList(),
                        ActionResult = added
                    });

                    Console.WriteLine("Подкидывает " + player2.GetName() + "  " + added);
                    ShowTable(table);

                    if (!defend)
                    {
                        while (table.Count > 0)
                        {
                            player1.AddToHand(table[0].Down); plHand1.Add(table[0].Down);
                            if (table[0].Beaten) { player1.AddToHand(table[0].Up); plHand1.Add(table[0].Up); }
                            table.RemoveAt(0);
                        }
                        break;
                    }
                    if (!added) break;
                }
            }

            if (defend)
            {
                foreach (var pair in table)
                {
                    bito.Add(pair.Down);
                    if (pair.Beaten) bito.Add(pair.Up);
                }
            }

            if (playerFirst) { if (defend) CheckHand(table, plHand2, false); }
            else             { if (defend) CheckHand(table, plHand1, false); }

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
                CurrentLog.Outcome = result.Value;
                CurrentLog.Trump = trump;
                CurrentLog.LoserCardCount = result.Value switch {
                    EndGame.First  => player2.GetCount(),
                    EndGame.Second => player1.GetCount(),
                    _              => 0
                };
                return result.Value;
            }
        }
    }

    private static void AddCardsFromDeck(bool first)
    {
        if (first)
        {
            while (player1.GetCount() < TotalCards && deck.Count > 0) { player1.AddToHand(deck[0]); plHand1.Add(deck[0]); deck.RemoveAt(0); }
            while (player2.GetCount() < TotalCards && deck.Count > 0) { player2.AddToHand(deck[0]); plHand2.Add(deck[0]); deck.RemoveAt(0); }
        }
        else
        {
            while (player2.GetCount() < TotalCards && deck.Count > 0) { player2.AddToHand(deck[0]); plHand2.Add(deck[0]); deck.RemoveAt(0); }
            while (player1.GetCount() < TotalCards && deck.Count > 0) { player1.AddToHand(deck[0]); plHand1.Add(deck[0]); deck.RemoveAt(0); }
        }
    }

    private static void CheckHand(List<SCardPair> table, List<SCard> plHand, bool down)
    {
        if (down)
        {
            foreach (var cp in table)
            {
                if (plHand.Contains(cp.Down)) plHand.Remove(cp.Down);
                else throw new Exception("Игрок сыграл картой, которой нет в руке");
            }
        }
        else
        {
            foreach (var cp in table)
            {
                if (cp.Beaten)
                {
                    if (plHand.Contains(cp.Up)) plHand.Remove(cp.Up);
                    else throw new Exception("Игрок отбился картой, которой нет в руке");
                }
                else throw new Exception("Не все карты покрыты");
            }
        }
    }

    public static void ShowCard(SCard card)
    {
        if ((int)card.Suit < 2) Console.ForegroundColor = ConsoleColor.Red;
        string msg = card.Suit switch {
            Suits.Hearts   => "ч",
            Suits.Diamonds => "б",
            Suits.Clubs    => "к",
            Suits.Spades   => "п",
            _              => "?"
        };
        msg += card.Rank switch {
            6  => "6", 7  => "7", 8  => "8", 9  => "9", 10 => "0",
            11 => "В", 12 => "Д", 13 => "К", 14 => "Т", _  => "?"
        };
        Console.Write(msg);
        Console.ResetColor();
    }

    public static void ShowTable(List<SCardPair> table)
    {
        foreach (var pair in table)
        {
            if (pair.Beaten) ShowCard(pair.Up);
            else Console.Write("  ");
            Console.Write(Separator);
        }
        Console.WriteLine();
        foreach (var pair in table)
        {
            ShowCard(pair.Down);
            Console.Write(Separator);
        }
        Console.WriteLine();
    }
}
