using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Transaction;

public class FibTransaction
{
    [Name("ID")]
    [TypeConverter(typeof(GuidConverter))]
    public Guid Id { get; set; } = Guid.Empty;

    [Name("COUNTERPARTY")]
    public string Counterparty { get; set; } = string.Empty;

    [Name("AMOUNT")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue Amount { get; set; } = new MonetaryValue(0, Currency.USD);

    [Name("FEE")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue Fee { get; set; } = new MonetaryValue(0, Currency.USD);

    [Name("BALANCE AFTER")]
    [TypeConverter(typeof(MonetaryValueConverter))]
    public MonetaryValue BalanceAfter { get; set; } = new MonetaryValue(0, Currency.USD);

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
    public string TransactionId { get; set; } = string.Empty;

    [Name("NOTE")]
    public string Note { get; set; } = string.Empty;
}