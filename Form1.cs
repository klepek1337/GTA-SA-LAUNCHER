using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameLauncher
{
    public partial class Form1 : Form
    {
        private List<string> gamePaths = new List<string>();

        public Form1()
        {
            InitializeComponent();
            SetBackgroundImage(); // Ustawienie tła
            InitializeButtons(); // Inicjalizacja przycisków
        }

        // Metoda do ustawienia tła z pliku lokalnego
        private void SetBackgroundImage()
        {
            string backgroundPath = @"C:\Users\fulek\GameLauncher\background.jpg"; // Ścieżka do pliku tła na dysku

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
            Controls.Clear();  // Czyści obecne kontrolki

            // Dodaj przyciski na podstawie liczby gier w liście gamePaths
            for (int i = 0; i < gamePaths.Count; i++)
            {
                Button gameButton = new Button();
                string gameName = Path.GetFileNameWithoutExtension(gamePaths[i]); // Wyciągamy nazwę pliku bez rozszerzenia
                gameButton.Text = gameName;
                gameButton.Location = new Point(50, 50 + (i * 60));

                // Używamy Tag, aby przypisać indeks gry do przycisku
                gameButton.Tag = i;  // Zapisanie indeksu gry w Tag
                gameButton.Click += GameButton_Click;
                Controls.Add(gameButton);
            }

            // Dodanie przycisku do dodania kolejnych gier
            Button addGameButton = new Button();
            addGameButton.Text = "Dodaj grę";
            addGameButton.Location = new Point(50, 50 + (gamePaths.Count * 60));
            addGameButton.Click += AddGame; // Zmiana na wywołanie funkcji do dodania gry
            Controls.Add(addGameButton);
        }

        // Funkcja obsługująca kliknięcie przycisku gry
        private void GameButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int gameIndex = (int)button.Tag;  // Pobranie indeksu z Tag

            // Uruchomienie gry na podstawie zapisanego indeksu
            LaunchGame(gamePaths[gameIndex]);
        }

        // Funkcja obsługująca dodanie nowych gier
        private void AddGame(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki wykonywalne (*.exe)|*.exe",
                Title = "Wybierz plik gry"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (gamePaths.Count < 5) // Maksymalnie 5 gier
                {
                    gamePaths.Add(openFileDialog.FileName); // Dodanie ścieżki gry do listy
                    InitializeButtons();  // Ponownie zainicjalizuj przyciski, aby uwzględnić nową grę
                }
                else
                {
                    MessageBox.Show("Osiągnięto maksymalną liczbę gier.");
                }
            }
        }

        // Funkcja do uruchomienia gry
        private void LaunchGame(string gamePath)
        {
            if (File.Exists(gamePath))
            {
                Process.Start(gamePath);  // Uruchomienie gry
            }
            else
            {
                MessageBox.Show("Plik gry nie istnieje.");
            }
        }
    }
}
