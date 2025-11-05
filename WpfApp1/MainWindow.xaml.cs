using System;
using System.Windows;
using System.Windows.Controls;
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


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                // Düymənin ikonunu "Böyüt" olaraq dəyiş
                (sender as Button).Content = "\uE922";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                // Düymənin ikonunu "Bərpa et" olaraq dəyiş
                (sender as Button).Content = "\uE923";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}