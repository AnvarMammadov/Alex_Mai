using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class CharacterStats : ObservableObject
    {
        [ObservableProperty]
        private int _affection = 0; // Sevgi -- Love

        [ObservableProperty]
        private int _trust = 0; // Güvən -- Trust

        [ObservableProperty]
        private int _stress = 10; // Stress -- Stress

        [ObservableProperty] private int _corruption = 0;
    }
}
