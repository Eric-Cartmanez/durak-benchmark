namespace DurakBenchmark;

/// <summary>
/// Интерфейс игрока. Реализуйте этот интерфейс, чтобы добавить своего бота.
/// </summary>
public interface IPlayer
{
    /// <summary>Отображаемое имя игрока.</summary>
    string GetName();

    /// <summary>Количество карт в руке.</summary>
    int GetCount();

    /// <summary>Добавляет карту в руку (вызывается движком).</summary>
    void AddToHand(SCard card);

    /// <summary>
    /// Атака: вернуть одну или несколько карт одинакового достоинства.
    /// При подкидывании — только карты, чьё достоинство уже есть на столе.
    /// </summary>
    List<SCard> LayCards();

    /// <summary>
    /// Защита: покрыть все карты на столе.
    /// Возвращает true — отбился, false — берёт все карты со стола.
    /// </summary>
    bool Defend(List<SCardPair> table);

    /// <summary>
    /// Подкидывание: добавить карты на стол после ответа защищающегося.
    /// Возвращает true — подкинул, false — пас.
    /// </summary>
    bool AddCards(List<SCardPair> table);

    /// <summary>Вывод руки в консоль (вызывается движком для отладки).</summary>
    void ShowHand();
}
