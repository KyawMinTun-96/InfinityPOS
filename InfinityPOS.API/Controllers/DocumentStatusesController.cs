using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentStatusesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public DocumentStatusesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentStatus>>> GetDocumentStatuses(
        [FromQuery] string? documentType = null)
    {
        var query = _db.DocumentStatuses
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            query = query.Where(x =>
                x.DocumentType == documentType);
        }

        var statuses = await query
            .OrderBy(x => x.DocumentStatusId)
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentStatus>> GetDocumentStatus(int id)
    {
        var status = await _db.DocumentStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DocumentStatusId == id);

        if (status == null)
        {
            return NotFound(new
            {
                message = "Document status not found."
            });
        }

        return Ok(status);
    }
}