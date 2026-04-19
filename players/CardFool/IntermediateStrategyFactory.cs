namespace DurakBenchmark;

internal class IntermediateStrategyFactory : IStrategyFactory
{
    public IAttackStrategy CreateAttackStrategy() => new RankGroupAttackStrategy();
    public IDefendStrategy CreateDefendStrategy() => new LowestCoverDefendStrategy();
}
