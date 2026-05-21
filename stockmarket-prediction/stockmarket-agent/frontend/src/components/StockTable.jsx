import React from 'react';
import './StockTable.css';

function StockTable({ stocks, sorting, onSort }) {
  const getTrendRowStyle = (trend) => {
    switch (trend) {
      case 'Bullish':
        return { backgroundColor: '#f1f8f9', color: '#2c3e50' }; // Very light green-gray
      case 'Bearish':
        return { backgroundColor: '#fef9f9', color: '#2c3e50' }; // Very light red-gray
      case 'Sideways':
        return { backgroundColor: '#fefaf5', color: '#2c3e50' }; // Very light orange-gray
      default:
        return { backgroundColor: '#ffffff', color: '#2c3e50' }; // White
    }
  };

  const formatNumber = (num) => {
    if (num === null || num === undefined) return '-';
    return num.toLocaleString('en-IN', { maximumFractionDigits: 2 });
  };

  const handleSort = (sortBy) => {
    if (onSort) {
      onSort(sortBy);
    }
  };

  const getSortIcon = (column) => {
    if (sorting.sortBy !== column) return '';
    return sorting.sortOrder === 'asc' ? ' ↑' : ' ↓';
  };

  return (
    <div className="stock-table-container">
      <h3>Stock Analysis Results ({stocks.length} stocks)</h3>
      <div className="table-wrapper">
        <table className="stock-table">
          <thead>
            <tr>
              <th onClick={() => handleSort('symbol')} className="sortable">Symbol{getSortIcon('symbol')}</th>
              <th onClick={() => handleSort('companyname')} className="sortable">Company Name{getSortIcon('companyname')}</th>
              <th onClick={() => handleSort('price')} className="sortable">Price{getSortIcon('price')}</th>
              <th onClick={() => handleSort('change')} className="sortable">Change %{getSortIcon('change')}</th>
              <th onClick={() => handleSort('trend')} className="sortable">Trend{getSortIcon('trend')}</th>
              <th onClick={() => handleSort('buyprice')} className="sortable">Buy Price{getSortIcon('buyprice')}</th>
              <th onClick={() => handleSort('targetprice')} className="sortable">Target Price{getSortIcon('targetprice')}</th>
              <th onClick={() => handleSort('week52high')} className="sortable">52W High{getSortIcon('week52high')}</th>
              <th onClick={() => handleSort('week52low')} className="sortable">52W Low{getSortIcon('week52low')}</th>
              <th onClick={() => handleSort('discountfromhigh')} className="sortable">52W Discount %{getSortIcon('discountfromhigh')}</th>
              <th onClick={() => handleSort('volume')} className="sortable">Volume{getSortIcon('volume')}</th>
              <th onClick={() => handleSort('rsi')} className="sortable">RSI{getSortIcon('rsi')}</th>
              <th onClick={() => handleSort('sector')} className="sortable">Sector{getSortIcon('sector')}</th>
              <th onClick={() => handleSort('marketcap')} className="sortable">Market Cap{getSortIcon('marketcap')}</th>
            </tr>
          </thead>
          <tbody>
            {stocks.map((stock) => (
              <tr key={stock.symbol} style={getTrendRowStyle(stock.trend)}>
                <td className="symbol">{stock.symbol}</td>
                <td className="company-name">{stock.companyName}</td>
                <td className="price">₹{formatNumber(stock.price)}</td>
                <td className={`change ${stock.priceChangePercentage >= 0 ? 'positive' : 'negative'}`}>
                  {stock.priceChangePercentage ? formatNumber(stock.priceChangePercentage) + '%' : '-'}
                </td>
                <td className="trend">
                  {stock.trend}
                </td>
                <td className="buy-price">₹{formatNumber(stock.buyPrice)}</td>
                <td className="target-price">₹{formatNumber(stock.targetPrice)}</td>
                <td className="week52-high">₹{formatNumber(stock.week52High)}</td>
                <td className="week52-low">₹{formatNumber(stock.week52Low)}</td>
                <td className="discount-from-high">
                  {stock.discountFromHigh ? formatNumber(stock.discountFromHigh) + '%' : '-'}
                </td>
                <td className="volume">
                  {stock.volume ? formatNumber(stock.volume) : '-'}
                </td>
                <td className="rsi">{stock.rsi ? formatNumber(stock.rsi) : '-'}</td>
                <td className="sector">{stock.sector}</td>
                <td className="market-cap">{stock.marketCapCategory}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default StockTable;
