import axios from 'axios';

const API_BASE_URL = 'http://localhost:5223/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Shopping API
export const shoppingApi = {
  getProducts: () => api.get('/shopping/products'),
  getProduct: (id) => api.get(`/shopping/products/${id}`),
  createProduct: (data) => api.post('/shopping/products', data),
  updateProduct: (id, data) => api.put(`/shopping/products/${id}`, data),
  updateProductStatus: (id, status) => api.put(`/shopping/products/${id}/status`, { status }),
  deleteProduct: (id) => api.delete(`/shopping/products/${id}`),
  getCategories: () => api.get('/shopping/categories'),
  createCategory: (data) => api.post('/shopping/categories', data),
  getOrders: () => api.get('/shopping/orders'),
  getUserOrders: (userId) => api.get(`/shopping/orders/user/${userId}`),
  createOrder: (data) => api.post('/shopping/orders', data),
  updateOrderStatus: (id, status) => api.put(`/shopping/orders/${id}/status`, { status }),
  deleteOrder: (id) => api.delete(`/shopping/orders/${id}`),
  getCart: (userId) => api.get(`/shopping/cart/${userId}`),
  addToCart: (data) => api.post('/shopping/cart', data),
  removeFromCart: (id) => api.delete(`/shopping/cart/${id}`),
  clearCart: (userId) => api.delete(`/shopping/cart/user/${userId}`),
};

// Advertising API
export const advertisingApi = {
  getAds: () => api.get('/advertising/ads'),
  getAd: (id) => api.get(`/advertising/ads/${id}`),
  getUserAds: (userId) => api.get(`/advertising/ads/user/${userId}`),
  createAd: (data) => api.post('/advertising/ads', data),
  updateAd: (id, data) => api.put(`/advertising/ads/${id}`, data),
  updateAdStatus: (id, status) => api.put(`/advertising/ads/${id}/status`, { status }),
  deleteAd: (id) => api.delete(`/advertising/ads/${id}`),
  getAdCategories: () => api.get('/advertising/categories'),
  createAdCategory: (data) => api.post('/advertising/categories', data),
  getAdResponses: (adId) => api.get(`/advertising/responses/${adId}`),
  createAdResponse: (data) => api.post('/advertising/responses', data),
};

// Recruitment API
export const recruitmentApi = {
  getJobs: () => api.get('/recruitment/jobs'),
  getJob: (id) => api.get(`/recruitment/jobs/${id}`),
  getCompanyJobs: (companyId) => api.get(`/recruitment/jobs/company/${companyId}`),
  createJob: (data) => api.post('/recruitment/jobs', data),
  updateJob: (id, data) => api.put(`/recruitment/jobs/${id}`, data),
  updateJobStatus: (id, status) => api.put(`/recruitment/jobs/${id}/status`, { status }),
  deleteJob: (id) => api.delete(`/recruitment/jobs/${id}`),
  getApplications: () => api.get('/recruitment/applications'),
  getJobApplications: (jobId) => api.get(`/recruitment/applications/job/${jobId}`),
  getApplicantApplications: (applicantId) => api.get(`/recruitment/applications/applicant/${applicantId}`),
  createApplication: (data) => api.post('/recruitment/applications', data),
  updateApplicationStatus: (id, status) => api.put(`/recruitment/applications/${id}/status`, { status }),
  getCandidates: () => api.get('/recruitment/candidates'),
  getCandidate: (id) => api.get(`/recruitment/candidates/${id}`),
  createCandidate: (data) => api.post('/recruitment/candidates', data),
  updateCandidate: (id, data) => api.put(`/recruitment/candidates/${id}`, data),
};

// Booking API
export const bookingApi = {
  getTransports: () => api.get('/booking/transport'),
  getTransport: (id) => api.get(`/booking/transport/${id}`),
  getTransportsByType: (type) => api.get(`/booking/transport/type/${type}`),
  createTransport: (data) => api.post('/booking/transport', data),
  updateTransport: (id, data) => api.put(`/booking/transport/${id}`, data),
  updateTransportStatus: (id, status) => api.put(`/booking/transport/${id}/status`, { status }),
  getPackages: () => api.get('/booking/packages'),
  getPackage: (id) => api.get(`/booking/packages/${id}`),
  createPackage: (data) => api.post('/booking/packages', data),
  updatePackage: (id, data) => api.put(`/booking/packages/${id}`, data),
  updatePackageStatus: (id, status) => api.put(`/booking/packages/${id}/status`, { status }),
  getMovies: () => api.get('/booking/movies'),
  getMovie: (id) => api.get(`/booking/movies/${id}`),
  createMovie: (data) => api.post('/booking/movies', data),
  updateMovie: (id, data) => api.put(`/booking/movies/${id}`, data),
  updateMovieStatus: (id, status) => api.put(`/booking/movies/${id}/status`, { status }),
  getShowtimes: () => api.get('/booking/showtimes'),
  getMovieShowtimes: (movieId) => api.get(`/booking/showtimes/movie/${movieId}`),
  createShowtime: (data) => api.post('/booking/showtimes', data),
  getBookings: () => api.get('/booking/bookings'),
  getUserBookings: (userId) => api.get(`/booking/bookings/user/${userId}`),
  createBooking: (data) => api.post('/booking/bookings', data),
  updateBookingStatus: (id, status) => api.put(`/booking/bookings/${id}/status`, { status }),
  deleteBooking: (id) => api.delete(`/booking/bookings/${id}`),
};

// Admin API
export const adminApi = {
  login: (data) => api.post('/admin/login', data),
  register: (data) => api.post('/admin/register', data),
  getUsers: () => api.get('/admin/users'),
  getUser: (id) => api.get(`/admin/users/${id}`),
  updateUser: (id, data) => api.put(`/admin/users/${id}`, data),
  deleteUser: (id) => api.delete(`/admin/users/${id}`),
  getDashboard: () => api.get('/admin/dashboard'),
  getAllOrders: () => api.get('/admin/orders/all'),
};

export default api;
