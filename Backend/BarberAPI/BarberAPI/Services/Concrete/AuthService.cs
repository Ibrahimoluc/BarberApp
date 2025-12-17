using BarberAPI.Data;
using BarberAPI.Dto;
using BarberAPI.Exceptions;
using BarberAPI.Helper.GmailHelper;
using BarberAPI.Helper.JwtHelper;
using BarberAPI.Models.Concrete;
using BarberAPI.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using BarberAPI.Helper.GoogleWebAppHelper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BarberAPI.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IMailService _mailService;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly GoogleWebAppOptions _googleWebAppOptions;

        public AuthService(AppDbContext context, IMailService mailService, IPasswordHasher<AppUser> passwordHasher, 
            ITokenService tokenService, IOptions<GoogleWebAppOptions> googleWebAppOptions)
        {
            _context = context;
            _mailService = mailService;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _googleWebAppOptions = googleWebAppOptions.Value;
        }


        public async Task RegisterBarberAsync(RegisterBarberDto request)
        {
            // --- KONTROL 1: Email Kontrolü (Guard Clause) ---
            // Transaction'a girmeden önce, ucuz ve hızlı bir sorguyla bakıyoruz.
            // AnyAsync: Kaydı getirme, sadece var mı diye bak (True/False). Çok hızlıdır.
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                // Özel bir hata fırlatıyoruz. 
                // Controller bunu yakalayıp kullanıcıya net mesaj gösterebilir.
                throw new Exception("Bu email adresi ile daha önce kayıt yapılmış.");
            }

            // Transaction da Async olmalı
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // --- ADIM 0: Doğrulama Kodunu Üret ---
                    var verificationCode = new Random().Next(100000, 999999).ToString();

                    // --- ADIM 1: Ana Kullanıcıyı Oluştur ---
                    var newUser = new AppUser
                    {
                        Email = request.Email,
                        Role = "Barber",
                        // Kodları buraya ekliyoruz
                        EmailVerificationCode = verificationCode,
                        EmailVerificationCodeExpiration = DateTime.UtcNow.AddMinutes(5),
                        IsEmailVerified = false,
                        GoogleId = null,
                        IsProfileCompleted = true,
                    };

                    newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync(); // Async kaydetme

                    // --- ADIM 2: Berber Profili ---
                    var newBarberProfile = new Barber
                    {
                        AppUserId = newUser.Id,
                        ShopName = request.ShopName,
                        ShopAddress = request.ShopAddress,
                        IsHomeServiceActive = request.IsHomeServiceActive,
                        IsShopServiceActive = request.IsShopServiceActive,
                        ServiceAreas = request.ServiceAreas?.Select(x => new BarberServiceArea
                        {
                            City = x.City,
                            District = x.District
                        }).ToList()
                    };

                    _context.Barbers.Add(newBarberProfile);
                    await _context.SaveChangesAsync();

                    // --- ADIM 3: MAİL GÖNDERME (Kritik Nokta) ---

                    var emailRequest = new SendEmailRequest
                    {
                        Recipient = newUser.Email,
                        Subject = "BerberApp Doğrulama Kodu",
                        Body = $"Hoşgeldiniz! Doğrulama kodunuz: {verificationCode}"
                    };

                    // Eğer burada hata alırsak catch bloğuna düşer ve Rollback olur.
                    // Böylece "Mail gitmedi ama kullanıcı oluştu" sorunu yaşanmaz.
                    await _mailService.SendEmailAsync(emailRequest);

                    // --- ADIM 4: Mutlu Son ---
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Hatayı fırlat ki Controller bunu yakalayıp 400 dönsün
                    throw new Exception("Kayıt işlemi başarısız: " + ex.Message);
                }
            }
        }


        //[Authorize] // Artık tokenı var, authorize olabilir!
        //[HttpPost("complete-barber-profile")]
        public async Task<string?> CompleteBarberProfileAsync(int userId, CompleteBarberProfileDto request)
        {
            // Token'dan User ID'yi al
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User bulunamadı");

            if (user.IsProfileCompleted)
                throw new Exception("Zaten tamamlanmış.");

            // --- BURADA TC KİMLİK DOĞRULAMA, Başka doğrulamalar YAPABİLİRSİN ---
            // bool tcValid = await _eDevletService.Verify(request.TcNo, request.BirthYear...);
            // if (!tcValid) return BadRequest("TC Kimlik doğrulanamadı.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Barber Tablosunu Şimdi Oluşturuyoruz (Zorunlu alanlarla)
                    var newBarber = new Barber
                    {
                        AppUserId = userId,
                        ShopName = request.ShopName,
                        ShopAddress = request.ShopAddress,
                        IsHomeServiceActive = request.IsHomeServiceActive,
                        IsShopServiceActive = request.IsShopServiceActive,
                        ServiceAreas = request.ServiceAreas?.Select(x => new BarberServiceArea
                        {
                            City = x.City,
                            District = x.District
                        }).ToList()
                    };

                    _context.Barbers.Add(newBarber);

                    // 2. User'ı güncelle
                    user.IsProfileCompleted = true; // Bayrağı kaldır

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return _tokenService.CreateToken(user);
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Kayıt tamamlama işlemi başarısız: " + ex.Message);
                }
            }
        }


        public async Task VerifyEmailCode(VerifyEmailCodeDto request)
        {
            // request den gelen email ve code FluentValid ile sonradan kontrol edilebilir.

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new Exception("Verilen email sistemde kayıtlı değil");
            }

            if (user.IsEmailVerified) return;

            string code = user.EmailVerificationCode;
            if (code == null)
            {
                throw new Exception("Verilen user için sistemde kod bulunmuyor");
            }

            if(request.Code != code)
            {
                throw new Exception("Girdiğiniz kod yanlış");
            }


            if(user.EmailVerificationCodeExpiration < DateTime.UtcNow)
            {
                throw new Exception("Girdiğiniz kodun geçerlilik süresi dolmuş.");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeExpiration = null;
            await _context.SaveChangesAsync();
        }


        public async Task<string?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return null;

            // Eğer şifresi null ise, bu adam Google ile gelmiştir. Şifreyle giremez!
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                // Burada null dönmek yerine "Lütfen Google ile giriş yapın" hatası fırlatmak daha şıktır
                // ama null dönerek "Email veya şifre hatalı" mesajı verdirmek de güvenlik açısından kabul görür.
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(result != PasswordVerificationResult.Success) return null;

            if (!user.IsEmailVerified) throw new UserNotVerifiedException("Hesabınız doğrulanmamış.");

            return _tokenService.CreateToken(user);
        }

        public async Task<string?> GoogleAuthAsync(GoogleAuthDto request)
        {
            Console.WriteLine("client_id:" + _googleWebAppOptions.ClientId);
            GoogleJsonWebSignature.Payload payload;
            try
            {
                // 1. Google'a soruyoruz: "Bu token senin mi?"
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _googleWebAppOptions.ClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                // payload içinde: Email, Sub (GoogleId), Name, Picture vs. var.
            }
            catch (InvalidJwtException)
            {
                throw new Exception("Geçersiz Google Token");
            }

            // 2. Kullanıcı bizde kayıtlı mı? (ÖNCE EMAİL KONTROLÜ)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
            bool isNewUser = false;

            if (user == null)
            {
                isNewUser = true;
                user = new AppUser
                {
                    Email = payload.Email,
                    GoogleId = payload.Subject, // Google'ın user ID'si
                    Role = "NewUser",
                    IsEmailVerified = true, // Google'dan geldiyse zaten email onaylıdır!
                    IsProfileCompleted = false,
                    PasswordHash = null // Şifresi yok
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Kullanıcı zaten var ama GoogleId'si yoksa ekle (Hesapları bağla)
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = payload.Subject;
                    user.IsEmailVerified = true; // Google ile girdiyse onayı hak etti
                    user.EmailVerificationCode = null; // 3. Varsa eski kod çöpünü sil
                    user.EmailVerificationCodeExpiration = null;
                    await _context.SaveChangesAsync();
                }
            }

            return _tokenService.CreateToken(user);
        }

        public async Task<string?> ChooseRoleAsync(int userId, string selectedRole)
        {
            // 1. Kullanıcıyı bul
            var user = await _context.Users.FindAsync(userId);

            // 2. Güvenlik: Zaten rol seçmişse tekrar değiştiremesin!
            if (user.Role != "NewUser")
                throw new Exception("Zaten bir rolünüz var.");

            // 3. Rol Ataması
            if (selectedRole == "Customer")
            {
                user.Role = "Customer";
                user.IsProfileCompleted = true; // Müşterinin işi bitti
            }
            else if (selectedRole == "Barber")
            {
                user.Role = "Barber";
                user.IsProfileCompleted = false; // Berberin daha işi var (Dükkan bilgileri)
            }
            else
            {
                throw new Exception("Geçersiz rol seçimi.");
            }

            await _context.SaveChangesAsync();

            // 4. ARTIK GERÇEK KİMLİĞİYLE YENİ TOKEN VERİYORUZ
            return _tokenService.CreateToken(user);
        }
    }
}
