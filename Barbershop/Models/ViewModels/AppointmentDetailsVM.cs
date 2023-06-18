namespace Barbershop.Models.ViewModels
{
    public class AppointmentDetailsVM
    {
        public Appointment Appointment { get; set; }
        public IEnumerable<AppointmentDetail> AppointmentDetails { get; set; }
    }
}
