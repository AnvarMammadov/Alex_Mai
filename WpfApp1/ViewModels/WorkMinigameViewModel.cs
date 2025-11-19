using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;
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

        private DispatcherTimer _gameTimer;

        // Animasiya dəyişənləri
        private Stopwatch _qteStopwatch = new Stopwatch();
        private double _lastElapsedMilliseconds;
        private bool _qteMovingRight = true;
        private Product _pendingProduct;

        private int _currentStep;

        [ObservableProperty] private Order _currentOrder;
        [ObservableProperty] private int _timeRemaining;
        [ObservableProperty] private int _moneyEarned;

        // --- QTE Properties ---
        [ObservableProperty] private bool _isQteActive = false;
        [ObservableProperty] private double _qteCursorPosition = 0;
        [ObservableProperty] private double _targetPosition = 0;
        [ObservableProperty] private double _targetWidth = 50;
        [ObservableProperty] private string _qteResultText = "";

        public ObservableCollection<Product> AvailableProducts { get; set; }

        public WorkMinigameViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            _orderService = new OrderService("Data/orders.json");
            _allOrders = _orderService.GetAllOrders();

            AvailableProducts = new ObservableCollection<Product>
            {
                new Product { Id = "coffee", Name = "Coffee", IconPath = "/Assets/Icons/icon_coffee.png" },
                new Product { Id = "sandwich", Name = "Sandwich", IconPath = "/Assets/Icons/icon_sandwich.png" },
                new Product { Id = "juice", Name = "Juice", IconPath = "/Assets/Icons/icon_juice.png" },
                new Product { Id = "cake", Name = "Cake", IconPath = "/Assets/Icons/icon_cake.png" }
            };

            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _gameTimer.Tick += GameTimer_Tick;
        }

        public void StartShift()
        {
            TimeRemaining = 60;
            MoneyEarned = 0;
            IsQteActive = false;
            _gameTimer.Start();
            GenerateNewOrder();
        }

        private void GenerateNewOrder()
        {
            _currentStep = 0;
            int index = _random.Next(_allOrders.Count);
            CurrentOrder = _allOrders[index];
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (IsQteActive) return;

            TimeRemaining--;
            if (TimeRemaining <= 0)
            {
                EndShift();
            }
        }

        [RelayCommand]
        private void SelectProduct(Product product)
        {
            if (IsQteActive) return;

            _pendingProduct = product;
            StartQte();
        }

        private void StartQte()
        {
            IsQteActive = true;
            QteResultText = "";
            QteCursorPosition = 0;
            _qteMovingRight = true;

            TargetPosition = _random.Next(20, 230);
            TargetWidth = 50;

            _lastElapsedMilliseconds = 0;
            _qteStopwatch.Restart();
            CompositionTarget.Rendering += OnQteRender;
        }

        private void OnQteRender(object sender, EventArgs e)
        {
            if (!IsQteActive) return;

            double currentElapsed = _qteStopwatch.Elapsed.TotalMilliseconds;
            double deltaTime = (currentElapsed - _lastElapsedMilliseconds);
            _lastElapsedMilliseconds = currentElapsed;

            // Sürəti tənzimləmək üçün: 0.4 yaxşı balansdır
            double speed = 0.4 * deltaTime;

            if (_qteMovingRight)
                QteCursorPosition += speed;
            else
                QteCursorPosition -= speed;

            if (QteCursorPosition >= 290)
            {
                QteCursorPosition = 290;
                _qteMovingRight = false;
            }
            else if (QteCursorPosition <= 0)
            {
                QteCursorPosition = 0;
                _qteMovingRight = true;
            }
        }

        private void StopQteLoop()
        {
            CompositionTarget.Rendering -= OnQteRender;
            _qteStopwatch.Stop();
            // DİQQƏT: IsQteActive = false; sətrini buradan sildim!
        }

        [RelayCommand]
        private async Task CommitQte()
        {
            // Yalnız aktivdirsə və hələ dayanmayıbsa icra et
            if (!IsQteActive || !_qteStopwatch.IsRunning) return;

            StopQteLoop(); // Sadəcə animasiyanı dayandır, pəncərəni bağlama

            bool isSuccess = QteCursorPosition >= TargetPosition &&
                             QteCursorPosition <= (TargetPosition + TargetWidth);

            if (isSuccess)
            {
                QteResultText = "PERFECT!";
                await Task.Delay(1000); // 1 saniyə nəticəni göstər
                ProcessProductLogic(true);
            }
            else
            {
                QteResultText = "MISS...";
                await Task.Delay(1000); // 1 saniyə nəticəni göstər
                ProcessProductLogic(false);
            }

            // İndi pəncərəni bağla
            IsQteActive = false;
            QteResultText = "";
        }

        private void ProcessProductLogic(bool qteSuccess)
        {
            bool isCorrectProduct = _pendingProduct.Id == CurrentOrder.ProductOrder[_currentStep];

            if (qteSuccess && isCorrectProduct)
            {
                _currentStep++;
                if (_currentStep >= CurrentOrder.ProductOrder.Count)
                {
                    MoneyEarned += CurrentOrder.Reward;
                    GenerateNewOrder();
                }
            }
            else
            {
                TimeRemaining -= 5;
            }
        }
        private void EndShift()
        {
            _gameTimer.Stop();
            StopQteLoop();
            IsQteActive = false;

            // YENİ: Minimum Zəmanətli Ödəniş (Base Pay)
            // Əgər oyunçu heç nə qazanmayıbsa və ya çox az qazanıbsa (10$-dan az),
            // Menecer ona "zəhmət haqqı" olaraq 10-15$ arası pul verir.
            if (MoneyEarned < 10)
            {
                MoneyEarned = _random.Next(10, 16); // 10 daxil, 16 xaric (yəni 10, 11, 12, 13, 14, 15)
            }

            _parentViewModel.EndWorkMinigame(MoneyEarned);
        }
    }
}