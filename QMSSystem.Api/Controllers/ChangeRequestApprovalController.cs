using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChangeRequestApprovalController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ChangeRequestApprovalController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // =========================================================
    // GET 1
    // Get all pending change requests
    // =========================================================

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>>
        GetPendingChangeRequests()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Database connection string is not configured.");
        }

        var changeRequests =
            new List<OperatorChangeRequest>();

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT
                Id,
                DocumentId,
                Title,
                ChangeType,
                Description,
                RequestedByUserId,
                RequestedDate,
                Decision,
                DecisionComment,
                DecisionByUserId,
                DecisionDate,
                Status
            FROM dbo.OperatorChangeRequests
            WHERE Status = 'Pending'
            ORDER BY RequestedDate DESC;
            """;

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var changeRequest =
                new OperatorChangeRequest
                {
                    Id = reader.GetInt32(
                        reader.GetOrdinal("Id")),

                    DocumentId = reader.GetInt32(
                        reader.GetOrdinal("DocumentId")),

                    Title = reader.GetString(
                        reader.GetOrdinal("Title")),

                    ChangeType = reader.GetString(
                        reader.GetOrdinal("ChangeType")),

                    Description = reader.GetString(
                        reader.GetOrdinal("Description")),

                    RequestedByUserId = reader.GetInt32(
                        reader.GetOrdinal("RequestedByUserId")),

                    RequestedDate = reader.GetDateTime(
                        reader.GetOrdinal("RequestedDate")),

                    Decision = reader.IsDBNull(
                        reader.GetOrdinal("Decision"))
                        ? string.Empty
                        : reader.GetString(
                            reader.GetOrdinal("Decision")),

                    DecisionComment = reader.IsDBNull(
                        reader.GetOrdinal("DecisionComment"))
                        ? string.Empty
                        : reader.GetString(
                            reader.GetOrdinal("DecisionComment")),

                    DecisionByUserId = reader.IsDBNull(
                        reader.GetOrdinal("DecisionByUserId"))
                        ? 0
                        : reader.GetInt32(
                            reader.GetOrdinal("DecisionByUserId")),

                    DecisionDate = reader.IsDBNull(
                        reader.GetOrdinal("DecisionDate"))
                        ? default
                        : reader.GetDateTime(
                            reader.GetOrdinal("DecisionDate")),

                    Status = reader.GetString(
                        reader.GetOrdinal("Status"))
                };

            changeRequests.Add(changeRequest);
        }

        return Ok(changeRequests);
    }


    // =========================================================
    // GET 2
    // Get one change request by ID
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OperatorChangeRequest>>
        GetChangeRequest(int id)
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Database connection string is not configured.");
        }

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT
                Id,
                DocumentId,
                Title,
                ChangeType,
                Description,
                RequestedByUserId,
                RequestedDate,
                Decision,
                DecisionComment,
                DecisionByUserId,
                DecisionDate,
                Status
            FROM dbo.OperatorChangeRequests
            WHERE Id = @Id;
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = id;

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return NotFound(
                $"Change request with ID {id} was not found.");
        }

        var changeRequest =
            new OperatorChangeRequest
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("Id")),

                DocumentId = reader.GetInt32(
                    reader.GetOrdinal("DocumentId")),

                Title = reader.GetString(
                    reader.GetOrdinal("Title")),

                ChangeType = reader.GetString(
                    reader.GetOrdinal("ChangeType")),

                Description = reader.GetString(
                    reader.GetOrdinal("Description")),

                RequestedByUserId = reader.GetInt32(
                    reader.GetOrdinal("RequestedByUserId")),

                RequestedDate = reader.GetDateTime(
                    reader.GetOrdinal("RequestedDate")),

                Decision = reader.IsDBNull(
                    reader.GetOrdinal("Decision"))
                    ? string.Empty
                    : reader.GetString(
                        reader.GetOrdinal("Decision")),

                DecisionComment = reader.IsDBNull(
                    reader.GetOrdinal("DecisionComment"))
                    ? string.Empty
                    : reader.GetString(
                        reader.GetOrdinal("DecisionComment")),

                DecisionByUserId = reader.IsDBNull(
                    reader.GetOrdinal("DecisionByUserId"))
                    ? 0
                    : reader.GetInt32(
                        reader.GetOrdinal("DecisionByUserId")),

                DecisionDate = reader.IsDBNull(
                    reader.GetOrdinal("DecisionDate"))
                    ? default
                    : reader.GetDateTime(
                        reader.GetOrdinal("DecisionDate")),

                Status = reader.GetString(
                    reader.GetOrdinal("Status"))
            };

        return Ok(changeRequest);
    }


    // =========================================================
    // POST
    // Update the SAME OperatorChangeRequests table
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> SubmitDecision(
        [FromBody] OperatorChangeRequest request)
    {
        if (request == null)
        {
            return BadRequest(
                "Request data is required.");
        }

        if (request.Id <= 0)
        {
            return BadRequest(
                "Invalid change request ID.");
        }

        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest(
                "Decision is required.");
        }

        if (!request.Decision.Equals(
                "accept",
                StringComparison.OrdinalIgnoreCase) &&
            !request.Decision.Equals(
                "reject",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Decision must be either accept or reject.");
        }

        if (string.IsNullOrWhiteSpace(
                request.DecisionComment))
        {
            return BadRequest(
                "Decision comment is required.");
        }

        if (request.DecisionComment.Length > 1000)
        {
            return BadRequest(
                "Decision comment cannot exceed 1000 characters.");
        }

        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Database connection string is not configured.");
        }

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();


        // -----------------------------------------------------
        // Check that the request exists and is still pending
        // -----------------------------------------------------

        const string checkSql = """
            SELECT COUNT(1)
            FROM dbo.OperatorChangeRequests
            WHERE Id = @Id
              AND Status = 'Pending';
            """;

        await using var checkCommand =
            new SqlCommand(checkSql, connection);

        checkCommand.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = request.Id;

        var count =
            Convert.ToInt32(
                await checkCommand.ExecuteScalarAsync());

        if (count == 0)
        {
            return NotFound(
                "Change request was not found or has already been processed.");
        }


        // -----------------------------------------------------
        // Update the SAME OperatorChangeRequests table
        // -----------------------------------------------------

        const string updateSql = """
            UPDATE dbo.OperatorChangeRequests
            SET
                Decision = @Decision,
                DecisionComment = @DecisionComment,
                DecisionByUserId = @DecisionByUserId,
                DecisionDate = @DecisionDate,
                Status = @Status
            WHERE Id = @Id;
            """;

        await using var updateCommand =
            new SqlCommand(updateSql, connection);

        var decision =
            request.Decision.ToLowerInvariant();

        var status =
            decision == "accept"
                ? "Approved"
                : "Rejected";

        updateCommand.Parameters.Add(
            "@Decision",
            SqlDbType.NVarChar,
            50).Value = decision;

        updateCommand.Parameters.Add(
            "@DecisionComment",
            SqlDbType.NVarChar,
            1000).Value =
                request.DecisionComment.Trim();

        updateCommand.Parameters.Add(
            "@DecisionByUserId",
            SqlDbType.Int).Value =
                request.DecisionByUserId;

        updateCommand.Parameters.Add(
            "@DecisionDate",
            SqlDbType.DateTime2).Value =
                DateTime.Now;

        updateCommand.Parameters.Add(
            "@Status",
            SqlDbType.NVarChar,
            50).Value = status;

        updateCommand.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = request.Id;

        var rowsAffected =
            await updateCommand.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return NotFound(
                "Change request could not be updated.");
        }

        return Ok(new
        {
            message =
                "Change request decision submitted successfully.",

            changeRequestId = request.Id,

            decision = decision,

            status = status
        });
    }
}