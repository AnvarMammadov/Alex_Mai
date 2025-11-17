using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        private readonly CalendarService _calendarService;
        private readonly GameState _gameState;

        // View (XAML) bu siyahıya bağlanacaq
        public ObservableCollection<CalendarEvent> Events { get; set; } = new ObservableCollection<CalendarEvent>();
        // === YENİ XÜSUSİYYƏTLƏR (PROPERTIES) ===
        // Bu xüsusiyyətlər View-dəki yeni "Tarix Bölməsi" üçün lazımdır
        public int CurrentDay => _gameState.CurrentDay;
        public string CurrentDayOfWeek => _gameState.DayOfWeek;
        public string CurrentMonth => "September"; // Oyun sentyabrda keçir
        // === YENİ XÜSUSİYYƏTLƏRİN SONU ===

        // Konstruktor PhoneViewModel və GameState-i qəbul edir
        public CalendarViewModel(PhoneViewModel parent, GameState gameState)
        {
            _parentViewModel = parent;
            _gameState = gameState;
            _calendarService = new CalendarService(); // Servisimizi yaradırıq

            LoadEvents();
        }

        private void LoadEvents()
        {
            Events.Clear();
            var allEvents = _calendarService.GetAllEvents();
            int currentDay = _gameState.CurrentDay; // Hazırkı günü GameState-dən alırıq

            foreach (var ev in allEvents)
            {
                // Məntiqi burada hesablayırıq
                ev.IsToday = (ev.Day == currentDay);
                ev.IsPast = (ev.Day < currentDay);

                Events.Add(ev);
            }
        }

        [RelayCommand]
        private void NavigateToHome()
        {
            Cleanup(); // Bu ekranı tərk edərkən resursları təmizlə
            _parentViewModel.NavigateToHome();
        }

        // Digər telefon tətbiqləri kimi (ChatViewModel, WalletViewModel)
        // bunun da təmizləmə metodu olmalıdır.
        public void Cleanup()
        {
            // Gələcəkdə GameState.PropertyChanged-ə abunə olsanız,
            // burada abunəlikdən çıxmalısınız (məsələn: _gameState.PropertyChanged -= ...).
            // Hazırda bu metod boş qala bilər, amma olması yaxşı praktikadır.
            Console.WriteLine("CalendarViewModel cleaned up.");
        }
    }
}
