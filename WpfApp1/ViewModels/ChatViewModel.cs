using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Alex_Mai.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        private readonly CharacterStats _maiStats; // Store the reference
        private readonly ChatService _chatService;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();

        private string _currentMaiStatus;
        private string _maiMoodIcon;

        public string CurrentMaiStatus
        {
            get
            {
                // Determine status based on stats
                if (_maiStats.Stress > 70) return "Feeling stressed... 😥";
                if (_maiStats.Affection < 10 && _maiStats.Trust < 10) return "A bit distant.";
                if (_maiStats.Affection > 50) return "Feeling happy today! 😊";
                if (_maiStats.Trust > 30) return "Online";
                return "Just chilling..."; // Default
            }
        }

        // *** YENİ: Hesablanmış İkon Xüsusiyyəti ***
        public string MaiMoodIcon
        {
            get
            {
                // Determine icon based on stats (using existing sprites for now)
                if (_maiStats.Stress > 70) return "/Assets/Sprites/mai_sad.png"; //
                if (_maiStats.Affection > 50) return "/Assets/Sprites/mai_happy.png"; //
                // Add more conditions if needed
                return "/Assets/Icons/ui_mai_icon.jpg"; // Default icon
            }
        }


        [ObservableProperty]
        private string _newMessageText;

        private string _currentConversationId = "mai_initial";

        [ObservableProperty]
        private bool _isMaiTyping = false;

        // *** YENİ: Konstruktoru CharacterStats qəbul etmək üçün yeniləyin ***
        public ChatViewModel(PhoneViewModel parent, CharacterStats characterStats)
        {
            _parentViewModel = parent;
            _maiStats = characterStats;
            _chatService = new ChatService(); // *** YENİ: ChatService yaradılır ***

            // Subscribe to PropertyChanged event of CharacterStats
            _maiStats.PropertyChanged += MaiStats_PropertyChanged;

            LoadConversationHistory(_currentConversationId);
        }

        // *** DƏYİŞİKLİK: Yalnız tarixi yükləyir ***
        private void LoadConversationHistory(string conversationId)
        {
            Messages.Clear();
            List<ChatMessage> historyMessages = _chatService.GetConversationHistory(conversationId);
            foreach (var message in historyMessages)
            {
                Messages.Add(message);
            }
            _currentConversationId = conversationId; // Update current conversation context
        }

        // *** YENİ: Statlar dəyişdikdə UI-ı yeniləmək üçün Event Handler ***
        private void MaiStats_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // When Affection or Stress changes, notify that our calculated properties need updating
            if (e.PropertyName == nameof(CharacterStats.Affection) ||
                e.PropertyName == nameof(CharacterStats.Stress) ||
                e.PropertyName == nameof(CharacterStats.Trust)) // Add Trust if it affects status
            {
                OnPropertyChanged(nameof(CurrentMaiStatus));
                OnPropertyChanged(nameof(MaiMoodIcon));
            }
        }

        // *** YENİ: Mesajları yükləmək üçün metod ***
        private void LoadConversation(string conversationId)
        {
            Messages.Clear(); // Clear existing messages if any
            List<ChatMessage> loadedMessages = _chatService.GetConversationHistory(conversationId);
            foreach (var message in loadedMessages)
            {
                Messages.Add(message);
            }
            // Optional: Trigger scroll to bottom after loading messages
            // Dispatcher.InvokeAsync(() => ScrollToBottom(), System.Windows.Threading.DispatcherPriority.Background);
            // Note: Scrolling logic is currently in ChatView.xaml.cs, this might need adjustment if loading happens after view is shown.
        }

        [RelayCommand]
        private async Task SendMessage() // Change to async Task
        {
            if (string.IsNullOrWhiteSpace(NewMessageText) || IsMaiTyping) return; // Prevent sending while Mai is "typing"

            string alexMessageText = NewMessageText; // Store before clearing
            NewMessageText = string.Empty; // Clear input immediately

            var alexMessage = new ChatMessage
            {
                Sender = "Alex",
                Text = alexMessageText,
                Timestamp = DateTime.Now,
                IsSentByUser = true
            };
            Messages.Add(alexMessage);
            
            IsMaiTyping = true;

            try // Use try-finally to ensure indicator is turned off
            {
                await Task.Delay(1500); // Simulate typing delay (1.5 seconds)

                List<ChatMessage> maiResponses = _chatService.FindResponse(_currentConversationId, alexMessageText);

                foreach (var response in maiResponses)
                {
                    Messages.Add(response);
                    if (maiResponses.Count > 1) await Task.Delay(700); // Shorter delay between multiple messages
                }
            }
            finally
            {
                // *** YENİ: Göstəricini deaktivləşdir ***
                IsMaiTyping = false;
            }
        }
        [RelayCommand]
        private void ViewMaiStatus()
        {
            System.Windows.MessageBox.Show($"Mai's current status: {CurrentMaiStatus}\n(Debug: Aff={_maiStats.Affection}, Trust={_maiStats.Trust}, Stress={_maiStats.Stress})");
        }

        [RelayCommand]
        private void NavigateToHome()
        {
            // *** YENİ: Abunəlikdən çıxmağı unutmayın ***
            _maiStats.PropertyChanged -= MaiStats_PropertyChanged;
            _parentViewModel.NavigateToHome();
        }

        public void Cleanup()
        {
            // MaiStats dəyişikliklərini izləməyi dayandır
            if (_maiStats != null) // Ehtiyat üçün null yoxlaması
            {
                _maiStats.PropertyChanged -= MaiStats_PropertyChanged;
                Console.WriteLine("ChatViewModel cleaned up."); // Debug üçün mesaj (opsional)
            }
        }

        // Optional: Ensure unsubscription if the ViewModel is somehow destroyed unexpectedly
        // You might need a more robust mechanism depending on ViewModel lifecycle management
        // For now, unsubscription in NavigateToHome should cover the main scenario.
    }
}