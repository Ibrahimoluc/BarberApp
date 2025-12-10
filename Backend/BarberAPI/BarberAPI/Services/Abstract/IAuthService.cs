using BarberAPI.Dto;

namespace BarberAPI.Services.Abstract
{
    public interface IAuthService
    {
        public void RegisterBarber(RegisterBarberDto request);
    }
}
