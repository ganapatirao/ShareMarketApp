import React, { useState } from 'react';
import './OptionsTable.css';

function OptionsTable({ options }) {
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState('');
  const [selectedOption, setSelectedOption] = useState(null);
  const [orderPrice, setOrderPrice] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [orderType, setOrderType] = useState('intraday');
  const [orderStatus, setOrderStatus] = useState('');

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

  const handleBuyClick = (option) => {
    setSelectedOption(option);
    setModalType('buy');
    setOrderPrice(Math.floor(option.regularPrice).toString());
    setQuantity('1');
    setOrderType('intraday');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleSellClick = (option) => {
    setSelectedOption(option);
    setModalType('sell');
    setOrderPrice(Math.floor(option.regularPrice).toString());
    setQuantity('1');
    setOrderType('intraday');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleOrderSubmit = async () => {
    try {
      const product = orderType === 'intraday' ? 'MIS' : 'CNC';
      const orderData = {
        symbol: selectedOption.symbol,
        tradingSymbol: selectedOption.symbol,
        transactionType: modalType.toUpperCase(),
        quantity: parseInt(quantity),
        price: parseFloat(orderPrice),
        exchange: 'NSE',
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
    setSelectedOption(null);
    setOrderPrice('');
    setQuantity('1');
    setOrderType('intraday');
    setOrderStatus('');
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
                <span className="value">{formatNumber(option.regularPrice)}</span>
              </div>
              <div className="price-row">
                <span className="label">Change:</span>
                <span className={`value ${option.change >= 0 ? 'positive' : 'negative'}`}>
                  {formatNumber(option.change)} ({formatNumber(option.changePercentApp)}%)
                </span>
              </div>
              {option.symbol === 'CRUDE' && (
                <div className="price-row">
                  <span className="label">Current Price:</span>
                  <span className="value">{formatNumber(option.regularPrice)}*95 = {formatNumber(option.regularPrice * 95)}</span>
                </div>
              )}
              <div className="price-row action-buttons">
                <button className="buy-button" onClick={() => handleBuyClick(option)}>Buy</button>
                <button className="sell-button" onClick={() => handleSellClick(option)}>Sell</button>
              </div>
            </div>

            <div className="card-section">
              <h5 className="section-title">Support Levels</h5>
              <div className="levels-grid">
                <div className="level-item">
                  <span className="level-label">S1</span>
                  <span className="level-value">{formatNumber(option.support1)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S2</span>
                  <span className="level-value">{formatNumber(option.support2)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S3</span>
                  <span className="level-value">{formatNumber(option.support3)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S4</span>
                  <span className="level-value">{formatNumber(option.support4)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">S5</span>
                  <span className="level-value">{formatNumber(option.support5)}</span>
                </div>
              </div>
            </div>

            <div className="card-section">
              <h5 className="section-title">Resistance Levels</h5>
              <div className="levels-grid">
                <div className="level-item">
                  <span className="level-label">R1</span>
                  <span className="level-value">{formatNumber(option.resistance1)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R2</span>
                  <span className="level-value">{formatNumber(option.resistance2)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R3</span>
                  <span className="level-value">{formatNumber(option.resistance3)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R4</span>
                  <span className="level-value">{formatNumber(option.resistance4)}</span>
                </div>
                <div className="level-item">
                  <span className="level-label">R5</span>
                  <span className="level-value">{formatNumber(option.resistance5)}</span>
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

      {modalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>{modalType === 'buy' ? 'Buy Order' : 'Sell Order'}</h3>
            <div className="modal-info">
              <p><strong>Symbol:</strong> {selectedOption?.symbol}</p>
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

export default OptionsTable;
