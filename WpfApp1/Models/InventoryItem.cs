using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class InventoryItem
    {
        public string ItemId { get; set; } // Əşyanın unikal adı, məs: "cigarette"
        public string Name { get; set; }   // Ekranda görünən adı, məs: "Siqaret Paketi"
        public string IconPath { get; set; } // Şəklinin yolu, məs: "/Assets/Icons/cigarette_icon.png"
    }
}
