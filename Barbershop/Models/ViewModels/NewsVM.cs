namespace Barbershop.Models.ViewModels
{
    public class NewsVM
    {
        public News News { get; set; }
        public List<NewsImages> NewsImages { get; set; }
        public List<int> ImageIds { get; set; }
    }
}
