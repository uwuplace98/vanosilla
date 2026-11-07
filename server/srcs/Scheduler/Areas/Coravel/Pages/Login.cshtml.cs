using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Communication.DbServer.AccountService;
using System.Security.Cryptography;
using System.Text;

namespace WingsEmu.ClusterScheduler.Areas.Coravel.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IServiceProvider _container;
        public LoginModel(IServiceProvider container)
        {
            _container = container;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            AccountLoadResponse res = _container.GetService<IAccountService>().LoadAccountByName(new()
            {
                Name = Username
            }).Result;

            // Check if the account exists
            if (res == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Hash the password provided by the user
            string hashedPasswordInput = HashPassword(Password);

            // Compare the hashed password with the one retrieved from the database
            if (res.AccountDto.Name == Username && VerifyPassword(hashedPasswordInput, res.AccountDto.Password) && res.AccountDto.Authority == DTOs.Account.AuthorityType.Root)
            {
                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.Name, Username),
                    new Claim(ClaimTypes.Role, "Admin") // Add role claim if user is an admin
                };

                var identity = new ClaimsIdentity(claims, "login");

                var principal = new ClaimsPrincipal(identity);

                // Sign in the user with the authentication scheme
                await HttpContext.SignInAsync(principal);

                // Redirect to a protected area of your application
                return RedirectToPage("/Dashboard");
            }

            // Authentication failed, display error message or redirect to login page
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        // Helper method to hash the password using SHA-512
        private string HashPassword(string password)
        {
            using (var sha512 = SHA512.Create())
            {
                byte[] hashedBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a hexadecimal string
                var builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method to verify if the provided password matches the hashed password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return password.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
