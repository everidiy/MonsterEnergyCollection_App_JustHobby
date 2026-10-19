using System.Collections.ObjectModel;
using System.Windows;

namespace MonsterEnergyCollectionApp
{

    public partial class Gallery : Window
    {
        private ObservableCollection<Monster> _monsters = new ObservableCollection<Monster>();

        public Gallery(ObservableCollection<Monster> collection)
        {
            InitializeComponent();

            _monsters = collection;
            MonstersListBox.ItemsSource = _monsters;

            CountQunatityAndPrices();
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            MonstersListBox.Items.Refresh();

            CountQunatityAndPrices();
        }

        private void CountQunatityAndPrices()
        {
            MonstersCountValue.Text = _monsters.Count.ToString() + " шт";   

            double prices = 0;
            foreach (Monster monster in _monsters)
            {
                prices += monster.Price;
            }

            MonstersPricesValue.Text = prices.ToString() + " ₽";
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
    }
}
