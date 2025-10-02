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
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string gameFolderName = "Alex_Mai"; // SaveLoadService-dəki adla eyni olsun
            string settingsFolderPath = Path.Combine(appDataPath, gameFolderName);

            Directory.CreateDirectory(settingsFolderPath);

            _settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
        }

        public void SaveSettings(GameSettings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, jsonString);
        }

        public GameSettings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                // Əgər settings faylı yoxdursa, standart dəyərlərlə yenisini yarat
                return new GameSettings();
            }

            string jsonString = File.ReadAllText(_settingsFilePath);
            try
            {
                return JsonSerializer.Deserialize<GameSettings>(jsonString);
            }
            catch
            {
                // Əgər fayl zədəlidirsə, yenə də standart dəyərlərlə başla
                return new GameSettings();
            }
        }
    }
}
