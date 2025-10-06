using TransactionAnalyzer.Services;

namespace Transaction.Test.Unit;

public class TransactionAnalysisServiceTest
{
    private readonly TransactionAnalysisService _service = new();

    #region Test Data Helpers

    private static List<FibTransaction> CreateTestTransactions()
    {
        return new List<FibTransaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(100m, Currency.USD),
                Fee = new MonetaryValue(2m, Currency.USD),
                BalanceAfter = new MonetaryValue(1100m, Currency.USD),
                TransactionType = "TRANSFER",
                Date = new DateTime(2023, 10, 1),
                Time = new TimeSpan(10, 0, 0),
                Counterparty = "John Doe",
                Status = "COMPLETED"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(-50m, Currency.USD),
                Fee = new MonetaryValue(1m, Currency.USD),
                BalanceAfter = new MonetaryValue(1050m, Currency.USD),
                TransactionType = "PAYMENT",
                Date = new DateTime(2023, 10, 2),
                Time = new TimeSpan(14, 30, 0),
                Counterparty = "Store ABC",
                Status = "COMPLETED"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(25m, Currency.USD),
                Fee = new MonetaryValue(0m, Currency.USD),
                BalanceAfter = new MonetaryValue(0m, Currency.USD),
                TransactionType = "MONEY_BOX_TRANSFER",
                Date = new DateTime(2023, 10, 3),
                Time = new TimeSpan(9, 0, 0),
                Counterparty = "Money Box",
                Status = "COMPLETED"
            }
        };
    }

    private static List<FibTransaction> CreateMultiCurrencyTransactions()
    {
        return new List<FibTransaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(100m, Currency.USD),
                Fee = new MonetaryValue(2m, Currency.USD),
                BalanceAfter = new MonetaryValue(100m, Currency.USD),
                TransactionType = "TRANSFER",
                Date = new DateTime(2023, 9, 1),
                Time = new TimeSpan(10, 0, 0),
                Counterparty = "USD Counterparty"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(200m, Currency.EUR),
                Fee = new MonetaryValue(3m, Currency.EUR),
                BalanceAfter = new MonetaryValue(200m, Currency.EUR),
                TransactionType = "PAYMENT",
                Date = new DateTime(2023, 9, 2),
                Time = new TimeSpan(11, 0, 0),
                Counterparty = "EUR Counterparty"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Amount = new MonetaryValue(300m, Currency.IQD),
                Fee = new MonetaryValue(1m, Currency.IQD),
                BalanceAfter = new MonetaryValue(300m, Currency.IQD),
                TransactionType = "DEPOSIT",
                Date = new DateTime(2023, 9, 3),
                Time = new TimeSpan(12, 0, 0),
                Counterparty = "IQD Counterparty"
            }
        };
    }

    private static List<FibTransaction> CreateMonthlyTestTransactions()
    {
        return new List<FibTransaction>
        {
            // January 2023
            new()
            {
                Amount = new MonetaryValue(500m, Currency.USD),
                Date = new DateTime(2023, 1, 15),
                TransactionType = "SALARY"
            },
            new()
            {
                Amount = new MonetaryValue(-100m, Currency.USD),
                Date = new DateTime(2023, 1, 20),
                TransactionType = "PAYMENT"
            },
            // February 2023
            new()
            {
                Amount = new MonetaryValue(600m, Currency.USD),
                Date = new DateTime(2023, 2, 15),
                TransactionType = "SALARY"
            },
            new()
            {
                Amount = new MonetaryValue(-200m, Currency.USD),
                Date = new DateTime(2023, 2, 25),
                TransactionType = "PAYMENT"
            }
        };
    }

    #endregion

    #region AnalyzeTransactions Tests

    [Fact]
    public void AnalyzeTransactions_WithValidTransactions_ReturnsCorrectResult()
    {
        // Arrange
        var transactions = CreateTestTransactions();

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalTransactionCount);
        Assert.Equal(3, result.FilteredTransactionCount);
        Assert.Single(result.CurrencyAnalyses);
        Assert.Contains(Currency.USD, result.CurrencyAnalyses.Keys);
    }

    [Fact]
    public void AnalyzeTransactions_WithIgnoreInternalTransactions_FiltersCorrectly()
    {
        // Arrange  
        var transactions = CreateTestTransactions();

        // Act
        var result = _service.Analyze(transactions, true, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.Equal(3, result.TotalTransactionCount);
        Assert.Equal(2, result.FilteredTransactionCount); // Money box transfer filtered out
    }

    [Fact]
    public void AnalyzeTransactions_WithEmptyTransactions_ReturnsEmptyResult()
    {
        // Arrange
        var transactions = new List<FibTransaction>();

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalTransactionCount);
        Assert.Equal(0, result.FilteredTransactionCount);
        Assert.Empty(result.CurrencyAnalyses);
    }

    [Fact]
    public void AnalyzeTransactions_WithNullTransactions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _service.Analyze(null, false, DateTime.MinValue, DateTime.MaxValue));
    }

    [Fact]
    public void AnalyzeTransactions_WithMultipleCurrencies_AnalyzesAllCurrencies()
    {
        // Arrange
        var transactions = CreateMultiCurrencyTransactions();

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.Equal(3, result.CurrencyAnalyses.Count);
        Assert.Contains(Currency.USD, result.CurrencyAnalyses.Keys);
        Assert.Contains(Currency.EUR, result.CurrencyAnalyses.Keys);
        Assert.Contains(Currency.IQD, result.CurrencyAnalyses.Keys);
    }

    #endregion

    #region Currency Analysis Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesBasicMetricsCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Fee = new MonetaryValue(2m, Currency.USD) },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), Fee = new MonetaryValue(1m, Currency.USD) },
            new() { Amount = new MonetaryValue(25m, Currency.USD), Fee = new MonetaryValue(0.5m, Currency.USD) }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(125m, usdAnalysis.TotalInflow);
        Assert.Equal(50m, usdAnalysis.TotalOutflow);
        Assert.Equal(75m, usdAnalysis.NetAmount);
        Assert.Equal(3.5m, usdAnalysis.TotalFees);
        Assert.Equal(3, usdAnalysis.TransactionCount);
    }

    #endregion

    #region Monthly Analysis Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesMonthlyAnalysisCorrectly()
    {
        // Arrange
        var transactions = CreateMonthlyTestTransactions();

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.MonthlyAnalyses.Count);

        // Check February (should be first due to descending order)
        var febAnalysis = usdAnalysis.MonthlyAnalyses.First();
        Assert.Equal(2023, febAnalysis.Year);
        Assert.Equal(2, febAnalysis.Month);
        Assert.Equal(600m, febAnalysis.Income);
        Assert.Equal(200m, febAnalysis.Expenses);
        Assert.Equal(2, febAnalysis.TransactionCount);

        // Check January
        var janAnalysis = usdAnalysis.MonthlyAnalyses.Last();
        Assert.Equal(2023, janAnalysis.Year);
        Assert.Equal(1, janAnalysis.Month);
        Assert.Equal(500m, janAnalysis.Income);
        Assert.Equal(100m, janAnalysis.Expenses);
        Assert.Equal(2, janAnalysis.TransactionCount);
    }

    [Fact]
    public void AnalyzeTransactions_FiltersInvalidDatesFromMonthlyAnalysis()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Date = DateTime.MinValue },
            new() { Amount = new MonetaryValue(200m, Currency.USD), Date = new DateTime(2023, 1, 1) }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Single(usdAnalysis.MonthlyAnalyses); // Only the valid date should be included
    }

    #endregion

    #region Yearly Analysis Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesYearlyAnalysisCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Date = new DateTime(2022, 6, 1) },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), Date = new DateTime(2022, 6, 2) },
            new() { Amount = new MonetaryValue(200m, Currency.USD), Date = new DateTime(2023, 1, 1) },
            new() { Amount = new MonetaryValue(-75m, Currency.USD), Date = new DateTime(2023, 1, 2) }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.YearlyAnalyses.Count);

        // Check 2023 (should be first due to descending order)
        var analysis2023 = usdAnalysis.YearlyAnalyses.First();
        Assert.Equal(2023, analysis2023.Year);
        Assert.Equal(200m, analysis2023.Income);
        Assert.Equal(75m, analysis2023.Expenses);
        Assert.Equal(2, analysis2023.TransactionCount);

        // Check 2022
        var analysis2022 = usdAnalysis.YearlyAnalyses.Last();
        Assert.Equal(2022, analysis2022.Year);
        Assert.Equal(100m, analysis2022.Income);
        Assert.Equal(50m, analysis2022.Expenses);
        Assert.Equal(2, analysis2022.TransactionCount);
    }

    #endregion

    #region Balance History Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesBalanceHistoryWithBalanceAfter()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new()
            {
                Amount = new MonetaryValue(100m, Currency.USD),
                BalanceAfter = new MonetaryValue(100m, Currency.USD),
                Date = new DateTime(2023, 1, 1),
                Time = new TimeSpan(10, 0, 0)
            },
            new()
            {
                Amount = new MonetaryValue(-50m, Currency.USD),
                BalanceAfter = new MonetaryValue(50m, Currency.USD),
                Date = new DateTime(2023, 1, 2),
                Time = new TimeSpan(11, 0, 0)
            }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.BalanceHistory.Count);
        Assert.Equal(100m, usdAnalysis.BalanceHistory.First().Balance);
        Assert.Equal(50m, usdAnalysis.BalanceHistory.Last().Balance);
    }

    [Fact]
    public void AnalyzeTransactions_CalculatesRunningBalanceWhenBalanceAfterIsZero()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new()
            {
                Amount = new MonetaryValue(100m, Currency.USD),
                BalanceAfter = new MonetaryValue(0m, Currency.USD),
                Date = new DateTime(2023, 1, 1),
                Time = new TimeSpan(10, 0, 0)
            },
            new()
            {
                Amount = new MonetaryValue(-30m, Currency.USD),
                BalanceAfter = new MonetaryValue(0m, Currency.USD),
                Date = new DateTime(2023, 1, 2),
                Time = new TimeSpan(11, 0, 0)
            }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.BalanceHistory.Count);
        Assert.Equal(100m, usdAnalysis.BalanceHistory.First().Balance);
        Assert.Equal(70m, usdAnalysis.BalanceHistory.Last().Balance);
    }

    #endregion

    #region Transaction Type Analysis Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesTransactionTypeAnalysisCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), TransactionType = "TRANSFER" },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), TransactionType = "TRANSFER" },
            new() { Amount = new MonetaryValue(-25m, Currency.USD), TransactionType = "PAYMENT" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.TransactionTypeAnalyses.Count);

        var transferAnalysis = usdAnalysis.TransactionTypeAnalyses.First(t => t.TransactionType == "TRANSFER");
        Assert.Equal(2, transferAnalysis.Count);
        Assert.Equal(150m, transferAnalysis.TotalAmount);
        Assert.Equal(100m, transferAnalysis.LargestAmount);

        var paymentAnalysis = usdAnalysis.TransactionTypeAnalyses.First(t => t.TransactionType == "PAYMENT");
        Assert.Equal(1, paymentAnalysis.Count);
        Assert.Equal(25m, paymentAnalysis.TotalAmount);
        Assert.Equal(25m, paymentAnalysis.LargestAmount);
    }

    [Fact]
    public void AnalyzeTransactions_CalculatesLargestTransactionsByTypeCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), TransactionType = "TRANSFER", Counterparty = "Big Transfer" },
            new() { Amount = new MonetaryValue(-200m, Currency.USD), TransactionType = "TRANSFER", Counterparty = "Huge Outflow" },
            new() { Amount = new MonetaryValue(-25m, Currency.USD), TransactionType = "PAYMENT", Counterparty = "Small Payment" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];

        // Should be ordered by largest amount descending
        var largestByType = usdAnalysis.LargestTransactionsByType;
        Assert.Equal("TRANSFER", largestByType.First().TransactionType);
        Assert.Equal(200m, largestByType.First().LargestAmount);
        Assert.Equal("Huge Outflow", largestByType.First().LargestTransaction.Counterparty);
    }

    #endregion

    #region Counterparty Analysis Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesTopCounterpartiesCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Counterparty = "Big Company" },
            new() { Amount = new MonetaryValue(50m, Currency.USD), Counterparty = "Big Company" },
            new() { Amount = new MonetaryValue(-200m, Currency.USD), Counterparty = "Huge Vendor" },
            new() { Amount = new MonetaryValue(-25m, Currency.USD), Counterparty = "Small Store" },
            new() { Amount = new MonetaryValue(0m, Currency.USD), Counterparty = "" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(3, usdAnalysis.TopCounterparties.Count);

        var bigCompany = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Big Company");
        Assert.Equal("Big Company", bigCompany.Counterparty);
        Assert.Equal(150m, bigCompany.TotalAmount);
        Assert.Equal(2, bigCompany.TransactionCount);

        var hugeVendor = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Huge Vendor");
        Assert.Equal("Huge Vendor", hugeVendor.Counterparty);
        Assert.Equal(200m, hugeVendor.TotalAmount);
        Assert.Equal(1, hugeVendor.TransactionCount);

        var smallStore = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Small Store");
        Assert.Equal("Small Store", smallStore.Counterparty);
        Assert.Equal(25m, smallStore.TotalAmount);
        Assert.Equal(1, smallStore.TransactionCount);
    }

    [Fact]
    public void AnalyzeTransactions_CalculatesCounterpartiesByTransactionTypeCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Counterparty = "Company A", TransactionType = "TRANSFER" },
            new() { Amount = new MonetaryValue(200m, Currency.USD), Counterparty = "Company B", TransactionType = "TRANSFER" },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), Counterparty = "Store A", TransactionType = "PAYMENT" },
            new() { Amount = new MonetaryValue(-75m, Currency.USD), Counterparty = "Store B", TransactionType = "PAYMENT" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.CounterpartiesByTransactionType.Count);

        Assert.Contains("TRANSFER", usdAnalysis.CounterpartiesByTransactionType.Keys);
        Assert.Contains("PAYMENT", usdAnalysis.CounterpartiesByTransactionType.Keys);

        var transferCounterparties = usdAnalysis.CounterpartiesByTransactionType["TRANSFER"];
        Assert.Equal(2, transferCounterparties.Count);
        Assert.Equal("Company B", transferCounterparties.First().Counterparty);
    }

    [Fact]
    public void AnalyzeTransactions_CalculatesCounterpartyAmountsSentReceivedAndNetCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Counterparty = "Company A" },
            new() { Amount = new MonetaryValue(50m, Currency.USD), Counterparty = "Company A" },
            new() { Amount = new MonetaryValue(-75m, Currency.USD), Counterparty = "Company A" },
            new() { Amount = new MonetaryValue(-150m, Currency.USD), Counterparty = "Company B" },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), Counterparty = "Company B" },
            new() { Amount = new MonetaryValue(300m, Currency.USD), Counterparty = "Company C" },
            new() { Amount = new MonetaryValue(100m, Currency.USD), Counterparty = "" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(3, usdAnalysis.TopCounterparties.Count);

        // Find Company A
        var companyA = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Company A");
        Assert.Equal(3, companyA.TransactionCount);
        Assert.Equal(225m, companyA.TotalAmount);
        Assert.Equal(75m, companyA.AmountSent);
        Assert.Equal(150m, companyA.AmountReceived);
        Assert.Equal(75m, companyA.NetAmount);

        // Find Company B
        var companyB = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Company B");
        Assert.Equal(2, companyB.TransactionCount);
        Assert.Equal(200m, companyB.TotalAmount);
        Assert.Equal(200m, companyB.AmountSent);
        Assert.Equal(0m, companyB.AmountReceived);
        Assert.Equal(-200m, companyB.NetAmount);

        // Find Company C
        var companyC = usdAnalysis.TopCounterparties.First(c => c.Counterparty == "Company C");
        Assert.Equal(1, companyC.TransactionCount);
        Assert.Equal(300m, companyC.TotalAmount);
        Assert.Equal(0m, companyC.AmountSent);
        Assert.Equal(300m, companyC.AmountReceived);
        Assert.Equal(300m, companyC.NetAmount);

        Assert.Contains("Company A", usdAnalysis.TopCounterparties.Select(c => c.Counterparty));
        Assert.Contains("Company B", usdAnalysis.TopCounterparties.Select(c => c.Counterparty));
        Assert.Contains("Company C", usdAnalysis.TopCounterparties.Select(c => c.Counterparty));
    }

    [Fact]
    public void AnalyzeTransactions_CounterpartyNetAmountCalculatedProperty_WorksCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(500m, Currency.USD), Counterparty = "Test Company" },
            new() { Amount = new MonetaryValue(-200m, Currency.USD), Counterparty = "Test Company" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        var testCompany = usdAnalysis.TopCounterparties.First();

        Assert.Equal("Test Company", testCompany.Counterparty);
        Assert.Equal(200m, testCompany.AmountSent);
        Assert.Equal(500m, testCompany.AmountReceived);
        Assert.Equal(300m, testCompany.NetAmount);

        Assert.Equal(testCompany.AmountReceived - testCompany.AmountSent, testCompany.NetAmount);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void AnalyzeTransactions_CalculatesIncomeStatisticsCorrectly()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Date = new DateTime(2023, 1, 1) },
            new() { Amount = new MonetaryValue(200m, Currency.USD), Date = new DateTime(2023, 2, 1) },
            new() { Amount = new MonetaryValue(50m, Currency.USD), Date = new DateTime(2023, 3, 1) },
            new() { Amount = new MonetaryValue(-25m, Currency.USD), Date = new DateTime(2023, 1, 15) } // Expense, should not affect income stats
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(116.67m, Math.Round(usdAnalysis.AverageMonthlyIncome, 2));
        Assert.Equal(200m, usdAnalysis.MaxMonthlyIncome);
        Assert.Equal(50m, usdAnalysis.MinMonthlyIncome);

        Assert.NotNull(usdAnalysis.BestIncomeMonth);
        Assert.Equal(2, usdAnalysis.BestIncomeMonth.Month);
        Assert.Equal(200m, usdAnalysis.BestIncomeMonth.Income);

        Assert.NotNull(usdAnalysis.WorstIncomeMonth);
        Assert.Equal(3, usdAnalysis.WorstIncomeMonth.Month);
        Assert.Equal(50m, usdAnalysis.WorstIncomeMonth.Income);
    }

    [Fact]
    public void AnalyzeTransactions_HandlesNoIncomeMonthsGracefully()
    {
        // Arrange - Only expense transactions
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(-100m, Currency.USD), Date = new DateTime(2023, 1, 1) },
            new() { Amount = new MonetaryValue(-50m, Currency.USD), Date = new DateTime(2023, 2, 1) }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(0m, usdAnalysis.AverageMonthlyIncome);
        Assert.Equal(0m, usdAnalysis.MaxMonthlyIncome);
        Assert.Equal(0m, usdAnalysis.MinMonthlyIncome);
        Assert.Null(usdAnalysis.BestIncomeMonth);
        Assert.Null(usdAnalysis.WorstIncomeMonth);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AnalyzeTransactions_HandlesAllMoneyBoxTransfers()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), TransactionType = "MONEY_BOX_TRANSFER" },
            new() { Amount = new MonetaryValue(50m, Currency.USD), TransactionType = "MONEY_BOX_TRANSFER" }
        };

        // Act
        var result = _service.Analyze(transactions, true, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.Equal(2, result.TotalTransactionCount);
        Assert.Equal(0, result.FilteredTransactionCount);
        Assert.Empty(result.CurrencyAnalyses);
    }

    [Fact]
    public void AnalyzeTransactions_HandlesTransactionsWithoutCounterparty()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(100m, Currency.USD), Counterparty = null },
            new() { Amount = new MonetaryValue(50m, Currency.USD), Counterparty = "" },
            new() { Amount = new MonetaryValue(25m, Currency.USD), Counterparty = "Valid Counterparty" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Single(usdAnalysis.TopCounterparties);
        Assert.Equal("Valid Counterparty", usdAnalysis.TopCounterparties.First().Counterparty);
    }

    [Fact]
    public void AnalyzeTransactions_HandlesZeroAmountTransactions()
    {
        // Arrange
        var transactions = new List<FibTransaction>
        {
            new() { Amount = new MonetaryValue(0m, Currency.USD), TransactionType = "ZERO_TRANSFER" },
            new() { Amount = new MonetaryValue(100m, Currency.USD), TransactionType = "NORMAL_TRANSFER" }
        };

        // Act
        var result = _service.Analyze(transactions, false, DateTime.MinValue, DateTime.MaxValue);

        // Assert
        var usdAnalysis = result.CurrencyAnalyses[Currency.USD];
        Assert.Equal(2, usdAnalysis.TransactionCount);
        Assert.Equal(100m, usdAnalysis.TotalInflow);
        Assert.Equal(0m, usdAnalysis.TotalOutflow);
        Assert.Equal(100m, usdAnalysis.NetAmount);
    }

    #endregion
}