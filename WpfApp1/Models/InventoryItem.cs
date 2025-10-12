using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class InventoryItem : ObservableObject
    {
        public string ItemId { get; set; } // Əşyanın unikal adı, məs: "cigarette"
        public string Name { get; set; }   // Ekranda görünən adı, məs: "Siqaret Paketi"
        public string IconPath { get; set; } // Şəklinin yolu, məs: "/Assets/Icons/cigarette_icon.png"

        // YENİ: Sayı izləmək üçün observable property
        [ObservableProperty] private int _quantity = 1;
    }
}
