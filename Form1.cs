using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private GameSettings gameSettings;

        public Form1()
        {
            InitializeComponent();

            // Ładujemy ustawienia gier z pliku
            gameSettings = GameSettings.LoadSettings();

            // Inicjalizujemy przyciski gier
            InitializeButtons();

            // Ustawienie tła aplikacji
            SetBackgroundImage();

            // Ustawienie ikony aplikacji na pasku zadań
            SetAppIcon();
        }

        // Funkcja do sprawdzania, czy Python jest zainstalowany
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
                progressBarPythonInstall.Style = ProgressBarStyle.Marquee;
                progressBarPythonInstall.Visible = true;

#pragma warning disable SYSLIB0014 // Type or member is obsolete
                using (var client = new WebClient())
                {
                    client.DownloadFile(pythonInstallerUrl, installerPath);
                }
#pragma warning restore SYSLIB0014 // Type or member is obsolete

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/quiet InstallAllUsers=1 PrependPath=1",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                _ = Task.Run(() =>
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    Process process = Process.Start(processStartInfo);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    process.WaitForExit();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    Invoke(new Action(() =>
                    {
                        progressBarPythonInstall.Style = ProgressBarStyle.Blocks;
                        progressBarPythonInstall.Value = 100;

                        MessageBox.Show("Python został pomyślnie zainstalowany. Uruchom ponownie aplikację.");
                        Application.Exit();
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas instalacji Pythona: {ex.Message}");
            }
        }

        // Funkcja do ustawienia tła z pliku
        private void SetBackgroundImage()
        {
            string backgroundPath = Path.Combine(Application.StartupPath, "background.jpg");
            MessageBox.Show($"Ścieżka do tła: {backgroundPath}"); // Debugowanie ścieżki

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

        // Funkcja do ustawienia ikony aplikacji na pasku
        private void SetAppIcon()
        {
            string iconPath = Path.Combine(Application.StartupPath, "gtasa.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }
            else
            {
                MessageBox.Show("Ikona aplikacji (gtasa.ico) nie została znaleziona.");
            }
        }

        // Inicjalizacja przycisków
        private void InitializeButtons()
        {
            Controls.Clear();

            for (int i = 0; i < gameSettings.GamePaths.Count; i++)
            {
                int currentIndex = i;
                Button gameButton = new Button
                {
                    Text = gameSettings.GamePaths[i] != null ? Path.GetFileNameWithoutExtension(gameSettings.GamePaths[i]) : $"Gra {i + 1}",
                    Location = new Point(50, 50 + (i * 60)),
                    Size = new Size(200, 50) // Ustawienie szerokości i wysokości przycisku
                };

                // Sprawdzenie, czy mamy ikonę i ustawienie jej
                gameButton.Image = GetGameIcon(gameSettings.GamePaths[i]);
                gameButton.TextImageRelation = TextImageRelation.ImageBeforeText;

                gameButton.Click += (sender, e) => OnGameButtonClick(currentIndex);
                Controls.Add(gameButton);
            }

            Button addGameButton = new Button
            {
                Text = "Dodaj grę",
                Location = new Point(50, 50 + (gameSettings.GamePaths.Count * 60) + 20),
                Size = new Size(200, 50)
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            addGameButton.Click += AddMoreGames;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            Controls.Add(addGameButton);
        }

        // Funkcja obsługująca kliknięcie przycisku gry
        private void OnGameButtonClick(int index)
        {
            if (index < 0 || index >= gameSettings.GamePaths.Count)
            {
                MessageBox.Show("Nieprawidłowy indeks przycisku gry.");
                return;
            }

            if (!string.IsNullOrEmpty(gameSettings.GamePaths[index]))
            {
                LaunchGame(gameSettings.GamePaths[index]);
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
                    gameSettings.GamePaths[index] = openFileDialog.FileName;
                    InitializeButtons();

                    if (gameSettings.GamePaths[index]?.ToLower().EndsWith(".lnk") == true && !IsPythonInstalled())
                    {
                        InstallPython();
                    }
                }
            }
        }

        // Funkcja do dodania nowych gier
        private void AddMoreGames(object sender, EventArgs e)
        {
            if (gameSettings.GamePaths.Count < 5)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                gameSettings.GamePaths.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                InitializeButtons();
            }
            else
            {
                MessageBox.Show("Osiągnięto maksymalną liczbę gier.");
            }
        }

        // Funkcja do uruchamiania gry
        private void LaunchGame(string gamePath)
        {
            if (File.Exists(gamePath))
            {
                try
                {
                    if (gamePath.ToLower().EndsWith(".lnk"))
                    {
                        string pythonScript = Path.Combine(Path.GetTempPath(), "launch_game.py");
                        File.WriteAllText(pythonScript, $"import os\nos.startfile('{gamePath}')");
                        Process.Start("python", pythonScript);
                    }
                    else
                    {
                        Process.Start(gamePath);
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

        // Funkcja do pobierania ikony w zależności od nazwy gry
        private Image? GetGameIcon(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath)) return null;

            string fileName = Path.GetFileNameWithoutExtension(gamePath).ToLower();

            if (fileName.Contains("gta-sa"))
            {
                return LoadIconFromResource("GameLauncher.samp.ico");
            }
            if (fileName.Contains("gta_sa"))
            {
                return LoadIconFromResource("GameLauncher.gta_sa.ico");
            }
            if (fileName.Contains("mta"))
            {
                return LoadIconFromResource("GameLauncher.mta.ico");
            }

            return null; // Jeśli brak ikony
        }

        // Funkcja do załadowania ikony z zasobów
        private Image? LoadIconFromResource(string iconName)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName))
                {
                    if (iconStream != null)
                    {
                        return new Icon(iconStream).ToBitmap();
                    }
                    else
                    {
                        return null;
                    }
                }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch
            {
                return null;
            }
        }

        // Zapisujemy ustawienia przed zamknięciem aplikacji
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gameSettings.SaveSettings();
            base.OnFormClosing(e);
        }
    }
}

