namespace DurakBenchmark;

internal class ExpertStrategyFactory : IStrategyFactory
{
    private readonly GameAnalytics _analytics = new();

    public IAttackStrategy CreateAttackStrategy() => new SmartAttackStrategy(_analytics);
    public IDefendStrategy CreateDefendStrategy() => new SmartDefendStrategy(_analytics);
}
