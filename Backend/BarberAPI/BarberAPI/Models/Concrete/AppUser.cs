using System.Threading;

namespace BarberAPI.Models.Concrete
{
    // 1. Temel Kullanıcı Modeli
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; } = false; 
        public string? EmailVerificationCode { get; set; }

        // Kodun Son Geçerlilik Tarihi
        public DateTime? EmailVerificationCodeExpiration { get; set; }
        public string? PasswordHash { get; set; } // Şifreyi asla düz metin saklama!
        public string Role { get; set; } // "Barber", "Customer"
        public string? GoogleId { get; set; }
        public bool IsProfileCompleted { get; set; } = false;
        public Barber BarberProfile { get; set; }
        public Customer CustomerProfile { get; set; }
    }
}
