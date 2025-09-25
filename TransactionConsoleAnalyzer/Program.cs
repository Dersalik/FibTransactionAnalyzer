using Transaction;

// Example usage of the FIB Transaction Reader - Top-level program
var transactionReader = new TransactionReader();

try
{
    Console.WriteLine("Please enter path for the file to be analyzed");
    var filePath = Console.ReadLine();

    // Read from file path
    var allTransactions = await transactionReader.ReadTransactionsAsync(filePath);

    // Filter out money box transfers
    var transactions = allTransactions
        .Where(t => t.TransactionType != "MONEY_BOX_TRANSFER")
        .ToList();

    Console.WriteLine("=== TRANSACTION ANALYSIS ===");
    Console.WriteLine($"📊 Total transactions loaded: {allTransactions.Count()}");
    Console.WriteLine($"🔍 Filtered transactions (excluding money box): {transactions.Count}");
    Console.WriteLine($"📦 Money box transfers filtered out: {allTransactions.Count() - transactions.Count}");
    Console.WriteLine();

    if (!transactions.Any())
    {
        Console.WriteLine("No transactions to analyze after filtering.");
        return;
    }

    // Group by currency for analysis
    var currencyGroups = transactions.GroupBy(t => t.Amount.Currency);

    foreach (var currencyGroup in currencyGroups)
    {
        var currency = currencyGroup.Key;
        var currencyTransactions = currencyGroup.ToList();

        Console.WriteLine($"💰 === {currency} ANALYSIS ===");
        Console.WriteLine($"Transaction count: {currencyTransactions.Count}");

        // Financial summary
        var totalInflow = currencyTransactions.Where(t => t.Amount.Amount > 0).Sum(t => t.Amount.Amount);
        var totalOutflow = currencyTransactions.Where(t => t.Amount.Amount < 0).Sum(t => Math.Abs(t.Amount.Amount));
        var netAmount = currencyTransactions.Sum(t => t.Amount.Amount);
        var totalFees = currencyTransactions.Sum(t => t.Fee.Amount);

        Console.WriteLine($"💵 Total Inflow:  {totalInflow:N2} {currency}");
        Console.WriteLine($"💸 Total Outflow: {totalOutflow:N2} {currency}");
        Console.WriteLine($"📈 Net Amount:    {netAmount:N2} {currency}");
        Console.WriteLine($"💳 Total Fees:    {totalFees:N2} {currency}");
        Console.WriteLine();

        // === MONTHLY INCOME ANALYSIS ===
        Console.WriteLine($"📅 === MONTHLY INCOME ANALYSIS ({currency}) ===");

        var monthlyData = currencyTransactions
            .Select(t => new
            {
                Transaction = t,
                Date = t.Date,
                Income = t.Amount.Amount > 0 ? t.Amount.Amount : 0,
                Expense = t.Amount.Amount < 0 ? Math.Abs(t.Amount.Amount) : 0
            })
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .ToList();

        if (monthlyData.Any())
        {
            Console.WriteLine("Month/Year        | Income      | Expenses    | Net Income  | Transactions");
            Console.WriteLine("------------------|-------------|-------------|-------------|-------------");

            foreach (var monthGroup in monthlyData)
            {
                var monthName = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1).ToString("MMM yyyy");
                var monthlyIncome = monthGroup.Sum(t => t.Income);
                var monthlyExpenses = monthGroup.Sum(t => t.Expense);
                var netIncome = monthlyIncome - monthlyExpenses;
                var transactionCount = monthGroup.Count();

                var netSign = netIncome >= 0 ? "+" : "";
                Console.WriteLine($"{monthName,-17} | {monthlyIncome,9:N0} {currency} | {monthlyExpenses,9:N0} {currency} | {netSign}{netIncome,9:N0} {currency} | {transactionCount,11}");
            }

            // Monthly income statistics
            var avgMonthlyIncome = monthlyData.Average(g => g.Sum(t => t.Income));
            var maxMonthlyIncome = monthlyData.Max(g => g.Sum(t => t.Income));
            var minMonthlyIncome = monthlyData.Min(g => g.Sum(t => t.Income));
            var bestMonth = monthlyData.OrderByDescending(g => g.Sum(t => t.Income)).First();
            var worstMonth = monthlyData.OrderBy(g => g.Sum(t => t.Income)).First();

            Console.WriteLine();
            Console.WriteLine($"📊 Income Statistics ({currency}):");
            Console.WriteLine($"  • Average Monthly Income: {avgMonthlyIncome:N2} {currency}");
            Console.WriteLine($"  • Highest Monthly Income: {maxMonthlyIncome:N2} {currency} (in {new DateTime(bestMonth.Key.Year, bestMonth.Key.Month, 1):MMM yyyy})");
            Console.WriteLine($"  • Lowest Monthly Income:  {minMonthlyIncome:N2} {currency} (in {new DateTime(worstMonth.Key.Year, worstMonth.Key.Month, 1):MMM yyyy})");

            // Income trend (compare last 3 months if available)
            if (monthlyData.Count >= 2)
            {
                var recentMonths = monthlyData.Take(3).ToList();
                Console.WriteLine();
                Console.WriteLine($"📈 Recent Income Trend ({currency}):");

                for (int i = 0; i < recentMonths.Count - 1; i++)
                {
                    var currentMonth = recentMonths[i];
                    var previousMonth = recentMonths[i + 1];

                    var currentIncome = currentMonth.Sum(t => t.Income);
                    var previousIncome = previousMonth.Sum(t => t.Income);
                    var change = currentIncome - previousIncome;
                    var changePercent = previousIncome > 0 ? (change / previousIncome) * 100 : 0;

                    var currentMonthName = new DateTime(currentMonth.Key.Year, currentMonth.Key.Month, 1).ToString("MMM yyyy");
                    var previousMonthName = new DateTime(previousMonth.Key.Year, previousMonth.Key.Month, 1).ToString("MMM yyyy");

                    var trendIcon = change > 0 ? "📈" : change < 0 ? "📉" : "➡️";
                    var changeSign = change >= 0 ? "+" : "";

                    Console.WriteLine($"  {trendIcon} {currentMonthName} vs {previousMonthName}: {changeSign}{change:N2} {currency} ({changeSign}{changePercent:F1}%)");
                }
            }
        }
        else
        {
            Console.WriteLine("❌ No valid dates found for monthly analysis");
        }
        Console.WriteLine();

        // Transaction type analysis
        Console.WriteLine("📋 Transaction Types:");
        var typeAnalysis = currencyTransactions
            .GroupBy(t => t.TransactionType)
            .OrderByDescending(g => g.Count())
            .Take(10);

        foreach (var typeGroup in typeAnalysis)
        {
            var typeTotal = typeGroup.Sum(t => t.Amount.Amount);
            Console.WriteLine($"  • {typeGroup.Key}: {typeGroup.Count()} transactions, Total: {typeTotal:N2} {currency}");
        }
        Console.WriteLine();

        // Status analysis
        Console.WriteLine("✅ Transaction Status:");
        var statusAnalysis = currencyTransactions
            .GroupBy(t => t.Status)
            .OrderByDescending(g => g.Count());

        foreach (var statusGroup in statusAnalysis)
        {
            Console.WriteLine($"  • {statusGroup.Key}: {statusGroup.Count()} transactions");
        }
        Console.WriteLine();

        // Top counterparties
        Console.WriteLine("🏢 Top Counterparties:");
        var counterpartyAnalysis = currencyTransactions
            .Where(t => !string.IsNullOrEmpty(t.Counterparty))
            .GroupBy(t => t.Counterparty)
            .OrderByDescending(g => Math.Abs(g.Sum(t => t.Amount.Amount)))
            .Take(5);

        foreach (var counterpartyGroup in counterpartyAnalysis)
        {
            var total = counterpartyGroup.Sum(t => t.Amount.Amount);
            var count = counterpartyGroup.Count();
            Console.WriteLine($"  • {counterpartyGroup.Key}: {count} transactions, Total: {total:N2} {currency}");
        }
        Console.WriteLine();

        // Largest transactions
        Console.WriteLine("🔝 Largest Transactions:");
        var largestTransactions = currencyTransactions
            .OrderByDescending(t => Math.Abs(t.Amount.Amount))
            .Take(3);

        foreach (var transaction in largestTransactions)
        {
            var direction = transaction.Amount.Amount > 0 ? "IN" : "OUT";
            Console.WriteLine($"  • {direction}: {transaction.Amount} | {transaction.TransactionType} | {transaction.Counterparty} | {transaction.Date}");
        }
        Console.WriteLine();
    }

    // Recent transactions (last 10, excluding money box)
    Console.WriteLine("🕒 Recent Transactions (Last 10):");
    var recentTransactions = transactions
        .OrderByDescending(t => DateTime.TryParse($"{t.Date} {t.Time}", out var date) ? date : DateTime.MinValue)
        .Take(10);

    foreach (var transaction in recentTransactions)
    {
        var direction = transaction.Amount.Amount > 0 ? "+" : "";
        Console.WriteLine($"  • {transaction.Date} {transaction.Time} | {direction}{transaction.Amount} | {transaction.TransactionType} | {transaction.Counterparty}");
    }

    Console.WriteLine();
    Console.WriteLine("=== ANALYSIS COMPLETE ===");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error reading transactions: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}