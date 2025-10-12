using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Alex_Mai.Models;

namespace Alex_Mai.Services
{
     public class OrderService
    {
        private readonly List<Order> _orders;

        public OrderService(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _orders = JsonSerializer.Deserialize<List<Order>>(jsonText, options);
        }

        public List<Order> GetAllOrders() => _orders;
    }
}
