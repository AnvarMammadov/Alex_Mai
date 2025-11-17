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
    public class CalendarService
    {
        private List<CalendarEvent> _allEvents = new List<CalendarEvent>();
        private readonly string _filePath;

        // Konstruktor, JSON faylının yolunu alır
        public CalendarService(string filePath = "Data/calendar_events.json")
        {
            _filePath = filePath;
            LoadEvents();
        }

        // JSON faylını oxuyan daxili metod
        private void LoadEvents()
        {
            try
            {
                var jsonText = File.ReadAllText(_filePath);

                // JsonSerializerOptions, JSON açarlarının (məsələn, "title") 
                // Model xüsusiyyətlərinə (məsələn, "Title") uyğunlaşmasını təmin edir
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var loadedEvents = JsonSerializer.Deserialize<List<CalendarEvent>>(jsonText, options);

                if (loadedEvents != null)
                {
                    _allEvents = loadedEvents;
                }
            }
            catch (Exception ex)
            {
                // Xəta baş verərsə (məsələn, fayl tapılmasa), konsola yazdır
                // və oyunun dayanmaması üçün boş siyahı ilə davam et
                Console.WriteLine($"Error loading calendar events from {_filePath}: {ex.Message}");
                _allEvents = new List<CalendarEvent>();
            }
        }

        // ViewModel-in çağıracağı əsas metod
        public List<CalendarEvent> GetAllEvents()
        {
            return _allEvents;
        }
    }
}
