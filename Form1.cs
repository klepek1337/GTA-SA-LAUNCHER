using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // Załaduj ikonę z zasobów
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GameLauncher.gtasa.ico"))
            {
                if (iconStream != null)
                {
                    this.Icon = new Icon(iconStream);
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Ładujemy ustawienia gier z pliku
            gameSettings = GameSettings.LoadSettings();

            // Inicjalizujemy przyciski gier
            InitializeButtons();

            SetBackgroundImage();
        }

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
			try
			{
				// Odczytanie obrazu z zasobów osadzonych
				using (var stream = GetType().Assembly.GetManifestResourceStream("GameLauncher.background.jpg"))
				{
					if (stream != null)
					{
						BackgroundImage = Image.FromStream(stream);
						BackgroundImageLayout = ImageLayout.Stretch;
					}
					else
					{
						MessageBox.Show("Obrazek tła nie został znaleziony w zasobach.");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Błąd podczas ładowania tła: {ex.Message}");
			}
		}

		// Inicjalizacja przycisków (zwiększanie rozmiaru i ustawienie poziome)
		private void InitializeButtons()
		{
			Controls.Clear();

			int buttonWidth = 200; // Szerokość przycisków
			int buttonHeight = 100; // Wysokość przycisków
			int spacing = 10; // Odstęp między przyciskami

			// Rozpoczynamy rozmieszczanie przycisków w poziomie
			int xPosition = 50; // Początkowa pozycja X
			int yPosition = 50; // Pozycja Y, przyciski będą w jednej linii poziomej

			for (int i = 0; i < gameSettings.GamePaths.Count; i++)
			{
				int currentIndex = i;
				Button gameButton = new Button
				{
					Text = gameSettings.GamePaths[i] != null ? Path.GetFileNameWithoutExtension(gameSettings.GamePaths[i]) : $"Gra {i + 1}",
					Location = new Point(xPosition, yPosition), // Ustawiamy pozycję przycisku
					Size = new Size(buttonWidth, buttonHeight), // Zwiększamy rozmiar przycisku
					Font = new Font("Arial", 12, FontStyle.Bold), // Zwiększamy czcionkę i ustawiamy pogrubienie
					BackColor = Color.LightSkyBlue, // Zmieniamy kolor tła przycisku
					ForeColor = Color.White, // Zmieniamy kolor tekstu
					FlatStyle = FlatStyle.Flat, // Zmieniamy styl przycisku na płaski
				};

				gameButton.FlatAppearance.BorderSize = 0; // Usuwamy obramowanie

				gameButton.Click += (sender, e) => OnGameButtonClick(currentIndex);
				Controls.Add(gameButton);

				// Przesuwamy pozycję X, aby przyciski były w poziomie
				xPosition += buttonWidth + spacing;
			}

			// Dodajemy przycisk "Dodaj grę"
			Button addGameButton = new Button
			{
				Text = "Dodaj grę",
				Location = new Point(xPosition, yPosition), // Pozycja przycisku dodawania gry
				Size = new Size(buttonWidth, buttonHeight),
				Font = new Font("Arial", 12, FontStyle.Bold),
				BackColor = Color.LightCoral,
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
			};
			addGameButton.FlatAppearance.BorderSize = 0;
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

		// Zapisujemy ustawienia przed zamknięciem aplikacji
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			gameSettings.SaveSettings();
			base.OnFormClosing(e);
		}
	}
}
