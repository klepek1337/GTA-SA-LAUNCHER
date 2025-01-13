using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private GameSettings gameSettings;
        private FlowLayoutPanel flowLayoutPanel;

        public Form1()
        {
            InitializeComponent();

            // Tworzenie FlowLayoutPanel
            flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent // Ustawienie przezroczystości
            };
            Controls.Add(flowLayoutPanel);

            // Ładujemy ustawienia gier z pliku
            gameSettings = GameSettings.LoadSettings();

            // Inicjalizujemy przyciski gier
            InitializeButtons();

            // Ustawienie tła aplikacji
            SetBackgroundImage();

            // Ustawienie ikony aplikacji na pasku zadań
            SetAppIcon();
        }

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

        private void InitializeButtons()
        {
            flowLayoutPanel.Controls.Clear();

            for (int i = 0; i < gameSettings.GamePaths.Count; i++)
            {
                int currentIndex = i;
                Button gameButton = new Button
                {
                    Text = GetGameName(gameSettings.GamePaths[i]),
                    Size = new Size(150, 50),
                    Margin = new Padding(10)
                };

                // Dodajemy ikonę gry
                gameButton.Image = GetGameIcon(gameSettings.GamePaths[i]);
                gameButton.TextImageRelation = TextImageRelation.ImageBeforeText;

                // Obsługa kliknięcia przycisku
                gameButton.Click += (sender, e) => OnGameButtonClick(currentIndex);

                flowLayoutPanel.Controls.Add(gameButton);
            }

            // Dodajemy przycisk do dodawania nowych gier
            Button addGameButton = new Button
            {
                Text = "Dodaj grę",
                Size = new Size(150, 50),
                Margin = new Padding(10)
            };
            addGameButton.Click += AddMoreGames;
            flowLayoutPanel.Controls.Add(addGameButton);
        }

        private string GetGameName(string? gamePath)
        {
            if (string.IsNullOrEmpty(gamePath))
                return "Nieznana Gra";

            string fileName = Path.GetFileNameWithoutExtension(gamePath).ToLower();
            if (fileName.Contains("gta-sa"))
                return "SAMP";
            if (fileName.Contains("gta_sa"))
                return "GTA SA";
            if (fileName.Contains("mta"))
                return "MTA";

            return Path.GetFileNameWithoutExtension(gamePath);
        }

        private void AddMoreGames(object sender, EventArgs e)
        {
            if (gameSettings.GamePaths.Count < 24)
            {
                gameSettings.GamePaths.Add(null);
                InitializeButtons();
            }
            else
            {
                MessageBox.Show("Osiągnięto maksymalną liczbę gier.");
            }
        }

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
                }
            }
        }

        private void LaunchGame(string gamePath)
        {
            if (File.Exists(gamePath))
            {
                try
                {
                    Process.Start(gamePath);
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

        private Image? GetGameIcon(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath)) return null;

            string fileName = Path.GetFileNameWithoutExtension(gamePath).ToLower();
            if (fileName.Contains("gta-sa"))
                return LoadIconFromResource("GameLauncher.samp.ico");
            if (fileName.Contains("gta_sa"))
                return LoadIconFromResource("GameLauncher.gta_sa.ico");
            if (fileName.Contains("mta"))
                return LoadIconFromResource("GameLauncher.mta.ico");

            return null;
        }

        private Image? LoadIconFromResource(string iconName)
        {
            try
            {
                using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName))
                {
                    return iconStream != null ? new Icon(iconStream).ToBitmap() : null;
                }
            }
            catch
            {
                return null;
            }
        }

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gameSettings.SaveSettings();
            base.OnFormClosing(e);
        }
    }
}
