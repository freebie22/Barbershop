// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Barbershop.Models;
using Mailjet.Client.Resources.SMS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Barbershop.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
 

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string FullName { get; set; }

            public string PhoneNumber { get; set; }

            public DateTime DateOfBirth { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if(!await _roleManager.RoleExistsAsync(WC.AdminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(WC.AdminRole));
                await _roleManager.CreateAsync(new IdentityRole(WC.BarberRole));
                await _roleManager.CreateAsync(new IdentityRole(WC.ClientRole));
            }

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                var user = new BarbershopUser { UserName = Input.UserName, Email = Input.Email, PhoneNumber = Input.PhoneNumber, FullName = Input.FullName, DateOfBirth = Input.DateOfBirth };

                var existUser = await _userManager.FindByEmailAsync(user.Email);

                if (existUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Дана електронна адреса вже використовується іншим користувачем.");
                    return Page();
                }

                var existName = await _userManager.FindByNameAsync(user.UserName);

                if (existName != null)
                {
                    ModelState.AddModelError(string.Empty, "Даний нікнейм вже використоується іншим користувачем.");
                    return Page();
                }

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {

                    if (User.IsInRole(WC.AdminRole))
                    {
                        await _userManager.AddToRoleAsync(user, WC.AdminRole);
                    }

                    else
                    {
                        await _userManager.AddToRoleAsync(user, WC.ClientRole);
                    }
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
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

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
