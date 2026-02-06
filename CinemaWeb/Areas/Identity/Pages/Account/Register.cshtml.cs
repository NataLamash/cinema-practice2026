using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using CinemaInfrastructure;
using CinemaDomain.Model;

namespace CinemaWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly CinemaDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            CinemaDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Це обов'язкове поле.")]
            [EmailAddress]
            [Display(Name = "Електронна адреса")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Це обов'язкове поле.")]
            [Display(Name = "Ім'я")]
            [StringLength(50)]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Це обов'язкове поле.")]
            [Display(Name = "Прізвище")]
            [StringLength(50)]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Це обов'язкове поле.")]
            [DataType(DataType.Date)]
            [Display(Name = "Дата народження")]
            [BirthDate]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Це обов'язкове поле.")]
            [StringLength(100, ErrorMessage = "{0} має бути від {2} до {1} символів.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Підтвердження пароля")]
            [Compare("Password", ErrorMessage = "Пароль та підтвердження паролю не збігаються.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new IdentityUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                user.Email = Input.Email;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var appUser = new CinemaDomain.Model.User
                    {
                        AzureIdentityId = user.Id,
                        Email = user.Email,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        RegistrationDate = DateTime.UtcNow,
                        DateOfBirth = Input.DateOfBirth,
                    };

                    _context.AppUsers.Add(appUser);
                    await _context.SaveChangesAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }

    public class BirthDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date > DateTime.Now)
                {
                    return new ValidationResult("Дата народження не може бути в майбутньому.");
                }

                if (date < new DateTime(1900, 1, 1))
                {
                    return new ValidationResult("Введіть реальну дату народження.");
                }

                 if (date > DateTime.Now.AddYears(-6))
                {
                    return new ValidationResult("Мінімальний вік для реєстрації 14 років.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Некоректний формат дати.");
        }
    }
}
