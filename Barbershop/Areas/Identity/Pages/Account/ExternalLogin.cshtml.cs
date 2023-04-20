// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Barbershop.Models;

namespace Barbershop.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            [Display(Name = "User Name")]
            public string UserName { get; set; }           
            [Required]
            public string FullName { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
            [Required]
            public DateTime DateOfBirth { get; set; }

        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Name)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new BarbershopUser { Email = Input.Email, FullName = Input.FullName, UserName = Input.UserName, DateOfBirth = Input.DateOfBirth, PhoneNumber = Input.PhoneNumber};

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, WC.ClientRole);
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Реєстрація на веб-сайті Барбершоп 'Оазис'!",
                            @"<!DOCTYPE html>
                            <html>
                            <head>
                                <title></title>
                                <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                                <meta name='viewport' content='width=device-width, initial-scale=1'>
                                <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                                <style type='text/css'>
	
                                    @media screen {
                                        @font-face {
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 400;
                                            src: local('Lato Regular'), local('Lato-Regular'), url(https://fonts.gstatic.com/s/lato/v11/qIIYRU-oROkIk8vfvxw6QvesZW2xOQ-xsNqO47m55DA.woff) format('woff');
                                        }

                                        @font-face {
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 700;
                                            src: local('Lato Bold'), local('Lato-Bold'), url(https://fonts.gstatic.com/s/lato/v11/qdgUG4U09HnJwhYI-uK18wLUuEpTyoUstqEm5AMlJo4.woff) format('woff');
                                        }

                                        @font-face {
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 400;
                                            src: local('Lato Italic'), local('Lato-Italic'), url(https://fonts.gstatic.com/s/lato/v11/RYyZNoeFgb0l7W3Vu1aSWOvvDin1pK8aKteLpeZ5c0A.woff) format('woff');
                                        }

                                        @font-face {
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 700;
                                            src: local('Lato Bold Italic'), local('Lato-BoldItalic'), url(https://fonts.gstatic.com/s/lato/v11/HkF_qI1x_noxlxhrhMQYELO3LdcAZYWl9Si6vvxL-qU.woff) format('woff');
                                        }
                                    }

                                    /* CLIENT-SPECIFIC STYLES */
                                    body,
                                    table,
                                    td,
                                    a {
                                        -webkit-text-size-adjust: 100%;
                                        -ms-text-size-adjust: 100%;
                                    }

                                    table,
                                    td {
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                    }

                                    img {
                                        -ms-interpolation-mode: bicubic;
                                    }

                                    /* RESET STYLES */
                                    img {
                                        border: 0;
                                        height: auto;
                                        line-height: 100%;
                                        outline: none;
                                        text-decoration: none;
                                    }

                                    table {
                                        border-collapse: collapse !important;
                                    }

                                    body {
                                        height: 100% !important;
                                        margin: 0 !important;
                                        padding: 0 !important;
                                        width: 100% !important;
                                    }

                                    /* iOS BLUE LINKS */
                                    a[x-apple-data-detectors] {
                                        color: inherit !important;
                                        text-decoration: none !important;
                                        font-size: inherit !important;
                                        font-family: inherit !important;
                                        font-weight: inherit !important;
                                        line-height: inherit !important;
                                    }

                                    /* MOBILE STYLES */
                                    @media screen and (max-width:600px) {
                                        h1 {
                                            font-size: 32px !important;
                                            line-height: 32px !important;
                                        }
                                    }

                                    /* ANDROID CENTER FIX */
                                    div[style*='margin: 16px 0;'] {
                                        margin: 0 !important;
                                    }
                                </style>
                            </head>

                            <body style='background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;'>
                            <!-- HIDDEN PREHEADER TEXT -->
                            <div style='display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Lato, Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;'> Вітаємо з реєстрацією на сторінці нашого Барбершопу!. </div>
                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                <!-- LOGO -->
                                <tr>
                                    <td bgcolor='#000000' align='center'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td align='center' valign='top' style='padding: 40px 10px 40px 10px;'> </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#000000' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' valign='top' style='padding: 40px 20px 20px 20px; border-radius: 4px 4px 0px 0px; color: #111111; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 48px; font-weight: 400; letter-spacing: 4px; line-height: 48px;'>
                                                    <h1 style='font-size: 24px; font-weight: 400; margin: 2;'>Вас вітає Барбершоп 'Оазис'!</h1> <img src=""https://i.ibb.co/tDWZZnD/Oasis.jpg"" width = '325' height = '220' alt=""Oasis"" border=""0"" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Вами було вказано даний E-Mail при реєстрації облікового запису в нашому веб-сервісі.</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                           <!-- <tr>
                                                <td bgcolor='#ffffff' align='left' style='padding: 20px 30px 20px 30px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0; text-align:center'>YOUR OPT : *****</p>
                                                </td>
                                            </tr>-->
                                            <tr>
                                                <td bgcolor='#ffffff' align='left'>
                                                    <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                                        <tr>
                                                            <td bgcolor='#ffffff' align='center' style='padding: 20px 30px 30px 30px;'>
                                                                <table border='0' cellspacing='0' cellpadding='0'>
                                                                    <tr>
                                                                        <td align='center' style='border-radius: 3px;' bgcolor='#000000'><a href='" + HtmlEncoder.Default.Encode(callbackUrl) + @"' target='_blank' style='font-size: 20px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; color: #ffffff; text-decoration: none; padding: 15px 25px; border-radius: 2px; border: 1px solid #000000; display: inline-block;'>Підтвердити</a></td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr> <!-- COPY -->
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Якщо ви не вказували даний E-Mail при реєстрації, або вказали інший - не переходьте за цим посиланням.</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor='#ffffff' align='left' style='padding: 0px 30px 40px 30px; border-radius: 0px 0px 4px 4px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Наші соціальні мережі:</p>
                                                    <div>
                                                        <a style='padding-right:10px' href='https://www.instagram.com/boykov_artem'><img src='https://cdn-icons-png.flaticon.com/512/2111/2111463.png' width='25'></a>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 30px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#000000' align='center' style='padding: 30px 30px 30px 30px; border-radius: 4px 4px 4px 4px; color: #fff; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <h2 style='font-size: 20px; font-weight: 400; color: #fff; margin: 0;'>Наші контакти</h2>
                                                    <p style='margin: 0;'><a href='tel:+380685034088' target='_blank' style='color: #fff;'>Тел: +380-68-503-40-88</a></p>
                                                        <p style='margin: 0;'><a href='mailto:barbershop.oasis@ukr.net' target='_blank' style='color: #fff;'>Email: barbershop.oasis@ukr.net</a></p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            </body>
                            </html>");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
