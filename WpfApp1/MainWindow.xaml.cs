using System;
using System.Windows;
using System.Windows.Media; // Bu using sətrini əlavə edin
using Alex_Mai.ViewModels;  // Bu namespace-i öz layihənizə uyğunlaşdırın

namespace Alex_Mai // Bu namespace-i öz layihənizə uyğunlaşdırın
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

       
    }
}