import { useState, useEffect } from 'react';
import { Plus, MapPin, Briefcase, DollarSign } from 'lucide-react';
import { recruitmentApi } from '../services/api';

export default function Recruitment() {
  const [jobs, setJobs] = useState([]);
  const [applications, setApplications] = useState([]);
  const [candidates, setCandidates] = useState([]);
  const [selectedJobType, setSelectedJobType] = useState('All');
  const [selectedLocation, setSelectedLocation] = useState('All');
  const [showApplyForm, setShowApplyForm] = useState(false);
  const [showPostJob, setShowPostJob] = useState(false);
  const [selectedJob, setSelectedJob] = useState(null);
  const [newJob, setNewJob] = useState({
    title: '',
    company: '',
    location: '',
    salary: '',
    type: '',
    description: '',
    skills: ''
  });
  const [application, setApplication] = useState({
    email: '',
    phone: '',
    resume: '',
    coverLetter: ''
  });

  useEffect(() => {
    loadJobs();
    loadApplications();
    loadCandidates();
  }, []);

  const loadJobs = async () => {
    try {
      const response = await recruitmentApi.getJobs();
      setJobs(response.data.filter(j => j.status === 'Active'));
    } catch (error) {
      console.error('Error loading jobs:', error);
    }
  };

  const loadApplications = async () => {
    try {
      const userId = localStorage.getItem('userId');
      if (userId) {
        const response = await recruitmentApi.getApplicantApplications(userId);
        setApplications(response.data);
      }
    } catch (error) {
      console.error('Error loading applications:', error);
    }
  };

  const loadCandidates = async () => {
    try {
      const response = await recruitmentApi.getCandidates();
      setCandidates(response.data);
    } catch (error) {
      console.error('Error loading candidates:', error);
    }
  };

  const handleApply = (job) => {
    setSelectedJob(job);
    setShowApplyForm(true);
  };

  const handleSubmitApplication = async (e) => {
    e.preventDefault();
    try {
      const userId = localStorage.getItem('userId');
      const userName = localStorage.getItem('userName');
      await recruitmentApi.createApplication({
        jobId: selectedJob.id,
        jobTitle: selectedJob.title,
        applicantId: userId,
        applicantName: userName || 'Anonymous',
        ...application
      });
      loadApplications();
      setApplication({
        email: '',
        phone: '',
        resume: '',
        coverLetter: ''
      });
      setShowApplyForm(false);
      alert('Application submitted successfully!');
    } catch (error) {
      console.error('Error submitting application:', error);
    }
  };

  const handlePostJob = async (e) => {
    e.preventDefault();
    try {
      await recruitmentApi.createJob({
        ...newJob,
        skills: newJob.skills.split(',').map(s => s.trim())
      });
      loadJobs();
      setNewJob({
        title: '',
        company: '',
        location: '',
        salary: '',
        type: '',
        description: '',
        skills: ''
      });
      setShowPostJob(false);
      alert('Job posted successfully!');
    } catch (error) {
      console.error('Error posting job:', error);
    }
  };

  const locations = [...new Set(jobs.map(j => j.location))];
  const jobTypes = [...new Set(jobs.map(j => j.type))];

  const filteredJobs = jobs.filter(job => {
    const typeMatch = selectedJobType === 'All' || job.type === selectedJobType;
    const locationMatch = selectedLocation === 'All' || job.location === selectedLocation;
    return typeMatch && locationMatch;
  });

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-4xl font-bold text-gray-800">Job Portal</h1>
          <button
            onClick={() => setShowPostJob(true)}
            className="bg-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center"
          >
            <Plus size={20} className="mr-2" />
            Post Job
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Job Type</label>
              <select
                value={selectedJobType}
                onChange={(e) => setSelectedJobType(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="All">All Types</option>
                {jobTypes.map((type) => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Location</label>
              <select
                value={selectedLocation}
                onChange={(e) => setSelectedLocation(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="All">All Locations</option>
                {locations.map((location) => (
                  <option key={location} value={location}>{location}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Jobs Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">
          {filteredJobs.map((job) => (
            <div key={job.id} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow p-6">
              <div className="flex items-center mb-4">
                <Briefcase size={32} className="text-blue-600" />
                <div className="ml-3">
                  <h3 className="font-semibold text-gray-800">{job.title}</h3>
                  <p className="text-sm text-gray-600">{job.company}</p>
                </div>
              </div>
              <div className="space-y-2 mb-4">
                <div className="flex items-center">
                  <MapPin size={16} className="text-gray-500" />
                  <span className="ml-2 text-sm text-gray-600">{job.location}</span>
                </div>
                <div className="flex items-center">
                  <DollarSign size={16} className="text-gray-500" />
                  <span className="ml-2 text-sm text-gray-600">{job.salary}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">{job.type}</span>
                </div>
              </div>
              <p className="text-sm text-gray-600 mb-4 line-clamp-2">{job.description}</p>
              <div className="flex flex-wrap gap-2 mb-4">
                {job.skills.slice(0, 3).map((skill, index) => (
                  <span key={index} className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">{skill}</span>
                ))}
              </div>
              <button
                onClick={() => handleApply(job)}
                className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
              >
                Apply Now
              </button>
            </div>
          ))}
        </div>

        {/* Applications */}
        {applications.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Your Applications</h2>
            <div className="space-y-4">
              {applications.map((app) => (
                <div key={app.id} className="border-b pb-4">
                  <div className="flex justify-between items-center">
                    <div>
                      <p className="font-medium text-gray-800">{app.jobTitle}</p>
                      <p className="text-sm text-gray-600">Applied: {new Date(app.createdAt).toLocaleDateString()}</p>
                    </div>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      app.status === 'Shortlisted' ? 'bg-green-100 text-green-800' :
                      app.status === 'Reviewed' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-blue-100 text-blue-800'
                    }`}>
                      {app.status}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Apply Form Modal */}
        {showApplyForm && selectedJob && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg max-w-md w-full mx-4 p-6">
              <h2 className="text-2xl font-bold text-gray-800 mb-4">Apply for {selectedJob.title}</h2>
              <form onSubmit={handleSubmitApplication} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input
                    type="email"
                    value={application.email}
                    onChange={(e) => setApplication({ ...application, email: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                  <input
                    type="tel"
                    value={application.phone}
                    onChange={(e) => setApplication({ ...application, phone: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Resume URL</label>
                  <input
                    type="url"
                    value={application.resume}
                    onChange={(e) => setApplication({ ...application, resume: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Cover Letter</label>
                  <textarea
                    value={application.coverLetter}
                    onChange={(e) => setApplication({ ...application, coverLetter: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    rows="3"
                    required
                  />
                </div>
                <div className="flex space-x-4">
                  <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
                  >
                    Submit Application
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowApplyForm(false)}
                    className="flex-1 bg-gray-300 text-gray-700 py-3 rounded-lg font-semibold hover:bg-gray-400 transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Post Job Modal */}
        {showPostJob && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg max-w-md w-full mx-4 p-6">
              <h2 className="text-2xl font-bold text-gray-800 mb-4">Post New Job</h2>
              <form onSubmit={handlePostJob} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Job Title</label>
                  <input
                    type="text"
                    value={newJob.title}
                    onChange={(e) => setNewJob({ ...newJob, title: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Company</label>
                    <input
                      type="text"
                      value={newJob.company}
                      onChange={(e) => setNewJob({ ...newJob, company: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Location</label>
                    <input
                      type="text"
                      value={newJob.location}
                      onChange={(e) => setNewJob({ ...newJob, location: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Salary Range</label>
                    <input
                      type="text"
                      value={newJob.salary}
                      onChange={(e) => setNewJob({ ...newJob, salary: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Job Type</label>
                    <select
                      value={newJob.type}
                      onChange={(e) => setNewJob({ ...newJob, type: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    >
                      <option value="">Select Type</option>
                      <option value="Full-time">Full-time</option>
                      <option value="Part-time">Part-time</option>
                      <option value="Remote">Remote</option>
                      <option value="Contract">Contract</option>
                      <option value="Internship">Internship</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                  <textarea
                    value={newJob.description}
                    onChange={(e) => setNewJob({ ...newJob, description: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    rows="3"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Skills (comma separated)</label>
                  <input
                    type="text"
                    value={newJob.skills}
                    onChange={(e) => setNewJob({ ...newJob, skills: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
                <div className="flex space-x-4">
                  <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
                  >
                    Post Job
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowPostJob(false)}
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
