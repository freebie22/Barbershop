using Microsoft.AspNetCore.Identity;
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
        [NotMapped]
        public string DeliveryMethod { get; set; }
        [NotMapped]
        public string Region { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        public string PostOffice { get; set; }
        [NotMapped]
        public string PaymentType { get; set; }





       
    }
}
