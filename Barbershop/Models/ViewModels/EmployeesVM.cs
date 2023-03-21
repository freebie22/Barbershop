namespace Barbershop.Models.ViewModels
{
    public class EmployeesVM
    {
        public IEnumerable<Barbers> Barbers { get; set; }
        public IEnumerable<WorkPositions> WorkPositions { get; set; }
    }
}
