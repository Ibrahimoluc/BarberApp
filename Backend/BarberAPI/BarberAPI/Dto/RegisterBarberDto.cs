using BarberAPI.Models.Concrete;

namespace BarberAPI.Dto
{
    public class RegisterBarberDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsShopServiceActive { get; set; }
        public bool IsHomeServiceActive { get; set; }
        public string? ShopName { get; set; }
        public string? ShopAddress { get; set; }
        public List<BarberServiceAreaDto>? ServiceAreas { get; set; }

    }
}
