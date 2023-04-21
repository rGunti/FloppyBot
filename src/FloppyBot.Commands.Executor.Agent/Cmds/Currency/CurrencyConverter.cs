using Microsoft.Extensions.Configuration;
using RestSharp;

namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

public class CurrencyConverter : ICurrencyConverter, IDisposable
{
    private readonly CurrencyCommandConfig _currencyCommandConfig;
    private readonly RestClient _restClient;

    public CurrencyConverter(IConfiguration configuration)
    {
        _currencyCommandConfig = configuration.GetSection("Currency").Get<CurrencyCommandConfig>();
        _restClient = new RestClient(_currencyCommandConfig.SourceUrl);
    }

    public async Task<CurrencyValue> Convert(CurrencyValue input, string targetCurrency)
    {
        var rate = await GetConversionRate(input.Currency, targetCurrency);
        return rate.ConvertFrom(input);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private async Task<CurrencyConversionRecord> GetConversionRate(string from, string to)
    {
        RestRequest request = new RestRequest($"/api/v7/convert")
            .AddQueryParameter("q", $"{from}_{to}")
            .AddQueryParameter("compact", "ultra")
            .AddQueryParameter("apiKey", _currencyCommandConfig.ApiKey);
        var responseData = await _restClient.GetAsync<Dictionary<string, decimal>>(request);

        if (responseData == null)
        {
            throw new InvalidOperationException("Could not load data as requested");
        }

        return new CurrencyConversionRecord(from, to, responseData[$"{from}_{to}"]);
    }
}
