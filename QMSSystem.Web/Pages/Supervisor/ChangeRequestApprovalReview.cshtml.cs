using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Supervisor;

public class ChangeRequestApprovalReviewModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OperatorChangeRequest? ChangeRequest { get; set; }

    [BindProperty]
    public int ChangeRequestId { get; set; }

    [BindProperty]
    public string Decision { get; set; } = string.Empty;

    [BindProperty]
    public string DecisionComment { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public ChangeRequestApprovalReviewModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    // =========================================================
    // GET
    // Get one change request from API
    // =========================================================

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ChangeRequestId = id;

        await LoadChangeRequestAsync(id);

        if (ChangeRequest == null)
        {
            ErrorMessage =
                "The requested change request was not found.";
        }

        return Page();
    }


    // =========================================================
    // POST
    // Submit Accept / Reject decision
    // =========================================================

    public async Task<IActionResult> OnPostAsync()
    {
        // -----------------------------------------------------
        // Validate Change Request ID
        // -----------------------------------------------------

        if (ChangeRequestId <= 0)
        {
            ErrorMessage =
                "Invalid change request.";

            return Page();
        }


        // -----------------------------------------------------
        // Validate Decision
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(Decision))
        {
            ErrorMessage =
                "Please select Accept or Reject.";

            await LoadChangeRequestAsync(ChangeRequestId);

            return Page();
        }

        if (!Decision.Equals(
                "accept",
                StringComparison.OrdinalIgnoreCase) &&
            !Decision.Equals(
                "reject",
                StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage =
                "Invalid decision selected.";

            await LoadChangeRequestAsync(ChangeRequestId);

            return Page();
        }


        // -----------------------------------------------------
        // Validate Comment
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(DecisionComment))
        {
            ErrorMessage =
                "Please enter a comment before submitting.";

            await LoadChangeRequestAsync(ChangeRequestId);

            return Page();
        }

        if (DecisionComment.Length > 1000)
        {
            ErrorMessage =
                "Comment cannot exceed 1000 characters.";

            await LoadChangeRequestAsync(ChangeRequestId);

            return Page();
        }


        try
        {
            var client =
                _httpClientFactory.CreateClient("QMSApi");


            // -------------------------------------------------
            // IMPORTANT:
            // Get the complete existing record first.
            //
            // This is required because your existing
            // OperatorChangeRequest DTO has [Required]
            // fields such as Title, Description, ChangeType
            // and Status.
            // -------------------------------------------------

            var getResponse =
                await client.GetAsync(
                    $"api/ChangeRequestApproval/{ChangeRequestId}");

            if (!getResponse.IsSuccessStatusCode)
            {
                ErrorMessage =
                    "Unable to load the change request.";

                await LoadChangeRequestAsync(ChangeRequestId);

                return Page();
            }

            var existingRequest =
                await getResponse.Content
                    .ReadFromJsonAsync<OperatorChangeRequest>();

            if (existingRequest == null)
            {
                ErrorMessage =
                    "Change request could not be found.";

                return Page();
            }


            // -------------------------------------------------
            // Temporary supervisor ID
            //
            // Later we will replace this with the actual
            // logged-in supervisor UserId.
            // -------------------------------------------------

            var decisionByUserId = 1;


            // -------------------------------------------------
            // Use the EXISTING DTO.
            //
            // Keep all existing database values and change
            // only the approval-related fields.
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
            // POST to API
            // -------------------------------------------------

            var response =
                await client.PostAsJsonAsync(
                    "api/ChangeRequestApproval",
                    request);


            // -------------------------------------------------
            // POST SUCCESS
            // -------------------------------------------------

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(
                    "/Supervisor/ChangeRequestApproval");
            }


            // -------------------------------------------------
            // POST ERROR
            // -------------------------------------------------

            var error =
                await response.Content.ReadAsStringAsync();

            ErrorMessage =
                string.IsNullOrWhiteSpace(error)
                    ? "Unable to submit the decision."
                    : error;

            ChangeRequest = existingRequest;

            return Page();
        }
        catch (Exception)
        {
            ErrorMessage =
                "Unable to connect to the QMS API.";

            await LoadChangeRequestAsync(ChangeRequestId);

            return Page();
        }
    }


    // =========================================================
    // Load change request from API
    // =========================================================

    private async Task LoadChangeRequestAsync(int id)
    {
        if (id <= 0)
        {
            ChangeRequest = null;
            return;
        }

        try
        {
            var client =
                _httpClientFactory.CreateClient("QMSApi");

            var response =
                await client.GetAsync(
                    $"api/ChangeRequestApproval/{id}");

            if (response.IsSuccessStatusCode)
            {
                ChangeRequest =
                    await response.Content
                        .ReadFromJsonAsync<OperatorChangeRequest>();
            }
            else
            {
                ChangeRequest = null;
            }
        }
        catch
        {
            ChangeRequest = null;
        }
    }
}