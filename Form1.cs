using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Timers;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private GameSettings gameSettings;
        private FlowLayoutPanel flowLayoutPanel;
        private System.Timers.Timer backgroundChangeTimer;
        private int currentBackgroundIndex = 0;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Form1()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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

            InitializeBackgroundChangeTimer();
        }

        private void InitializeBackgroundChangeTimer()
        {
            backgroundChangeTimer = new System.Timers.Timer(60000); // 10 sekund
            backgroundChangeTimer.Elapsed += (s, e) => Invoke((Action)(() => SetBackgroundImage()));
            backgroundChangeTimer.AutoReset = true;
            backgroundChangeTimer.Start();
        }

        private void SetBackgroundImage()
        {
            string[] backgroundPaths = new string[]
            {
                "background.jpg",
                "background2.jpg",
                "background3.jpg",
                "background4.jpg",
                "background5.jpg",
                "background6.jpg",
                "background7.jpg",
                "background8.jpg"
            };

            currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundPaths.Length;
            string backgroundPath = backgroundPaths[currentBackgroundIndex];

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GameLauncher.{backgroundPath}"))
            {
                if (stream != null)
                {
                    BackgroundImage = Image.FromStream(stream);
                    BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    MessageBox.Show($"Obrazek tła {backgroundPath} nie został znaleziony.");
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            backgroundChangeTimer?.Stop();
            backgroundChangeTimer?.Dispose();
            gameSettings.SaveSettings();
            base.OnFormClosing(e);
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

                gameButton.Image = GetGameIcon(gameSettings.GamePaths[i]);
                gameButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                gameButton.Click += (sender, e) => OnGameButtonClick(currentIndex);

                flowLayoutPanel.Controls.Add(gameButton);
            }

            Button addGameButton = new Button
            {
                Text = "Dodaj grę",
                Size = new Size(150, 50),
                Margin = new Padding(10)
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            addGameButton.Click += AddMoreGames;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
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

            return fileName;
        }

        private void AddMoreGames(object sender, EventArgs e)
        {
            if (gameSettings.GamePaths.Count < 24)
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
                return LoadIconFromResource("samp.ico");
            if (fileName.Contains("gta_sa"))
                return LoadIconFromResource("gta_sa.ico");
            if (fileName.Contains("mta"))
                return LoadIconFromResource("mta.ico");

            return null;
        }

        private Image? LoadIconFromResource(string iconName)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GameLauncher.{iconName}"))
                {
                    return iconStream != null ? new Icon(iconStream).ToBitmap() : null;
                }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch
            {
                return null;
            }
        }

        private void SetAppIcon()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GameLauncher.gtasa.ico"))
            {
                if (iconStream != null)
                {
                    Icon = new Icon(iconStream);
                }
                else
                {
                    MessageBox.Show("Ikona aplikacji (gtasa.ico) nie została znaleziona.");
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }
}

