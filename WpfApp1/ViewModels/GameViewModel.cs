using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Alex_Mai.Models;
using Alex_Mai.Services;
using Alex_Mai.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Linq;
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

        private Dictionary<string, string> _locationIndex;
        private string _currentNodeId = "";

        // Cari məkan (map keçidlərindən sonra yenilənir)
        private string _currentPlaceId = null;

        // locationIndex-in tərsi: nodeId -> placeId
        private Dictionary<string, string> _nodeToPlace;

        [NotifyPropertyChangedFor(nameof(MainTimeOfDay))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot1))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot2))]
        [NotifyPropertyChangedFor(nameof(NextTimeSlot3))]
        [NotifyPropertyChangedFor(nameof(MaiMood))]
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
        // YENİ: ChatService əlavə olundu
        private readonly ChatService _chatService;

        // Zaman göstəricisi üçün xüsusiyyətlər
        public string MainTimeOfDay => CurrentGameState.TimeOfDay.ToString();
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
            // YENİ: ChatService yaradılır
            _chatService = new ChatService();

            // YENİ: Əgər tarixçə boşdursa, ilk salamlaşmanı yüklə
            if (CurrentGameState.ChatHistory.Count == 0)
            {
                var initialMsgs = _chatService.GetConversationHistory("mai_initial");
                foreach (var msg in initialMsgs)
                {
                    CurrentGameState.ChatHistory.Add(msg);
                }
                CurrentGameState.UnreadMessageCount = initialMsgs.Count;
            }

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
            _currentNodeId = "start_game";
        }

        // CheckForProactiveMessages metodunu TAMAMİLƏ YENİLƏYİN:
        private async void CheckForProactiveMessages()
        {
            string placeToCheck = _currentPlaceId ?? "alex_room";
            string dialogIdToTrigger = null;

            // Ssenari 1: səhər park
            if (placeToCheck == "park" && CurrentGameState.TimeOfDay == TimeOfDay.Morning)
            {
                dialogIdToTrigger = "mai_park_morning";
            }
            // Ssenari 2: axşam işdən sonra trip
            else if (placeToCheck == "part_time" && CurrentGameState.TimeOfDay == TimeOfDay.Evening)
            {
                // Səhər gələn mesaja baxmayıbsa
                if (CurrentGameState.UnreadMessageCount > 0)
                {
                    dialogIdToTrigger = "mai_job_evening_trip";

                    // 1) Yeni trip mesaj(lar)ını əlavə et
                    var newMessages = _chatService.GetConversationHistory(dialogIdToTrigger);
                    foreach (var msg in newMessages)
                    {
                        msg.Timestamp = DateTime.Now;
                        CurrentGameState.ChatHistory.Add(msg);
                    }

                    // 2) Yalnız SON Mai mesajı qalsın, qalan hamısı silinsin
                    var lastMai = CurrentGameState.ChatHistory
                        .LastOrDefault(m => !m.IsSentByUser);

                    if (lastMai != null)
                    {
                        foreach (var msg in CurrentGameState.ChatHistory)
                        {
                            if (!msg.IsSentByUser && !ReferenceEquals(msg, lastMai))
                            {
                                msg.IsDeleted = true;   // silindi kimi işarələ
                            }
                        }
                    }

                    // 3) Stress artırsın
                    MainCharacterStats.Stress += 5;
                    await ShowNotification("Mai mesaj sildi... 🚫");

                    // 4) UnreadMessageCount – indi sadəcə trip mesaj(lar)ı oxunmayıb
                    CurrentGameState.UnreadMessageCount += newMessages.Count;

                    // Artıq aşağıdakı ümumi blok işləməsin
                    _audioService.PlaySFX("notification.mp3");
                    return;
                }
            }

            // --- NORMAL PROAKTİV MESAJ BLOKU ---
            if (!string.IsNullOrEmpty(dialogIdToTrigger))
            {
                _audioService.PlaySFX("notification.mp3");
                await ShowNotification("Mai-dən yeni mesaj 💬");

                var newMessages = _chatService.GetConversationHistory(dialogIdToTrigger);
                foreach (var msg in newMessages)
                {
                    msg.Timestamp = DateTime.Now;
                    CurrentGameState.ChatHistory.Add(msg);
                }

                // BURADA sadəcə ++ yox, gələn mesaj sayını əlavə et
                CurrentGameState.UnreadMessageCount += newMessages.Count;
            }
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
                        break;
                    }
                    CurrentDialogueText += c;
                    await Task.Delay(10);
                }
            }
            finally
            {
                CurrentDialogueText = fullText;
                AreChoicesVisible = true;
            }
        }

        public void SkipDialogueAnimation()
        {
            _isSkippingAnimation = true;
        }

        private async void ShowDialogue(string nodeId)
        {
            _currentNodeId = nodeId;
            AreChoicesVisible = false;
            CurrentChoices.Clear();

            var node = _dialogueService.GetNode(nodeId);
            if (node != null)
            {
                CharacterName = node.Character;
                if (_nodeToPlace != null && _nodeToPlace.TryGetValue(nodeId, out var placeFromNode))
                {
                    _currentPlaceId = placeFromNode;
                }
                await AnimateDialogueText(node.Text);

                if (!string.IsNullOrEmpty(node.Sprite))
                {
                    CurrentCharacterSprite = $"/Assets/Sprites/{node.Sprite}";
                    CharacterSpriteChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    CurrentCharacterSprite = null;
                }

                if (!string.IsNullOrEmpty(node.BGM))
                {
                    _audioService.PlayBGM(node.BGM);
                }

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
        }

        [RelayCommand]
        private async Task SelectChoice(Choice choice)
        {
            if (IsBusy) return;
            if (choice == null) return;
            IsBusy = true;

            try
            {
                _audioService.PlaySFX("click.wav");

                int energyCost = GetEnergyCostFromActions(choice.Actions);
                if (energyCost > 0 && energyCost > CurrentGameState.CurrentEnergy)
                {
                    await ShowNotification("Enerjiniz çatmır...");
                    return;
                }

                if (!await TryUseEventSlot(choice.ActionCost))
                {
                    return;
                }

                ProcessActions(choice.Actions);

                if (!string.IsNullOrEmpty(choice.NextNodeId))
                {
                    if (choice.NextNodeId == "start_cooking_minigame") StartCookingMinigame();
                    else if (choice.NextNodeId == "start_part_time_minigame") { StartWorkMinigame(); }
                    else if (choice.NextNodeId == "go_to_sleep")
                    {
                        SleepAndRecover();
                        ShowDialogue("go_to_sleep");
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
                    case "Energy": statValue = CurrentGameState.CurrentEnergy; break;
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
                        if (!await TryUseEventSlot(1)) return;

                        ApplyEnergy(15);
                        await ShowNotification("+15 Enerji bərpa olundu");
                        break;

                    case "phone":
                        if (!await TryUseEventSlot(0)) return;
                        TogglePhoneView();
                        break;

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
            IsInventoryOpen = false;
            IsPhoneOpen = !IsPhoneOpen;
        }

        [RelayCommand]
        public void ToggleMap()
        {
            if (IsBusy) return;
            IsMapOpen = !IsMapOpen;
        }

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

                    int slotsToUse = 1;

                    bool fromIsHome = IsHomePlace(_currentPlaceId ?? "alex_room");
                    bool toIsHome = IsHomePlace(placeId);

                    if (fromIsHome && toIsHome)
                        slotsToUse = 0;

                    if (!await TryUseEventSlot(slotsToUse)) return;

                    ShowDialogue(nodeId);
                    _currentPlaceId = placeId;

                    // YENİ: Məkan dəyişəndə mesaj gəlib-gəlmədiyini yoxla
                    CheckForProactiveMessages();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static bool IsHomePlace(string placeId)
        {
            return placeId == "alex_room"
                || placeId == "mai_room"
                || placeId == "kitchen"
                || placeId == "bathroom"
                || placeId == "living_room";
        }

        public void ToggleMarket()
        {
            IsInventoryOpen = false;
            IsMarketOpen = !IsMarketOpen;
        }

        private void LoadLocationIndex()
        {
            _locationIndex = _dialogueService.GetLocationIndex();

            _nodeToPlace = new Dictionary<string, string>();
            if (_locationIndex != null)
            {
                foreach (var kv in _locationIndex)
                {
                    var placeId = kv.Key;
                    var nodeId = kv.Value;
                    if (!string.IsNullOrEmpty(nodeId))
                        _nodeToPlace[nodeId] = placeId;
                }
            }
        }

        private async Task<bool> TryUseEventSlot(int slotsToUse = 1)
        {
            if (slotsToUse == 0) return true;

            if (CurrentGameState.TimeOfDay == TimeOfDay.Night && CurrentGameState.CurrentEventSlots <= 0)
            {
                await ShowNotification("Artıq çox gecdir... Yatmalısan.");
                return false;
            }

            if (CurrentGameState.CurrentEventSlots > 0)
            {
                CurrentGameState.CurrentEventSlots -= slotsToUse;
            }

            if (CurrentGameState.CurrentEventSlots <= 0 && CurrentGameState.TimeOfDay != TimeOfDay.Night)
            {
                await AdvanceTime();
                await ShowNotification("Zaman irəliləyir...");
            }

            return true;
        }

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
                    break;
            }

            // YENİ: Zaman dəyişəndə mesaj gəlib-gəlmədiyini yoxla
            CheckForProactiveMessages();
        }

        private void SleepAndRecover()
        {
            CurrentGameState.TimeOfDay = TimeOfDay.Morning;
            CurrentGameState.CurrentDay += 1;
            ApplyEnergy(+CurrentGameState.MaxEnergy);
        }

        private int GetEnergyCostFromActions(List<GameAction> actions)
        {
            if (actions == null) return 0;
            var energyAction = actions.FirstOrDefault(a => a.Stat == "Energy" && a.Operator == "Subtract");
            return energyAction?.Value ?? 0;
        }

        public void StartCookingMinigame()
        {
            CookingGame.Reset();
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
            WorkGame.StartShift();
            IsWorkMinigameOpen = true;
        }

        public void EndWorkMinigame(int moneyEarned)
        {
            IsWorkMinigameOpen = false;

            if (moneyEarned > 0)
            {
                int balanceBefore = CurrentGameState.PlayerMoney;
                CurrentGameState.PlayerMoney += moneyEarned;
                CurrentGameState.AddTransaction("Part-time Job Earnings", moneyEarned, TransactionType.Income, DateTime.Now);
                ShowDialogue("reaction_work_end_success");
            }
            else
            {
                if (moneyEarned < 0)
                {
                    CurrentGameState.PlayerMoney += moneyEarned;
                    CurrentGameState.AddTransaction("Work Penalty", -moneyEarned, TransactionType.Expense, DateTime.Now);
                }
                ShowDialogue("reaction_work_end_fail");
            }
        }

        public async Task PurchaseItem(MarketItem item)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
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