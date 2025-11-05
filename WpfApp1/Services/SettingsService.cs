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
    // Bu xidməti "Singleton" edirik ki, bütün proqram eyni tənzimləmə obyektindən istifadə etsin.
    public class SettingsService
    {
        private static readonly Lazy<SettingsService> lazy = new Lazy<SettingsService>(() => new SettingsService());
        public static SettingsService Instance => lazy.Value;

        private readonly string _settingsFilePath;
        private GameSettings _cachedSettings; // Tənzimləmələri yaddaşda saxlamaq üçün

        // Konstruktoru 'private' edirik ki, kimsə kənardan yenisini yarada bilməsin
        private SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string gameFolderName = "Alex_Mai";
            string settingsFolderPath = Path.Combine(appDataPath, gameFolderName);

            Directory.CreateDirectory(settingsFolderPath);

            _settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
        }

        // Tənzimləmələri yükləyən (və ya yaddaşdan gətirən) əsas metod
        public GameSettings GetSettings()
        {
            if (_cachedSettings == null)
            {
                _cachedSettings = LoadSettingsFromFile();
            }
            return _cachedSettings;
        }

        // Yaddaşdakı tənzimləmələri fayla yazır
        public void SaveSettings()
        {
            if (_cachedSettings == null) return; // Yaddaşda heç nə yoxdursa, heç nə etmə

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_cachedSettings, options);
            File.WriteAllText(_settingsFilePath, jsonString);
        }

        // Fayldan yükləyən daxili metod
        private GameSettings LoadSettingsFromFile()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new GameSettings(); // Standart tənzimləmələr
            }

            string jsonString = File.ReadAllText(_settingsFilePath);
            try
            {
                return JsonSerializer.Deserialize<GameSettings>(jsonString) ?? new GameSettings();
            }
            catch
            {
                return new GameSettings(); // Zədəli fayl halında standart
            }
        }
    }
}
