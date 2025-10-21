using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Alex_Mai.Models;
using Alex_Mai.Services;
using Alex_Mai.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {

        public event EventHandler CharacterSpriteChanged;
        public InventoryViewModel Inventory { get; }
        public PhoneViewModel Phone { get; }
        public MapViewModel Map { get; }
        public CookingGameViewModel CookingGame { get; }
        public WorkMinigameViewModel WorkGame { get; }
        public MarketViewModel Market { get; }

        private Dictionary<string, string> _locationIndex;///


        [NotifyPropertyChangedFor(nameof(MainTimeOfDay))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot1))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot2))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot3))]
        [NotifyPropertyChangedFor(nameof(MaiMood))]
       // [NotifyPropertyChangedFor(nameof(TimeOfDayIcon))] // Bu hələlik qala bilər
        [ObservableProperty] private GameState _currentGameState;

        [ObservableProperty] private string _notificationMessage;



        [NotifyPropertyChangedFor(nameof(MaiMood))]
        [ObservableProperty]
        private CharacterStats _mainCharacterStats;



        [ObservableProperty] private string _currentDialogueText;
        [ObservableProperty] private string _characterName;
        [ObservableProperty] private string _currentCharacterSprite;
        [ObservableProperty] private bool _areChoicesVisible;
        [ObservableProperty] private string _currentBackground;
        [ObservableProperty] private bool _isInventoryOpen = false;
        [ObservableProperty] private bool _isPhoneOpen = false;
        [ObservableProperty] private bool _isMapOpen = false;
        [ObservableProperty] private bool _isCookingMinigameOpen = false;
        [ObservableProperty] private bool _isWorkMinigameOpen = false;
        [ObservableProperty] private bool _isMarketOpen = false;
        [ObservableProperty] private bool _isNotificationVisible = false;

        public ObservableCollection<Choice> CurrentChoices { get; set; } = new ObservableCollection<Choice>();

        private readonly DialogueService _dialogueService;
        private readonly AudioService _audioService;




        // Zaman göstəricisi üçün yeni xüsusiyyətlər
        public string MainTimeOfDay => CurrentGameState.TimeOfDay.ToString(); // enum-u string-ə çeviririk
        public string NextTimeSlot1
        {
            get
            {
                switch (CurrentGameState.TimeOfDay)
                {
                    case TimeOfDay.Morning: return "Afternoon";
                    case TimeOfDay.Afternoon: return "Evening";
                    case TimeOfDay.Evening: return "Night";
                    case TimeOfDay.Night: default: return "Morning";
                }
            }
        }
        public string NextTimeSlot2
        {
            get
            {
                switch (CurrentGameState.TimeOfDay)
                {
                    case TimeOfDay.Morning: return "Evening";
                    case TimeOfDay.Afternoon: return "Night";
                    case TimeOfDay.Evening: return "Morning";
                    case TimeOfDay.Night: default: return "Afternoon";
                }
            }
        }
        public string NextTimeSlot3
        {
            get
            {
                switch (CurrentGameState.TimeOfDay)
                {
                    case TimeOfDay.Morning: return "Night";
                    case TimeOfDay.Afternoon: return "Morning";
                    case TimeOfDay.Evening: return "Afternoon";
                    case TimeOfDay.Night: default: return "Evening";
                }
            }
        }





        public GameViewModel(GameState gameState = null, CharacterStats characterStats = null)
        {
            CurrentGameState = gameState ?? new GameState();
            MainCharacterStats = characterStats ?? new CharacterStats();
            _dialogueService = new DialogueService("Data/dialogue.json");
            _audioService = AudioService.Instance;
            CurrentCharacterSprite = "/Assets/Sprites/alex_normal.png";


            CurrentGameState.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(GameState.TimeOfDay))
                {
                    OnPropertyChanged(nameof(MainTimeOfDay));
                    OnPropertyChanged(nameof(NextTimeSlot1));
                    OnPropertyChanged(nameof(NextTimeSlot2));
                    OnPropertyChanged(nameof(NextTimeSlot3));
                }
            };


            LoadLocationIndex();

            Inventory = new InventoryViewModel(this);
            Phone = new PhoneViewModel(this);
            Map = new MapViewModel(this);
            CookingGame = new CookingGameViewModel(this);
            WorkGame = new WorkMinigameViewModel(this);
            Market = new MarketViewModel(this);

            _locationIndex = _dialogueService.GetLocationIndex();


            ShowDialogue("start_game");
        }

        private async Task ShowNotification(string message, int durationMs = 2000)
        {
            NotificationMessage = message;
            IsNotificationVisible = true;
            await Task.Delay(durationMs);
            IsNotificationVisible = false;
        }




        private bool _isSkippingAnimation = false;
        private async Task AnimateDialogueText(string fullText)
        {
            _isSkippingAnimation = false;
            CurrentDialogueText = "";

            try
            {
                foreach (char c in fullText)
                {
                    if (_isSkippingAnimation)
                    {
                        break; // Döngünü dayandır
                    }
                    CurrentDialogueText += c;
                    await Task.Delay(50);
                }
            }
            finally
            {
                CurrentDialogueText = fullText; // Hər halda tam mətni göstər
                AreChoicesVisible = true; // VƏ SEÇİMLƏRİ GÖRÜNƏN ET!
            }
        }


        // Bu yeni metodu əlavə edin
        public void SkipDialogueAnimation()
        {
            _isSkippingAnimation = true;
        }


        // BU METOD YENİLƏNİB
        private async void ShowDialogue(string nodeId)
        {
            AreChoicesVisible = false; // <-- BU SƏTRİ ƏLAVƏ EDİN
            CurrentChoices.Clear();    // <-- Bu sətri də bura köçürün

            var node = _dialogueService.GetNode(nodeId);
            if (node != null)
            {
                CharacterName = node.Character;
                await AnimateDialogueText(node.Text);

                // 1. Şəkil məntiqi
                if (!string.IsNullOrEmpty(node.Sprite))
                {
                    // Əgər JSON-da yeni şəkil adı varsa, onu təyin et
                    CurrentCharacterSprite = $"/Assets/Sprites/{node.Sprite}";
                    CharacterSpriteChanged?.Invoke(this, EventArgs.Empty); // Fade animasiyası üçün siqnal
                }
                else
                {
                    // Əgər şəkil adı yoxdursa (null və ya boşdursa), şəkli təmizlə
                    CurrentCharacterSprite = null;
                }

                // 2. Musiqi məntiqi (ayrı yoxlanılır)
                if (!string.IsNullOrEmpty(node.BGM))
                {
                    _audioService.PlayBGM(node.BGM);
                }

                // 3. YENİ: Arxa Fon Məntiqi
                if (!string.IsNullOrEmpty(node.Background))
                {
                    CurrentBackground = $"/Assets/Backgrounds/{node.Background}";
                }

                foreach (var choice in node.Choices)
                {
                    if (AreConditionsMet(choice.Conditions))
                    {
                        CurrentChoices.Add(choice);
                    }
                }
            }
        }

        [RelayCommand]
        private void SaveGame()
        {
            var saveData = new SaveData
            {
                GameState = this.CurrentGameState,
                CharacterStats = this.MainCharacterStats
            };

            var saveLoadService = new SaveLoadService();
            saveLoadService.SaveGame(saveData);

            // İstəsəniz, "Oyun yadda saxlanıldı" kimi bir mesaj göstərə bilərsiniz
        }

        // BU METOD YENİLƏNİB
        // Seçim klikində də vaxt/enerji JSON-dan gələ bilər (ProcessActions ilə)
        [RelayCommand]
        private void SelectChoice(Choice choice)
        {
            _audioService.PlaySFX("click.wav");
            if (choice == null) return;

            ProcessActions(choice.Actions); // vaxt/enerji/pul burada dəyişə bilər

            if (!string.IsNullOrEmpty(choice.NextNodeId))
            {
                if (choice.NextNodeId == "start_cooking_minigame") StartCookingMinigame();
                else if (choice.NextNodeId == "start_part_time_minigame") { StartWorkMinigame(); }
                else if (choice.NextNodeId == "go_to_sleep") { SleepAndRecover(); ShowDialogue("day2_morning_hub"); }     
                else ShowDialogue(choice.NextNodeId);
            }

            // Əgər seçimdə xüsusi bir "action" varsa, onu icra et.
            if (!string.IsNullOrEmpty(choice.Action))
            {
                switch (choice.Action)
                {
                    case "market_interface":
                        ToggleMarket();
                        break;

                        // Gələcəkdə bura "OpenMap", "OpenPhone" kimi başqa əmrlər də əlavə edə bilərik.
                }
            }


        }


        private void AdvanceTime(int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                switch (CurrentGameState.TimeOfDay)
                {
                    case TimeOfDay.Morning:
                        CurrentGameState.TimeOfDay = TimeOfDay.Afternoon; break;
                    case TimeOfDay.Afternoon:
                        CurrentGameState.TimeOfDay = TimeOfDay.Evening; break;
                    case TimeOfDay.Evening:
                        CurrentGameState.TimeOfDay = TimeOfDay.Night; break;
                    case TimeOfDay.Night:
                    default:
                        CurrentGameState.TimeOfDay = TimeOfDay.Morning;
                        CurrentGameState.CurrentDay += 1; // yeni gün
                        break;
                }
            }
            // NextSlot-lar üçün PropertyChanged yuxarıda subscription ilə gələcək
        }

        private void ApplyEnergy(int delta)
        {
            var v = CurrentGameState.CurrentEnergy + delta;
            if (v < 0) v = 0;
            if (v > CurrentGameState.MaxEnergy) v = CurrentGameState.MaxEnergy;
            CurrentGameState.CurrentEnergy = v;
        }

        private void SleepAndRecover()
        {
            // Gecə deyilsə gecəyə ötür, sonra səhərə keç və günü artır
            if (CurrentGameState.TimeOfDay != TimeOfDay.Night)
                CurrentGameState.TimeOfDay = TimeOfDay.Night;

            AdvanceTime(1); // Night -> Morning (+1 gün)
            ApplyEnergy(+CurrentGameState.MaxEnergy); // tam doldur (clamp var)
        }



        // BU, YENİ METODDUR
        // === ACTION PROCESSING genişləndirildi ===
        private void ProcessActions(List<GameAction> actions)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                switch (action.Stat)
                {
                    case "Affection":
                        if (action.Operator == "Add") MainCharacterStats.Affection += action.Value;
                        else if (action.Operator == "Subtract") MainCharacterStats.Affection -= action.Value;
                        else if (action.Operator == "Set") MainCharacterStats.Affection = action.Value;
                        break;

                    case "Trust":
                        if (action.Operator == "Add") MainCharacterStats.Trust += action.Value;
                        else if (action.Operator == "Subtract") MainCharacterStats.Trust -= action.Value;
                        else if (action.Operator == "Set") MainCharacterStats.Trust = action.Value;
                        break;

                    case "Stress":
                        if (action.Operator == "Add") MainCharacterStats.Stress += action.Value;
                        else if (action.Operator == "Subtract") MainCharacterStats.Stress -= action.Value;
                        else if (action.Operator == "Set") MainCharacterStats.Stress = action.Value;
                        break;

                    // ⬇️ YENİ: vaxt irəliləyişi
                    case "TimeAdvance":
                        // Add -> həmin qədər slot irəli; Set -> dəqiq addım sayı kimi istifadə edirik
                        if (action.Operator == "Add" || action.Operator == "Set")
                            AdvanceTime(Math.Max(1, action.Value));
                        break;

                    // ⬇️ YENİ: enerji idarəsi
                    case "Energy":
                        if (action.Operator == "Add") ApplyEnergy(+action.Value);
                        else if (action.Operator == "Subtract") ApplyEnergy(-action.Value);
                        else if (action.Operator == "Set") CurrentGameState.CurrentEnergy = Math.Clamp(action.Value, 0, CurrentGameState.MaxEnergy);
                        break;

                    // ⬇️ YENİ: pul
                    case "Money":
                        int previousMoney = CurrentGameState.PlayerMoney;
                        int amount = action.Value;
                        TransactionType type = TransactionType.Expense; // Default
                        string description = "Unknown Transaction"; // Default
                        if (action.Operator == "Add") {
                            CurrentGameState.PlayerMoney += amount;
                            type = TransactionType.Income;
                            description = "Received Money";
                        }
                        else if (action.Operator == "Subtract")
                        {
                            CurrentGameState.PlayerMoney -= amount;
                            type = TransactionType.Expense;
                            description = "Spent Money"; // Daha yaxşı description dialoqdan gələ bilər
                        }
                        else if (action.Operator == "Set")
                        {
                            // Set üçün tarixçə əlavə etmək mürəkkəb ola bilər, fərqi hesablamaq lazımdır
                            // Hələlik bunu buraxaq və ya fərqi hesablayıb əlavə edək
                            CurrentGameState.PlayerMoney = amount;
                            // Opsional: Fərqi hesablayıb əlavə et
                            int difference = amount - previousMoney;
                            if (difference != 0)
                            {
                                CurrentGameState.AddTransaction("Balance Set (Dialogue)", Math.Abs(difference), difference > 0 ? TransactionType.Income : TransactionType.Expense, DateTime.Now);
                            }
                        }
                        // Əgər pul dəyişibsə əməliyyatı qeyd et (Set xaricində)
                        if (action.Operator == "Add" || action.Operator == "Subtract")
                        {
                            CurrentGameState.AddTransaction(description, amount, type, DateTime.Now);
                        }
                        break;

                    // ⬇️ İstəsən “Sleep” kimi flag da dəstəkləyə bilərik
                    case "Sleep":
                        // hər hansı seçimdən sonra yatmaq üçün
                        SleepAndRecover();
                        break;
                }
            }
        }


        // BU DA YENİ METODDUR
        private bool AreConditionsMet(List<Condition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var condition in conditions)
            {
                int statValue = 0;
                switch (condition.Stat)
                {
                    case "Affection": statValue = MainCharacterStats.Affection; break;
                    case "Trust": statValue = MainCharacterStats.Trust; break;
                    case "Stress": statValue = MainCharacterStats.Stress; break;
                }

                bool conditionMet = false;
                switch (condition.Operator)
                {
                    case "GreaterThan": conditionMet = statValue > condition.Value; break;
                    case "LessThan": conditionMet = statValue < condition.Value; break;
                    case "Equals": conditionMet = statValue == condition.Value; break;
                }

                if (!conditionMet) return false;
            }
            return true;
        }


        // Köhnə komandalar olduğu kimi qalır
        [RelayCommand]
        private void IncreaseAffection()
        {
            if (MainCharacterStats.Stress > 0) MainCharacterStats.Stress -= 1;
            MainCharacterStats.Affection += 1;
        }

        [RelayCommand]
        private void GoToNextDay()
        {
            CurrentGameState.CurrentDay += 1;
            MainCharacterStats.Stress = 10;
        }



        [RelayCommand]
        public void ToggleInventory()
        {
            IsInventoryOpen = !IsInventoryOpen;
        }

        public void UseInventoryItem(string itemId)
        {
            // Bir əşya istifadə edildikdə, rahatlıq üçün inventar pəncərəsini bağlayırıq.
            IsInventoryOpen = false;

            // Hansı əşyanın istifadə edildiyini yoxlayıb, müvafiq hadisəni başlayırıq.
            switch (itemId)
            {
                case "cigarette":
                    ShowDialogue("event_use_cigarette");
                    break;

                case "zippo":
                    ShowDialogue("event_use_zippo");
                    break;

                case "phone":
                    // Gələcəkdə bu, xüsusi telefon pəncərəsini açacaq.
                    // Hələlik test üçün sadə bir dialoq başladaq.
                    ShowDialogue("event_use_phone");
                    break;
            }
        }

        public void TogglePhoneView()
        {
            IsInventoryOpen = false; // İnventarı bağla
            IsPhoneOpen = !IsPhoneOpen;
        }

        [RelayCommand]
        public void ToggleMap()
        {
            IsMapOpen = !IsMapOpen;
        }

        // === NAVİQASİYA: otağa keçəndə enerjini və vaxtı hərəkət etdir (opsional) ===
        public void GoToPlace(string placeId)
        {
            IsMapOpen = false;
            IsMarketOpen = false;

            if (_locationIndex != null && _locationIndex.TryGetValue(placeId, out var nodeId) && !string.IsNullOrEmpty(nodeId))
            {
                // hər otaq keçidi 1 slot vaxt aparsın və 3 enerji xərcləsin (istəyə görə dəyiş)
                AdvanceTime(1);
                ApplyEnergy(-3);
                ShowDialogue(nodeId);
            }
        }

        // Yeni metodları əlavə edin
        public void ToggleMarket()
        {
            IsInventoryOpen = false; // İnventarı bağla
            IsMarketOpen = !IsMarketOpen;
        }

        private void LoadLocationIndex()
        {
            // Ən yaxşısı: _dialogueService.GetLocationIndex();
            // Minimal: sərt kodla (JSON-a əlavə etdikcə buranı da doldur):
            _locationIndex = new Dictionary<string, string>
            {
                ["alex_room"] = "hub_alex_room",
                ["mai_room"] = "hub_mai_room",
                ["kitchen"] = "hub_kitchen",
                ["bathroom"] = "hub_bathroom"
            };
        }



        public void StartCookingMinigame()
        {
            CookingGame.Reset(); // Oyunu sıfırla
            IsCookingMinigameOpen = true;
        }

        public void EndCookingMinigame(bool success)
        {
            IsCookingMinigameOpen = false;
            if (success)
            {
                ShowDialogue("reaction_cook_success");
            }
            else
            {
                ShowDialogue("reaction_cook_fail");
            }
        }


        private void StartWorkMinigame()
        {
           // CloseAllOverlays();
            WorkGame.StartShift();
            IsWorkMinigameOpen = true;
        }

        public void EndWorkMinigame(int moneyEarned)
        {
            IsWorkMinigameOpen = false;
            CurrentGameState.PlayerMoney += moneyEarned; // Qazanılan pulu əlavə et

            if (moneyEarned > 0)
            {
                // *** YENİ: Əməliyyatı qeyd et ***
                // Pul əlavə olunmazdan *sonra* balans düzgün görünsün deyə
                // bu metodu PlayerMoney yeniləndikdən sonra çağırmaq daha yaxşıdır
                // Və ya AddTransaction metodunu balansı ayrıca alacaq şəkildə dəyişmək
                // Hələlik:
                int balanceBefore = CurrentGameState.PlayerMoney;
                CurrentGameState.PlayerMoney += moneyEarned; // Pul əlavə et
                CurrentGameState.AddTransaction("Part-time Job Earnings", moneyEarned, TransactionType.Income, DateTime.Now);   // *** DƏYİŞİKLİK: Birbaşa GameState-ə əlavə et ***
                ShowDialogue("reaction_work_end_success");
            }
            else
            {
                CurrentGameState.PlayerMoney += moneyEarned; // For potential negative earnings/fees? Update if needed.
                                                             // Optionally log failed work attempt if needed
                ShowDialogue("reaction_work_end_fail");
            }

            // Make sure PlayerMoney change notification reaches WalletViewModel
            // The PropertyChanged subscription should handle this.

            // TODO: Pulun dəyişdiyi digər yerlərdə də (məsələn, dialoq actionları) AddTransactionToWallet çağırılmalıdır.
        }

        public async void PurchaseItem(MarketItem item)
        {
            const int MAX_ITEM_STACK = 3; // Maksimum say limiti
            int currentItemCount = Inventory.GetItemCount(item.ItemId);

            // Limiti yoxla
            if (currentItemCount >= MAX_ITEM_STACK)
            {
                await ShowNotification("Bu əşyadan kifayət qədər var!");
                return;
            }

            // Pulu yoxla
            if (CurrentGameState.PlayerMoney >= item.Price)
            {
                // Pulu çıx
                CurrentGameState.PlayerMoney -= item.Price;

                // *** DƏYİŞİKLİK: Birbaşa GameState-ə əlavə et ***
                CurrentGameState.AddTransaction($"Purchased: {item.Name}", item.Price, TransactionType.Expense, DateTime.Now);

                // İnventara əlavə et
                Inventory.AddItem(new InventoryItem
                {
                    ItemId = item.ItemId,
                    Name = item.Name,
                    IconPath = item.IconPath
                });

                // Uğurlu alış-veriş bildirişi
                await ShowNotification($"+1 {item.Name}");
            }
            else
            {
                // Kifayət qədər pul yoxdur bildirişi
                await ShowNotification("Kifayət qədər pulunuz yoxdur!");
            }
        }


        public string MaiMood
        {
            get
            {
                if (MainCharacterStats.Stress > 70) return "Gərgin";
                if (MainCharacterStats.Affection > 50) return "Xoşbəxt";
                if (MainCharacterStats.Affection > 20) return "Razı";
                return "Neytral";
            }
        }
    }
}