import { useState, useEffect } from 'react';
import { ShoppingCart, Plus, Filter, Star } from 'lucide-react';
import { shoppingApi } from '../services/api';

export default function Shopping({ onCartChange }) {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [cart, setCart] = useState([]);
  const [orders, setOrders] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [showCheckout, setShowCheckout] = useState(false);
  const [showCartReview, setShowCartReview] = useState(false);
  const [newProduct, setNewProduct] = useState({
    name: '',
    description: '',
    price: '',
    categoryName: '',
    stock: '',
    seller: '',
    imageUrl: ''
  });

  useEffect(() => {
    loadProducts();
    loadCategories();
    loadCart();
    loadOrders();
  }, []);

  const loadProducts = async () => {
    try {
      const response = await shoppingApi.getProducts();
      setProducts(response.data.filter(p => p.status === 'Active'));
    } catch (error) {
      console.error('Error loading products:', error);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await shoppingApi.getCategories();
      setCategories(response.data);
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  };

  const loadCart = async () => {
    try {
      const userId = localStorage.getItem('userId');
      if (userId) {
        const response = await shoppingApi.getCart(userId);
        setCart(response.data);
      }
    } catch (error) {
      console.error('Error loading cart:', error);
    }
  };

  const loadOrders = async () => {
    try {
      const userId = localStorage.getItem('userId');
      if (userId) {
        const response = await shoppingApi.getUserOrders(userId);
        setOrders(response.data);
      }
    } catch (error) {
      console.error('Error loading orders:', error);
    }
  };

  const handleAddToCart = async (product) => {
    try {
      const userId = localStorage.getItem('userId');
      if (!userId) {
        alert('Please login to add items to cart');
        return;
      }
      await shoppingApi.addToCart({
        userId,
        productId: product.id,
        quantity: 1
      });
      loadCart();
      onCartChange();
      alert('Product added to cart!');
    } catch (error) {
      console.error('Error adding to cart:', error);
    }
  };

  const handleAddProduct = async (e) => {
    e.preventDefault();
    try {
      await shoppingApi.createProduct({
        ...newProduct,
        price: parseFloat(newProduct.price),
        stock: parseInt(newProduct.stock),
        rating: 0,
        reviewCount: 0
      });
      loadProducts();
      setNewProduct({
        name: '',
        description: '',
        price: '',
        categoryName: '',
        stock: '',
        seller: '',
        imageUrl: ''
      });
      alert('Product created successfully!');
    } catch (error) {
      console.error('Error creating product:', error);
    }
  };

  const handleCategoryChange = (category) => {
    setSelectedCategory(category);
  };

  const filteredProducts = selectedCategory === 'All'
    ? products
    : products.filter(p => p.categoryName === selectedCategory);

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4">
        <h1 className="text-4xl font-bold text-gray-800 mb-8">Shopping</h1>

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
                {category.name}
              </button>
            ))}
          </div>
        </div>

        {/* Products Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {filteredProducts.map((product) => (
            <div key={product.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
              <img src={product.imageUrl} alt={product.name} className="w-full h-48 object-cover" />
              <div className="p-4">
                <h3 className="font-semibold text-gray-800 mb-2">{product.name}</h3>
                <p className="text-sm text-gray-600 mb-2 line-clamp-2">{product.description}</p>
                <div className="flex items-center mb-2">
                  <Star size={16} className="text-yellow-500 fill-current" />
                  <span className="ml-1 text-sm text-gray-600">{product.rating} ({product.reviewCount})</span>
                </div>
                <div className="flex justify-between items-center">
                  <p className="text-lg font-bold text-blue-600">${product.price.toFixed(2)}</p>
                  <button
                    onClick={() => handleAddToCart(product)}
                    className="bg-blue-600 text-white p-2 rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    <ShoppingCart size={20} />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Cart Review */}
        {cart.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Your Cart ({cart.length} items)</h2>
            <div className="space-y-4">
              {cart.map((item) => {
                const product = products.find(p => p.id === item.productId);
                return (
                  <div key={item.id} className="flex justify-between items-center border-b pb-4">
                    <div>
                      <p className="font-medium text-gray-800">{product?.name || 'Product'}</p>
                      <p className="text-sm text-gray-600">Quantity: {item.quantity}</p>
                    </div>
                    <p className="font-bold text-blue-600">${(product?.price || 0) * item.quantity}</p>
                  </div>
                );
              })}
            </div>
            <button
              onClick={() => setShowCartReview(true)}
              className="mt-4 w-full bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
            >
              Proceed to Checkout
            </button>
          </div>
        )}

        {/* Order History */}
        {orders.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Order History</h2>
            <div className="overflow-x-auto">
              <table className="min-w-full">
                <thead>
                  <tr className="bg-gray-50">
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Order ID</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Total</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Status</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Date</th>
                  </tr>
                </thead>
                <tbody>
                  {orders.map((order) => (
                    <tr key={order.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm text-gray-800">{order.id.substring(0, 8)}...</td>
                      <td className="px-4 py-3 text-sm text-gray-800">${order.total.toFixed(2)}</td>
                      <td className="px-4 py-3">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                          order.status === 'Delivered' ? 'bg-green-100 text-green-800' :
                          order.status === 'Shipped' ? 'bg-yellow-100 text-yellow-800' :
                          'bg-blue-100 text-blue-800'
                        }`}>
                          {order.status}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{new Date(order.createdAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Add Product Form (Admin) */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-2xl font-bold text-gray-800 mb-4">Add New Product</h2>
          <form onSubmit={handleAddProduct} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Product Name</label>
              <input
                type="text"
                value={newProduct.name}
                onChange={(e) => setNewProduct({ ...newProduct, name: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
              <textarea
                value={newProduct.description}
                onChange={(e) => setNewProduct({ ...newProduct, description: e.target.value })}
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
                  value={newProduct.price}
                  onChange={(e) => setNewProduct({ ...newProduct, price: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Stock</label>
                <input
                  type="number"
                  value={newProduct.stock}
                  onChange={(e) => setNewProduct({ ...newProduct, stock: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Category</label>
                <select
                  value={newProduct.categoryName}
                  onChange={(e) => setNewProduct({ ...newProduct, categoryName: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                >
                  <option value="">Select Category</option>
                  {categories.map((category) => (
                    <option key={category.id} value={category.name}>{category.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Seller</label>
                <input
                  type="text"
                  value={newProduct.seller}
                  onChange={(e) => setNewProduct({ ...newProduct, seller: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Image URL</label>
              <input
                type="url"
                value={newProduct.imageUrl}
                onChange={(e) => setNewProduct({ ...newProduct, imageUrl: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>
            <button
              type="submit"
              className="w-full bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center"
            >
              <Plus size={20} className="mr-2" />
              Add Product
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
