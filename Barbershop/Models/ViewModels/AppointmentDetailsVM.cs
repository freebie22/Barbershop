namespace Barbershop.Models.ViewModels
{
    public class AppointmentDetailsVM
    {
        public Appointment Appointment { get; set; }
        public IEnumerable<Appointment> Services { get; set; }
    }
}
