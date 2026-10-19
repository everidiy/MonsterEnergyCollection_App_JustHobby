using System;
using System.Windows;
using System.Windows.Controls;

namespace MonsterEnergyCollectionApp
{
    /// <summary>
    /// Логика взаимодействия для MonsterCard.xaml
    /// </summary>
    public partial class MonsterCard : UserControl
    {
        public Edit editWindow;

        public MonsterCard()
        {
            InitializeComponent();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var mainWin = Application.Current.MainWindow as MainWindow;

            if (mainWin == null) return;

            if (this.DataContext is Monster currentMonster)
            {
                editWindow = new Edit(currentMonster, mainWin.MonstersCollection);
                editWindow.Show();

                mainWin.Hide();
            }
        }
    }
}
