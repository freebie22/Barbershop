namespace Barbershop.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Barbers = new Barbers();
        }

        public Barbers Barbers { get; set; }
    }
}
