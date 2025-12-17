using BarberAPI.Dto;

namespace BarberAPI.Services.Abstract
{
    public interface IAuthService
    {
        public Task RegisterBarberAsync(RegisterBarberDto request);
        public Task<string?> CompleteBarberProfileAsync(int userId, CompleteBarberProfileDto request);

        public Task VerifyEmailCode(VerifyEmailCodeDto request);

        public Task<string?> LoginAsync(LoginRequestDto request);
        public Task<string?> GoogleAuthAsync(GoogleAuthDto request);
        public Task<string?> ChooseRoleAsync(int userId, string selectedRole);
    }
}
