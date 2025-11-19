using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alex_Mai.Models; // CharacterStats və ChatMessage üçün

namespace Alex_Mai.Services
{
    // --- JSON Strukturu üçün Köməkçi Siniflər ---
    public class ChatRoot
    {
        public NlpData Nlp { get; set; }
        public GameState State { get; set; } // Əgər yoxdursa
        public List<DialogFlow> Dialogs { get; set; }
        public Dictionary<string, List<JsonChatMessage>> ResponseMessages { get; set; }
        // YENİ: Triggerləri oxumaq üçün
        public List<ChatTrigger> Triggers { get; set; }
    }

    // YENİ KLASS
    public class ChatTrigger
    {
        public string Id { get; set; }
        public string Condition { get; set; } // Məs: "Location == park"
        public string DialogId { get; set; }
        public double Chance { get; set; } // 0.0 - 1.0
        public bool Repeatable { get; set; }
    }

    public class NlpData
    {
        public List<Intent> Intents { get; set; }
    }

    public class Intent
    {
        public string Id { get; set; }
        public string Regex { get; set; }
        public List<string> Keywords { get; set; }
    }

    public class DialogFlow
    {
        public string Id { get; set; }
        public List<JsonChatMessage> History { get; set; }
        public List<DialogChoice> Choices { get; set; }
    }

    public class DialogChoice
    {
        public string Intent { get; set; }
        public string GoTo { get; set; }
        public bool Fallback { get; set; }
    }

    public class JsonChatMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public List<string> TextOptions { get; set; }

