using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Alex_Mai.Models;
using System.IO;

namespace Alex_Mai.Services
{
    public class MarketService
    {
        private readonly List<MarketItem> _items;

        public MarketService(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _items = JsonSerializer.Deserialize<List<MarketItem>>(jsonText, options);
        }

        public List<MarketItem> GetAllItems() => _items;
    }
}
