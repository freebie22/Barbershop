namespace Barbershop
{
    public static class WC
    {
        public static string BarberPath = @"\images\barber_images\";
        public static string SpecPath = @"\images\spec_images\";
        public static string ProductPath = @"\images\prod_images\";
        public static string GalleryPath = @"\images\prod_gallery\";

        public const string SessionCart = "ShoppingCartSession";

        public const string AdminRole = "Admin";
        public const string BarberRole = "Barber";
        public const string ClientRole = "Client";

        public const string StatusPending = "В очікуванні";
        public const string StatusApproved = "Прийняте до обробки";
        public const string StatusInProcess = "В дорозі";
        public const string StatusArrived = "Очікує замовника";
        public const string StatusReceived = "Отримане";
        public const string StatusCancelled = "Відмінене";
        public const string StatusReturned = "Повернене";


    }
}
