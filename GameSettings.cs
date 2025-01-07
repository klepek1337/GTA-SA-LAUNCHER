using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GameLauncher
{
    public class GameSettings
    {
        public List<string> GamePaths { get; set; } = new List<string>();

        private const string SettingsFilePath = "gamesettings.json";

        // Metoda do zapisywania ustawień do pliku JSON
        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisywania ustawień: {ex.Message}");
            }
        }

        // Metoda do ładowania ustawień z pliku JSON
        public static GameSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    return JsonConvert.DeserializeObject<GameSettings>(json) ?? new GameSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas ładowania ustawień: {ex.Message}");
            }
            return new GameSettings(); // Domyślnie pusty zestaw gier
        }
    }
}
