namespace BarberAPI.Models.Concrete
{
    public class Customer
    {
        public int Id { get; set; }

        // --- BAĞLANTI (Foreign Key) ---
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } // Kimlik kartına bağlantı

        // --- MÜŞTERİYE ÖZEL ALANLAR ---

        // Müşterinin kayıtlı adresleri (Eve hizmet için kritik!)
        // Not: Address diye ayrı bir model açıp One-to-Many ilişki kurabilirsin.
        // Şimdilik basit tutmak için string diyelim veya JSON tutalım.
        public List<string> SavedAddresses { get; set; }

        //public int LoyaltyPoints { get; set; } // Puanlar

        public string? ProfileImage { get; set; } // Müşteri avatarı
    }

    // Müşterinin Adresleri için küçük bir yardımcı model
    public class CustomerAddress
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; } // "Evim", "Ofis"
        public string FullAddress { get; set; }
        public string LocationLat { get; set; }
        public string LocationLng { get; set; }
    }
}
