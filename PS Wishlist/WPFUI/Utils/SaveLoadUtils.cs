using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace WPFUI
{
   public class SaveLoadUtils
    {
       
        public static void SaveToJson(List<GameItem> gameItems, string fileName)
        {

            string json = JsonConvert.SerializeObject(gameItems, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(json);
            }

        }

        public static List<GameItem> LoadFromJson(string fileName)
        {
            string json;
            using (StreamReader sr = new StreamReader(fileName))
            {
                json = sr.ReadToEnd();
            }
            List<GameItem> loadedData = JsonConvert.DeserializeObject<List<GameItem>>(json);
            return loadedData;
        }


        public static void SaveGamePriceHistroyToJson(PriceHistroy priceHist, string fileName)
        {
            List<PriceHistroy> lph;

            if (System.IO.File.Exists(fileName))
            {
                lph = LoadPriceHistoryFromJson(fileName);
                lph.Add(priceHist);
            }
            else
            {
                lph = new List<PriceHistroy>();
                lph.Add(priceHist);
            }

            string json = JsonConvert.SerializeObject(lph, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(json);
            }

        }


        public static List<PriceHistroy> LoadPriceHistoryFromJson(string fileName)
        {
            string json;
            using (StreamReader sr = new StreamReader(fileName))
            {
                json = sr.ReadToEnd();
            }
            List<PriceHistroy> loadedData = JsonConvert.DeserializeObject<List<PriceHistroy>>(json);
            return loadedData;
        }


        public static string GetGameNameID(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                System.Random rnd = new System.Random();
                return rnd.Next().ToString();
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

    }
}
