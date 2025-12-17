using System.Security.Claims;
using BarberAPI.Dto;
using BarberAPI.Exceptions;
using BarberAPI.Helper.GmailHelper;
using BarberAPI.Helper.JwtHelper;
using BarberAPI.Models.Concrete;
using BarberAPI.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService; 
        private readonly IMailService _mailService;

        public AuthController(IAuthService authService, IMailService mailService)
        {
            _authService = authService;
            _mailService = mailService;
        }

        [HttpPost("register-barber")]
        public async Task<IActionResult> RegisterBarber(RegisterBarberDto request)
        {
            // 1. Validasyon (Gerekirse)
            //if (!ModelState.IsValid) return BadRequest();

            // 2. İşi Servise Pasla
            await _authService.RegisterBarberAsync(request);
            return Ok();
        }


        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode(VerifyEmailCodeDto request)
        {
            await _authService.VerifyEmailCode(request);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);

                if (token == null)
                {
                    return Unauthorized(new { message = "E-posta veya şifre hatalı." });
                }

                return Ok(new { token });
            }

            catch (UserNotVerifiedException ex)
            {
                // Frontendci Arkadaşa Not: 
                // 403 Status Code gelirse kullanıcıyı "Doğrulama Kodu Gir" ekranına at.
                return StatusCode(403, new
                {
                    error = "UserNotVerified",
                    message = ex.Message
                });
            }
            // --- GENEL HATA BLOĞU ---
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası." });
            }
        }

        [Authorize(Roles = "Barber")]
        [HttpPost("complete-barber-profile")]
        public async Task<IActionResult> CompleteBarberProfile(CompleteBarberProfileDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var token = await _authService.CompleteBarberProfileAsync(userId, request);

            return Ok(new {token});
        }

        [HttpPost("google-auth")]
        public async Task<IActionResult> GoogleAuth(GoogleAuthDto request)
        {
            var token = await _authService.GoogleAuthAsync(request);
            // 4. Frontend'e "Ne yapması gerektiğini" söyle
            return Ok(new { token });
        }

        [Authorize]
        [HttpPost("choose-role")]
        public async Task<IActionResult> ChooseRole([FromBody] ChooseRoleDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var newToken = await _authService.ChooseRoleAsync(userId, request.SelectedRole);
                return Ok(new { token = newToken });
            }
            catch (Exception ex)
            {
                // "Zaten rolünüz var" veya "Geçersiz rol" hatalarını 400 dönüyoruz
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            await _mailService.SendEmailAsync(request);
            return Ok();
        }
    }
}
