using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MonsterEnergyCollectionApp
{
    public partial class Add : Window
    {
        private RatingTable ratingTable;

        private ObservableCollection<Monster> _monsters = new ObservableCollection<Monster>();

        public Add(ObservableCollection<Monster> collection, RatingTable ratingTable)
        {
            InitializeComponent();

            this.ratingTable = ratingTable;
            _monsters = collection;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            Application.Current.MainWindow.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private string tempImgStr = string.Empty;

        [STAThread]
        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.heic)|*.jpg;*.jpeg;*.png;*.bmp;*.heic|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    tempImgStr = openFileDialog.FileName;

                    try
                    {
                        Button btn = (Button)sender;

                        Image imgElement = btn.Template.FindName("SelectedMonsterImage", btn) as Image;
                        StackPanel promptElement = btn.Template.FindName("UploadPrompt", btn) as StackPanel;

                        if (imgElement != null && promptElement != null)
                        {
                            BitmapImage bitmap = new BitmapImage(new Uri(tempImgStr));
                            imgElement.Source = bitmap;
                            promptElement.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при чтении или кодировании файла: {ex.Message}");
                    }
                }
            }
        }

        private void AddNewMonster_Click(object sender, RoutedEventArgs e)
        {
            string priceText = PriceValue.Text.Replace('.', ',');
            if (!double.TryParse(priceText, out double price))
            {
                price = 0;
            }

            string type = TypesValue.Text.Trim();
            string mark = string.Empty;
            string taste = string.Empty;
            string design = string.Empty;

            if (MarksValue.SelectedItem is ComboBoxItem selectedMark)
            {
                mark = selectedMark.Content.ToString() ?? "-";
            }

            if (DesignValue.SelectedItem is ComboBoxItem selectedDesign)
            {
                design = selectedDesign.Content.ToString() ?? "-";
            }

            if (int.Parse(mark) > 7)
            {
                taste = "Вкусный";
            }
            else if (int.Parse(mark) > 4 && int.Parse(mark) < 8)
            {
                taste = "Обычный";
            }
            else
            {
                taste = "Ужасный";
            }

            string savedImagePath = "";

            if (!string.IsNullOrEmpty(tempImgStr) && File.Exists(tempImgStr))
            {
                try
                {
                    // Путь к нашей папке Images
                    string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    // Получаем расширение файла (например, .jpg или .png)
                    string extension = Path.GetExtension(tempImgStr);

                    // Генерируем уникальное имя файла с помощью Guid (например, "a1b2c3d4...jpg")
                    string uniqueFileName = Guid.NewGuid().ToString() + extension;

                    // Итоговый путь, куда сохранится картинка
                    string finalPath = Path.Combine(targetFolder, uniqueFileName);

                    // Копируем файл в папку приложения
                    File.Copy(tempImgStr, finalPath, true);

                    // В модель монстра сохраняем ИМЕННО этот новый постоянный путь!
                    savedImagePath = finalPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить картинку в папку: {ex.Message}");
                    savedImagePath = "-";
                }
            }

            Monster newMonster = new Monster()
            {
                Image = savedImagePath,
                Number = _monsters.Count() + 1,
                Name = $"Monster Energy {type} " + NameValue.Text.ToString().Trim(),
                Type = string.IsNullOrEmpty(type) ? "—" : type,
                Taste = string.IsNullOrEmpty(taste) ? "—" : taste,
                Price = price,
                Mark = int.Parse(mark),
                Date = MonsterDateValue.SelectedDate?.ToString("dd.MM.yyyy") ?? "—",
                Design = string.IsNullOrEmpty(design) ? "-" : design
            };

            _monsters.Add(newMonster);
            ratingTable.AddObjectToTier(newMonster);

            PriceValue.Text = string.Empty;
            NameValue.Text = string.Empty;

            TypesValue.SelectedIndex = -1;
            MarksValue.SelectedIndex = -1;
            DesignValue.SelectedIndex = -1;

            MonsterDateValue.SelectedDate = null;

            var imgElement = AddImageBtn.Template.FindName("SelectedMonsterImage", AddImageBtn) as System.Windows.Controls.Image;
            StackPanel promptElement = AddImageBtn.Template.FindName("UploadPrompt", AddImageBtn) as StackPanel;

            if (imgElement != null && promptElement != null)
            {
                imgElement.Source = null;
                promptElement.Visibility = Visibility.Visible;
            }
        }
    }
}
