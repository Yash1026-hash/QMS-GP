using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserDbContext _context;

    public AdminController(UserDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // 1. DASHBOARD SUMMARY STATS
    // GET: api/admin/stats
    // =========================================================
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalDeviations = await _context.DeviationRequests.CountAsync();
        var pendingDeviations = await _context.DeviationRequests.CountAsync(d => d.Status == 0);
        var activeDeviations = await _context.DeviationRequests.CountAsync(d => d.Status == 1);
        var inactiveDeviations = await _context.DeviationRequests.CountAsync(d => d.Status == 2);
        var approvedDeviations = await _context.DeviationRequests.CountAsync(d => d.Decision == 1);
        var rejectedDeviations = await _context.DeviationRequests.CountAsync(d => d.Decision == 2);

        var totalDocuments = await _context.DocumentCreations.CountAsync();
        var totalChangeRequests = await _context.OperatorChangeRequests.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var recentDeviations = await _context.DeviationRequests
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedDate)
            .Take(10)
            .Select(d => new DeviationRequestDto
            {
                Id = d.Id,
                DocumentId = d.DocumentId,
                Title = d.Title,
                Description = d.Description,
                Priority = d.Priority,
                Status = d.Status,
                CreatedBy = d.CreatedBy,
                CreatedDate = d.CreatedDate,
                Decision = d.Decision,
                DecisionBy = d.DecisionBy,
                DecisionOn = d.DecisionOn,
                DecisionComments = d.DecisionComments
            })
            .ToListAsync();

        var recentChangeRequests = await _context.OperatorChangeRequests
            .AsNoTracking()
            .OrderByDescending(c => c.RequestedDate)
            .Take(10)
            .ToListAsync();

        var recentDocuments = await _context.DocumentCreations
            .AsNoTracking()
            .OrderByDescending(d => d.CreationOn)
            .Take(10)
            .Select(d => new Document
            {
                Id = d.Id,
                DocumentNumber = d.DocumentNumber,
                Title = d.Title,
                Department = d.Department,
                DocumentVersion = d.DocumentVersion,
                Status = d.Status,
                FileName = d.FileName,
                ContentType = d.ContentType,
                CreatedBy = d.CreatedBy,
                CreationOn = d.CreationOn,
                Comment = d.Comment
            })
            .ToListAsync();

        return Ok(new
        {
            totalDeviations,
            pendingDeviations,
            activeDeviations,
            inactiveDeviations,
            approvedDeviations,
            rejectedDeviations,
            totalDocuments,
            totalChangeRequests,
            totalUsers,
            recentDeviations,
            recentChangeRequests,
            recentDocuments
        });
    }

    // =========================================================
    // 2. CHANGE REQUESTS
    // GET: api/admin/change-requests
    // =========================================================
    [HttpGet("change-requests")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>> GetChangeRequests(
        [FromQuery] string? status = null,
        [FromQuery] string? changeType = null,
        [FromQuery] string? search = null)
    {
        var query = _context.OperatorChangeRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(changeType))
        {
            query = query.Where(c => c.ChangeType.ToLower() == changeType.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Title.Contains(term) ||
                c.Description.Contains(term) ||
                c.ChangeType.Contains(term));
        }

        var result = await query
            .OrderByDescending(c => c.RequestedDate)
            .ToListAsync();

        return Ok(result);
    }

    // GET: api/admin/change-requests/{id}
    [HttpGet("change-requests/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChangeRequestById(int id)
    {
        var changeRequest = await _context.OperatorChangeRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (changeRequest == null)
        {
            return NotFound(new { message = $"Change request #{id} not found." });
        }

        var linkedDeviations = await _context.OperatorChangeRequestDeviations
            .AsNoTracking()
            .Where(d => d.ChangeRequestId == id)
            .ToListAsync();

        return Ok(new
        {
            changeRequest,
            linkedDeviations
        });
    }

    // =========================================================
    // 3. DEVIATIONS
    // GET: api/admin/deviations
    // =========================================================
    [HttpGet("deviations")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DeviationRequestDto>>> GetDeviations(
        [FromQuery] int? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] int? decision = null,
        [FromQuery] string? search = null)
    {
        var query = _context.DeviationRequests.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = query.Where(d => d.Priority.ToLower() == priority.ToLower());
        }

        if (decision.HasValue)
        {
            if (decision.Value == -1)
            {
                query = query.Where(d => !d.Decision.HasValue);
            }
            else
            {
                query = query.Where(d => d.Decision == decision.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d =>
                d.Title.Contains(term) ||
                d.Description.Contains(term) ||
                d.CreatedBy.Contains(term));
        }

        var result = await query
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new DeviationRequestDto
            {
                Id = d.Id,
                DocumentId = d.DocumentId,
                Title = d.Title,
                Description = d.Description,
                Priority = d.Priority,
                Status = d.Status,
                CreatedBy = d.CreatedBy,
                CreatedDate = d.CreatedDate,
                Decision = d.Decision,
                DecisionBy = d.DecisionBy,
                DecisionOn = d.DecisionOn,
                DecisionComments = d.DecisionComments
            })
            .ToListAsync();

        return Ok(result);
    }

    // GET: api/admin/deviations/{id}
    [HttpGet("deviations/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDeviationById(int id)
    {
        var deviation = await _context.DeviationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deviation == null)
        {
            return NotFound(new { message = $"Deviation #{id} not found." });
        }

        var deviationDto = new DeviationRequestDto
        {
            Id = deviation.Id,
            DocumentId = deviation.DocumentId,
            Title = deviation.Title,
            Description = deviation.Description,
            Priority = deviation.Priority,
            Status = deviation.Status,
            CreatedBy = deviation.CreatedBy,
            CreatedDate = deviation.CreatedDate,
            Decision = deviation.Decision,
            DecisionBy = deviation.DecisionBy,
            DecisionOn = deviation.DecisionOn,
            DecisionComments = deviation.DecisionComments
        };

        var reports = await _context.DeviationReportRequests
            .AsNoTracking()
            .Where(r => r.DeviationId == id)
            .Select(r => new DeviationReportDto
            {
                Id = r.Id,
                DeviationId = r.DeviationId,
                DocumentId = r.DocumentId,
                AttemptNumber = r.AttemptNumber,
                Summary = r.Summary,
                Proof = r.Proof,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                Status = r.Status,
                Decision = r.Decision,
                DecisionBy = r.DecisionBy,
                DecisionOn = r.DecisionOn,
                DecisionComments = r.DecisionComments
            })
            .ToListAsync();

        var linkedCrIds = await _context.OperatorChangeRequestDeviations
            .AsNoTracking()
            .Where(l => l.DeviationId == id)
            .Select(l => l.ChangeRequestId)
            .ToListAsync();

        var linkedChangeRequests = await _context.OperatorChangeRequests
            .AsNoTracking()
            .Where(cr => linkedCrIds.Contains(cr.Id))
            .ToListAsync();

        return Ok(new
        {
            deviation = deviationDto,
            reports,
            linkedChangeRequests
        });
    }

    // =========================================================
    // 4. CONTROLLED DOCUMENTS
    // GET: api/admin/documents
    // =========================================================
    [HttpGet("documents")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocuments(
        [FromQuery] string? status = null,
        [FromQuery] string? department = null,
        [FromQuery] string? search = null)
    {
        var query = _context.DocumentCreations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(d => d.Status.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(d => d.Department.ToLower() == department.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d =>
                d.DocumentNumber.Contains(term) ||
                d.Title.Contains(term) ||
                d.Department.Contains(term));
        }

        var result = await query
            .OrderByDescending(d => d.CreationOn)
            .Select(d => new Document
            {
                Id = d.Id,
                DocumentNumber = d.DocumentNumber,
                Title = d.Title,
                Department = d.Department,
                DocumentVersion = d.DocumentVersion,
                Status = d.Status,
                FileName = d.FileName,
                ContentType = d.ContentType,
                CreatedBy = d.CreatedBy,
                CreationOn = d.CreationOn,
                Comment = d.Comment
            })
            .ToListAsync();

        return Ok(result);
    }

    // GET: api/admin/documents/{id}
    [HttpGet("documents/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Document>> GetDocumentById(int id)
    {
        var doc = await _context.DocumentCreations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
        {
            return NotFound(new { message = $"Document #{id} not found." });
        }

        var dto = new Document
        {
            Id = doc.Id,
            DocumentNumber = doc.DocumentNumber,
            Title = doc.Title,
            Department = doc.Department,
            DocumentVersion = doc.DocumentVersion,
            Status = doc.Status,
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            FileData = doc.FileData,
            CreatedBy = doc.CreatedBy,
            CreationOn = doc.CreationOn,
            Comment = doc.Comment
        };

        return Ok(dto);
    }

    // =========================================================
    // 5. USERS & ROLES MANAGEMENT
    // GET: api/admin/users
    // =========================================================
    [HttpGet("users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.UserId)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.FullName,
                u.Email,
                u.Department,
                u.Role,
                u.RegistrationStatus,
                u.IsActive,
                u.CreatedAt,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    // POST: api/admin/users
    [HttpPost("users")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "User data is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Password is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new { message = "Role is required." });
        }

        var normalizedUsername = request.Username.Trim();
        var exists = await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername.ToLower());
        if (exists)
        {
            return Conflict(new { message = $"Username '{request.Username}' already exists." });
        }

        var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == request.Role.Trim().ToLower());
        if (roleEntity == null)
        {
            return BadRequest(new { message = $"Role '{request.Role}' does not exist in the system." });
        }

        var userAccount = new UserAccount
        {
            Username = normalizedUsername,
            Password = request.Password,
            FullName = request.FullName?.Trim() ?? string.Empty,
            Email = request.Email?.Trim() ?? string.Empty,
            Department = request.Department?.Trim() ?? string.Empty,
            Role = roleEntity.Name,
            IsActive = request.IsActive,
            RegistrationStatus = string.IsNullOrWhiteSpace(request.RegistrationStatus) ? "Registered" : request.RegistrationStatus.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(userAccount);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole
        {
            UserId = userAccount.UserId,
            RoleId = roleEntity.RoleId
        });
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new { id = userAccount.UserId }, new
        {
            message = $"User '{userAccount.Username}' created successfully.",
            userId = userAccount.UserId,
            username = userAccount.Username,
            role = userAccount.Role
        });
    }

    // GET: api/admin/roles
    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.RoleId,
                r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }

    // PUT: api/admin/users/{userId}/role
    [HttpPut("users/{userId:int}/role")]
    [HttpPost("users/{userId:int}/role")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangeUserRole(int userId, [FromBody] ChangeUserRoleRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new { message = "Role name is required." });
        }

        var normalizedRole = request.Role.Trim();

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == normalizedRole.ToLower());

        if (roleEntity == null)
        {
            return BadRequest(new { message = $"Role '{request.Role}' does not exist in the system." });
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound(new { message = $"User #{userId} was not found." });
        }

        // Update the primary role on user
        user.Role = roleEntity.Name;

        // Clear existing role assignments and assign the selected role
        _context.UserRoles.RemoveRange(user.UserRoles);
        user.UserRoles.Clear();

        user.UserRoles.Add(new UserRole
        {
            UserId = user.UserId,
            RoleId = roleEntity.RoleId
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Role for user '{user.Username}' changed to '{roleEntity.Name}' successfully.",
            userId = user.UserId,
            username = user.Username,
            role = roleEntity.Name
        });
    }

    // PUT: api/admin/users/{userId}/status
    [HttpPut("users/{userId:int}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return NotFound(new { message = $"User #{userId} was not found." });
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.RegistrationStatus))
        {
            user.RegistrationStatus = request.RegistrationStatus.Trim();
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"User #{userId} status updated successfully.",
            userId = user.UserId,
            isActive = user.IsActive,
            registrationStatus = user.RegistrationStatus
        });
    }

    // =========================================================
    // 6. AUDIT TRAIL
    // GET: api/admin/audit-trail
    // =========================================================
    [HttpGet("audit-trail")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuditTrail()
    {
        var auditRecords = await _context.DocumentHistories
            .AsNoTracking()
            .Where(h => h.DocumentNumber != "string" && h.DocumentId > 0)
            .OrderByDescending(h => h.ArchivedOn)
            .ToListAsync();

        return Ok(auditRecords);
    }

    // =========================================================
    // 7. REPORTS
    // GET: api/admin/reports
    // =========================================================
    [HttpGet("reports")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReports()
    {
        var deviationReports = await _context.DeviationReportRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return Ok(deviationReports);
    }
}


