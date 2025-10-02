using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Alex_Mai.Models
{
    public class DialogueNode
    {
        public string Character { get; set; }
        public string Text { get; set; }
        public string Sprite { get; set; }
        public string BGM { get; set; }
        public string Background { get; set; }
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }

    public class Choice
    {
        public string Text { get; set; }
        public string NextNodeId { get; set; }
        public List<GameAction> Actions { get; set; } = new List<GameAction>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        // --- YENİ SAHƏLƏR ---
        public string Action { get; set; } // məsələn "open_map"
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();
        public int? CooldownMinutes { get; set; }
        public string Tooltip { get; set; }
    }


    public class Requirement
    {
        public string Stat { get; set; }
        public int? Gte { get; set; }
        public int? Lte { get; set; }
    }

    public class Effect
    {
        public string Stat { get; set; }
        public string Op { get; set; }     // "Add" | "Subtract" | "Set"
        public int? Value { get; set; }
        public string Flag { get; set; }
        public bool? BoolValue { get; set; }
        public List<string> InventoryAdd { get; set; }
    }

    public class Outcome
    {
        public List<Effect> Effects { get; set; } = new();
        public string NextNodeId { get; set; }
    }

    public class GameAction
    {
        public string Stat { get; set; }
        public string Operator { get; set; }
        public int Value { get; set; }
    }

    public class Condition
    {
        public string Stat { get; set; }
        public string Operator { get; set; }
        public int Value { get; set; }
    }
}
