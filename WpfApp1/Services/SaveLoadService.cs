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
    public class SaveLoadService
    {
        private readonly string _saveFilePath;

        public SaveLoadService()
        {
            // Save faylını AppData qovluğunda saxlamaq peşəkar yanaşmadır.
            // Bu, oyunu siləndə və ya başqa yerə köçürəndə save faylının itməməsini təmin edir.
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string gameFolderName = "Alex_and_Mai"; // Bura öz oyununuzun adını yazın
            string saveFolderPath = Path.Combine(appDataPath, gameFolderName);

            // Qovluq yoxdursa, onu yarat
            Directory.CreateDirectory(saveFolderPath);

            _saveFilePath = Path.Combine(saveFolderPath, "save.json");
        }

        public void SaveGame(SaveData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_saveFilePath, jsonString);
        }

        public SaveData LoadGame()
        {
            if (!File.Exists(_saveFilePath))
            {
                return null; // Save faylı yoxdur
            }

            string jsonString = File.ReadAllText(_saveFilePath);
            SaveData data = JsonSerializer.Deserialize<SaveData>(jsonString);
            return data;
        }
    }
}
