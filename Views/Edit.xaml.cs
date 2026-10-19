using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Dapper;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MonsterEnergyCollectionApp
{
    public partial class Edit : Window
    {
        private readonly Monster _monster;
        private ObservableCollection<Monster> _monsters = new ObservableCollection<Monster>();
        private string tempImgStr = string.Empty;

        public Edit(Monster monster, ObservableCollection<Monster> monsters)
        {
            InitializeComponent();

            _monster = monster ?? throw new ArgumentNullException(nameof(monster));
            _monsters = monsters;

            LoadMonsterData();
        }

        private void LoadMonsterData()
        {
            EditNameValue.Text = _monster.Name;

            EditPriceValue.Text = _monster.Price.ToString(
                CultureInfo.InvariantCulture);

            EditTypesValue.Text = _monster.Type;

            EditMonsterDateValue.SelectedDate =
                DateTime.TryParseExact(
                    _monster.Date,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date)
                ? date
                : null;

            SelectComboBoxItem(EditMarksValue, _monster.Mark.ToString());
            SelectComboBoxItem(EditDesignValue, _monster.Design);

            if (!string.IsNullOrWhiteSpace(_monster.Image) &&
                File.Exists(_monster.Image))
            {
                ShowImage(_monster.Image);
            }
        }

        private void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
        }

        private void ShowImage(string path)
        {
            try
            {
                Button btn = EditImageBtn;
                Image imgElement = btn.Template.FindName("SelectedMonsterImage", btn) as Image;
                StackPanel promptElement = btn.Template.FindName("UploadPrompt", btn) as StackPanel;

                if (imgElement != null && promptElement != null)
                {
                    string absolutePath = Path.GetFullPath(path);

                    // Проверяем, существует ли файл физически на диске
                    if (!File.Exists(absolutePath)) return;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    // Передаем строго абсолютный путь
                    bitmap.UriSource = new Uri(absolutePath, UriKind.Absolute);
                    // None решает проблему падения встроенного кодека HEIC в Windows
                    bitmap.CacheOption = BitmapCacheOption.None;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Замораживаем для стабильности UI

                    imgElement.Source = bitmap;
                    promptElement.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.heic)|*.jpg;*.jpeg;*.png;*.bmp;*.heic|Все файлы (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    tempImgStr = openFileDialog.FileName;

                    if (sender is Button btn)
                    {
                        Image imgElement = btn.Template.FindName("SelectedMonsterImage", btn) as Image;
                        StackPanel promptElement = btn.Template.FindName("UploadPrompt", btn) as StackPanel;

                        if (imgElement != null && promptElement != null)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(tempImgStr, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.None; // Синхронизируем настройки
                            bitmap.EndInit();
                            bitmap.Freeze();

                            imgElement.Source = bitmap;
                            promptElement.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Вспомогательная функция для безопасного извлечения текста из ComboBox в WPF
            string GetComboBoxText(ComboBox comboBox)
            {
                if (comboBox == null) return "";
                if (comboBox.SelectedItem is ComboBoxItem item)
                {
                    return item.Content?.ToString()?.Trim() ?? "";
                }
                return comboBox.SelectedItem?.ToString()?.Trim() ?? comboBox.Text?.Trim() ?? "";
            }

            string type = EditTypesValue.Text.Trim();
            string name = EditNameValue.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название монстра.");
                return;
            }

            if (!double.TryParse(
                EditPriceValue.Text.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.");
                return;
            }

            int mark = int.Parse(GetComboBoxText(EditMarksValue));
            string design = GetComboBoxText(EditDesignValue);

            if (mark < 1 || mark > 10)
            {
                MessageBox.Show("Выберите оценку от 1 до 10.");
                return;
            }

            string taste = mark > 7
                ? "Вкусный"
                : mark > 4
                    ? "Обычный"
                    : "Ужасный";

            string savedImagePath = _monster.Image;

            if (!string.IsNullOrWhiteSpace(tempImgStr))
            {
                try
                {
                    string targetFolder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images");

                    Directory.CreateDirectory(targetFolder);

                    string extension = Path.GetExtension(tempImgStr);
                    string fileName = Guid.NewGuid() + extension;
                    string finalPath = Path.Combine(targetFolder, fileName);

                    File.Copy(tempImgStr, finalPath);

                    savedImagePath = finalPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Не удалось сохранить изображение: {ex.Message}");
                    return;
                }
            }

            _monster.Image = savedImagePath;
            _monster.Name = name;
            _monster.Type = string.IsNullOrWhiteSpace(type) ? "—" : type;
            _monster.Taste = taste;
            _monster.Price = price;
            _monster.Mark = mark;
            _monster.Date = EditMonsterDateValue.SelectedDate?.ToString("dd.MM.yyyy") ?? "—";
            _monster.Design = string.IsNullOrWhiteSpace(design) ? "—" : design;

            string directoryPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "monstersData");

            Directory.CreateDirectory(directoryPath);

            string dbPath = Path.Combine(directoryPath, "MonsterEnergy.db");
            string connectionStr = $"Data Source={dbPath};";

            try
            {
                using IDbConnection db = new SqliteConnection(connectionStr);
                db.Open();

                using var transaction = db.BeginTransaction();

                string updateQuery = """
            UPDATE Monsters
            SET
                Image = @Image,
                Name = @Name,
                Type = @Type,
                Taste = @Taste,
                Price = @Price,
                Mark = @Mark,
                Date = @Date,
                Design = @Design
            WHERE MonsterId = @MonsterId;
            """;

                var parameters = new
                {
                    _monster.Image,
                    _monster.Name,
                    _monster.Type,
                    _monster.Taste,
                    _monster.Price,
                    _monster.Mark,
                    _monster.Date,
                    _monster.Design, // Убедитесь, что в классе свойство называется именно 'Design' с большой буквы
                    _monster.MonsterId
                };

                int rowsAffected = db.Execute(
                    updateQuery,
                    parameters,
                    transaction);

                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    MessageBox.Show(
                        $"Монстр с ID {_monster.MonsterId} не найден в базе данных. Изменения не сохранены.");
                    return;
                }

                transaction.Commit();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении базы данных:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
        }


        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
        }
    }
}
