using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFUI
{
    public class WishlistViewModel : INotifyPropertyChanged

    {
        private string _jsonFilePath;
        private object _locker = new object();


        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }


        private bool _isGameOnSale = false;

        public bool IsGameOnSale
        {
            get { return _isGameOnSale; }
            set
            {
                _isGameOnSale = value;
                OnPropertyChanged();
            }
        }


        private List<GameItem> _games;
        public List<GameItem> Games
        {
            get
            {
                return _games;
            }
            set
            {
                _games = value;
                OnPropertyChanged();
            }
        }

        string _dataDirectory;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public WishlistViewModel()
        {
            CreateDataDirectory();

            Games = new List<GameItem>();
            try
            {
                lock (_locker)
                {
                    IsBusy = true;
                    Games = SaveLoadUtils.LoadFromJson(_jsonFilePath);
                    foreach (var game in Games)
                    {
                        game.ImageSource = LoadImage(GetImagePath(game.Title));
                    }
                    IsBusy = false;
                }
            }
            catch (Exception e)
            {
                //ignore
            }

        }

        public void OpenGameUrl(object item)
        {
            GameItem game = item as GameItem;
            if (game == null)
            {
                return;
            }
            try
            {
                Process.Start(game.URL);
            }
            catch (Exception e)
            {
                ShowMessage(e.ToString(), MessageType.Error);
            }

        }

        private void ShowMessage(string message, MessageType messageType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                var popup = new Popup();
                popup.SetMessage(message, messageType);
                popup.ShowDialog();
            });
        }

        public void UpdateGamePrices()
        {
            lock (_locker)
            {
                IsBusy = true;
                foreach (var game in Games)
                {
                    try
                    {
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(game.URL);
                        ScrapePrices(game, doc);

                        if (game.FinalPrice != "")
                            ImageSave(game, doc);

                        SavePriceHistory(game);

                    }
                    catch (Exception e)
                    {
                        ShowMessage(e.ToString(), MessageType.Error);
                    }

                }
                SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
                CheckIfGameIsOnSale();
                IsBusy = false;
            }
        }

        private void CheckIfGameIsOnSale()
        {
            IsGameOnSale = false;
            foreach (var game in Games)
            {
                if (!string.IsNullOrEmpty(game.OriginalPrice))
                {
                    IsGameOnSale = true;
                    break;
                }
            }

            foreach (var game in Games)
            {
                if (!string.IsNullOrEmpty(game.PSPlusPrice))
                {
                    IsGameOnSale = true;
                    break;
                }
            }
        }

        public void AddGameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (CheckIfDuplicate(url))
            {
                ShowMessage("This game is already in the list 🙂", MessageType.Info);
                return;
            }

            lock (_locker)
            {

                IsBusy = true;
                try
                {

                    DoAddGameFromUrl(url);
                }
                catch (Exception e)
                {
                    ShowMessage(e.ToString(), MessageType.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public bool CheckIfDuplicate(string url)
        {
            foreach (var game in Games)
            {
                if (game.URL == url)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveGameFromList(object game)
        {
            var gameItem = game as GameItem;
            if (gameItem == null)
            {
                return;
            }
            lock (_locker)
            {
                if (File.Exists(GetImagePath(gameItem.Title)))
                {
                    try
                    {
                        File.Delete(GetImagePath(gameItem.Title));
                    }
                    catch (Exception e)
                    {
                        ShowMessage(e.ToString(), MessageType.Error);
                    }
                }


                Games.Remove(gameItem);
                SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
                CheckIfGameIsOnSale();
            }
        }

        private void CreateDataDirectory()
        {
            
            string cwd = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _dataDirectory = Path.Combine(cwd, "PSWishlist");
            Directory.CreateDirectory(_dataDirectory);
            _jsonFilePath = Path.Combine(_dataDirectory, "Wishlist.json");

        }

        private void DoAddGameFromUrl(string url)
        {
            GameItem game = new GameItem()
            {
                Title = string.Empty,
                FinalPrice = string.Empty,
                OriginalPrice = string.Empty,
                PSPlusPrice = string.Empty,
                URL = url
            };
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h1[@data-qa]");
                       

            game.Title = titleNodes.FirstOrDefault().InnerHtml;

            ScrapePrices(game, doc);

            if (game.FinalPrice != "")
                ImageSave(game, doc);

            Games.Add(game);
            SavePriceHistory(game);
            SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
            CheckIfGameIsOnSale();
        }


        private void ImageSave(GameItem game, HtmlDocument doc)
        {
            string coverImagePath = GetImagePath(game.Title);

            if (!System.IO.File.Exists(coverImagePath))
            {

                HtmlNodeCollection imageNodes = doc.DocumentNode.SelectNodes("//img[@data-qa]");
                string imageFullURL = string.Empty;

                foreach (var node in imageNodes)
                {
                    if (node.OuterHtml.Contains("data-qa=\"gameBackgroundImage"))
                    {
                        string outerHtml = node.OuterHtml;
                        string[] separator = new string[] { "src=" };
                        var arrays = outerHtml.Split(new string[] { "src=" }, StringSplitOptions.None);
                        var imageUrlArray = arrays[1].Split(' ');
                        string imageUrl = imageUrlArray[0].Split('>')[0];

                        imageFullURL = imageUrl.Trim('\"');
                        break;
                    }

                }


                using (WebClient webclient = new WebClient())
                {
                    byte[] data = webclient.DownloadData(imageFullURL);
                    using (MemoryStream memStream = new MemoryStream(data))
                    {
                        using (var myImage = Image.FromStream(memStream))
                        {
                            myImage.Save(coverImagePath, ImageFormat.Png);
                            game.ImageSource = LoadImage(coverImagePath);
                        }
                    }
                }

            }

        }

        private void SavePriceHistory(GameItem game)
        {
            PriceHistroy ph = new PriceHistroy();
            ph.CheckDateTime = System.DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " "
                + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00");

            if (game.OriginalPrice == "")
            {
                ph.DiscountPrice = "";
                ph.OriginalPrice = game.FinalPrice;
            }
            else
            {
                ph.DiscountPrice = game.FinalPrice;
                ph.OriginalPrice = game.OriginalPrice;
            }
           

            ph.PSPlusPrice = game.PSPlusPrice;
            string gamehistorid = SaveLoadUtils.GetGameNameID(game.Title);
            string jsongamehistoryFilePath = Path.Combine(_dataDirectory, gamehistorid + "_hist.json");
            SaveLoadUtils.SaveGamePriceHistroyToJson(ph, jsongamehistoryFilePath);
        }


        private void ScrapePrices(GameItem game, HtmlDocument htmlDocument)
        {
            game.OriginalPrice = string.Empty;
            game.FinalPrice = string.Empty;
            game.PSPlusPrice = string.Empty;


            HtmlNodeCollection priceNodes = htmlDocument.DocumentNode.SelectNodes("//span[@data-qa]");
            foreach (var node in priceNodes)
            {
                if (node.OuterHtml.Contains("finalPrice"))
                {
                    game.FinalPrice = node.InnerText;
                    break;
                }
            }

            foreach (var node in priceNodes)
            {
                if (node.OuterHtml.Contains("originalPrice"))
                {
                    game.OriginalPrice = node.InnerText;
                    break;
                }
            }


            try
            {
                int dpidxsps = htmlDocument.Text.IndexOf("[\"ps-plus\"]");
                int dpidxs = htmlDocument.Text.IndexOf("discountedPrice", dpidxsps) + 18;
                int dpidxe = htmlDocument.Text.IndexOf("Ft", dpidxs) +  2;

                game.PSPlusPrice = htmlDocument.Text.Substring(dpidxs, dpidxe - dpidxs);
            }
            catch { };

        }

        private string GetImagePath(string strTitle)
        {
            string imgname = SaveLoadUtils.GetGameNameID(strTitle);

            StringBuilder sb = new StringBuilder();
            sb.Append(_dataDirectory);
            sb.Append("\\");
            sb.Append(imgname);
            sb.Append(".png");
            return sb.ToString();
        }



        private ImageSource LoadImage(string path)
        {
            var bitmapImage = new BitmapImage();

            if (System.IO.File.Exists(path))
            {

                using (var stream = new FileStream(path, FileMode.Open))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // optional
                }
            }

            return bitmapImage;
        }

    }

    enum MessageType
    {
        Error,
        Info
    }
}
