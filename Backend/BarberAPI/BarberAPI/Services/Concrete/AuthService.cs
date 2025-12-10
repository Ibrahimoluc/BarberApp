using BarberAPI.Data;
using BarberAPI.Dto;
using BarberAPI.Models.Concrete;
using BarberAPI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BarberAPI.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        
        public void RegisterBarber(RegisterBarberDto request)
        {
            // Transaction'ı başlatıyoruz
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // --- ADIM 1: Ana Kullanıcıyı (AppUser) Oluştur ---

                    // Not: Şifreleme (Hashing) işlemi normalde burada yapılır.
                    // Örn: var hashedPassword = BCrypt.HashPassword(request.Password);

                    var newUser = new AppUser
                    {
                        Email = request.Email,
                        PasswordHash = "hashed_sifre_buraya",
                        Role = "Barber" // Rolünü sabitliyoruz
                    };

                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    // DİKKAT: SaveChanges burada şart! 
                    // Neden? Çünkü newUser.Id henüz oluşmadı (0). 
                    // Kaydettiğimiz an veritabanı ona bir ID (örn: 5) atar.

                    // --- ADIM 2: Berber Profilini ve Bölgeleri Hazırla ---
                    // Not: Bu kısım da AppUser içine taşınıp tek saveChanges de işlem halledilebilir.

                    var newBarberProfile = new Barber
                    {
                        AppUserId = newUser.Id,
                        ShopName = request.ShopName,
                        ShopAddress = request.ShopAddress,
                        IsHomeServiceActive = request.IsHomeServiceActive,
                        IsShopServiceActive = request.IsShopServiceActive,

                        // BURASI YENİ:
                        // Eğer listede veri varsa, DTO'ları Entity'e çevirip listeye ekliyoruz.
                        ServiceAreas = request.ServiceAreas != null
                            ? request.ServiceAreas.Select(x => new BarberServiceArea
                            {
                                City = x.City,
                                District = x.District
                                // BarberId vermene gerek yok! EF Core otomatik bağlayacak.
                            }).ToList()
                            : new List<BarberServiceArea>()
                    };

                    _context.Barbers.Add(newBarberProfile);

                    // --- ADIM 3: Tek Seferde Kaydet ---

                    // Burada SaveChanges dediğinde:
                    // 1. Önce Barber tablosuna yazar (ID: 5 oluşur).
                    // 2. Sonra ServiceAreas listesindeki her elemana BarberId = 5 atar.
                    // 3. Onları da BarberServiceArea tablosuna yazar.
                    _context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // --- ADIM 4: Felaket Senaryosu (Rollback) ---

                    // Herhangi bir hata olursa (örn: Dükkan adı null geldi, SQL hatası vs.)
                    // Transaction.Commit() çalışmadığı için, using bloğundan çıkarken
                    // otomatik olarak Rollback yapılır. 
                    // Yani AppUser kaydedilmiş olsa bile SİLİNİR (geri alınır).

                    transaction.Rollback();
                    throw new Exception("Kayıt sırasında bir hata oluştu: " + ex.Message);
                }
            }
        }
    }
}
