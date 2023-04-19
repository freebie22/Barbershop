﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbershop.Models
{
    public class BarbershopUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        public BarbershopUser()
        {
            DateOfBirth = new DateTime(1900, 1, 1);
        }
    }
}