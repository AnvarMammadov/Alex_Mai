using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class CalendarEvent : ObservableObject
    {
        public int Day { get; set; } // Hadisənin hansı gündə baş verəcəyi (məsələn, 5)
        public string Title { get; set; } // Hadisənin başlığı (məsələn, "Mai's Exam")
        public string IconKey { get; set; } // "Exam", "Birthday", "Festival" (gələcəkdə ikon göstərmək üçün)

        // Bu xüsusiyyətlər (properties) JSON-dan yüklənməyəcək,
        // ViewModel tərəfindən hesablanacaq.
        [ObservableProperty]
        private bool _isPast = false;

        [ObservableProperty]
        private bool _isToday = false;
    }
}
