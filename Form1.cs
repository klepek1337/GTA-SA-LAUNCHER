using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private List<string?> gamePaths = new List<string?>(); // Użyj nullable

        public Form1()
        {
            InitializeComponent();

            // Początkowo ukryj pasek postępu
            progressBarPythonInstall.Visible = false;

            // Sprawdzamy, czy Python jest zainstalowany
            if (!IsPythonInstalled())
            {
                DialogResult result = MessageBox.Show("Python nie jest zainstalowany. Czy chcesz go zainstalować?", "Brak Pythona", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    InstallPython();  // Instaluje Pythona
                }
                else
                {
                    MessageBox.Show("Aplikacja nie będzie działać bez Pythona.");
                    Application.Exit();  // Zakończ aplikację
                }
            }
            else
            {
                SetBackgroundImage();
                InitializeButtons();
            }
        }

        // Funkcja sprawdzająca, czy Python jest zainstalowany
        private bool IsPythonInstalled()
        {
            try
            {
                Process.Start("python", "--version");
                return true; // Python jest zainstalowany
            }
            catch
            {
                return false; // Python nie jest zainstalowany
            }
        }

        // Funkcja do instalacji Pythona
        private void InstallPython()
        {
            string pythonInstallerUrl = "https://www.python.org/ftp/python/3.10.5/python-3.10.5-amd64.exe"; // Link do instalatora
            string installerPath = Path.Combine(Path.GetTempPath(), "python_installer.exe");

            try
            {
                // Ustawiamy widoczność paska postępu
                progressBarPythonInstall.Style = ProgressBarStyle.Marquee;
                progressBarPythonInstall.Visible = true;

                // Pobieramy instalator w tle
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                using (var client = new WebClient())
                {
                    client.DownloadFile(pythonInstallerUrl, installerPath);
                }
#pragma warning restore SYSLIB0014 // Type or member is obsolete

                // Uruchamiamy instalator w trybie cichym (bez interakcji użytkownika)
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/quiet InstallAllUsers=1 PrependPath=1", // Parametry do cichej instalacji i dodania Pythona do PATH
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                // Używamy nowego wątku, aby uruchomić instalator, aby nie blokować głównego wątku aplikacji
                _ = Task.Run(() =>
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    Process process = Process.Start(processStartInfo);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    process.WaitForExit();  // Czekamy na zakończenie procesu instalacji
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    // Po zakończeniu instalacji
                    Invoke(new Action(() =>
                    {
                        progressBarPythonInstall.Style = ProgressBarStyle.Blocks;  // Przechodzimy do zwykłego paska postępu
                        progressBarPythonInstall.Value = 100;  // Ustawiamy 100% na pasku

                        // Ukrywamy pasek postępu
                        MessageBox.Show("Python został pomyślnie zainstalowany. Uruchom ponownie aplikację.");
                        Application.Exit();  // Zakończ aplikację, aby użytkownik mógł ją uruchomić ponownie po instalacji
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas pobierania lub instalacji Pythona: {ex.Message}");
            }
        }

        // Funkcja do ustawienia tła z pliku lokalnego
        private void SetBackgroundImage()
        {
            string backgroundPath = @"C:\Users\fulek\GameLauncher\background.jpg";

            if (File.Exists(backgroundPath))
            {
                BackgroundImage = Image.FromFile(backgroundPath);
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                MessageBox.Show("Obrazek tła nie został znaleziony.");
            }
        }

        // Inicjalizacja przycisków
        private void InitializeButtons()
        {
            Controls.Clear(); // Usuń istniejące kontrolki, aby uniknąć duplikatów

            for (int i = 0; i < gamePaths.Count; i++)
            {
                int currentIndex = i; // Lokalne przypisanie zmiennej, aby uniknąć problemów z delegatami
                Button gameButton = new Button
                {
                    Text = gamePaths[i] != null ? Path.GetFileNameWithoutExtension(gamePaths[i]!) : $"Gra {i + 1}",
                    Location = new Point(50, 50 + (i * 60))
                };
                gameButton.Click += (sender, e) => OnGameButtonClick(currentIndex);
                Controls.Add(gameButton);
            }

            // Dodanie przycisku do dodania kolejnych gier
            Button addGameButton = new Button
            {
                Text = "Dodaj grę",
                Location = new Point(50, 50 + (gamePaths.Count * 60) + 20)
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            addGameButton.Click += AddMoreGames;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            Controls.Add(addGameButton);
        }

        // Funkcja obsługująca kliknięcie przycisku gry
        private void OnGameButtonClick(int index)
        {
            if (index < 0 || index >= gamePaths.Count)
            {
                MessageBox.Show("Nieprawidłowy indeks przycisku gry.");
                return;
            }

            if (!string.IsNullOrEmpty(gamePaths[index]))
            {
                LaunchGame(gamePaths[index]!); // Uruchom grę, jeśli ścieżka istnieje
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Pliki wykonywalne (*.exe, *.lnk)|*.exe;*.lnk",
                    Title = "Wybierz plik gry"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePaths[index] = openFileDialog.FileName;
                    InitializeButtons(); // Zaktualizuj przyciski
                }
            }
        }

        // Funkcja obsługująca dodanie nowych gier
        private void AddMoreGames(object sender, EventArgs e)
        {
            if (gamePaths.Count < 5) // Maksymalnie 5 gier
            {
                gamePaths.Add(null);
                InitializeButtons(); // Odśwież przyciski
            }
            else
            {
                MessageBox.Show("Osiągnięto maksymalną liczbę gier.");
            }
        }

        // Funkcja do uruchamiania gry lub skrótu
        private void LaunchGame(string gamePath)
        {
            if (File.Exists(gamePath))
            {
                try
                {
                    if (gamePath.ToLower().EndsWith(".lnk"))
                    {
                        // Uruchamiamy plik .lnk (skrót)
                        string pythonScript = Path.Combine(Path.GetTempPath(), "launch_game.py");

                        // Tworzymy skrypt Python
                        File.WriteAllText(pythonScript, $"import os\nos.startfile('{gamePath}')");

                        // Uruchamiamy skrypt Python
                        Process.Start("python", pythonScript);
                    }
                    else
                    {
                        Process.Start(gamePath); // Uruchamiamy plik .exe
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas uruchamiania gry: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Plik gry nie istnieje.");
            }
        }
    }
}
