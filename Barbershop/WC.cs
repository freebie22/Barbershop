using System.Collections.ObjectModel;

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

        public const string AppointmentReceived = "Очікує на клієнта";
        public const string AppointmentDone = "Заверешене";
        public const string AppointmentCancelled = "Відмінене";

        public const string ClientOnline = "Онлайн";
        public const string ClientPhone = "По телефону";
        public const string ClientOnPlace = "В салоні";

        public const string CardPay = "Оплата картою";
        public const string ReceivePay = "Оплата при отриманні";

        public static readonly IEnumerable<string> listStatus = new ReadOnlyCollection<string>(
           new List<string>
           {
                 StatusPending, StatusApproved, StatusInProcess, StatusArrived, StatusReceived, StatusCancelled, StatusReturned
           });
        public static readonly IEnumerable<string> listAppointmentStatus = new ReadOnlyCollection<string>(
           new List<string>
           {
                 AppointmentReceived, AppointmentDone, AppointmentCancelled
           });
        public static readonly IEnumerable<string> listAppointmentType = new ReadOnlyCollection<string>(
           new List<string>
           {
                 ClientOnline, ClientPhone, ClientOnPlace
           });
    }

}

