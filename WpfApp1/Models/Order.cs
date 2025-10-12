using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class Order
    {
        public string Name { get; set; } // Məs: "Səhər Sifarişi"
        public List<string> ProductOrder { get; set; } // Düzgün ardıcıllıq
        public int Reward { get; set; } // Uğurlu sifariş üçün qazanılacaq pul
    }
}
