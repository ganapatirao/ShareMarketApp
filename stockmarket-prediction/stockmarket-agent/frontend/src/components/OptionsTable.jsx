import React from 'react';
import './OptionsTable.css';

function OptionsTable({ options }) {
  const formatNumber = (num) => {
    if (num === null || num === undefined) return '-';
    return num.toLocaleString('en-IN', { maximumFractionDigits: 2 });
  };

  return (
    <div className="options-table-container">
      <h3>Options Chain Data ({options.length} indices)</h3>
      <div className="table-wrapper">
        <table className="options-table">
          <thead>
            <tr>
              <th>Symbol</th>
              <th>Regular Price</th>
              <th>Previous Close</th>
              <th>Change</th>
              <th>Change %</th>
              <th>Trend</th>
              <th>Support 1</th>
              <th>Resistance 1</th>
              <th>Volume</th>
              <th>Last Updated</th>
            </tr>
          </thead>
          <tbody>
            {options.map((option, index) => (
              <tr key={index}>
                <td className="symbol">{option.symbol}</td>
                <td className="price">₹{formatNumber(option.regularPrice)}</td>
                <td className="previous-close">₹{formatNumber(option.previousClose)}</td>
                <td className={`change ${option.change >= 0 ? 'positive' : 'negative'}`}>
                  ₹{formatNumber(option.change)}
                </td>
                <td className={`change-percent ${option.changePercentApp >= 0 ? 'positive' : 'negative'}`}>
                  {formatNumber(option.changePercentApp)}%
                </td>
                <td className="trend" style={{ 
                  color: option.trend === 'Bullish' ? '#4caf50' : 
                         option.trend === 'Bearish' ? '#f44336' : '#ff9800' 
                }}>
                  {option.trend}
                </td>
                <td className="support">₹{formatNumber(option.support1)}</td>
                <td className="resistance">₹{formatNumber(option.resistance1)}</td>
                <td className="volume">{formatNumber(option.volume)}</td>
                <td className="last-updated">
                  {option.lastUpdated ? new Date(option.lastUpdated).toLocaleString() : '-'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default OptionsTable;
