namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

public record CurrencyValue(
    string Currency,
    decimal Value)
{
    public override string ToString()
    {
        return $"{Value:0.00#} {Currency.ToUpperInvariant()}";
    }
}
