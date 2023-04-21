namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

public record CurrencyConversionRecord(string From, string To, decimal ConversionRate)
{
    public CurrencyValue ConvertFrom(CurrencyValue value)
    {
        return new CurrencyValue(To, value.Value * ConversionRate);
    }
}
