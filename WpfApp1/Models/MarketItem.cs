using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class MarketItem
    {
        public string ItemId { get; set; }      // İnventara əlavə olunacaq unikal ID, məs: "cigarette_pack"
        public string Name { get; set; }        // Ekranda görünən ad, məs: "Siqaret Qutusu"
        public int Price { get; set; }          // Qiyməti
        public string IconPath { get; set; }    // Marketdəki şəkli
        public string Category { get; set; }    // Kateqoriyası, məs: "Food", "Other"
        public string Description { get; set; } // Açıqlama (opsional)
    }
}
