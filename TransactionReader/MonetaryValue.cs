using System.Globalization;
using System.Text.RegularExpressions;

namespace Transaction;

public readonly struct MonetaryValue
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public MonetaryValue(decimal amount, Currency currency = Currency.IQD)
    {
        Amount = amount;
        Currency = currency;
    }

    public static MonetaryValue Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new MonetaryValue(0, Currency.IQD);

        value = value.Trim();

        // Pattern to match: optional minus, number (with decimals/commas), optional space, currency code
        var pattern = @"^(-?\d+(?:[,\.]\d+)*)\s*([A-Z]{3})?$";
        var match = Regex.Match(value, pattern);

        if (!match.Success)
            throw new FormatException($"Unable to parse monetary value: '{value}'");

        var amountStr = match.Groups[1].Value;
        var currencyCode = match.Groups[2].Value;

        // Parse the amount (handle both comma and dot as decimal separators)
        decimal amount;
        if (amountStr.Contains(',') && !amountStr.Contains('.'))
        {
            var commaIndex = amountStr.LastIndexOf(',');
            var afterComma = amountStr.Substring(commaIndex + 1);

            // If there are 1-2 digits after the comma, treat it as decimal separator
            if (afterComma.Length <= 2 && afterComma.All(char.IsDigit))
            {
                amountStr = amountStr.Replace(',', '.');
            }
        }

        if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            // Try with different culture if first attempt fails
            if (!decimal.TryParse(amountStr.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                throw new FormatException($"Unable to parse amount: '{amountStr}'");
            }
        }

        // Parse currency
        var currency = string.IsNullOrEmpty(currencyCode)
            ? Currency.IQD
            : CurrencyExtensions.FromCode(currencyCode);

        return new MonetaryValue(amount, currency);
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency.ToCode()}";
    }

    public string ToString(string format)
    {
        return $"{Amount.ToString(format)} {Currency.ToCode()}";
    }

    public string ToDisplayString()
    {
        return $"{Currency.GetSymbol()}{Amount:F2}";
    }

    public override bool Equals(object? obj)
    {
        return obj is MonetaryValue other &&
               Amount == other.Amount &&
               Currency == other.Currency;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public static implicit operator decimal(MonetaryValue monetaryValue)
    {
        return monetaryValue.Amount;
    }
}