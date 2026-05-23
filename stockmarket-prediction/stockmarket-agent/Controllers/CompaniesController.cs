using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stockmarket_agent.Data;
using stockmarket_agent.Models;

namespace stockmarket_agent.Controllers
{
    /// <summary>
    /// API endpoints for company data management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly StockMarketDbContext _context;

        public CompaniesController(StockMarketDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all companies with optional filtering
        /// </summary>
        /// <param name="marketCapCategory">Filter by market cap category</param>
        /// <param name="sector">Filter by sector</param>
        /// <returns>List of companies</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies(
            [FromQuery] string? marketCapCategory = null,
            [FromQuery] string? sector = null)
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrEmpty(marketCapCategory))
            {
                query = query.Where(c => c.MarketCapCategory == marketCapCategory);
            }

            if (!string.IsNullOrEmpty(sector))
            {
                query = query.Where(c => c.Sector == sector);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        /// <summary>
        /// Create a new company
        /// </summary>
        /// <param name="company">Company object to create</param>
        /// <returns>Created company</returns>
        [HttpPost]
        public async Task<ActionResult<Company>> CreateCompany(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<Company>>> BulkCreateCompanies(IEnumerable<Company> companies)
        {
            _context.Companies.AddRange(companies);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompanies), companies);
        }

        /// <summary>
        /// Update an existing company
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <param name="company">Company object with updated data</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Soft delete a company (set IsActive to false)
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            company.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("symbols")]
        public async Task<ActionResult<IEnumerable<string>>> GetSymbols()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .Select(c => c.Symbol)
                .ToListAsync();
        }

        [HttpGet("sectors")]
        public async Task<ActionResult<IEnumerable<string>>> GetSectors()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .Select(c => c.Sector)
                .Distinct()
                .ToListAsync();
        }

        [HttpGet("by-sector")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompaniesBySector()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .GroupBy(c => c.Sector)
                .Select(g => new
                {
                    Sector = g.Key,
                    Count = g.Count(),
                    Companies = g.Select(c => new { c.Id, c.Symbol, c.Name })
                })
                .ToListAsync();
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
