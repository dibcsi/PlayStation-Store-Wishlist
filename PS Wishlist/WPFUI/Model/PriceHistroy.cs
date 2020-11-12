using Newtonsoft.Json;
using System.Security.Policy;
using System.Windows.Media;

namespace WPFUI
{
    public class PriceHistroy
    {
        public string CheckDateTime { get; set; }
        public string OriginalPrice { get; set; }
        public string DiscountPrice { get; set; }
        public string PSPlusPrice { get; set; }


    }
}
