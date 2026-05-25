import React, { useState } from 'react';
import './StockTable.css';

function StockTable({ stocks, sorting, onSort }) {
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState(''); // 'buy' or 'sell'
  const [selectedStock, setSelectedStock] = useState(null);
  const [orderPrice, setOrderPrice] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [orderType, setOrderType] = useState('intraday'); // 'intraday' or 'overnight'
  const [exchange, setExchange] = useState('NSE'); // 'NSE' or 'BSE'
  const [orderStatus, setOrderStatus] = useState('');

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

  const handleBuyClick = (stock) => {
    setSelectedStock(stock);
    setModalType('buy');
    setOrderPrice(stock.buyPrice?.toString() || '');
    setQuantity('1');
    setOrderType('intraday');
    setExchange('NSE');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleSellClick = (stock) => {
    setSelectedStock(stock);
    setModalType('sell');
    setOrderPrice(stock.targetPrice?.toString() || '');
    setQuantity('1');
    setOrderType('intraday');
    setExchange('NSE');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleOrderSubmit = async () => {
    try {
      const product = orderType === 'intraday' ? 'MIS' : 'CNC';
      const orderData = {
        symbol: selectedStock.symbol,
        tradingSymbol: selectedStock.symbol,
        transactionType: modalType.toUpperCase(),
        quantity: parseInt(quantity),
        price: parseFloat(orderPrice),
        exchange: exchange,
        product: product,
        orderType: 'LIMIT'
      };

      const response = await fetch('http://localhost:5088/api/kite/order', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(orderData),
      });

      const result = await response.json();
      
      if (response.ok) {
        setOrderStatus('Order placed successfully! Order ID: ' + result.data?.orderId);
      } else {
        const errorMessage = result.message || result.error_type || 'Unknown error';
        const errorDetails = result.error_type ? ` (${result.error_type})` : '';
        setOrderStatus('Order failed: ' + errorMessage + errorDetails);
        console.error('Order failed:', result);
      }
    } catch (error) {
      setOrderStatus('Error placing order: ' + error.message);
      console.error('Error placing order:', error);
    }
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setSelectedStock(null);
    setOrderPrice('');
    setQuantity('1');
    setOrderType('intraday');
    setExchange('NSE');
    setOrderStatus('');
  };

  return (
    <div className="stock-table-container">
      <h3>Stock Analysis Results ({stocks.length} stocks)</h3>
      <div className="table-wrapper">
        <table className="stock-table">
          <thead>
            <tr>
              <th onClick={() => handleSort('companyname')} className="sortable">Company Name{getSortIcon('companyname')}</th>
              <th onClick={() => handleSort('price')} className="sortable">Price{getSortIcon('price')}</th>
              <th onClick={() => handleSort('change')} className="sortable">Change %{getSortIcon('change')}</th>
              <th onClick={() => handleSort('trend')} className="sortable">Trend{getSortIcon('trend')}</th>
              <th onClick={() => handleSort('buyprice')} className="sortable">Buy Price{getSortIcon('buyprice')}</th>
              <th>Action</th>
              <th onClick={() => handleSort('targetprice')} className="sortable">Target Price{getSortIcon('targetprice')}</th>
              <th>Action</th>
              <th onClick={() => handleSort('week52high')} className="sortable">52W High{getSortIcon('week52high')}</th>
              <th onClick={() => handleSort('week52low')} className="sortable">52W Low{getSortIcon('week52low')}</th>
              <th onClick={() => handleSort('discountfromhigh')} className="sortable">52W Disc %{getSortIcon('discountfromhigh')}</th>
              <th onClick={() => handleSort('volume')} className="sortable">Volume{getSortIcon('volume')}</th>
              <th onClick={() => handleSort('rsi')} className="sortable">RSI{getSortIcon('rsi')}</th>
              <th onClick={() => handleSort('sector')} className="sortable">Sector{getSortIcon('sector')}</th>
              <th onClick={() => handleSort('marketcap')} className="sortable">Market Cap{getSortIcon('marketcap')}</th>
            </tr>
          </thead>
          <tbody>
            {stocks.map((stock) => (
              <tr key={stock.symbol} style={getTrendRowStyle(stock.trend)}>
                <td className="company-name">{stock.companyName}</td>
                <td className="price">₹{formatNumber(stock.price)}</td>
                <td className={`change ${stock.priceChangePercentage >= 0 ? 'positive' : 'negative'}`}>
                  {stock.priceChangePercentage ? formatNumber(stock.priceChangePercentage) + '%' : '-'}
                </td>
                <td className="trend">
                  {stock.trend}
                </td>
                <td className="buy-price">₹{formatNumber(stock.buyPrice)}</td>
                <td className="action-cell">
                  <button className="buy-button" onClick={() => handleBuyClick(stock)}>Buy</button>
                </td>
                <td className="target-price">₹{formatNumber(stock.targetPrice)}</td>
                <td className="action-cell">
                  <button className="sell-button" onClick={() => handleSellClick(stock)}>Sell</button>
                </td>
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

      {modalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>{modalType === 'buy' ? 'Buy Order' : 'Sell Order'}</h3>
            <div className="modal-info">
              <p><strong>Symbol:</strong> {selectedStock?.symbol}</p>
              <p><strong>Company:</strong> {selectedStock?.companyName}</p>
            </div>
            <div className="modal-form">
              <div className="form-group">
                <label>Price:</label>
                <input
                  type="number"
                  step="1"
                  value={orderPrice}
                  onChange={(e) => setOrderPrice(e.target.value)}
                />
              </div>
              <div className="form-group">
                <label>Quantity:</label>
                <input
                  type="number"
                  min="1"
                  value={quantity}
                  onChange={(e) => setQuantity(e.target.value)}
                />
              </div>
              <div className="form-group">
                <label>Order Type:</label>
                <div className="radio-group">
                  <label className="radio-label">
                    <input
                      type="radio"
                      value="intraday"
                      checked={orderType === 'intraday'}
                      onChange={(e) => setOrderType(e.target.value)}
                    />
                    <span>Intraday (MIS)</span>
                  </label>
                  <label className="radio-label">
                    <input
                      type="radio"
                      value="overnight"
                      checked={orderType === 'overnight'}
                      onChange={(e) => setOrderType(e.target.value)}
                    />
                    <span>Overnight (CNC)</span>
                  </label>
                </div>
              </div>
              <div className="form-group">
                <label>Exchange:</label>
                <div className="radio-group">
                  <label className="radio-label">
                    <input
                      type="radio"
                      value="NSE"
                      checked={exchange === 'NSE'}
                      onChange={(e) => setExchange(e.target.value)}
                    />
                    <span>NSE</span>
                  </label>
                  <label className="radio-label">
                    <input
                      type="radio"
                      value="BSE"
                      checked={exchange === 'BSE'}
                      onChange={(e) => setExchange(e.target.value)}
                    />
                    <span>BSE</span>
                  </label>
                </div>
              </div>
            </div>
            {orderStatus && (
              <div className={`order-status ${orderStatus.includes('success') ? 'success' : 'error'}`}>
                {orderStatus}
              </div>
            )}
            <div className="modal-actions">
              <button className="modal-button cancel" onClick={handleCloseModal}>Cancel</button>
              <button className="modal-button confirm" onClick={handleOrderSubmit}>
                {modalType === 'buy' ? 'Place Buy Order' : 'Place Sell Order'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default StockTable;
