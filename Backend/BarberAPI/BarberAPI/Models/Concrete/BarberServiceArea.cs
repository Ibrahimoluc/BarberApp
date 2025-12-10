namespace BarberAPI.Models.Concrete
{
    // Yeni Tablo: Berberin Hizmet Verdiği Bölgeler
    public class BarberServiceArea
    {
        public int Id { get; set; }

        public int BarberId { get; set; } // Hangi berber?
        public Barber Barber { get; set; }

        public string City { get; set; } // Örn: "İstanbul"
        public string District { get; set; } // Örn: "Kadıköy"
    }
}
