using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Supervisor;

public class ChangeRequestApprovalModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<OperatorChangeRequest> ChangeRequests { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public string SuccessMessage { get; set; } = string.Empty;


    [BindProperty]
    public int ChangeRequestId { get; set; }


    [BindProperty]
    public string Decision { get; set; } = string.Empty;


    [BindProperty]
    public string DecisionComment { get; set; } = string.Empty;


    public ChangeRequestApprovalModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    // =========================================================
    // GET - LOAD PENDING CHANGE REQUESTS
    // =========================================================

    public async Task OnGetAsync()
    {
        await LoadPendingChangeRequestsAsync();
    }


    // =========================================================
    // POST - SUBMIT APPROVAL DECISION
    // =========================================================

    public async Task<IActionResult> OnPostAsync()
    {
        // -----------------------------------------------------
        // Validate Change Request ID
        // -----------------------------------------------------

        if (ChangeRequestId <= 0)
        {
            ErrorMessage = "Invalid change request.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }


        // -----------------------------------------------------
        // Validate Decision
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(Decision))
        {
            ErrorMessage =
                "Please select Accept or Reject.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }


        if (!Decision.Equals(
                "accept",
                StringComparison.OrdinalIgnoreCase)
            &&
            !Decision.Equals(
                "reject",
                StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage =
                "Invalid decision selected.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }


        // -----------------------------------------------------
        // Validate Comment
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(DecisionComment))
        {
            ErrorMessage =
                "Please enter a comment before submitting.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }


        if (DecisionComment.Length > 1000)
        {
            ErrorMessage =
                "Comment cannot exceed 1000 characters.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }


        try
        {
            var client =
                _httpClientFactory.CreateClient("ApiClient");


            // -------------------------------------------------
            // GET THE EXISTING CHANGE REQUEST
            // -------------------------------------------------
            // We need the complete record because the existing
            // OperatorChangeRequest DTO has required fields.
            // -------------------------------------------------

            var getResponse =
                await client.GetAsync(
                    $"api/ChangeRequestApproval/{ChangeRequestId}");


            if (!getResponse.IsSuccessStatusCode)
            {
                ErrorMessage =
                    "Unable to load the change request.";

                await LoadPendingChangeRequestsAsync();

                return Page();
            }


            var existingRequest =
                await getResponse.Content
                    .ReadFromJsonAsync<OperatorChangeRequest>();


            if (existingRequest == null)
            {
                ErrorMessage =
                    "Change request could not be found.";

                await LoadPendingChangeRequestsAsync();

                return Page();
            }


            // -------------------------------------------------
            // TEMPORARY SUPERVISOR USER ID
            // -------------------------------------------------
            // Replace this later when real logged-in user
            // information is connected.
            // -------------------------------------------------

            var decisionByUserId = 1;


            // -------------------------------------------------
            // CREATE COMPLETE REQUEST FOR POST
            // -------------------------------------------------

            var request = new OperatorChangeRequest
            {
                Id = existingRequest.Id,

                DocumentId = existingRequest.DocumentId,

                Title = existingRequest.Title,

                ChangeType = existingRequest.ChangeType,

                Description = existingRequest.Description,

                RequestedByUserId =
                    existingRequest.RequestedByUserId,

                RequestedDate =
                    existingRequest.RequestedDate,

                Decision =
                    Decision.ToLowerInvariant(),

                DecisionComment =
                    DecisionComment.Trim(),

                DecisionByUserId =
                    decisionByUserId,

                DecisionDate =
                    DateTime.Now,

                Status =
                    Decision.Equals(
                        "accept",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Approved"
                        : "Rejected"
            };


            // -------------------------------------------------
            // POST TO API
            // -------------------------------------------------

            var response =
                await client.PostAsJsonAsync(
                    "api/ChangeRequestApproval",
                    request);


            // -------------------------------------------------
            // SUCCESS
            // -------------------------------------------------

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(
                    "/Supervisor/ChangeRequestApproval");
            }


            // -------------------------------------------------
            // API ERROR
            // -------------------------------------------------

            var error =
                await response.Content.ReadAsStringAsync();


            ErrorMessage =
                string.IsNullOrWhiteSpace(error)
                    ? "Unable to submit the decision."
                    : error;


            await LoadPendingChangeRequestsAsync();

            return Page();
        }
        catch (Exception)
        {
            ErrorMessage =
                "Unable to connect to the QMS API.";

            await LoadPendingChangeRequestsAsync();

            return Page();
        }
    }


    // =========================================================
    // LOAD PENDING CHANGE REQUESTS
    // =========================================================

    private async Task LoadPendingChangeRequestsAsync()
    {
        try
        {
            var client =
                _httpClientFactory.CreateClient("ApiClient");


            var response =
                await client.GetAsync(
                    "api/ChangeRequestApproval/pending");


            if (response.IsSuccessStatusCode)
            {
                ChangeRequests =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<OperatorChangeRequest>>()
                    ?? new List<OperatorChangeRequest>();
            }
            else
            {
                ErrorMessage =
                    "Unable to load pending change requests.";
            }
        }
        catch (Exception)
        {
            ErrorMessage =
                "Unable to connect to the QMS API.";
        }
    }
}