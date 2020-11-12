using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PriceHistory : Window
    {
        private readonly PriceHistoryViewModel _viewModel;

        public PriceHistory(string gametitle)
        {
            InitializeComponent();

            string gamepricehistoryid = SaveLoadUtils.GetGameNameID(gametitle);
            _viewModel = new PriceHistoryViewModel(gamepricehistoryid);

            lvPrices.ItemsSource = _viewModel.Prices;
            txtTitle.Text = gametitle;

        }



    }
}
