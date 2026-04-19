namespace DurakBenchmark;

public interface IStrategyFactory
{
    IAttackStrategy CreateAttackStrategy();
    IDefendStrategy CreateDefendStrategy();
}
