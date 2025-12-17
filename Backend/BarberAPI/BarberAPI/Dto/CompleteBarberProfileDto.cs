using BarberAPI.Models.Concrete;

namespace BarberAPI.Dto
{
    public class CompleteBarberProfileDto
    {
        public bool IsShopServiceActive { get; set; }
        public bool IsHomeServiceActive { get; set; }
        public string? ShopName { get; set; }
        public string? ShopAddress { get; set; }
        public List<BarberServiceAreaDto>? ServiceAreas { get; set; }
    }
}
