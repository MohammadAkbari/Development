using System.ComponentModel.DataAnnotations;

namespace MvcWork.Controllers
{
    public enum SellType
    {
        Weight,
        Count
    }

    public class SellVm
    {
        [Required]
        public SellType SellType { get; set; }

        public WeightSellVm WeightSellVm { get; set; }

        public CountSellVm CountSellVm { get; set; }
    }

    public class WeightSellVm
    {
        [Required]
        [Range(1, 5)]
        public int Weight { get; set; }

        //more properties with validation
    }

    public class CountSellVm
    {
        [Required]
        [Range(1, 20)]
        public int Count { get; set; }

        //more properties with validation
    }
}
