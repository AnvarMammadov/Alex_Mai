using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class GameState : ObservableObject
    {
        [NotifyPropertyChangedFor(nameof(DayOfWeek))]
        [NotifyPropertyChangedFor(nameof(FullDateString))]
        [ObservableProperty]
        private int _currentDay = 1;

        // DƏYİŞİKLİK: TimeOfDay dəyişdikdə, CurrentEventSlots-u sıfırlamalıyıq.
        [NotifyPropertyChangedFor(nameof(CurrentEventSlots))] // <-- YENİ
        [ObservableProperty]
        private TimeOfDay _timeOfDay = TimeOfDay.Morning;

        [ObservableProperty]
        private int _playerMoney = 450;

        [ObservableProperty]
        private int _currentEnergy = 74;

        public int MaxEnergy { get; } = 100; // Maksimum enerji


        // --- YENİ HADİSƏ SLOTU SİSTEMİ (ADDIM 2) ---
        public int MaxEventSlots { get; } = 2; // Hər hissədə neçə hadisə ola bilər

        [ObservableProperty]
        private int _currentEventSlots = 2; // Başlanğıcda slotlar doludur

        // --- YENİ: Zaman dəyişdikdə slotları sıfırlamaq üçün xüsusi metod ---
        partial void OnTimeOfDayChanged(TimeOfDay value)
        {
            // Zaman hissəsi dəyişəndə (Səhərdən Günortaya)
            // hadisə slotlarını yenidən doldururuq.
            // "Night" üçün də doldurulur, lakin istifadəsi məhdudlaşdırılacaq.
            CurrentEventSlots = MaxEventSlots;
        }
        // --- HADİSƏ SLOTU SİSTEMİ SONU ---

        // *** YENİ: Əməliyyat Tarixçəsi ***
        public ObservableCollection<Transaction> Transactions { get; } = new ObservableCollection<Transaction>();


        // Həftənin gününü hesablayan yeni xüsusiyyət
        public string DayOfWeek
        {
            get
            {
                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                // Oyun 1-ci gündən başladığı üçün (index 0) (_currentDay - 1) istifadə edirik
                return days[(CurrentDay - 1) % 7];
            }
        }


        // Ayın gününü mətn formatında göstərən yeni xüsusiyyət (Oyunun Sentyabrda başladığını fərz edirik)
        public string FullDateString => $"September {CurrentDay}";



        // *** YENİ: Əməliyyat Əlavə Etmə Metodu ***
        public void AddTransaction(string description, int amount, TransactionType type, DateTime timestamp)
        {
            // Əməliyyat əlavə olunmazdan *əvvəlki* balans vacib deyil,
            // çünki bu metod çağırıldıqda PlayerMoney artıq yenilənmiş olacaq.
            int absoluteAmount = Math.Abs(amount);

            var transaction = new Transaction
            {
                Description = description,
                Amount = absoluteAmount,
                Type = type,
                Timestamp = timestamp,
                BalanceAfter = this.PlayerMoney // Cari (yenilənmiş) balans
            };

            // Ən başa əlavə et
            Transactions.Insert(0, transaction);

            // Limit
            const int maxHistory = 50;
            while (Transactions.Count > maxHistory)
            {
                Transactions.RemoveAt(Transactions.Count - 1);
            }
        }


        // *** YENİ: Save/Load üçün metodlar (Opsional, amma tövsiyə olunur) ***
        public void ClearTransactions() // Yeni oyuna başlayanda və ya yükləyəndə lazım ola bilər
        {
            Transactions.Clear();
        }

        public void LoadTransactions(List<Transaction> loadedTransactions) // Yüklənmiş əməliyyatları əlavə etmək üçün
        {
            ClearTransactions();
            if (loadedTransactions != null)
            {
                // Adətən save faylında tərs sırada saxlanılır, ona görə də yükləyərkən düzgün sıraya salmaq lazım ola bilər
                foreach (var t in loadedTransactions.OrderByDescending(t => t.Timestamp)) // Və ya sadəcə foreach
                {
                    Transactions.Add(t); // Və ya Insert(0, t)
                }
            }
        }
    }
}
