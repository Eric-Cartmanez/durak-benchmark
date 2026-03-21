using System.Reflection;
using DurakBenchmark;

// Авто-обнаружение всех реализаций IPlayer в сборке
static Dictionary<string, Func<IPlayer>> DiscoverPlayers()
{
    var result = new Dictionary<string, Func<IPlayer>>(StringComparer.OrdinalIgnoreCase);
    var types = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => typeof(IPlayer).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
    foreach (var type in types)
    {
        var t = type;
        result[t.Name] = () => (IPlayer)Activator.CreateInstance(t)!;
    }
    return result;
}

string? playerArg = null;
bool printLogs = false;
int games = 1_000_000;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--player" && i + 1 < args.Length)
        playerArg = args[++i];
    else if (args[i] == "--print-logs")
        printLogs = true;
    else if (args[i] == "--games" && i + 1 < args.Length && int.TryParse(args[++i], out int g))
        games = g;
}

var players = DiscoverPlayers();

if (playerArg != null)
{
    if (!players.ContainsKey(playerArg))
    {
        Console.WriteLine($"Неизвестный игрок: {playerArg}");
        Console.WriteLine($"Доступные: {string.Join(", ", players.Keys.Order())}");
        return 1;
    }
    RunVsAll(playerArg, games, printLogs);
}
else
{
    RunAllVsAll(games, printLogs);
}

return 0;

void RunVsAll(string playerName, int games, bool printLogs)
{
    Console.WriteLine($"=== {playerName} vs все ({games} партий каждый матч) ===\n");
    foreach (var (name, factory) in players.Where(p => p.Key != playerName))
    {
        var result = Benchmark.Run(players[playerName], factory, games, collectLogs: printLogs);
        result.Print();
        if (printLogs && result.Logs.Count > 0) PrintGameLog(result.Logs[0]);
        Console.WriteLine();
    }
}

void RunAllVsAll(int games, bool printLogs)
{
    var names = players.Keys.Order().ToList();
    Console.WriteLine($"=== Все против всех ({games} партий каждый матч) ===\n");
    for (int i = 0; i < names.Count; i++)
    {
        for (int j = i + 1; j < names.Count; j++)
        {
            var result = Benchmark.Run(players[names[i]], players[names[j]], games, collectLogs: false);
            result.Print();
            Console.WriteLine();
        }
    }
}

static string CardStr(SCard c) => $"{c.Suit.ToString()[0]}{c.Rank}";

static void PrintGameLog(GameLog log)
{
    Console.WriteLine("=== Лог партии ===");
    Console.WriteLine($"Козырь: {CardStr(log.Trump)}  Итог: {log.Outcome}  Карт у проигравшего: {log.LoserCardCount}");
    Console.WriteLine();
    foreach (var a in log.Actions)
    {
        string actor  = a.Player1Acts ? "P1" : "P2";
        string hand   = string.Join(" ", a.ActorHand.Select(CardStr));
        string played = string.Join(" ", a.PlayedCards.Select(CardStr));
        string table  = string.Join(" ", a.Table.Select(p =>
            p.Beaten ? $"{CardStr(p.Down)}>{CardStr(p.Up)}" : CardStr(p.Down)));
        Console.WriteLine($"{a.Type,-8} {actor}  рука=[{hand}]  стол=[{table}]  сыграл=[{played}]  бито:{a.SeenCards.Count}  колода:{a.DeckSize}  ok:{a.ActionResult}");
    }
    Console.WriteLine();
}

internal struct BenchmarkResult
{
    public string Bot1Name;
    public string Bot2Name;
    public int Wins1;
    public int Wins2;
    public int Draws;
    public double AvgLoserCards;
    public List<GameLog> Logs;

    public void Print()
    {
        int total = Wins1 + Wins2 + Draws;
        Console.WriteLine($"{Bot1Name} vs {Bot2Name}  ({total} партий)");
        Console.WriteLine($"  {Bot1Name,-25} {Wins1,6} ({100.0 * Wins1 / total:F1}%)");
        Console.WriteLine($"  {Bot2Name,-25} {Wins2,6} ({100.0 * Wins2 / total:F1}%)");
        Console.WriteLine($"  Ничьи                     {Draws,6} ({100.0 * Draws / total:F1}%)");
        Console.WriteLine($"  Среднее карт у проигравшего: {AvgLoserCards:F2}");
    }
}

internal static class Benchmark
{
    public static BenchmarkResult Run(Func<IPlayer> player1, Func<IPlayer> player2, int games, bool collectLogs = false)
    {
        var result = new BenchmarkResult { Logs = new List<GameLog>() };
        int totalLoserCards = 0;

        var originalOut = Console.Out;
        Console.SetOut(TextWriter.Null);

        for (int i = 0; i < games; i++)
        {
            IPlayer p1 = player1();
            IPlayer p2 = player2();

            MTable.Initialize(p1, p2);
            EndGame outcome = MTable.Play(Random.Shared.Next(2) == 1);

            if (i == 0)
            {
                result.Bot1Name = p1.GetName();
                result.Bot2Name = p2.GetName();
            }

            switch (outcome)
            {
                case EndGame.First:
                    result.Wins1++;
                    totalLoserCards += p2.GetCount();
                    break;
                case EndGame.Second:
                    result.Wins2++;
                    totalLoserCards += p1.GetCount();
                    break;
                case EndGame.Draw:
                    result.Draws++;
                    break;
            }

            if (collectLogs) result.Logs.Add(MTable.CurrentLog);
        }

        Console.SetOut(originalOut);
        result.AvgLoserCards = (double)totalLoserCards / games;
        return result;
    }
}
