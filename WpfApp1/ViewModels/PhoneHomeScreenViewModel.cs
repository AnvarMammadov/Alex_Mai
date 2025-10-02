using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class PhoneHomeScreenViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        public PhoneHomeScreenViewModel(PhoneViewModel parent) { _parentViewModel = parent; }

        [RelayCommand]
        private void NavigateToStats() { _parentViewModel.NavigateToStats(); }
    }
}
