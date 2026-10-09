namespace QMSSystem.Shared.DTOs;

using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

public class AdminDashboardStatsDto
{
    public int TotalDeviations { get; set; }
    public int PendingDeviations { get; set; }
    public int ActiveDeviations { get; set; }
    public int InactiveDeviations { get; set; }
    public int ApprovedDeviations { get; set; }
    public int RejectedDeviations { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalChangeRequests { get; set; }
    public int TotalUsers { get; set; }
    public List<DeviationRequestDto> RecentDeviations { get; set; } = [];
    public List<OperatorChangeRequest> RecentChangeRequests { get; set; } = [];
    public List<Document> RecentDocuments { get; set; } = [];
}

public class AdminUserItemDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RegistrationStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class AdminRoleItemDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AdminChangeRequestDetailDto
{
    public OperatorChangeRequest? ChangeRequest { get; set; }
    public List<OperatorChangeRequestDeviation> LinkedDeviations { get; set; } = [];
}

public class AdminDeviationDetailDto
{
    public DeviationRequestDto? Deviation { get; set; }
    public List<DeviationReportDto> Reports { get; set; } = [];
    public List<OperatorChangeRequest> LinkedChangeRequests { get; set; } = [];
}

public class ChangeUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserStatusRequest
{
    public bool? IsActive { get; set; }
    public string? RegistrationStatus { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string RegistrationStatus { get; set; } = "Registered";
}


