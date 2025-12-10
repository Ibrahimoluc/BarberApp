using BarberAPI.Dto;
using BarberAPI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BarberAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService; 

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-barber")]
        public IActionResult RegisterBarber(RegisterBarberDto request)
        {
            // 1. Validasyon (Gerekirse)
            //if (!ModelState.IsValid) return BadRequest();

            // 2. İşi Servise Pasla
            _authService.RegisterBarber(request);
            return Ok();
        }
    }
}
