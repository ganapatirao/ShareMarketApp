# Indian Stock Market Analysis Application

A full-stack stock market analysis application for Indian stocks (NSE) that provides real-time stock recommendations, technical analysis, and options data.

## Technology Stack

### Backend (ASP.NET Core 11)
- Framework: .NET 11.0
- Language: C#
- API: ASP.NET Core Web API
- Database: MongoDB (primary) + Entity Framework InMemory (fallback)
- External API: Yahoo Finance
- Documentation: Swagger/OpenAPI

### Frontend (React 18)
- Framework: React 18.2.0
- Build Tool: Create React App (react-scripts 5.0.1)
- Language: JavaScript (JSX)
- Styling: CSS
- HTTP Client: Axios

## Key Features

- Real-time stock data fetching from Yahoo Finance API
- Technical analysis indicators (RSI, MACD, Volatility, Support/Resistance)
- Buy and Target price calculations using sophisticated algorithms
- Stock filtering by trend, sector, market cap, and company name
- Options chain data display
- Pagination and caching for performance optimization
- Background service for daily data updates
- Responsive React-based web interface

## Prerequisites

### Backend Requirements
- .NET 11.0 SDK
- MongoDB (local instance or replica set)
- Windows, Linux, or macOS operating system
- Internet connection for Yahoo Finance API

### Frontend Requirements
- Node.js (v14 or higher recommended)
- npm (comes with Node.js)
- Modern web browser

## Installation

### 1. Backend Setup

```bash
# Navigate to the project directory
cd stockmarket-agent

# Restore NuGet packages
dotnet restore

# Configure MongoDB connection in appsettings.json
# Update the ConnectionString with your MongoDB credentials
```

#### MongoDB Configuration
Update `appsettings.json` with your MongoDB connection details:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "StockMarketDb"
  },
  "FinnhubSettings": {
    "ApiKey": "YOUR_FINNHUB_API_KEY"
  }
}
```

### 2. Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# For development mode
npm start

# For production build
npm run build
```

## Running the Application

### Development Mode

#### Backend
```bash
cd stockmarket-agent
dotnet run
```
The backend will start on:
- HTTP: http://localhost:5269
- HTTPS: https://localhost:7056
- Swagger UI: http://localhost:5269/swagger

#### Frontend (Development)
```bash
cd frontend
npm start
```
The frontend will start on: http://localhost:3000

### Production Mode

#### Build Frontend
```bash
cd frontend
npm run build
```

#### Run Backend (serves both API and frontend)
```bash
cd stockmarket-agent
dotnet run
```
The backend will serve the frontend build from the `frontend/build` directory and the API from the same port.

## API Endpoints

### Companies
- `GET /api/companies` - Get all companies (optional filters: marketCapCategory, sector)
- `GET /api/companies/{id}` - Get company by ID
- `POST /api/companies` - Create new company
- `POST /api/companies/bulk` - Bulk create companies
- `PUT /api/companies/{id}` - Update company
- `DELETE /api/companies/{id}` - Soft delete company
- `GET /api/companies/symbols` - Get company symbols
- `GET /api/companies/sectors` - Get all sectors
- `GET /api/companies/by-sector` - Get companies grouped by sector

### Stocks
- `GET /api/stocks` - Get stocks with pagination and filtering
  - Query parameters: trend, sector, marketCapCategory, companyName, condition (AND/OR), page, pageSize
- `GET /api/stocks/{symbol}` - Get stock by symbol
- `GET /api/stocks/company-names` - Get all company names
- `GET /api/stocks/{symbol}/technical-analysis` - Get technical analysis
- `GET /api/stocks/compare?symbols=SYMBOL1,SYMBOL2` - Compare stocks
- `GET /api/stocks/sector-analysis` - Get sector analysis
- `GET /api/stocks/multibagger` - Get multibagger stocks
- `GET /api/stocks/valuable` - Get valuable stocks

### Options
- `GET /api/options` - Get options data
- `GET /api/options/{symbol}` - Get options by symbol

### Health
- `GET /api/health` - Health check endpoint

## Database Schema

### MongoDB Collections

