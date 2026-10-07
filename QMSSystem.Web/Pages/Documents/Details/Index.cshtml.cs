using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMS.Pages.Documents
{
    public class DetailsModel : PageModel
    {
        public Document Document { get; set; } = new();

        public void OnGet(int id)
        {
            // For now, sample data
            Document = new Document
            {
                Id = id,
                Title = "Employee Leave Procedure",
                Department = "HR",
                DocumentNumber = "QMS-HR-001"
            };
        }
    }
}