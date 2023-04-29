using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }
        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public BarbershopUser CreatedBy { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        [Required]
        public decimal FinalOrderTotal { get; set; }
        public string OrderStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string PostOffice{ get; set; }
        [Required]
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
