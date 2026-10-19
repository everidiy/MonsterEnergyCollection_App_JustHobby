using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MonsterEnergyCollectionApp
{
    public partial class RatingTable : Window
    {
        public Add addWindow;

        public ObservableCollection<TierRow> TierRows { get; set; }

        public ObservableCollection<Monster> MonstersCollection { get; set; }

        public RatingTable(ObservableCollection<Monster> collection)
        {
            InitializeComponent();

            MonstersCollection = collection;
            TierRows = new ObservableCollection<TierRow>();

            string[] colors =
            {
                "#10B981",
                "#34D399",
                "#6EE7B7",
                "#A3E635",
                "#D9F064",
                "#FDE047",
                "#FBBF24",
                "#FB923C",
                "#F87171",
                "#EF4444"
            };

            for (int i = 10; i >= 1; i--)
            {
                TierRows.Add(new TierRow
                {
                    RatingName = i.ToString(),
                    BackgroundColor = colors[10 - i]
                });
            }

            DataContext = this;

            _ = GetDataFromDb();
        }

        private BitmapImage LoadImgAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "-" || !File.Exists(path)) return null;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);

            //загрузка и декодирование переносятся в фоновый поток
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private async Task GetDataFromDb()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string directoryPath = Path.Combine(baseDirectory, "monstersData");
            string dbPath = Path.Combine(directoryPath, "MonsterEnergy.db");
            string connectionStr = $"Data Source={dbPath};";

            // Читаем базу данных в фоне
            var finalItems = await Task.Run(() =>
            {
                using IDbConnection db = new SqliteConnection(connectionStr);
                db.Open();
                return db.Query<Monster>("SELECT * FROM Monsters").ToList();
            });

            foreach (var item in finalItems)
            {
                var row = TierRows.FirstOrDefault(r => r.RatingName == item.Mark.ToString());

                if (row != null && !string.IsNullOrEmpty(item.Image))
                {
                    BitmapImage bitmap = await Task.Run(() => LoadImgAsync(item.Image));

                    if (bitmap != null)
                    {
                        row.Images.Add(bitmap);
                    }
                }
            }
        }

        public void AddObjectToTier(Monster monster)
        {
            int rating = monster.Mark;

            if (rating < 1 || rating > 10) return;

            var row = TierRows.FirstOrDefault(r => r.RatingName == rating.ToString());

            if (row != null)
            {
                BitmapImage bitmap = LoadImgAsync(monster.Image);
                if (bitmap != null)
                {
                    row.Images.Add(bitmap);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            Application.Current.MainWindow.Show();
        }

        private void AddMonster_Click(object sender, RoutedEventArgs e)
        {
            addWindow = new Add(MonstersCollection, this);
            addWindow.Show();

            this.Hide();
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var row in TierRows)
            {
                row.Images.Clear();
            }

            GetDataFromDb();
        }
    }
}
