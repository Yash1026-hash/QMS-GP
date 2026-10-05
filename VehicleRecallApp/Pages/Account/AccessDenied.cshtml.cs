using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Account;

[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
}
