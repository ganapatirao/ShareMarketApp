using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BusinessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecruitmentController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public RecruitmentController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _context.Jobs.Find(_ => true).ToListAsync();
            return Ok(jobs);
        }

        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJob(string id)
        {
            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }
            return Ok(job);
        }

        [HttpGet("jobs/company/{companyId}")]
        public async Task<IActionResult> GetCompanyJobs(string companyId)
        {
            var jobs = await _context.Jobs.Find(j => j.Company == companyId).ToListAsync();
            return Ok(jobs);
        }

        [HttpPost("jobs")]
        [Authorize]
        public async Task<IActionResult> CreateJob([FromBody] Job job)
        {
            job.Id = null;
            job.CreatedAt = DateTime.UtcNow;
            await _context.Jobs.InsertOneAsync(job);
            return Ok(new { message = "Job created successfully", job });
        }

        [HttpPut("jobs/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateJob(string id, [FromBody] Job job)
        {
            job.Id = id;
            var result = await _context.Jobs.ReplaceOneAsync(j => j.Id == id, job);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Job not found" });
            }
            return Ok(new { message = "Job updated successfully" });
        }

        [HttpPut("jobs/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateJobStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Job>.Update.Set(j => j.Status, update.Status);
            var result = await _context.Jobs.UpdateOneAsync(j => j.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Job not found" });
            }
            return Ok(new { message = "Job status updated successfully" });
        }

        [HttpDelete("jobs/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteJob(string id)
        {
            var result = await _context.Jobs.DeleteOneAsync(j => j.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Job not found" });
            }
            return Ok(new { message = "Job deleted successfully" });
        }

        [HttpGet("applications")]
        [Authorize]
        public async Task<IActionResult> GetApplications()
        {
            var applications = await _context.JobApplications.Find(_ => true).ToListAsync();
            return Ok(applications);
        }

        [HttpGet("applications/job/{jobId}")]
        public async Task<IActionResult> GetJobApplications(string jobId)
        {
            var applications = await _context.JobApplications.Find(a => a.JobId == jobId).ToListAsync();
            return Ok(applications);
        }

        [HttpGet("applications/applicant/{applicantId}")]
        [Authorize]
        public async Task<IActionResult> GetApplicantApplications(string applicantId)
        {
            var applications = await _context.JobApplications.Find(a => a.ApplicantId == applicantId).ToListAsync();
            return Ok(applications);
        }

        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] JobApplication application)
        {
            application.Id = null;
            application.CreatedAt = DateTime.UtcNow;
            await _context.JobApplications.InsertOneAsync(application);
            return Ok(new { message = "Application created successfully", application });
        }

        [HttpPut("applications/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateApplicationStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<JobApplication>.Update.Set(a => a.Status, update.Status);
            var result = await _context.JobApplications.UpdateOneAsync(a => a.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Application not found" });
            }
            return Ok(new { message = "Application status updated successfully" });
        }

        [HttpGet("candidates")]
        public async Task<IActionResult> GetCandidates()
        {
            var candidates = await _context.Candidates.Find(_ => true).ToListAsync();
            return Ok(candidates);
        }

        [HttpGet("candidates/{id}")]
        public async Task<IActionResult> GetCandidate(string id)
        {
            var candidate = await _context.Candidates.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (candidate == null)
            {
                return NotFound(new { message = "Candidate not found" });
            }
            return Ok(candidate);
        }

        [HttpPost("candidates")]
        public async Task<IActionResult> CreateCandidate([FromBody] Candidate candidate)
        {
            candidate.Id = null;
            candidate.CreatedAt = DateTime.UtcNow;
            await _context.Candidates.InsertOneAsync(candidate);
            return Ok(new { message = "Candidate created successfully", candidate });
        }

        [HttpPut("candidates/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCandidate(string id, [FromBody] Candidate candidate)
        {
            candidate.Id = id;
            var result = await _context.Candidates.ReplaceOneAsync(c => c.Id == id, candidate);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Candidate not found" });
            }
            return Ok(new { message = "Candidate updated successfully" });
        }
    }
}
