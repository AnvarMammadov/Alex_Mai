// Services/ChatService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Alex_Mai.Models; // ChatMessage modelinə çıxış üçün

namespace Alex_Mai.Services
{
    // JSON strukturuna uyğun köməkçi siniflər (internal qalması daha yaxşıdır)
    internal class JsonChatMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
    }

    internal class ConversationData
    {
        public List<JsonChatMessage> History { get; set; } = new List<JsonChatMessage>();
        // Alex-in mesajlarına qarşı case-insensitive müqayisə üçün
        public Dictionary<string, string> Responses { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    internal class ChatFileStructure
    {
        // Söhbət ID-ləri üçün case-insensitive müqayisə
        public Dictionary<string, ConversationData> Conversations { get; set; } = new Dictionary<string, ConversationData>(StringComparer.OrdinalIgnoreCase);
        // Cavab ID-ləri üçün case-insensitive müqayisə
        public Dictionary<string, List<JsonChatMessage>> ResponseMessages { get; set; } = new Dictionary<string, List<JsonChatMessage>>(StringComparer.OrdinalIgnoreCase);
    }

    // Əsas servis sinfi
    public class ChatService
    {
        private ChatFileStructure _chatData = new ChatFileStructure(); // Bütün JSON məlumatını burada saxlayırıq
        private readonly string _filePath;

        public ChatService(string filePath = "Data/chats.json")
        {
            _filePath = filePath;
            LoadConversations(); // Konstruktorda faylı yükləyirik
        }

        // JSON faylını oxuyub _chatData obyektinə deserializasiya edən metod
        private void LoadConversations()
        {
            try
            {
                var jsonText = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // JSON açarlarının böyük/kiçik hərfinə həssas olmaması üçün
                    AllowTrailingCommas = true // JSON-da sondakı vergüllərə icazə verir (rahatlıq üçün)
                };

                // Bütün JSON strukturunu birbaşa _chatData obyektinə deserializasiya edirik
                var loadedData = JsonSerializer.Deserialize<ChatFileStructure>(jsonText, options);
                if (loadedData != null)
                {
                    _chatData = loadedData;
                }
                else
                {
                    // Əgər fayl boşdursa və ya struktur uyğun gəlmirsə, _chatData boş qalacaq
                    Console.WriteLine($"Warning: Chat file '{_filePath}' is empty or has an incorrect structure.");
                    _chatData = new ChatFileStructure(); // Boş obyektlə başlatmaq daha təhlükəsizdir
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: Chat file not found at '{_filePath}'");
                _chatData = new ChatFileStructure(); // Fayl tapılmasa da boş obyektlə başlat
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing chat file '{_filePath}': {ex.Message}");
                _chatData = new ChatFileStructure(); // JSON xətası olsa da boş obyektlə başlat
            }
            catch (Exception ex) // Digər gözlənilməz xətalar üçün
            {
                Console.WriteLine($"An unexpected error occurred while loading chats: {ex.Message}");
                _chatData = new ChatFileStructure(); // Digər xətalar olsa da boş obyektlə başlat
            }
        }

        // Verilən ID-li söhbətin yalnız ilkin tarixçəsini (history) qaytaran metod
        public List<ChatMessage> GetConversationHistory(string conversationId)
        {
            var messages = new List<ChatMessage>();
            // Əgər _chatData-da həmin conversationId varsa və onun History siyahısı boş deyilsə
            if (_chatData.Conversations.TryGetValue(conversationId, out var conversationData) && conversationData.History != null)
            {
                // Hər bir JsonChatMessage-i ChatMessage-ə çevirib siyahıya əlavə et
                foreach (var jsonMsg in conversationData.History)
                {
                    messages.Add(ConvertJsonMessage(jsonMsg));
                }
            }
            return messages; // Tapılmasa boş siyahı qaytarır
        }

        // Alex-in mesajına uyğun cavabı (və ya cavabları) tapan metod
        public List<ChatMessage> FindResponse(string conversationId, string alexMessage)
        {
            var responseMessages = new List<ChatMessage>();
            string responseId = null; // Tapılacaq cavabın ID-si

            // Əgər həmin conversationId mövcuddursa
            if (_chatData.Conversations.TryGetValue(conversationId, out var conversationData))
            {
                // Əvvəlcə Alex-in mesajının tam uyğunluğunu yoxla
                if (conversationData.Responses != null && conversationData.Responses.TryGetValue(alexMessage, out responseId))
                {
                    // Tam uyğun cavab tapıldı
                }
                // Əgər tam uyğunluq tapılmasa, "default" cavabı axtar
                else if (conversationData.Responses != null && conversationData.Responses.TryGetValue("default", out responseId))
                {
                    // "default" cavab tapıldı
                }
            }

            // Əgər bir responseId tapdıqsa
            if (responseId != null && _chatData.ResponseMessages.TryGetValue(responseId, out var jsonResponseMessages) && jsonResponseMessages != null)
            {
                // Həmin ID-yə uyğun mesajları (bir və ya bir neçə ola bilər) ChatMessage-ə çevir
                foreach (var jsonMsg in jsonResponseMessages)
                {
                    responseMessages.Add(ConvertJsonMessage(jsonMsg));
                }
            }
            // Əgər heç bir cavab ID-si tapılmadısa və söhbət Mai ilədirsə, standart "..." cavabı əlavə et
            else if (responseId == null && conversationId.StartsWith("mai_", StringComparison.OrdinalIgnoreCase))
            {
                responseMessages.Add(new ChatMessage { Sender = "Mai", Text = "...", IsSentByUser = false, Timestamp = DateTime.Now });
            }

            return responseMessages; // Tapılan cavabları (və ya boş siyahını) qaytar
        }

        // JsonChatMessage-i ChatMessage-ə çevirən köməkçi metod
        private ChatMessage ConvertJsonMessage(JsonChatMessage jsonMsg)
        {
            if (jsonMsg == null) return null; // Null yoxlaması
            return new ChatMessage
            {
                Sender = jsonMsg.Sender ?? "Unknown", // Null olarsa "Unknown"
                Text = jsonMsg.Text ?? "", // Null olarsa boş string
                IsSentByUser = (jsonMsg.Sender ?? "").Equals("Alex", StringComparison.OrdinalIgnoreCase), // Sender null olarsa false
                Timestamp = DateTime.Now // Yükləmə zamanı vaxtı təyin et
            };
        }
    }
}