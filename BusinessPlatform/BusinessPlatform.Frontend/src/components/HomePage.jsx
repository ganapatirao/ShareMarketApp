import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { ShoppingCart, Briefcase, Calendar, Plane, Star, MapPin, ArrowRight } from 'lucide-react';
import { shoppingApi, advertisingApi, recruitmentApi, bookingApi } from '../services/api';

export default function HomePage() {
  const [products, setProducts] = useState([]);
  const [ads, setAds] = useState([]);
  const [jobs, setJobs] = useState([]);
  const [transports, setTransports] = useState([]);
  const [packages, setPackages] = useState([]);
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const [productsRes, adsRes, jobsRes, transportsRes, packagesRes, moviesRes] = await Promise.all([
        shoppingApi.getProducts(),
        advertisingApi.getAds(),
        recruitmentApi.getJobs(),
        bookingApi.getTransports(),
        bookingApi.getPackages(),
        bookingApi.getMovies()
      ]);

      setProducts(productsRes.data.filter(p => p.status === 'Active').slice(0, 4));
      setAds(adsRes.data.filter(a => a.status === 'Active').slice(0, 4));
      setJobs(jobsRes.data.filter(j => j.status === 'Active').slice(0, 4));
      setTransports(transportsRes.data.filter(t => t.status === 'Active').slice(0, 3));
      setPackages(packagesRes.data.filter(p => p.status === 'Active').slice(0, 3));
      setMovies(moviesRes.data.filter(m => m.status === 'Active').slice(0, 4));
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-blue-600 to-purple-600 text-white py-20">
        <div className="max-w-7xl mx-auto px-4">
          <div className="text-center">
            <h1 className="text-5xl font-bold mb-6">Welcome to Business Platform</h1>
            <p className="text-xl mb-8 opacity-90">Your one-stop destination for Shopping, Advertising, Recruitment, and Booking</p>
            <div className="flex justify-center space-x-4">
              <Link to="/shopping" className="bg-white text-blue-600 px-8 py-3 rounded-lg font-semibold hover:bg-gray-100 transition-colors">
                Start Shopping
              </Link>
              <Link to="/advertising" className="bg-transparent border-2 border-white text-white px-8 py-3 rounded-lg font-semibold hover:bg-white hover:text-blue-600 transition-colors">
                Post an Ad
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Products */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Featured Products</h2>
            <Link to="/shopping" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {products.map((product) => (
              <div key={product.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
                <img src={product.imageUrl} alt={product.name} className="w-full h-48 object-cover" />
                <div className="p-4">
                  <h3 className="font-semibold text-gray-800 mb-2">{product.name}</h3>
                  <div className="flex items-center mb-2">
                    <Star size={16} className="text-yellow-500 fill-current" />
                    <span className="ml-1 text-sm text-gray-600">{product.rating} ({product.reviewCount})</span>
                  </div>
                  <p className="text-lg font-bold text-blue-600">${product.price.toFixed(2)}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Ads */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Featured Ads</h2>
            <Link to="/advertising" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {ads.map((ad) => (
              <div key={ad.id} className="bg-gray-50 rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
                <img src={ad.imageUrl} alt={ad.title} className="w-full h-48 object-cover" />
                <div className="p-4">
                  <h3 className="font-semibold text-gray-800 mb-2">{ad.title}</h3>
                  <div className="flex items-center mb-2">
                    <MapPin size={16} className="text-gray-500" />
                    <span className="ml-1 text-sm text-gray-600">{ad.location}</span>
                  </div>
                  <p className="text-lg font-bold text-green-600">${ad.price.toFixed(2)}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Jobs */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Featured Jobs</h2>
            <Link to="/recruitment" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {jobs.map((job) => (
              <div key={job.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow p-6">
                <div className="flex items-center mb-3">
                  <Briefcase size={24} className="text-blue-600" />
                  <h3 className="font-semibold text-gray-800 ml-2">{job.title}</h3>
                </div>
                <p className="text-gray-600 mb-2">{job.company}</p>
                <div className="flex items-center mb-2">
                  <MapPin size={16} className="text-gray-500" />
                  <span className="ml-1 text-sm text-gray-600">{job.location}</span>
                </div>
                <p className="text-lg font-bold text-green-600">{job.salary}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Transport Options */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Transport Options</h2>
            <Link to="/booking" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {transports.map((transport) => (
              <div key={transport.id} className="bg-gradient-to-br from-blue-50 to-purple-50 rounded-lg shadow-md hover:shadow-lg transition-shadow p-6">
                <div className="flex items-center mb-4">
                  <Calendar size={32} className="text-blue-600" />
                  <div className="ml-3">
                    <h3 className="font-semibold text-gray-800">{transport.name}</h3>
                    <p className="text-sm text-gray-600">{transport.type}</p>
                  </div>
                </div>
                <div className="flex justify-between items-center mb-2">
                  <span className="text-gray-600">{transport.source}</span>
                  <ArrowRight size={16} className="text-gray-400" />
                  <span className="text-gray-600">{transport.destination}</span>
                </div>
                <p className="text-lg font-bold text-blue-600">${transport.price.toFixed(2)}</p>
                <p className="text-sm text-gray-500 mt-1">{transport.duration}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Travel Packages */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Travel Packages</h2>
            <Link to="/booking" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {packages.map((pkg) => (
              <div key={pkg.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
                <img src={pkg.imageUrl} alt={pkg.name} className="w-full h-48 object-cover" />
                <div className="p-4">
                  <div className="flex items-center mb-2">
                    <Plane size={20} className="text-purple-600" />
                    <h3 className="font-semibold text-gray-800 ml-2">{pkg.name}</h3>
                  </div>
                  <p className="text-sm text-gray-600 mb-2">{pkg.duration}</p>
                  <p className="text-lg font-bold text-purple-600">${pkg.price.toFixed(2)}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Movies */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800">Now Showing</h2>
            <Link to="/booking" className="text-blue-600 hover:text-blue-700 flex items-center">
              View All <ArrowRight size={20} className="ml-2" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {movies.map((movie) => (
              <div key={movie.id} className="bg-gray-50 rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden">
                <img src={movie.imageUrl} alt={movie.title} className="w-full h-64 object-cover" />
                <div className="p-4">
                  <h3 className="font-semibold text-gray-800 mb-2">{movie.title}</h3>
                  <p className="text-sm text-gray-600 mb-2">{movie.genre} • {movie.language}</p>
                  <div className="flex items-center">
                    <Star size={16} className="text-yellow-500 fill-current" />
                    <span className="ml-1 text-sm text-gray-600">{movie.rating}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}
