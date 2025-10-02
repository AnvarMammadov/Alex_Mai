// Services/DialogueService.cs
using System.Collections.Generic;
using System.IO;
using System.Text.Json; // Bunu əlavə edin
using Alex_Mai.Models;


namespace Alex_Mai.Services
{
    public class DialogueService
    {
        private Dictionary<string, DialogueNode> _dialogueScript;

        public DialogueService(string scriptPath)
        {
            var jsonText = File.ReadAllText(scriptPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _dialogueScript = JsonSerializer.Deserialize<Dictionary<string, DialogueNode>>(jsonText, options);
        }

        public DialogueNode GetNode(string nodeId)
        {
            if (_dialogueScript.ContainsKey(nodeId))
            {
                return _dialogueScript[nodeId];
            }
            return null;
        }


        public Dictionary<string, string> GetLocationIndex()
        {
            using var stream = File.OpenRead("Data/dialogue.json");        // _jsonPath: "Data/dialogue.json"
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (root.TryGetProperty("locationIndex", out var locIdx))
            {
                foreach (var prop in locIdx.EnumerateObject())
                {
                    dict[prop.Name] = prop.Value.GetString();
                }
            }
            return dict;
        }
    }
}