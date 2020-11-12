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
    public class PriceHistoryViewModel
    {
        public List<PriceHistroy> Prices { get; set; }


        public PriceHistoryViewModel(string gamepricehistoryid)
        {

            string cwd = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataDirectory = Path.Combine(cwd, "PSWishlist");
            Directory.CreateDirectory(dataDirectory);

            string jsonFilePath = Path.Combine(dataDirectory, gamepricehistoryid + "_hist.json");

            Prices = new List<PriceHistroy>();
            try
            {
                Prices = SaveLoadUtils.LoadPriceHistoryFromJson(jsonFilePath);
            }
            catch { }

        }


    }


}
