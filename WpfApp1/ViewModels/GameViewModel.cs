using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Alex_Mai.Models;
using Alex_Mai.Services;
using Alex_Mai.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks; // <-- YENİ
using System.Linq; // <-- ƏLAVƏ EDİLDİ
using System;

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
        private string _currentNodeId = ""; // <-- YENİ (Addım 4 üçün)

        // Cari məkan (map keçidlərindən sonra yenilənir)
        private string _currentPlaceId = null;

        // locationIndex-in tərsi: nodeId -> placeId
        private Dictionary<string, string> _nodeToPlace;


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

        [ObservableProperty]
        private bool _isBusy = false;

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
            Phone = new PhoneViewModel(this, CurrentGameState);
            Map = new MapViewModel(this);
            CookingGame = new CookingGameViewModel(this);
            WorkGame = new WorkMinigameViewModel(this);
            Market = new MarketViewModel(this);

            _locationIndex = _dialogueService.GetLocationIndex();


            ShowDialogue("start_game");
            _currentNodeId = "start_game"; // Başlanğıc nodu təyin edirik
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
                    await Task.Delay(10);
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
            _currentNodeId = nodeId; // YENİ: Cari nodu həmişə izləyirik
            AreChoicesVisible = false; // <-- BU SƏTRİ ƏLAVƏ EDİN
            CurrentChoices.Clear();    // <-- Bu sətri də bura köçürün

            var node = _dialogueService.GetNode(nodeId);
            if (node != null)
            {
                CharacterName = node.Character;
                // Əgər göstərilən node hər hansı məkana aiddirsə, cari placeId-i yenilə
                if (_nodeToPlace != null && _nodeToPlace.TryGetValue(nodeId, out var placeFromNode))
                {
                    _currentPlaceId = placeFromNode;
                }
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

        /// <summary>
        /// Dialoq seçimi üçün əsas komanda.
        /// </summary>
        [RelayCommand]
        private async Task SelectChoice(Choice choice)
        {
            if (IsBusy) return;
            if (choice == null) return;
            IsBusy = true;

            try
            {
                _audioService.PlaySFX("click.wav");

                // --- BUG 2 HƏLLİ ---
                // 1. Enerji Tələbatını Yoxla
                int energyCost = GetEnergyCostFromActions(choice.Actions);
                // YALNIZ enerji SƏRF EDƏN hadisələr yoxlanılmalıdır
                if (energyCost > 0 && energyCost > CurrentGameState.CurrentEnergy)
                {
                    await ShowNotification("Enerjiniz çatmır...");
                    return; // Hərəkəti blokla
                }
                // --- HƏLLİN SONU ---

                // 2. Slotu Sərf Etməyə Cəhd Et
                if (!await TryUseEventSlot(choice.ActionCost))
                {
                    return; // Hərəkəti blokla (Gecə və slot = 0)
                }

                // 3. Hərəkəti İcra Et
                ProcessActions(choice.Actions);

                // 4. Növbəti Addımı Təyin Et
                if (!string.IsNullOrEmpty(choice.NextNodeId))
                {
                    if (choice.NextNodeId == "start_cooking_minigame") StartCookingMinigame();
                    else if (choice.NextNodeId == "start_part_time_minigame") { StartWorkMinigame(); }
                    else if (choice.NextNodeId == "go_to_sleep")
                    {
                        SleepAndRecover(); // Yalnız bu komanda yeni günə keçirir
                        ShowDialogue("go_to_sleep"); // (Bu, "go_to_sleep" nodunun içindəki nextNodeId olmalıdır)
                    }
                    else ShowDialogue(choice.NextNodeId);
                }

                if (!string.IsNullOrEmpty(choice.Action))
                {
                    switch (choice.Action)
                    {
                        case "market_interface":
                            ToggleMarket();
                            break;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

    

        private void ApplyEnergy(int delta)
        {
            var v = CurrentGameState.CurrentEnergy + delta;
            if (v < 0) v = 0;
            if (v > CurrentGameState.MaxEnergy) v = CurrentGameState.MaxEnergy;
            CurrentGameState.CurrentEnergy = v;
        }

        /// <summary>
        /// Statları (Enerji, Pul, Sevgi) tətbiq edir.
        /// </summary>
        private void ProcessActions(List<GameAction> actions)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                switch (action.Stat)
                {
                    // "TimeAdvance" və "Sleep" sətirləri buradan tamamilə silinib (bu, düzgündür).
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
                    case "Energy":
                        if (action.Operator == "Add") ApplyEnergy(+action.Value);
                        else if (action.Operator == "Subtract") ApplyEnergy(-action.Value);
                        else if (action.Operator == "Set") CurrentGameState.CurrentEnergy = Math.Clamp(action.Value, 0, CurrentGameState.MaxEnergy);
                        break;
                    case "Money":
                        int previousMoney = CurrentGameState.PlayerMoney;
                        int amount = action.Value;
                        TransactionType type = TransactionType.Expense;
                        string description = "Unknown Transaction";
                        if (action.Operator == "Add")
                        {
                            CurrentGameState.PlayerMoney += amount;
                            type = TransactionType.Income;
                            description = "Received Money";
                        }
                        else if (action.Operator == "Subtract")
                        {
                            CurrentGameState.PlayerMoney -= amount;
                            type = TransactionType.Expense;
                            description = "Spent Money";
                        }
                        else if (action.Operator == "Set")
                        {
                            CurrentGameState.PlayerMoney = amount;
                            int difference = amount - previousMoney;
                            if (difference != 0)
                            {
                                CurrentGameState.AddTransaction("Balance Set (Dialogue)", Math.Abs(difference), difference > 0 ? TransactionType.Income : TransactionType.Expense, DateTime.Now);
                            }
                        }
                        if (action.Operator == "Add" || action.Operator == "Subtract")
                        {
                            CurrentGameState.AddTransaction(description, amount, type, DateTime.Now);
                        }
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
                    case "Energy": statValue = CurrentGameState.CurrentEnergy; break; // <-- ŞƏRTLƏR ÜÇÜN ENERJİNİ ƏLAVƏ ETDİM
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

        /// <summary>
        /// Use inventory item
        /// </summary>
        public async Task UseInventoryItem(string itemId)
        {

            IsBusy = true;

            try
            {
                IsInventoryOpen = false;

                switch (itemId)
                {
                    case "cigarette":
                        if (!await TryUseEventSlot(1)) return;
                        ShowDialogue("event_use_cigarette");
                        break;

                    case "zippo":
                        if (!await TryUseEventSlot(1)) return;
                        ShowDialogue("event_use_zippo");
                        break;

                    case "soda":
                        if (CurrentGameState.CurrentEnergy >= CurrentGameState.MaxEnergy)
                        {
                            await ShowNotification("Your energy is maximum..");
                            return;
                        }
                        if (!await TryUseEventSlot(1)) return; // 1 slot aparır

                        ApplyEnergy(15);
                        await ShowNotification("+15 Enerji bərpa olundu");
                        // TODO: Inventardan əşyanı silmək məntiqi
                        break;

                    // --- BUG 1 HƏLLİ ---
                    case "phone":
                        // Telefonu açmaq PULSUZ olmalıdır (0 slot)
                        if (!await TryUseEventSlot(0)) return;
                        TogglePhoneView();
                        break;
                    // --- HƏLLİN SONU ---

                    default:
                        IsInventoryOpen = true;
                        break;
                }
            }
            finally
            {
                IsBusy = false;
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
            if (IsBusy) return;
            IsMapOpen = !IsMapOpen;
        }

        /// <summary>
        /// Məkana getmək üçün əsas komanda.
        /// </summary>
        public async Task GoToPlace(string placeId)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                IsMapOpen = false;
                IsMarketOpen = false;

                if (_locationIndex != null
                    && _locationIndex.TryGetValue(placeId, out var nodeId)
                    && !string.IsNullOrEmpty(nodeId))
                {
                    if (_currentNodeId == nodeId) return;

                    // --- SLOT QAYDASI ---
                    // Home -> Home  : 0 slot (pulsuz)
                    // Home <-> Out  : 1 slot
                    // Out  -> Out   : 1 slot
                    int slotsToUse = 1; // default

                    bool fromIsHome = IsHomePlace(_currentPlaceId ?? "alex_room"); // ilk dəfə üçün fallback
                    bool toIsHome = IsHomePlace(placeId);

                    if (fromIsHome && toIsHome)
                        slotsToUse = 0;

                    if (!await TryUseEventSlot(slotsToUse)) return;

                    // Uğurlu oldu — dialoqa keç və cari yeri yenilə
                    ShowDialogue(nodeId);
                    _currentPlaceId = placeId;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Ev daxilindəki yerləri yoxlayan köməkçi metod (klassın içinə əlavə et)
        private static bool IsHomePlace(string placeId)
        {
            // Səndə map-də bu id-lər var: alex_room, mai_room, kitchen, bathroom, living_room
            return placeId == "alex_room"
                || placeId == "mai_room"
                || placeId == "kitchen"
                || placeId == "bathroom"
                || placeId == "living_room";
        }


        // Yeni metodları əlavə edin
        public void ToggleMarket()
        {
            IsInventoryOpen = false; // İnventarı bağla
            IsMarketOpen = !IsMarketOpen;
        }

        private void LoadLocationIndex()
        {
            _locationIndex = _dialogueService.GetLocationIndex();

            // nodeId -> placeId tərs xəritəsi
            _nodeToPlace = new Dictionary<string, string>();
            if (_locationIndex != null)
            {
                foreach (var kv in _locationIndex)
                {
                    var placeId = kv.Key;      // məsələn "kitchen"
                    var nodeId = kv.Value;    // məsələn "hub_kitchen" və ya "day2_kitchen_hub"
                    if (!string.IsNullOrEmpty(nodeId))
                        _nodeToPlace[nodeId] = placeId;
                }
            }
        }




        /// <summary>
        /// Mərkəzi metod: Hadisə slotunu sərf etməyə CƏHD EDİR.
        /// </summary>
        private async Task<bool> TryUseEventSlot(int slotsToUse = 1)
        {
            if (slotsToUse == 0) return true; // Pulsuz hərəkətlər həmişə uğurludur

            // GECƏ MƏNTİQİ: Gecədirsə və slot qalmayıbsa, hərəkətə icazə verilmir.
            if (CurrentGameState.TimeOfDay == TimeOfDay.Night && CurrentGameState.CurrentEventSlots <= 0)
            {
                await ShowNotification("Artıq çox gecdir... Yatmalısan.");
                return false; // Hərəkət bloklandı
            }

            // Slotu Sərf Et
            if (CurrentGameState.CurrentEventSlots > 0)
            {
                CurrentGameState.CurrentEventSlots -= slotsToUse;
            }

            // Slotlar Bitdisə, Zamanı İrəli Çək (GECƏ XARİC)
            if (CurrentGameState.CurrentEventSlots <= 0 && CurrentGameState.TimeOfDay != TimeOfDay.Night)
            {
                await AdvanceTime();
                await ShowNotification("Zaman irəliləyir...");
            }

            return true; // Hərəkət uğurludur
        }



        /// <summary>
        /// Zamanı 1 addım irəli aparır. GECƏDƏ DAYANIR.
        /// </summary>
        private async Task AdvanceTime()
        {
            switch (CurrentGameState.TimeOfDay)
            {
                case TimeOfDay.Morning:
                    CurrentGameState.TimeOfDay = TimeOfDay.Afternoon;
                    break;
                case TimeOfDay.Afternoon:
                    CurrentGameState.TimeOfDay = TimeOfDay.Evening;
                    break;
                case TimeOfDay.Evening:
                    CurrentGameState.TimeOfDay = TimeOfDay.Night;
                    ShowNotification("Artıq gecədir...");
                    break;
                case TimeOfDay.Night:
                default:
                    // Gecə vaxtı avtomatik günə keçmirik. "SleepAndRecover" gözlənilir.
                    break;
            }
        }


        /// <summary>
        /// Yalnız bu metod yeni günə başlayır.
        /// </summary>
        private void SleepAndRecover()
        {
            CurrentGameState.TimeOfDay = TimeOfDay.Morning;
            CurrentGameState.CurrentDay += 1; // Yeni gün
            ApplyEnergy(+CurrentGameState.MaxEnergy); // Enerjini tam doldur
        }



        /// <summary>
        /// Bir hadisənin (actions) nə qədər enerji sərf edəcəyini yoxlayır (tətbiq etmir).
        /// </summary>
        private int GetEnergyCostFromActions(List<GameAction> actions)
        {
            if (actions == null) return 0;

            // Biz yalnız "Energy" ilə "Subtract" əməliyyatını axtarırıq.
            // "Add" (məsələn, yatmaq) bir "xərc" deyil.
            var energyAction = actions.FirstOrDefault(a => a.Stat == "Energy" && a.Operator == "Subtract");

            return energyAction?.Value ?? 0; // Əgər yoxdursa 0 qaytarır
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
           // CurrentGameState.PlayerMoney += moneyEarned; // Qazanılan pulu əlavə et

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
                if (moneyEarned < 0)
                {
                    CurrentGameState.PlayerMoney += moneyEarned; // azaldır
                    CurrentGameState.AddTransaction("Work Penalty", -moneyEarned, TransactionType.Expense, DateTime.Now);
                }
                                                             // Optionally log failed work attempt if needed
                ShowDialogue("reaction_work_end_fail");
            }

            // Make sure PlayerMoney change notification reaches WalletViewModel
            // The PropertyChanged subscription should handle this.

            // TODO: Pulun dəyişdiyi digər yerlərdə də (məsələn, dialoq actionları) AddTransactionToWallet çağırılmalıdır.
        }

        /// <summary>
        /// Marketdən əşya almaq üçün əsas komanda.
        /// </summary>
        public async Task PurchaseItem(MarketItem item)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Alış-veriş 1 slot aparır
                if (!await TryUseEventSlot(1)) return;

                const int MAX_ITEM_STACK = 3;
                int currentItemCount = Inventory.GetItemCount(item.ItemId);

                if (currentItemCount >= MAX_ITEM_STACK)
                {
                    await ShowNotification("Bu əşyadan kifayət qədər var!");
                    return;
                }

                if (CurrentGameState.PlayerMoney >= item.Price)
                {
                    CurrentGameState.PlayerMoney -= item.Price;
                    CurrentGameState.AddTransaction($"Purchased: {item.Name}", item.Price, TransactionType.Expense, DateTime.Now);

                    Inventory.AddItem(new InventoryItem
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        IconPath = item.IconPath
                    });
                    await ShowNotification($"+1 {item.Name}");
                }
                else
                {
                    await ShowNotification("Kifayət qədər pulunuz yoxdur!");
                }
            }
            finally
            {
                IsBusy = false;
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