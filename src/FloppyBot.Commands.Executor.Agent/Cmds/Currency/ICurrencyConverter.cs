namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

public interface ICurrencyConverter
{
    Task<CurrencyValue> Convert(CurrencyValue input, string targetCurrency);
}
