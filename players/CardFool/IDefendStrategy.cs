namespace DurakBenchmark;

public interface IDefendStrategy
{
    bool Defend(List<SCard> hand, List<SCardPair> table, Suits trump);
}
