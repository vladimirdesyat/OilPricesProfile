using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OilPricesProfile.Models;

namespace OilPricesProfile.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(UserManager<User> userManager, ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Page(
                    "/ResetPassword",
                    pageHandler: null,
                    values: new { code = token },
                    protocol: Request.Scheme);

                // You can send the reset link to the user via email or display it on the page
                // Here, we'll just display it for demonstration purposes
                ViewData["ResetLink"] = resetLink;

                return Page();
            }

            // Log ModelState errors if the request is invalid
            _logger.LogError("Invalid forgot password request. ModelState errors: {Errors}", ModelState);

            return Page();
        }
    }
}
