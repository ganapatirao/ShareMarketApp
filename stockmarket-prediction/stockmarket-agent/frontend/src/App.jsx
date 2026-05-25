import React, { useState, useEffect, useCallback } from 'react';
import StockTable from './components/StockTable';
import Filters from './components/Filters';
import OptionsTable from './components/OptionsTable';
import OptionsChain from './components/OptionsChain';
import './App.css';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5088/api';

function App() {
  const [stocks, setStocks] = useState([]);
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('stocks');
  const [selectedSymbol, setSelectedSymbol] = useState('NIFTY');
  const [filters, setFilters] = useState({
    trend: '',
    sector: '',
    marketCapCategory: '',
    companyName: ''
  });
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 50,
    totalCount: 0,
    totalPages: 0
  });
  const [sorting, setSorting] = useState({
    sortBy: null,
    sortOrder: 'asc'
  });

  const fetchStocks = useCallback(async () => {
    try {
      setLoading(true);
      const queryParams = new URLSearchParams();
      if (filters.trend) queryParams.append('trend', filters.trend);
      if (filters.sector) queryParams.append('sector', filters.sector);
      if (filters.marketCapCategory) queryParams.append('marketCapCategory', filters.marketCapCategory);
      if (filters.companyName) queryParams.append('companyName', filters.companyName);
      queryParams.append('page', pagination.page);
      queryParams.append('pageSize', pagination.pageSize);
      if (sorting.sortBy) {
        queryParams.append('sortBy', sorting.sortBy);
        queryParams.append('sortOrder', sorting.sortOrder);
      }

      const response = await fetch(`${API_URL}/stocks?${queryParams}`);
      const result = await response.json();
      setStocks(result.data || []);
      setPagination(prev => ({
        ...prev,
        totalCount: result.totalCount || 0,
        totalPages: result.totalPages || 0
      }));
    } catch (error) {
      console.error('Error fetching stocks:', error);
    } finally {
      setLoading(false);
    }
  }, [filters, pagination.page, pagination.pageSize, sorting]);

  useEffect(() => {
    fetchStocks();
    fetchOptions();
  }, [fetchStocks]);

  const fetchOptions = async () => {
    try {
      const response = await fetch(`${API_URL}/options`);
      const data = await response.json();
      setOptions(data);
    } catch (error) {
      console.error('Error fetching options:', error);
    }
  };

  const handleFilterChange = (newFilters) => {
    setFilters(newFilters);
    setPagination({ ...pagination, page: 1 });
  };

  const handleSort = (sortBy) => {
    if (sorting.sortBy === sortBy) {
      setSorting({
        sortBy,
        sortOrder: sorting.sortOrder === 'asc' ? 'desc' : 'asc'
      });
    } else {
      setSorting({
        sortBy,
        sortOrder: 'asc'
      });
    }
  };

  const handlePageChange = (newPage) => {
    setPagination({ ...pagination, page: newPage });
  };

  const handlePageSizeChange = (newPageSize) => {
    setPagination({ ...pagination, pageSize: newPageSize, page: 1 });
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Indian Stock Market Analysis</h1>
        <p>Real-time stock recommendations and technical analysis</p>
      </header>

      <div className="tabs">
        <button
          className={activeTab === 'stocks' ? 'active' : ''}
          onClick={() => setActiveTab('stocks')}
        >
          Stocks
        </button>
        <button
          className={activeTab === 'options' ? 'active' : ''}
          onClick={() => setActiveTab('options')}
        >
          Options
        </button>
      </div>

      {activeTab === 'stocks' && (
        <div className="stocks-section">
          <Filters filters={filters} onFilterChange={handleFilterChange} />
          {loading ? (
            <div className="loading">Loading...</div>
          ) : (
            <>
              <StockTable 
                stocks={stocks} 
                sorting={sorting}
                onSort={handleSort}
              />
              <div className="pagination">
                <div className="pagination-info">
                  Showing {((pagination.page - 1) * pagination.pageSize) + 1} to {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)} of {pagination.totalCount} stocks
                </div>
                <div className="pagination-controls">
                  <button 
                    onClick={() => handlePageChange(pagination.page - 1)}
                    disabled={pagination.page === 1}
                    className="pagination-button"
                  >
                    Previous
                  </button>
                  <span className="pagination-page">
                    Page {pagination.page} of {pagination.totalPages}
                  </span>
                  <button 
                    onClick={() => handlePageChange(pagination.page + 1)}
                    disabled={pagination.page === pagination.totalPages}
                    className="pagination-button"
                  >
                    Next
                  </button>
                  <select 
                    value={pagination.pageSize}
                    onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
                    className="pagination-page-size"
                  >
                    <option value="25">25 per page</option>
                    <option value="50">50 per page</option>
                    <option value="100">100 per page</option>
                  </select>
                </div>
              </div>
            </>
          )}
        </div>
      )}

      {activeTab === 'options' && (
        <div className="options-section">
          <div className="symbol-selector">
            <label>Select Symbol:</label>
            <select 
              value={selectedSymbol} 
              onChange={(e) => setSelectedSymbol(e.target.value)}
            >
              <option value="NIFTY">NIFTY</option>
              <option value="BANKNIFTY">BANKNIFTY</option>
              <option value="SENSEX">SENSEX</option>
              <option value="CRUDE">CRUDE</option>
            </select>
          </div>
          <OptionsTable options={options} />
          <OptionsChain symbol={selectedSymbol} />
        </div>
      )}
    </div>
  );
}

export default App;
