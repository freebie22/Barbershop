// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Barbershop.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Запит на зміну електронної пошти",
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
                            <div style='display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Lato, Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;'> Запит на зміну електронної пошти </div>
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
                                                    <h1 style='font-size: 24px; font-weight: 400; margin: 2;'>Зміна електронної пошти</h1> <img src=""https://i.ibb.co/tDWZZnD/Oasis.jpg"" width = '325' height = '220' alt=""Oasis"" border=""0"" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>З Вашого облікового запису надійшов запит на зміну електронної пошти.</p>
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
                                                                        <td align='center' style='border-radius: 3px;' bgcolor='#000000'><a href='" + HtmlEncoder.Default.Encode(callbackUrl) + @"' target='_blank' style='font-size: 20px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; color: #ffffff; text-decoration: none; padding: 15px 25px; border-radius: 2px; border: 1px solid #000000; display: inline-block;'>Змінити</a></td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr> <!-- COPY -->
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Якщо ви не робили запит на зміну електронної пошти - проігноруйте це повідомлення.</p>
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

                StatusMessage = "Перевірте Вашу електронну пошту на наявність повідомлення про зміну E-Mail";
                return RedirectToPage();
            }

            StatusMessage = "Ваша електронна пошта не змінилась!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Запит на підвтердження електронної пошти",
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
                            <div style='display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Lato, Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;'> Запит на підтвердження електронної пошти </div>
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
                                                    <h1 style='font-size: 24px; font-weight: 400; margin: 2;'>Підтвердження електронної пошти</h1> <img src=""https://i.ibb.co/tDWZZnD/Oasis.jpg"" width = '325' height = '220' alt=""Oasis"" border=""0"" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>З Вашого облікового запису надійшов запит на підтвердження електронної пошти.</p>
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
                                                    <p style='margin: 0;'>Якщо ви не робили запит на підтвердження електронної пошти - проігноруйте це повідомлення.</p>
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

            StatusMessage = "Перевірте Вашу електронну пошту на наявність повідомлення про підтвердження E-Mail";
            return RedirectToPage();
        }
    }
}
