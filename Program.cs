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
    if (args[i] == "--help" || args[i] == "-h")
    {
        Console.WriteLine("Использование: dotnet run -- [опции]");
        Console.WriteLine();
        Console.WriteLine("Опции:");
        Console.WriteLine("  --player <ClassName>   Запустить игрока против всех остальных");
        Console.WriteLine("  --games <N>            Число партий на матч (по умолчанию: 1 000 000)");
        Console.WriteLine("  --print-logs           Показать одну партию с выводом хода игры");
        Console.WriteLine("  --help, -h             Показать эту справку");
        Console.WriteLine();
        Console.WriteLine("Доступные игроки:");
        var allPlayers = DiscoverPlayers();
        foreach (string name in allPlayers.Keys.Order())
            Console.WriteLine($"  {name}");
        Console.WriteLine();
        Console.WriteLine("Примеры:");
        Console.WriteLine("  dotnet run -c Release");
        Console.WriteLine("  dotnet run -c Release -- --player MinimalDefendPlayer --games 100000");
        Console.WriteLine("  dotnet run -c Release -- --player MinimalDefendPlayer --print-logs");
        return 0;
    }
    else if (args[i] == "--player" && i + 1 < args.Length)
        playerArg = args[++i];
    else if (args[i] == "--print-logs")
        printLogs = true;
    else if (args[i] == "--games" && i + 1 < args.Length && int.TryParse(args[++i], out int g))
        games = g;
}

var players = DiscoverPlayers();

if (printLogs)
{
    // Одна партия с полным выводом хода игры
    var names = players.Keys.Order().ToList();
    string name1 = playerArg ?? names[0];
    string name2 = names.First(n => n != name1);

    if (!players.ContainsKey(name1))
    {
        Console.WriteLine($"Неизвестный игрок: {name1}");
        Console.WriteLine($"Доступные: {string.Join(", ", names)}");
        return 1;
    }

    Console.WriteLine($"=== {name1} vs {name2} (1 партия с выводом) ===\n");
    IPlayer p1 = players[name1]();
    IPlayer p2 = players[name2]();
    MTable.Initialize(p1, p2);
    EndGame outcome = MTable.Play(true);
    Console.WriteLine($"\nИтог: {outcome}");
    return 0;
}

if (playerArg != null)
{
    if (!players.ContainsKey(playerArg))
    {
        Console.WriteLine($"Неизвестный игрок: {playerArg}");
        Console.WriteLine($"Доступные: {string.Join(", ", players.Keys.Order())}");
        return 1;
    }
    RunVsAll(playerArg, games);
}
else
{
    RunAllVsAll(games);
}

return 0;

void RunVsAll(string playerName, int games)
{
    Console.WriteLine($"=== {playerName} vs все ({games} партий каждый матч) ===\n");
    foreach (var (name, factory) in players.Where(p => p.Key != playerName))
    {
        BenchmarkResult result = Benchmark.Run(players[playerName], factory, games);
        result.Print();
        Console.WriteLine();
    }
}

void RunAllVsAll(int games)
{
    var names = players.Keys.Order().ToList();
    Console.WriteLine($"=== Все против всех ({games} партий каждый матч) ===\n");
    for (int i = 0; i < names.Count; i++)
    {
        for (int j = i + 1; j < names.Count; j++)
        {
            BenchmarkResult result = Benchmark.Run(players[names[i]], players[names[j]], games);
            result.Print();
            Console.WriteLine();
        }
    }
}

internal struct BenchmarkResult
{
    public string Bot1Name;
    public string Bot2Name;
    public int Wins1;
    public int Wins2;
    public int Draws;
    public double AvgLoserCards;

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
    public static BenchmarkResult Run(Func<IPlayer> player1, Func<IPlayer> player2, int games)
    {
        BenchmarkResult result = new BenchmarkResult();
        int totalLoserCards = 0;

        TextWriter originalOut = Console.Out;
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
        }

        Console.SetOut(originalOut);
        result.AvgLoserCards = (double)totalLoserCards / games;
        return result;
    }
}
