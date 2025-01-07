using System.Diagnostics;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private List<string?> gamePaths = new List<string?>(); // Użyj nullable

        public Form1()
        {
            InitializeComponent();
            SetBackgroundImage(); // Ustawienie tła
            InitializeButtons(); // Inicjalizacja przycisków
        }

        // Metoda do ustawienia tła z pliku lokalnego
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
            addGameButton.Click += AddMoreGames;
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
                        string? targetPath = GetTargetPathFromShortcut(gamePath);
                        if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                        {
                            Process.Start(targetPath);
                        }
                        else
                        {
                            MessageBox.Show("Nie można znaleźć pliku docelowego skrótu.");
                        }
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

        // Funkcja do odczytania celu skrótu .lnk
        private string? GetTargetPathFromShortcut(string shortcutPath)
        {
            try
            {
                var wshShell = new WshShell();
                var shortcut = wshShell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            catch
            {
                return null;
            }
        }
    }

    internal interface IWshShortcut
    {
        string? TargetPath { get; }
    }

    internal class WshShell
    {
        public WshShell() { }

        internal IWshShortcut CreateShortcut(string shortcutPath)
        {
            throw new NotImplementedException();
        }
    }
}
