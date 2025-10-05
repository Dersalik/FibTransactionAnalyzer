# Transaction Analyzer

A powerful, privacy-focused financial transaction analysis tool that transforms your CSV transaction data into comprehensive insights and visualizations.

## What This Tool Does

Transaction Analyzer takes your exported transaction files and creates detailed financial reports with interactive charts, trends analysis, and spending patterns. Think of it as your personal financial dashboard that helps you understand where your money comes from and where it goes.

The tool supports multiple currencies (USD, EUR, IQD) and provides deep insights into your financial behavior without storing any of your sensitive data.

## Key Features

**Financial Insights**
- Monthly income vs expense tracking with trend analysis
- Account balance history visualization  
- Transaction categorization and spending pattern analysis
- Multi-currency support with separate analytics for each currency

**Privacy & Security**
- Zero data storage - your information never leaves your session
- Local processing with immediate data cleanup
- No tracking, cookies, or analytics
- HTTPS encryption for all communications

**Visual Analytics**
- Interactive charts showing balance trends over time
- Cash flow analysis with income/expense comparisons
- Transaction type distribution with detailed breakdowns
- Yearly and monthly financial performance reports

## Screenshots

*Dashboard Overview*

<img width="1737" height="711" alt="Transaction Analysis Results - Transaction Analyzer - Google Chrome 26_09_2025 4_33_49 PM" src="https://github.com/user-attachments/assets/d65be72f-92b4-4798-82a2-b0005acc1580" />

*Monthly Cash Flow Analysis*  

<img width="787" height="525" alt="Transaction Analysis Results - Transaction Analyzer - Google Chrome 26_09_2025 1_34_39 AM" src="https://github.com/user-attachments/assets/c9890dc8-d159-438e-936b-f067bf2ed42d" />

*Transaction Type Breakdown*

<img width="788" height="513" alt="Transaction Analysis Results - Transaction Analyzer - Google Chrome 26_09_2025 12_08_24 AM" src="https://github.com/user-attachments/assets/16c60043-c275-482f-8a8a-7429ab214050" />

*Top Counterparties*

<img width="769" height="850" alt="Transaction Analysis Results - Transaction Analyzer - Google Chrome 26_09_2025 1_41_38 AM" src="https://github.com/user-attachments/assets/d113bb4d-1561-48a7-a894-39ca84ef9734" />

*Counterparties - Sent vs Received*

<img width="1673" height="676" alt="Transaction Analysis Results - Transaction Analyzer - Google Chrome 26_09_2025 4_31_37 PM" src="https://github.com/user-attachments/assets/7c0ee6c7-987c-499a-a6b3-0cacd5187577" />

## Quick Start

**Requirements**
- .NET 8.0 SDK
- A modern web browser

**Running the Application**

1. Clone this repository
2. Navigate to the TransactionAnalyzer directory
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser to `https://localhost:8081`
5. Upload your transaction CSV file and start analyzing

**Supported File Format**
The tool works with FIB (First Iraqi Bank) transaction export format, but the CSV structure can be adapted for other banking formats. Expected columns include transaction ID, counterparty, amount, fees, dates, and transaction types.

## Project Structure

This solution consists of two main components that work together to provide comprehensive transaction analysis:

**Transaction Library** - The core engine that handles CSV parsing, data validation, and transaction modeling. This library includes custom converters for different data types like monetary values, dates, and GUIDs, making it robust enough to handle real-world banking data with its inconsistencies.

**TransactionAnalyzer Web App** - A clean, modern web interface built with ASP.NET Core that provides the analysis service and visualization dashboard. The web app transforms raw transaction data into meaningful insights through statistical analysis and interactive charts.

## Technical Highlights

The codebase demonstrates several advanced .NET concepts including custom CSV type converters, comprehensive unit testing with xUnit, dependency injection, and responsive web design with Chart.js integration.

The analysis engine calculates complex financial metrics like running balances, monthly cash flows, counterparty relationships, and transaction patterns while maintaining excellent performance even with large datasets.

## Development & Testing

```bash
# Run all tests
dotnet test

# Build the entire solution
dotnet build TransactionAnalyzer.sln

# Run with development settings
dotnet run --environment Development
```

The project includes comprehensive test coverage with both unit tests and integration tests, ensuring reliability when processing your financial data.

## Contributing

This project welcomes contributions! The modular architecture makes it easy to add new analysis features, support additional file formats, or enhance the visualization capabilities.

## License

[MIT License](https://github.com/Dersalik/FibTransactionAnalyzer/blob/master/LICENSE)

---

**Privacy Promise**: This tool processes your data locally and never stores, shares, or transmits your financial information to external services. Your privacy and security are our top priorities.
