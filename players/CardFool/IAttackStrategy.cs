namespace DurakBenchmark;

public interface IAttackStrategy
{
    List<SCard> LayCards(List<SCard> hand, Suits trump);
    bool AddCards(List<SCard> hand, List<SCardPair> table, Suits trump);
}
