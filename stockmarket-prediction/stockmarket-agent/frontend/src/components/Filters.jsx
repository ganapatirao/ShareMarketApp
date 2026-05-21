import React, { useState, useEffect } from 'react';
import './Filters.css';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5088/api';

function Filters({ filters, onFilterChange }) {
  const [sectors, setSectors] = useState([]);
  const [marketCaps, setMarketCaps] = useState([]);

  useEffect(() => {
    fetchSectors();
    fetchMarketCaps();
  }, []);

  const fetchSectors = async () => {
    try {
      const response = await fetch(`${API_URL}/stocks/sectors`);
      const data = await response.json();
      setSectors(data);
    } catch (error) {
      console.error('Error fetching sectors:', error);
    }
  };

  const fetchMarketCaps = async () => {
    try {
      const response = await fetch(`${API_URL}/stocks/market-caps`);
      const data = await response.json();
      setMarketCaps(data);
    } catch (error) {
      console.error('Error fetching market caps:', error);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    onFilterChange({ ...filters, [name]: value });
  };

  const handleReset = () => {
    onFilterChange({
      trend: '',
      sector: '',
      marketCapCategory: '',
      companyName: ''
    });
  };

  return (
    <div className="filters">
      <h3>Filter Stocks</h3>
      <div className="filter-grid">
        <div className="filter-item">
          <label>Trend:</label>
          <select
            name="trend"
            value={filters.trend}
            onChange={handleChange}
          >
            <option value="">All</option>
            <option value="Bullish">Bullish</option>
            <option value="Bearish">Bearish</option>
            <option value="Sideways">Sideways</option>
          </select>
        </div>

        <div className="filter-item">
          <label>Sector:</label>
          <select
            name="sector"
            value={filters.sector}
            onChange={handleChange}
          >
            <option value="">All</option>
            {sectors.map(sector => (
              <option key={sector} value={sector}>{sector}</option>
            ))}
          </select>
        </div>

        <div className="filter-item">
          <label>Market Cap:</label>
          <select
            name="marketCapCategory"
            value={filters.marketCapCategory}
            onChange={handleChange}
          >
            <option value="">All</option>
            {marketCaps.map(cap => (
              <option key={cap} value={cap}>{cap}</option>
            ))}
          </select>
        </div>

        <div className="filter-item">
          <label>Company Name:</label>
          <input
            type="text"
            name="companyName"
            value={filters.companyName}
            onChange={handleChange}
            placeholder="Search company..."
          />
        </div>

        <button className="reset-btn" onClick={handleReset}>
          Reset Filters
        </button>
      </div>
    </div>
  );
}

export default Filters;
