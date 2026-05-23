import React from 'react';
import './OptionsTable.css';

function OptionsTable({ options }) {
  const formatNumber = (num) => {
    if (num === null || num === undefined) return '-';
    return num.toLocaleString('en-IN', { maximumFractionDigits: 2 });
  };

  const formatDate = (date) => {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-IN', { 
      day: '2-digit', 
      month: 'short', 
      year: 'numeric' 
    });
  };

  return (
    <div className="options-container">
      <h3>Options Chain Data ({options.length} indices)</h3>
      <div className="options-grid">
        {options.map((option, index) => (
          <div key={index} className="option-card">
            <div className="card-header">
              <h4 className="symbol">{option.symbol}</h4>
              <span className={`trend-badge ${option.trend?.toLowerCase()}`}>
                {option.trend}
              </span>
            </div>
            
            <div className="card-price-info">
              <div className="price-row">
                <span className="label">Price:</span>
                <span className="value">₹{formatNumber(option.regularPrice)}</span>
              </div>
              <div className="price-row">
                <span className="label">Change:</span>
                <span className={`value ${option.change >= 0 ? 'positive' : 'negative'}`}>
                  ₹{formatNumber(option.change)} ({formatNumber(option.changePercentApp)}%)
                </span>
              </div>
            </div>

            <div className="card-section">
              <h5 className="section-title">Support Levels</h5>
              <div className="levels-grid">
                <div className="level-item">
                  <span className="level-label">S1</span>
                  <span className="level-value">₹{formatNumber(option.support1)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S2</span>
                  <span className="level-value">₹{formatNumber(option.support2)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S3</span>
                  <span className="level-value">₹{formatNumber(option.support3)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S4</span>
                  <span className="level-value">₹{formatNumber(option.support4)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S5</span>
                  <span className="level-value">₹{formatNumber(option.support5)}</span>
                </div>
              </div>
            </div>

            <div className="card-section">
              <h5 className="section-title">Resistance Levels</h5>
              <div className="levels-grid">
                <div className="level-item">
                  <span className="level-label">R1</span>
                  <span className="level-value">₹{formatNumber(option.resistance1)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R2</span>
                  <span className="level-value">₹{formatNumber(option.resistance2)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R3</span>
                  <span className="level-value">₹{formatNumber(option.resistance3)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R4</span>
                  <span className="level-value">₹{formatNumber(option.resistance4)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R5</span>
                  <span className="level-value">₹{formatNumber(option.resistance5)}</span>
                </div>
              </div>
            </div>

            <div className="card-footer">
              <div className="footer-item">
                <span className="footer-label">OI:</span>
                <span className="footer-value">{formatNumber(option.openInterest)}</span>
              </div>
              <div className="footer-item">
                <span className="footer-label">Volume:</span>
                <span className="footer-value">{formatNumber(option.volume)}</span>
              </div>
              <div className="footer-item">
                <span className="footer-label">Expiry:</span>
                <span className="footer-value">{formatDate(option.expiryDate)}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default OptionsTable;
