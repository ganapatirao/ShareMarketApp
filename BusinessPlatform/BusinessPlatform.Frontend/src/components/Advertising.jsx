import { useState, useEffect } from 'react';
import { Plus, Filter, MapPin, MessageCircle } from 'lucide-react';
import { advertisingApi } from '../services/api';

export default function Advertising() {
  const [ads, setAds] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [showPostAd, setShowPostAd] = useState(false);
  const [newAd, setNewAd] = useState({
    title: '',
    description: '',
    price: '',
    categoryName: '',
    location: '',
    condition: '',
    imageUrl: ''
  });

  useEffect(() => {
    loadAds();
    loadCategories();
  }, []);

  const loadAds = async () => {
    try {
      const response = await advertisingApi.getAds();
      setAds(response.data.filter(a => a.status === 'Active'));
    } catch (error) {
      console.error('Error loading ads:', error);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await advertisingApi.getAdCategories();
      setCategories(response.data);
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  };

  const handlePostAd = async (e) => {
    e.preventDefault();
    try {
      const userId = localStorage.getItem('userId');
      const userName = localStorage.getItem('userName');
      await advertisingApi.createAd({
        ...newAd,
        price: parseFloat(newAd.price),
        sellerId: userId,
        sellerName: userName || 'Anonymous'
      });
      loadAds();
      setNewAd({
        title: '',
        description: '',
        price: '',
        categoryName: '',
        location: '',
        condition: '',
        imageUrl: ''
      });
      setShowPostAd(false);
      alert('Ad posted successfully!');
    } catch (error) {
      console.error('Error posting ad:', error);
    }
  };

  const handleCategoryChange = (category) => {
    setSelectedCategory(category);
  };

  const handleRespond = (adId) => {
    const message = prompt('Enter your message:');
    if (message) {
      alert('Response sent! (Demo mode)');
    }
  };

  const filteredAds = selectedCategory === 'All'
    ? ads
    : ads.filter(a => a.categoryName === selectedCategory);

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-4xl font-bold text-gray-800">Classified Ads</h1>
          <button
            onClick={() => setShowPostAd(true)}
            className="bg-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center"
          >
            <Plus size={20} className="mr-2" />
            Post Ad
          </button>
        </div>

        {/* Category Filter */}
        <div className="mb-8">
          <div className="flex items-center space-x-4 overflow-x-auto pb-2">
            <button
              onClick={() => handleCategoryChange('All')}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                selectedCategory === 'All'
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 hover:bg-gray-100'
              }`}
            >
              All
            </button>
            {categories.map((category) => (
              <button
                key={category.id}
                onClick={() => handleCategoryChange(category.name)}
                className={`px-4 py-2 rounded-lg font-medium transition-colors whitespace-nowrap ${
                  selectedCategory === category.name
                    ? 'bg-blue-600 text-white'
                    : 'bg-white text-gray-700 hover:bg-gray-100'
                }`}
              >
                {category.emoji} {category.name}
              </button>
            ))}
          </div>
        </div>

        {/* Ads Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {filteredAds.map((ad) => (
            <div key={ad.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
              <img src={ad.imageUrl} alt={ad.title} className="w-full h-48 object-cover" />
              <div className="p-4">
                <h3 className="font-semibold text-gray-800 mb-2">{ad.title}</h3>
                <p className="text-sm text-gray-600 mb-2 line-clamp-2">{ad.description}</p>
                <div className="flex items-center mb-2">
                  <MapPin size={16} className="text-gray-500" />
                  <span className="ml-1 text-sm text-gray-600">{ad.location}</span>
                </div>
                <div className="flex justify-between items-center mb-3">
                  <p className="text-lg font-bold text-green-600">${ad.price.toFixed(2)}</p>
                  <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">{ad.condition}</span>
                </div>
                <button
                  onClick={() => handleRespond(ad.id)}
                  className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors flex items-center justify-center"
                >
                  <MessageCircle size={16} className="mr-2" />
                  Respond
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Post Ad Modal */}
        {showPostAd && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg max-w-md w-full mx-4 p-6">
              <h2 className="text-2xl font-bold text-gray-800 mb-4">Post New Ad</h2>
              <form onSubmit={handlePostAd} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Title</label>
                  <input
                    type="text"
                    value={newAd.title}
                    onChange={(e) => setNewAd({ ...newAd, title: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                  <textarea
                    value={newAd.description}
                    onChange={(e) => setNewAd({ ...newAd, description: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    rows="3"
                    required
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Price</label>
                    <input
                      type="number"
                      step="0.01"
                      value={newAd.price}
                      onChange={(e) => setNewAd({ ...newAd, price: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Category</label>
                    <select
                      value={newAd.categoryName}
                      onChange={(e) => setNewAd({ ...newAd, categoryName: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    >
                      <option value="">Select Category</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.name}>{category.name}</option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Location</label>
                    <input
                      type="text"
                      value={newAd.location}
                      onChange={(e) => setNewAd({ ...newAd, location: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Condition</label>
                    <select
                      value={newAd.condition}
                      onChange={(e) => setNewAd({ ...newAd, condition: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    >
                      <option value="">Select Condition</option>
                      <option value="New">New</option>
                      <option value="Like New">Like New</option>
                      <option value="Good">Good</option>
                      <option value="Fair">Fair</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Image URL</label>
                  <input
                    type="url"
                    value={newAd.imageUrl}
                    onChange={(e) => setNewAd({ ...newAd, imageUrl: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div className="flex space-x-4">
                  <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
                  >
                    Post Ad
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowPostAd(false)}
                    className="flex-1 bg-gray-300 text-gray-700 py-3 rounded-lg font-semibold hover:bg-gray-400 transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
