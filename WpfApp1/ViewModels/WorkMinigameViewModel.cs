using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class WorkMinigameViewModel : ObservableObject
    {
        private readonly GameViewModel _parentViewModel;
        private readonly OrderService _orderService;
        private List<Order> _allOrders;
        private Random _random = new Random();
        private DispatcherTimer _timer;
        private int _currentStep;

        [ObservableProperty] private Order _currentOrder;
        [ObservableProperty] private int _timeRemaining;
        [ObservableProperty] private int _moneyEarned;

        public ObservableCollection<Product> AvailableProducts { get; set; }

        public WorkMinigameViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            _orderService = new OrderService("Data/orders.json");
            _allOrders = _orderService.GetAllOrders();

            AvailableProducts = new ObservableCollection<Product>
            {
                new Product { Id = "coffee", Name = "Kofe", IconPath = "/Assets/Icons/icon_coffee.png" },
                new Product { Id = "sandwich", Name = "Sendviç", IconPath = "/Assets/Icons/icon_sandwich.png" },
                new Product { Id = "juice", Name = "Şirə", IconPath = "/Assets/Icons/icon_juice.png" },
                new Product { Id = "cake", Name = "Tort", IconPath = "/Assets/Icons/icon_cake.png" }
            };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
        }

        public void StartShift()
        {
            TimeRemaining = 60; // 60 saniyəlik iş vaxtı
            MoneyEarned = 0;
            _timer.Start();
            GenerateNewOrder();
        }

        private void GenerateNewOrder()
        {
            _currentStep = 0;
            int index = _random.Next(_allOrders.Count);
            CurrentOrder = _allOrders[index];
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeRemaining--;
            if (TimeRemaining <= 0)
            {
                EndShift();
            }
        }

        private void EndShift()
        {
            _timer.Stop();
            _parentViewModel.EndWorkMinigame(MoneyEarned);
        }

        [RelayCommand]
        private void SelectProduct(Product product)
        {
            if (product.Id == CurrentOrder.ProductOrder[_currentStep])
            {
                // Düzgün seçim
                _currentStep++;
                if (_currentStep >= CurrentOrder.ProductOrder.Count)
                {
                    // Sifariş tamamlandı
                    MoneyEarned += CurrentOrder.Reward;
                    GenerateNewOrder();
                }
            }
            else
            {
                // Səhv seçim, cəza olaraq vaxt itir
                TimeRemaining -= 5;
            }
        }
    }
}
