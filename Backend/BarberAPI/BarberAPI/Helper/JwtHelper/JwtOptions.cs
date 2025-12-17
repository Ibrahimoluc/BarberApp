namespace BarberAPI.Helper.JwtHelper
{
    public class JwtOptions
    {
        public string Issuer { get; set; }   // Kim dağıtıyor? (www.berberapp.com)
        public string Audience { get; set; } // Kim kullanacak? (www.berberapp.com)
        public string SecretKey { get; set; } // İmza için gizli anahtar
        public int AccessTokenExpiration { get; set; } // Dakika cinsinden ömür
    }
}
