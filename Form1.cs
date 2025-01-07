using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

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
            for (int i = 0; i < 3; i++)
            {
                Button gameButton = new Button();
                gameButton.Text = $"Gra {i + 1}";
                gameButton.Location = new Point(50, 50 + (i * 60));
                gameButton.Click += (sender, e) => OnGameButtonClick(i);
                Controls.Add(gameButton);
            }

            // Dodanie przycisku do dodania kolejnych gier
            Button addGameButton = new Button();
            addGameButton.Text = "Dodaj grę";
            addGameButton.Location = new Point(50, 230);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            addGameButton.Click += AddMoreGames;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            Controls.Add(addGameButton);
        }

        // Funkcja obsługująca kliknięcie przycisku gry
        private void OnGameButtonClick(int index)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki wykonywalne (*.exe)|*.exe",
                Title = "Wybierz plik gry"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                gamePaths[index] = openFileDialog.FileName;
                MessageBox.Show($"Gra {index + 1} wybrana: {gamePaths[index]}");
            }
        }

        // Funkcja obsługująca dodanie nowych gier
        private void AddMoreGames(object sender, EventArgs e)
        {
            if (gamePaths.Count < 5) // Maksymalnie 5 gier
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                gamePaths.Add(null); // Dodaj nową pozycję na liście
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                InitializeButtons(); // Ponownie zainicjalizuj przyciski, aby uwzględnić nową grę
            }
            else
            {
                MessageBox.Show("Osiągnięto maksymalną liczbę gier.");
            }
        }
    }
}
