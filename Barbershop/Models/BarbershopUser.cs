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

            OrderCount = 0;
            OrderSuccessCount = 0;
            OrderPoints = 0;

            AppointmentCount = 0;
            AppointmentSuccessCount = 0;
            AppointmentPoints = 0;

            LastPromoCodeGeneratedDate = DateTime.MinValue;
        }

        public int OrderCount { get; set; } = 0;
        public int OrderSuccessCount { get; set; } = 0;
        public int OrderPoints { get; set; } = 0;

        public int AppointmentCount { get; set; } = 0; 
        public int AppointmentSuccessCount { get; set; } = 0;
        public int AppointmentPoints { get; set; } = 0;


        public DateTime LastPromoCodeGeneratedDate { get; set; }

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