        // *** YENİ: Şərt sahəsi (məs: "Affection > 50") ***
        public string Condition { get; set; }
    }

    // --- Əsas Servis ---
    public class ChatService
    {
        private ChatRoot _chatData;
        private readonly string _filePath;

        public ChatService(string filePath = "Data/chats.json")
        {
            _filePath = filePath;
            LoadChatData();
        }

        private void LoadChatData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var jsonText = File.ReadAllText(_filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _chatData = JsonSerializer.Deserialize<ChatRoot>(jsonText, options);
                }
                else
                {
                    _chatData = new ChatRoot();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chat Data Load Error: {ex.Message}");
                _chatData = new ChatRoot();
            }
        }

        // 1. Tarixçəni yükləyir
        public List<ChatMessage> GetConversationHistory(string dialogId)
        {
            var messages = new List<ChatMessage>();
            if (_chatData?.Dialogs == null) return messages;

            var dialog = _chatData.Dialogs.FirstOrDefault(d => d.Id == dialogId);
            if (dialog != null && dialog.History != null)
            {
                foreach (var jsonMsg in dialog.History)
                {
                    messages.Add(ConvertJsonMessage(jsonMsg));
                }
            }
            return messages;
        }

        // 2. Əsas "BEYİN" hissəsi: İndi CharacterStats qəbul edir
        public List<ChatMessage> FindResponse(string currentDialogId, string userText, CharacterStats stats)
        {
            var responses = new List<ChatMessage>();
            if (_chatData == null) return responses;

            // A. Cari dialoqu tapırıq
            var currentDialog = _chatData.Dialogs?.FirstOrDefault(d => d.Id == currentDialogId);
            if (currentDialog == null || currentDialog.Choices == null) return responses;

            // B. İstifadəçinin yazdığı mətndən "Niyyət"i (Intent) tapırıq
            string detectedIntentId = DetectIntent(userText);

            // C. Bu niyyətə uyğun seçimi dialoqdan tapırıq
            var matchedChoice = currentDialog.Choices.FirstOrDefault(c => c.Intent == detectedIntentId);

            // Əgər niyyət tapılmadısa, "Fallback" seçimi yoxlayırıq
            if (matchedChoice == null)
            {
                matchedChoice = currentDialog.Choices.FirstOrDefault(c => c.Fallback);
            }

            // D. Nəticə varsa, cavab mesajlarını gətiririk və ŞƏRTLƏRİ yoxlayırıq
            if (matchedChoice != null && !string.IsNullOrEmpty(matchedChoice.GoTo))
            {
                if (_chatData.ResponseMessages.TryGetValue(matchedChoice.GoTo, out var responseList))
                {
                    foreach (var jsonMsg in responseList)
                    {
                        // *** AĞILLI MƏNTİQ: Şərt ödənirsə, bu mesajı seçirik ***
                        if (CheckCondition(jsonMsg.Condition, stats))
                        {
                            responses.Add(ConvertJsonMessage(jsonMsg));

                            // Əgər bir neçə ardıcıl mesaj göndərmək istəmirsənsə, burada 'break' edə bilərsən.
                            // Amma adətən vizual romanlarda şərtə uyğun GƏLƏN İLK variantı götürmək daha təhlükəsizdir.
                            break;
                        }
                    }
                }
            }

            return responses;
        }

        // *** YENİ: Şərtləri Pars edən və Yoxlayan Metod ***
        private bool CheckCondition(string condition, CharacterStats stats)
        {
            // Şərt yoxdursa və ya boşdursa, true qaytar (göstərilsin)
            if (string.IsNullOrEmpty(condition) || condition.Trim().ToLower() == "default") return true;
            if (stats == null) return true; // Stats yoxdursa, hər şeyi göstər

            try
            {
                // Nümunə format: "Affection > 50" və ya "Stress < 20"
                var parts = condition.Trim().Split(' ');
                if (parts.Length != 3) return false; // Format səhvdirsə göstərmə

                string statName = parts[0];
                string oper = parts[1];
                int value = int.Parse(parts[2]);

                int currentStatValue = 0;

                // Statı tapırıq
                switch (statName)
                {
                    case "Affection": currentStatValue = stats.Affection; break;
                    case "Stress": currentStatValue = stats.Stress; break;
                    case "Trust": currentStatValue = stats.Trust; break;
                    // Lazım olsa digərlərini əlavə et
                    default: return false;
                }

                // Müqayisə edirik
                if (oper == ">") return currentStatValue > value;
                if (oper == "<") return currentStatValue < value;
                if (oper == ">=") return currentStatValue >= value;
                if (oper == "<=") return currentStatValue <= value;
                if (oper == "==") return currentStatValue == value;

                return false;
            }
            catch
            {
                return false; // Xəta olsa göstərmə
            }
        }

        // *** YENİLƏNMİŞ: Daha dəqiq Intent tapma ***
        private string DetectIntent(string text)
        {
            if (_chatData?.Nlp?.Intents == null) return null;

            // 1. Mətni normallaşdır
            string normalizedText = NormalizeText(text);
            string bestIntentId = null;
            double bestScore = 0.0;

            foreach (var intent in _chatData.Nlp.Intents)
            {
                // A. Regex Yoxlaması (Hələ də ən güclü prioritetdir)
                if (!string.IsNullOrEmpty(intent.Regex) && Regex.IsMatch(text, intent.Regex))
                    return intent.Id;

                // B. Keywords ilə Fuzzy Matching (Score sistemi)
                if (intent.Keywords != null)
                {
                    foreach (var keyword in intent.Keywords)
                    {
                        double similarity = CalculateSimilarity(normalizedText, keyword.ToLower());

                        // Əgər oxşarlıq 0.75-dən böyükdürsə və indiki ən yaxşı nəticədən yüksəkdirsə
                        if (similarity > 0.75 && similarity > bestScore)
                        {
                            bestScore = similarity;
                            bestIntentId = intent.Id;
                        }
                    }
                }
            }
            return bestIntentId;
        }

        private string NormalizeText(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            // Simvolları sil, kiçilt
            string clean = Regex.Replace(input.ToLower(), @"[^\w\s]", "");
            return clean.Trim();
        }

        private ChatMessage ConvertJsonMessage(JsonChatMessage jsonMsg)
        {
            string textToSend = jsonMsg.Text;

            if (jsonMsg.TextOptions != null && jsonMsg.TextOptions.Count > 0)
            {
                var random = new Random();
                textToSend = jsonMsg.TextOptions[random.Next(jsonMsg.TextOptions.Count)];
            }

            return new ChatMessage
            {
                Sender = jsonMsg.Sender,
                Text = textToSend,
                Timestamp = DateTime.Now // Timestamp əlavə etmək faydalıdır
            };
        }

        // --- Levenshtein Distance (Dəyişilmədi) ---
        public static double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

            source = source.ToLower();
            target = target.ToLower();

            int distance = ComputeLevenshteinDistance(source, target);
            int maxLength = Math.Max(source.Length, target.Length);

            return 1.0 - ((double)distance / maxLength);
        }

        private static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public string CheckForTrigger(string locationId, TimeOfDay timeOfDay)
        {
            if (_chatData?.Triggers == null) return null;

            var random = new Random();

            foreach (var trigger in _chatData.Triggers)
            {
                // Sadə şərt yoxlanışı (Daha mürəkkəb parser gələcəkdə yazıla bilər)
                bool locationMatch = trigger.Condition.Contains($"Location == {locationId}") || !trigger.Condition.Contains("Location");
                bool timeMatch = trigger.Condition.Contains($"TimeOfDay == {timeOfDay}") || !trigger.Condition.Contains("TimeOfDay");

                if (locationMatch && timeMatch)
                {
                    // Şans faktoru (məsələn 50% ehtimal)
                    if (random.NextDouble() <= trigger.Chance)
                    {
                        return trigger.DialogId; // Dialoqun ID-sini qaytar
                    }
                }
            }
            return null;
        }

    }
}