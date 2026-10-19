using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MonsterEnergyCollectionApp
{
    public class TierRow
    {
        public string RatingName { get; set; }
        public string BackgroundColor { get; set; } 

        public ObservableCollection<BitmapImage> Images { get; set; } = new ObservableCollection<BitmapImage>();
    }
}
