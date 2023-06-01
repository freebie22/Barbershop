// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Areas.Identity.Pages.Account.Manage
{
    public class UserAppointmentsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserAppointmentsModel(
            UserManager<IdentityUser> userManager, ApplicationDbContext db
           )
        {
            _db = db;
            _userManager = userManager;

        }

        public async Task <IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            AppointmentListVM appointmentListVM = new AppointmentListVM()
            {
                AppointmentList = _db.Appointments.Include(a => a.Barber).Where(a => a.UserId == user.Id),
                StatusList = WC.listAppointmentStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                }),
                AppointmentType = WC.listAppointmentType.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            return Partial("UserAppointments", appointmentListVM);
        }

            //private async Task LoadAsync(BarbershopUser user)
            //{
            //    var userName = await _userManager.GetUserNameAsync(user);
            //    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //    var fullName = user.FullName;
            //    var dateofbirth= user.DateOfBirth;

            //    Input = new InputModel
            //    {
            //        UserName = userName,
            //        PhoneNumber = phoneNumber,
            //        FullName = fullName,
            //        DateOfBirth = dateofbirth
            //    };
            //}

            //public async Task<IActionResult> OnGetAsync()
            //{
            //    var user = (BarbershopUser)await _userManager.GetUserAsync(User);
            //    if (user == null)
            //    {
            //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //    }

            //    await LoadAsync(user);
            //    return Page();
            //}

            //public async Task<IActionResult> OnPostAsync()
            //{
            //    var user = (BarbershopUser)await _userManager.GetUserAsync(User);
            //    if (user == null)
            //    {
            //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //    }

            //    if (!ModelState.IsValid)
            //    {
            //        await LoadAsync(user);
            //        return Page();
            //    }

            //    if (Input.PhoneNumber != user.PhoneNumber)
            //    {
            //        user.PhoneNumber = Input.PhoneNumber;
            //        var setPhoneResult = await _userManager.UpdateAsync(user);
            //        if (!setPhoneResult.Succeeded)
            //        {
            //            StatusMessage = "Unexpected error when trying to set phone number.";
            //            return RedirectToPage();
            //        }
            //    }

            //    if (Input.UserName != user.UserName)
            //    {
            //        user.UserName = Input.UserName;
            //        var setUsernameResult = await _userManager.UpdateAsync(user);
            //        if (!setUsernameResult.Succeeded)
            //        {
            //            StatusMessage = "Unexpected error when trying to set user name.";
            //            return RedirectToPage();
            //        }
            //    }

            //    if (Input.FullName != user.FullName)
            //    {
            //        user.FullName = Input.FullName;
            //        var setFullNameResult = await _userManager.UpdateAsync(user);
            //        if (!setFullNameResult.Succeeded)
            //        {
            //            StatusMessage = "Unexpected error when trying to set user name.";
            //            return RedirectToPage();
            //        }
            //    }

            //    await _signInManager.RefreshSignInAsync(user);
            //    StatusMessage = "Дані вашого профіля було успішно оновлено!";
            //    return RedirectToPage();
            //}
        }
}
