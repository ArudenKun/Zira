using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Presentation.Pages;

[Authorize]
public class PrivacyModel : PageModel
{
    public void OnGet() { }
}
