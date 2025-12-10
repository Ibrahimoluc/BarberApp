namespace BarberAPI.Models.Concrete
{
    public class Barber
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // --- HİZMET TÜRÜ AYARLARI ---

        // Dükkanda hizmet veriyor mu?
        public bool IsShopServiceActive { get; set; }

        // Eve hizmet veriyor mu?
        public bool IsHomeServiceActive { get; set; }

        // --- DÜKKAN BİLGİLERİ (Nullable / Opsiyonel) ---
        // Eğer IsShopServiceActive = false ise buralar NULL olabilir.
        public string? ShopName { get; set; }
        public string? ShopAddress { get; set; }

        // Harita için
        //public string? ShopLocationLat { get; set; } 
        //public string? ShopLocationLng { get; set; }


        // --- EV HİZMETİ BİLGİLERİ (Nullable / Opsiyonel) ---
        // Eğer IsHomeServiceActive = false ise buralar NULL olabilir.

        // Kaç km uzağa kadar gidiyor? (Örn: 10km)
        // Bu tam konum alınıp müşteri ve berber arasındaki mesafe hesaplanmaya başlandığı zaman kullanılcak
        // Şimdilik il, ilçe seçimine göre adres konusu halledilcek
        //public int? ServiceRadiusKm { get; set; }
        public List<BarberServiceArea>? ServiceAreas { get; set; }

        // Eve hizmet için ekstra yol ücreti alıyor mu?
        //public decimal? HomeServiceExtraFee { get; set; }

        // Minimum sepet tutarı (Örn: Eve gelmem için en az 500TL'lik işlem yapmalısın)
        //public decimal? MinHomeServiceOrderPrice { get; set; }
    }
}
