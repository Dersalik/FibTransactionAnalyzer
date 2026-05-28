using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Transaction;

public class FibTransaction
{
    private static readonly HashSet<string> InternalTransactionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AUTO_SAVING_TRANSFER",
        "MONEY_BOX_TRANSFER"
    };

    [Name("ID")]
    [Required]
    public Guid Id { get; set; }

    [Name("COUNTERPARTY")]
    public string Counterparty { get; set; } = string.Empty;

    [Name("AMOUNT")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue Amount { get; set; } = new MonetaryValue(0, Currency.IQD);

    [Name("FEE")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue Fee { get; set; } = new MonetaryValue(0, Currency.IQD);

    [Name("BALANCE AFTER")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue BalanceAfter { get; set; } = new MonetaryValue(0, Currency.IQD);

    [Name("TRANSACTION TYPE")]
    public string TransactionType { get; set; } = string.Empty;

    [Name("DATE")]
    [TypeConverter(typeof(DateTimeConverter))]
    public DateTime Date { get; set; } = DateTime.MinValue;

    [Name("TIME")]
    [TypeConverter(typeof(TimeSpanConverter))]
    public TimeSpan Time { get; set; } = TimeSpan.Zero;

    [Name("STATUS")]
    public string Status { get; set; } = string.Empty;

    [Name("TRANSACTION ID")]
    public Guid? TransactionId { get; set; }

    [Name("NOTE")]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether this transaction is an internal transfer between accounts.
    /// </summary>
    public bool IsInternal => InternalTransactionTypes.Contains(TransactionType);
}