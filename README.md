# Durak Benchmark

Бенчмарк для карточной игры **«Дурак подкидной»** (2 игрока, 36 карт).
Добавь своего бота через Pull Request — CI автоматически прогонит его против всех существующих игроков и опубликует результаты в комментарии к PR.

---

## Условие задачи

Реализовать AI-игрока для подкидного дурака. Цель — **минимизировать сумму штрафных очков** за 100 партий против случайного/наивного игрока.

**Краткие правила:**
- 36 карт: масти ♠♣♥♦, ранги 6–14 (Туз=14)
- Каждому раздаётся по 6 карт, определяется козырная масть
- Атакующий кладёт карты → защищающийся отбивается → атакующий может подкинуть ещё
- Козырь бьёт любую некозырную карту; внутри масти нужна старшая карта
- Нельзя отбиться — берёшь все карты со стола и пропускаешь ход
- Добираем из колоды до 6 карт после каждого хода
- Побеждает тот, кто первым избавился от всех карт

Подробнее: [Wikipedia — Дурак](https://ru.wikipedia.org/wiki/Дурак_(карточная_игра))

---

## Быстрый старт

**Требования:** .NET 8.0

```bash
# Клонировать репозиторий
git clone https://github.com/<owner>/durak-benchmark.git
cd durak-benchmark

# Собрать
dotnet build

# Запустить все против всех (1 000 000 партий)
dotnet run -c Release

# Запустить конкретного игрока против всех (100 000 партий)
dotnet run -c Release -- --player MinimalDefendPlayer --games 100000

# Посмотреть лог первой партии
dotnet run -c Release -- --player MinimalDefendPlayer --games 1000 --print-logs
```

**Флаги:**

| Флаг | Описание |
|------|----------|
| `--player <ClassName>` | Запустить конкретного игрока vs все остальные |
| `--games <N>` | Количество партий на каждый матч (по умолчанию 1 000 000) |
| `--print-logs` | Вывести детальный лог первой партии каждого матча |

---

## Как добавить своего бота

### Шаг 1 — Форкнуть репозиторий

Нажмите **Fork** в правом верхнем углу страницы GitHub.

### Шаг 2 — Создать файл игрока

Добавьте файл в папку `players/`, например `players/MyPlayer.cs`.

**Минимальный шаблон:**

```csharp
namespace DurakBenchmark;

internal class MyPlayer : IPlayer
{
    private List<SCard> hand = new();

    public string GetName() => "My Player";
    public int GetCount() => hand.Count;
    public void AddToHand(SCard card) => hand.Add(card);
    public void ShowHand()
    {
        Console.WriteLine("Hand " + GetName());
        foreach (var c in hand) { MTable.ShowCard(c); Console.Write(MTable.Separator); }
        Console.WriteLine();
    }

    // Атака: вернуть одну или несколько карт для хода
    public List<SCard> LayCards()
    {
        var card = hand[0];
        hand.RemoveAt(0);
        return [card];
    }

    // Защита: true = отбился, false = берёт карты
    public bool Defend(List<SCardPair> table)
    {
        Suits trump = MTable.GetTrump().Suit;
        for (int i = 0; i < table.Count; i++)
        {
            SCardPair pair = table[i];
            if (pair.Beaten) continue;
            // Найти карту для отбивания pair.Down...
            // Если не можешь — return false
        }
        return true;
    }

    // Подкидывание: true = подкинул карту, false = пас
    public bool AddCards(List<SCardPair> table)
    {
        return false; // пас
    }
}
```

### Шаг 3 — Проверить локально

```bash
dotnet build
dotnet run -c Release -- --player MyPlayer --games 10000
```

### Шаг 4 — Открыть Pull Request

1. Закоммитьте и запушьте файл в свой форк
2. Откройте Pull Request в оригинальный репозиторий
3. GitHub Actions автоматически запустит бенчмарк
4. Результаты появятся в комментарии к PR через ~2-3 минуты

---

## Интерфейс IPlayer

```csharp
interface IPlayer {
    string GetName();                    // отображаемое имя
    int GetCount();                      // карт в руке
    void AddToHand(SCard card);          // движок добавляет карту
    List<SCard> LayCards();              // атака
    bool Defend(List<SCardPair> table);  // true = отбился
    bool AddCards(List<SCardPair> table);// true = подкинул, false = пас
    void ShowHand();                     // для отладки
}
```

**Ключевые структуры:**

```csharp
struct SCard {
    Suits Suit;  // Hearts, Diamonds, Clubs, Spades
    int Rank;    // 6–14 (Валет=11, Дама=12, Король=13, Туз=14)
}

struct SCardPair {
    SCard Down;    // атакующая карта
    SCard Up;      // отбивающая карта (если Beaten == true)
    bool Beaten;   // отбита ли пара
    bool SetUp(SCard up, Suits trump); // попытаться отбить карту Down картой up
}
```

> **Важно про SCardPair:** это struct (значимый тип). После `pair.SetUp(card, trump)` обязательно пишите `table[i] = pair`, иначе изменения теряются.

**Козырь:**

```csharp
SCard trump = MTable.GetTrump(); // доступен в любой момент
```

**Правила атаки:**
- В первом ходе раунда — любые карты одинакового достоинства
- При подкидывании (`AddCards`) — только карты, чьё достоинство **уже есть на столе**

**Что нельзя:**
- Обращаться к руке соперника напрямую
- Изменять MTable.cs
- Ходить картами, которых нет в руке

---

## Существующие игроки

| Класс | Имя | Стратегия |
|-------|-----|-----------|
| `RandomPlayer` | Simple bot | Случайная карта; не отбивается |
| `AlwaysDefendPlayer` | Always defend player | Случайная карта; всегда пытается отбиться |
| `LowestCardPlayer` | Lowest card player | Минимальный ранг; отбивается первой подходящей |
| `MinimalDefendPlayer` | Minimal defend player | Минимальный ранг; минимально достаточная защита |
| `HighestCardPlayer` | Highest card player | Максимальный ранг некозырных; первая подходящая |
| `MostDuplicatesPlayer` | Most duplicates player | Группа с наибольшим числом дубликатов |

### Иерархия (1 000 000 партий)

```
Random < AlwaysDefend < Highest < MostDuplicates < Lowest ≈ MinimalDefend
```

| Матч | Победитель | % побед |
|------|-----------|---------|
| MinimalDefend vs Highest | MinimalDefend | 83% |
| MinimalDefend vs MostDuplicates | MinimalDefend | 60% |
| MinimalDefend vs Lowest | MinimalDefend | 56% |
| Lowest vs Highest | Lowest | 81% |
| Lowest vs MostDuplicates | Lowest | 55% |
| MostDuplicates vs Highest | MostDuplicates | 75% |

---

## Архитектура

```
durak-benchmark/
├── .github/
│   ├── workflows/benchmark.yml   — CI: запуск бенчмарка на PR
│   └── PULL_REQUEST_TEMPLATE.md
├── players/                      — папка для игроков (добавляй сюда)
│   ├── RandomPlayer.cs
│   ├── AlwaysDefendPlayer.cs
│   ├── LowestCardPlayer.cs
│   ├── MinimalDefendPlayer.cs
│   ├── HighestCardPlayer.cs
│   └── MostDuplicatesPlayer.cs
├── IPlayer.cs                    — интерфейс игрока
├── MTable.cs                     — движок игры (не изменять)
├── GameLog.cs                    — структуры для логирования
├── Program.cs                    — точка входа, бенчмарк
└── DurakBenchmark.csproj
```

Новые игроки **обнаруживаются автоматически** через reflection — достаточно добавить класс, реализующий `IPlayer`, в папку `players/`.
