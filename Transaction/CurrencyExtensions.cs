namespace Transaction;

public static class CurrencyExtensions
{
    /// <summary>
    /// Converts the currency enum to its string code representation
    /// </summary>
    public static string ToCode(this Currency currency)
    {
        return currency.ToString();
    }

    /// <summary>
    /// Converts a currency code string to the corresponding Currency enum
    /// </summary>
    public static Currency FromCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return Currency.IQD; // Default currency

        return currencyCode.Trim().ToUpperInvariant() switch
        {
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            "IQD" => Currency.IQD,
            _ => throw new ArgumentException($"Unknown currency code: {currencyCode}", nameof(currencyCode))
        };
    }

    /// <summary>
    /// Gets the currency symbol for display purposes
    /// </summary>
    public static string GetSymbol(this Currency currency)
    {
        return currency switch
        {
            Currency.USD => "$",
            Currency.EUR => "€",
            Currency.IQD => "د.ع", // Iraqi Dinar symbol
            _ => currency.ToCode()
        };
    }

    /// <summary>
    /// Gets the appropriate culture info for currency formatting
    /// </summary>
    public static string GetCultureCode(this Currency currency)
    {
        return currency switch
        {
            Currency.USD => "en-US", 
            Currency.EUR => "de-DE",
            Currency.IQD => "ar-IQ", 
            _ => throw new NotImplementedException(),
        };
    }
}