#### StockData Collection
```javascript
{
  id: ObjectId,
  Symbol: string,
  CompanyName: string,
  Price: double,
  Week52High: double,
  Week52Low: double,
  DiscountFromHigh: double,
  MarketCapCategory: string,
  BuyPrice: double,
  TargetPrice: double,
  Trend: string,
  Volume: long,
  Sector: string,
  Industry: string,
  PriceChangePercentage: double,
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  // Technical indicators
  PreviousClose: double?,
  PriceChange: double?,
  High: double?,
  Low: double?,
  Open: double?,
  Close: double?,
  LastUpdated: DateTime?,
  RSI: double?,
  MACD: double?,
  VolumeSMA: double?,
  SupportLevel: double?,
  ResistanceLevel: double?,
  Volatility: double?,
  Momentum: double?,
  AnalysisDate: DateTime?
}
```

### InMemory Database Tables

#### Companies Table
```csharp
public class Company
{
  public int Id { get; set; }
  public string Symbol { get; set; }
  public string Name { get; set; }
  public string Sector { get; set; }
  public string Industry { get; set; }
  public string MarketCapCategory { get; set; }
  public double BasePrice { get; set; }
  public double PERatio { get; set; }
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
```

## Technical Analysis Algorithms

### Buy Price Calculation
The buy price is calculated considering:
- RSI (Relative Strength Index) levels
- Support levels
- Volatility
- MACD trend
- Discount from 52-week high
- Price momentum

### Target Price Calculation
The target price is calculated considering:
- RSI levels
- Resistance levels
- Volatility
- MACD trend
- Market cap category
- Discount from 52-week high
- 52-week high caps

### Technical Indicators
- **RSI**: Calculated using 14-period historical close prices
- **MACD**: Calculated based on price change percentage
- **Volatility**: Calculated using high, low, and close prices
- **Support/Resistance**: Derived from daily high/low prices
- **Momentum**: Based on price change percentage

## Project Structure

```
stockmarket-agent/
├── Controllers/
│   ├── CompaniesController.cs
│   ├── StocksController.cs
│   ├── OptionsController.cs
│   └── HealthController.cs
├── Services/
│   ├── StockAnalysisService.cs
│   ├── StockDataFetcherService.cs
│   ├── MongoStockService.cs
│   ├── StockDataService.cs
│   ├── OptionsDataService.cs
│   ├── FinnhubService.cs
│   └── DailyStockDataUpdateService.cs
├── Models/
│   ├── StockData.cs
│   ├── Company.cs
│   └── OptionsData.cs
├── Data/
│   ├── StockMarketDbContext.cs
│   └── DbSeeder.cs
├── Settings/
│   └── MongoDbSettings.cs
├── frontend/
│   ├── public/
│   │   └── index.html
│   ├── src/
│   │   ├── components/
│   │   │   ├── StockTable.jsx
│   │   │   ├── Filters.jsx
│   │   │   └── OptionsTable.jsx
│   │   ├── App.jsx
│   │   ├── App.css
│   │   ├── index.jsx
│   │   └── index.css
│   └── package.json
├── Program.cs
├── appsettings.json
└── stockmarket-agent.csproj
```

## Performance Optimization

### Caching
- Backend: IMemoryCache with 5-minute duration
- Frontend: Component-level caching with 5-minute expiry
- Reduces API calls and improves response times

### Pagination
- Stocks API supports pagination (default: 100 per page)
- Reduces data transfer and improves UI performance
- Configurable page size

## Security Considerations

### MongoDB Credentials
- Store connection strings in environment variables for production
- Use strong authentication mechanisms
- Implement IP whitelisting if possible
- Regularly rotate credentials

### API Security
- Implement authentication/authorization for production
- Use HTTPS in production environments
- Implement rate limiting
- Validate and sanitize all inputs

### CORS Configuration
- The current setup allows all origins (AllowAll policy)
- Restrict to specific domains in production
- Implement proper CORS policies

## Troubleshooting

### MongoDB Connection Issues
- Verify connection string in appsettings.json
- Ensure MongoDB server is accessible
- Check authentication credentials
- Verify replica set configuration

### Frontend Cannot Connect to Backend
- Ensure backend is running on correct port (5269)
- Check CORS configuration in backend
- Verify API_URL constant in frontend components

### Stock Data Not Updating
- Check background service logs
- Verify Yahoo Finance API accessibility
- Ensure MongoDB connection is working
- Check scheduled service configuration

### Build Errors
- Ensure .NET 11.0 SDK is installed
- Run `dotnet restore` to restore packages
- Check for missing NuGet packages
- Verify Node.js version for frontend

## License

This project is for educational and analysis purposes. Stock market data is provided by Yahoo Finance and may have delays. Always verify data from official sources before making investment decisions.
