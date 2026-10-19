using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MonsterEnergyCollectionApp
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Monster> MonstersCollection { get; set; }

        public Add addWindow;
        public Edit editWindow;
        public Gallery galleryWindow;
        public RatingTable ratingTable;

        public MainWindow()
        {
            InitializeComponent();

            MonstersCollection = new ObservableCollection<Monster>();

            ratingTable = new RatingTable(MonstersCollection);
            addWindow = new Add(MonstersCollection, ratingTable);
            galleryWindow = new Gallery(MonstersCollection);

            LoadData();

            string imagesFolderPath = 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            MonsterVault_Main.Text = MonstersCollection.Count.ToString();
            MonstersCollection.CollectionChanged += (s, e) => CountTastySimpleYukyMonsters();
            MonstersCollection.CollectionChanged += (s, e) => MonsterVault_Main.Text = MonstersCollection.Count.ToString();
            MonstersCollection.CollectionChanged += (s, e) => SaveData();
            MonstersCollection.CollectionChanged += (s, e) => GetLastMonsterFromCollection();

            CountTastySimpleYukyMonsters();
            GetLastMonsterFromCollection();
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();

            MonstersCollection.CollectionChanged += (s, ev) => CountTastySimpleYukyMonsters();
            MonstersCollection.CollectionChanged += (s, ev) => GetLastMonsterFromCollection();
            MonstersCollection.CollectionChanged += (s, ev) => MonsterVault_Main.Text = MonstersCollection.Count.ToString();
            MonstersCollection.CollectionChanged += (s, ev) => SaveData();

            MonsterVault_Main.Text = MonstersCollection.Count.ToString();
            CountTastySimpleYukyMonsters();
            GetLastMonsterFromCollection();
        }


        private void SaveData()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string directoryPath = Path.Combine(baseDirectory, "monstersData");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string dbPath = Path.Combine(directoryPath, "MonsterEnergy.db");
            string connectionStr = $"Data Source={dbPath};";

            using IDbConnection db = new SqliteConnection(connectionStr);
            db.Open();

            using var transaction = db.BeginTransaction();

            try
            {
                db.Execute("DELETE FROM Monsters;", transaction);

                db.Execute("DELETE FROM sqlite_sequence WHERE name='Monsters';", null, transaction);

                string insertQuery = """
                    INSERT INTO Monsters 
                    (Image, Name, Number, Type, Price, Taste, Mark, Date, Design) 
                    VALUES (@Image, @Name, @Number, @Type, @Price, @Taste, @Mark, @Date, @Design);
                    """;

                db.Execute(insertQuery, MonstersCollection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void LoadData()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string directoryPath = Path.Combine(baseDirectory, "monstersData");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string dbPath = Path.Combine(directoryPath, "MonsterEnergy.db");
            string connectionStr = $"Data Source={dbPath};";

            using IDbConnection db = new SqliteConnection(connectionStr);
            db.Open();

            string createTableQuery = """
                CREATE TABLE IF NOT EXISTS Monsters
                (
                    MonsterId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Image TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Number INT NOT NULL,
                    Type TEXT NOT NULL,
                    Price NUMERIC NOT NULL CHECK (Price >= 0),
                    Taste TEXT NOT NULL,
                    Mark INT NOT NULL CHECK (Mark > 0 AND Mark < 11),
                    Date DATE NOT NULL,
                    Design Text NULL
                );
                """;
            db.Execute(createTableQuery);

            var finalItems = db.Query<Monster>("SELECT * FROM Monsters");
            MonstersCollection.Clear();
            foreach (var item in finalItems)
            {
                MonstersCollection.Add(item);
            }
        }

        private void GetLastMonsterFromCollection()
        {
            Monster zero = new Monster()
            {
                Image = "-",
                Name = "-",
                Type = "-",
                Price = 0,
                Taste = "-",
                Mark = 1
            };

            Monster monster = MonstersCollection.Count > 0 ? MonstersCollection[MonstersCollection.Count - 1] : zero;

            LastMonsterMark_Value.Text = monster.Mark + " / 10";
            LastMonsterName_Value.Text = monster.Name;
            LastMonsterPrice_Value.Text = monster.Price.ToString() + " ₽";
            LastMonsterTaste_Value.Text = monster.Taste;
            LastMonsterType_Value.Text = monster.Type;
            LastMonsterDate_Value.Text = monster.Date;
            LastMonsterDesign_Value.Text = monster.Design + " / 10";

            try 
            {
                if (!string.IsNullOrEmpty(monster.Image) && monster.Image != "-" && File.Exists(monster.Image))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(monster.Image, UriKind.Absolute));
                    LastMonsterImage.Source = bitmap;
                }
                else
                {
                    LastMonsterImage.Source = null;
                }

                TestImg.Text = string.Empty;
                ImageNumber_Value.Text = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка декодирования картинки: {ex.Message}");
                LastMonsterImage.Source = null;
                MessageBox.Show($"Ошибка сохранения в БД: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CountTastySimpleYukyMonsters()
        {
            int tasty = 0;
            int simple = 0;
            int yukky = 0;

            foreach (Monster monster in MonstersCollection)
            {
                if (monster.Taste == "Вкусный")
                {
                    tasty += 1;
                }
                else if (monster.Taste == "Обычный")
                {
                    simple += 1;
                }
                else
                {
                    yukky += 1;
                }
            }

            TastyMonsters_Count.Text = tasty.ToString();
            SimpleMonsters_Count.Text = simple.ToString();
            YukkyMonsters_Count.Text = yukky.ToString();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            addWindow.Show();

            this.Hide();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            editWindow = new Edit(MonstersCollection[MonstersCollection.Count - 1], MonstersCollection);
            editWindow.Show();
        }

        private void GalleryBtn_Click(object sender, RoutedEventArgs e)
        {
            galleryWindow.Show();

            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void RatingBtn_Click(object sender, RoutedEventArgs e)
        {
            ratingTable.Show();

            this.Hide();
        }
    }
}