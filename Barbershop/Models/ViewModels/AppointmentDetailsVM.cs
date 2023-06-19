namespace Barbershop.Models.ViewModels
{
    public class AppointmentDetailsVM
    {
        public Appointment Appointment { get; set; }
        public BarbershopUser User { get; set; }
        public string Guest { get; set; }
        public IEnumerable<AppointmentDetail> AppointmentDetails { get; set; }
    }
}
