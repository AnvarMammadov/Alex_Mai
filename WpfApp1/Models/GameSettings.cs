using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class GameSettings : ObservableObject
    {
        [ObservableProperty]
        private int _masterVolume = 80; // 0-100 arasında bir dəyər

        [ObservableProperty]
        private int _musicVolume = 100;

        [ObservableProperty]
        private int _sfxVolume = 100;

        // Gələcəkdə bura mətn sürəti, pəncərə rejimi kimi başqa ayarlar da əlavə edəcəyik
    }
}
