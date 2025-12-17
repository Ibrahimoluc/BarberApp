using BarberAPI.Models.Concrete;

namespace BarberAPI.Helper.JwtHelper
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}
