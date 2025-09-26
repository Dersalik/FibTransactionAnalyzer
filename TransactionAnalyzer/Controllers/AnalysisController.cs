using Microsoft.AspNetCore.Mvc;
using Transaction;
using TransactionAnalyzer.Models;

namespace TransactionAnalyzer.Controllers;

public class AnalysisController : Controller
{
    private readonly ITransactionReader _transactionReader;
    private readonly ITransactionAnalysisService _analysisService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        ITransactionReader transactionReader,
        ITransactionAnalysisService analysisService,
        ILogger<AnalysisController> logger)
    {
        _transactionReader = transactionReader;
        _analysisService = analysisService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAndAnalyze(IFormFile file, bool ignoreInternalTransactions, DateTime? dateFrom, DateTime? dateTo)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please select a valid CSV file.");
            return View("Index");
        }

        if (!IsValidCsvFile(file))
        {
            ModelState.AddModelError("file", "Please upload a valid CSV file.");
            return View("Index");
        }

        try
        {
            _logger.LogInformation("Starting transaction analysis for file: {FileName} ({FileSize} bytes)",
                file.FileName, file.Length);

            IEnumerable<FibTransaction> transactions;
            using (var stream = file.OpenReadStream())
            {
                transactions = await _transactionReader.ReadTransactionsAsync(stream);
            }

            var analysisResult = await _analysisService.AnalyzeTransactionsAsync(
                transactions, 
                ignoreInternalTransactions, 
                dateFrom ?? DateTime.MinValue,
                dateTo ?? DateTime.MaxValue
                );

            _logger.LogInformation("Analysis completed. Total transactions: {Total}, Filtered: {Filtered}",
                analysisResult.TotalTransactionCount, analysisResult.FilteredTransactionCount);

            return View("Results", analysisResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing transactions from file: {FileName}", file.FileName);
            ModelState.AddModelError("file", "An error occurred while analyzing the file. Please ensure it's a valid transaction CSV file.");
            return View("Index");
        }
    }

    private bool IsValidCsvFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".csv", ".txt" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return false;

        var allowedMimeTypes = new[] { "text/csv", "text/plain", "application/csv" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }
}