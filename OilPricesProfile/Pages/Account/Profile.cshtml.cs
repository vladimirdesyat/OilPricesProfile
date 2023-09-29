using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OilPricesProfile.Data;
using OilPricesProfile.Data.Context;
using OilPricesProfile.Models;

namespace OilPricesProfile.Pages.Account
{
    [AllowAnonymous]
    public class ProfileModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _dbContext;
        private readonly DataParser _DataParser;
        private readonly string ggnpLink = "https://ggnpsales.ru/trading-result-petroleum/";
        private readonly int[] ggpnPages = new int[10];
        public List<Price> SortedPrices { get; set; }
        public bool DisplayPriceTable { get; set; } = false;
        public List<Price> Prices { get; set; } = new List<Price>();
        public ProfileModel(
            SignInManager<User> signInManager,
            DataParser dataParser,
            AppDbContext dbContext
            )
        {
            _signInManager = signInManager;
            _DataParser = dataParser;
            _dbContext = dbContext;

            ggpnPages = new[] { 170, 171, 172, 173, 174, 175, 176, 177, 178, 179 }; // 171,172,173,174,175,176,177,178,179
        }
        public async Task<IActionResult> OnPostLoadDataAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                int index = 0;
                while (index < ggpnPages.Length)
                {
                    // Replace the URL with the desired web page URL you want to parse
                    var urlToParse = ggnpLink + ggpnPages[index]; // Update with your URL
                    try
                    {
                        // Parse and store web page data
                        await _DataParser.ParseAndStoreWebPageDataAsync(urlToParse);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., log the error)
                        // Optionally, you can add error handling here
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    index++;
                }
            }

            // Redirect back to the profile page
            return RedirectToPage();
        }

        public string Email { get; private set; }

        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User))
            {
                // Retrieve the email from the user's claims
                var emailClaim = User.FindFirst(c => c.Type == "Email");

                if (emailClaim != null)
                {
                    Email = emailClaim.Value;
                }
            }

            return Page();
        }

        public IActionResult OnPostDisplayPrices()
        {
            // Retrieve all prices from the "Prices" table
            Prices = _dbContext.Prices
                .Include(p => p.PetroleumProduct)
                .Include(p => p.OilDepot)
                .OrderBy(p => p.PetroleumProductId) // Sort by PetroleumProductId
                .ThenBy(p => p.OilDepotId) // Then, sort by OilDepotId
                .ThenBy(p => p.Date.Date) // Finally, sort by Date
                .ToList();


            DisplayPriceTable = true;

            return Page();
        }

        public IActionResult OnPostMaximumPrices()
        {
            SortedPrices = _dbContext.Prices
                .Where(p => p.MaxPricePerLiterInclVat.HasValue && p.MaxPricePerLiterInclVat.Value != 0)
                .OrderBy(p => p.MaxPricePerLiterInclVat)
                .ThenByDescending(p => p.MaxPricePerLiterInclVat)
                .ThenBy(p => p.Date.Date)
                .ToList();


            return Page();
        }
    }
}
