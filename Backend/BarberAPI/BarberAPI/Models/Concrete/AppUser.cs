using System.Threading;

namespace BarberAPI.Models.Concrete
{
    // 1. Temel Kullanıcı Modeli
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Şifreyi asla düz metin saklama!
        public string Role { get; set; } // "Barber", "Customer"

        // İlişki (Opsiyonel ama iyi olur)
        public Barber BarberProfile { get; set; }
        public Customer CustomerProfile { get; set; }
    }
}
