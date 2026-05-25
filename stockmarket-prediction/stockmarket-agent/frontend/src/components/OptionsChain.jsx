import React, { useState, useEffect, useCallback } from 'react';
import './OptionsChain.css';

function OptionsChain({ symbol }) {
  const [optionsChain, setOptionsChain] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  // Modal state
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState('');
  const [selectedOption, setSelectedOption] = useState(null);
  const [orderPrice, setOrderPrice] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [orderType, setOrderType] = useState('intraday');
  const [orderStatus, setOrderStatus] = useState('');

  const fetchOptionsChain = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await fetch(`http://localhost:5088/api/options/${symbol}/chain`);
      if (response.ok) {
        const data = await response.json();
        setOptionsChain(data || []);
      } else {
        setError('Failed to fetch options chain');
      }
    } catch (err) {
      setError('Error fetching options chain: ' + err.message);
    } finally {
      setLoading(false);
    }
  }, [symbol]);

  useEffect(() => {
    if (symbol) {
      fetchOptionsChain();
    }
  }, [symbol, fetchOptionsChain]);

  const formatNumber = (num) => {
    if (num === null || num === undefined) return '-';
    return num.toLocaleString('en-IN', { maximumFractionDigits: 2 });
  };

  const handleBuyClick = (option, optionType) => {
    const optionData = optionType === 'CE' ? option.CE : option.PE;
    if (!optionData) return;
    
    setSelectedOption({ ...option, optionType });
    setModalType('buy');
    setOrderPrice(Math.floor(optionData.LastPrice || 0).toString());
    setQuantity('1');
    setOrderType('intraday');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleSellClick = (option, optionType) => {
    const optionData = optionType === 'CE' ? option.CE : option.PE;
    if (!optionData) return;
    
    setSelectedOption({ ...option, optionType });
    setModalType('sell');
    setOrderPrice(Math.floor(optionData.LastPrice || 0).toString());
    setQuantity('1');
    setOrderType('intraday');
    setOrderStatus('');
    setModalOpen(true);
  };

  const handleOrderSubmit = async () => {
    try {
      const product = orderType === 'intraday' ? 'MIS' : 'CNC';
      const tradingSymbol = `${symbol}${selectedOption.StrikePrice}${selectedOption.optionType}`;
      
      const orderData = {
        symbol: symbol,
        tradingSymbol: tradingSymbol,
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
    <div className="options-chain-container">
      <div className="chain-header">
        <h3>Options Chain - {symbol}</h3>
        <button className="refresh-button" onClick={fetchOptionsChain} disabled={loading}>
          {loading ? 'Loading...' : 'Refresh'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {optionsChain.length > 0 && (
        <div className="chain-table-wrapper">
          <table className="chain-table">
            <thead>
              <tr>
                <th>Strike Price</th>
                <th colSpan="4">Call (CE)</th>
                <th colSpan="4">Put (PE)</th>
              </tr>
              <tr>
                <th></th>
                <th>Last Price</th>
                <th>OI</th>
                <th>Volume</th>
                <th>Actions</th>
                <th>Actions</th>
                <th>Last Price</th>
                <th>OI</th>
                <th>Volume</th>
              </tr>
            </thead>
            <tbody>
              {optionsChain.map((option, index) => (
                <tr key={index}>
                  <td className="strike-price">{formatNumber(option.StrikePrice)}</td>
                  
                  {/* CE Data */}
                  <td className="price">{formatNumber(option.CE?.LastPrice)}</td>
                  <td className="oi">{formatNumber(option.CE?.OpenInterest)}</td>
                  <td className="volume">{formatNumber(option.CE?.TotalTradedVolume)}</td>
                  <td className="actions">
                    <button 
                      className="buy-button" 
                      onClick={() => handleBuyClick(option, 'CE')}
                      disabled={!option.CE}
                    >
                      Buy
                    </button>
                    <button 
                      className="sell-button" 
                      onClick={() => handleSellClick(option, 'CE')}
                      disabled={!option.CE}
                    >
                      Sell
                    </button>
                  </td>
                  
                  {/* PE Data */}
                  <td className="actions">
                    <button 
                      className="buy-button" 
                      onClick={() => handleBuyClick(option, 'PE')}
                      disabled={!option.PE}
                    >
                      Buy
                    </button>
                    <button 
                      className="sell-button" 
                      onClick={() => handleSellClick(option, 'PE')}
                      disabled={!option.PE}
                    >
                      Sell
                    </button>
                  </td>
                  <td className="price">{formatNumber(option.PE?.LastPrice)}</td>
                  <td className="oi">{formatNumber(option.PE?.OpenInterest)}</td>
                  <td className="volume">{formatNumber(option.PE?.TotalTradedVolume)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>{modalType === 'buy' ? 'Buy Order' : 'Sell Order'}</h3>
            <div className="modal-info">
              <p><strong>Symbol:</strong> {symbol}</p>
              <p><strong>Strike Price:</strong> {formatNumber(selectedOption?.StrikePrice)}</p>
              <p><strong>Option Type:</strong> {selectedOption?.optionType}</p>
              <p><strong>Live Price:</strong> {formatNumber(selectedOption?.optionType === 'CE' ? selectedOption.CE?.LastPrice : selectedOption.PE?.LastPrice)}</p>
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

export default OptionsChain;
