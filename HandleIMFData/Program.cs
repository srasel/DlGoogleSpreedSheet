using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace HandleIMFData
{
    static class Program
    {
        static string[] SplitQuoted(this string input, char separator, char quotechar)
        {
            List<string> tokens = new List<string>();

            StringBuilder sb = new StringBuilder();
            bool escaped = false;
            foreach (char c in input)
            {
                if (c.Equals(separator) && !escaped)
                {
                    // we have a token
                    tokens.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else if (c.Equals(separator) && escaped)
                {
                    // ignore but add to string
                    sb.Append(c);
                }
                else if (c.Equals(quotechar))
                {
                    escaped = !escaped;
                    sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            tokens.Add(sb.ToString().Trim());

            return tokens.ToArray();
        }

        static string[] getTokens(string line)
        {
            string[] tokens;
            //if (line[0] == '\"')
            tokens = SplitQuoted(line, ',', '\"');
            //else
            //tokens = line.Split(',');
            return tokens;
        }

        public static WebClient wc;
        static void processGDPPerCap(string countryCode)
        {
            try
            {
                string url = @"https://www.quandl.com/api/v1/datasets/ODA/{0}_NGDPPC.csv?auth_token=CzsJ9zvxo3QGrGSucTxn&exclude_headers=true";
                url = String.Format(url, countryCode);
                string path = @"C:\Users\shahnewaz\Documents\GapMinder\IMF Data\allIMFData.txt";

                //if (File.Exists(path))
                //    File.Delete(path);
                var outputCSVdata = wc.DownloadString(url);
                using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
                {
                    outputCSVdata =  outputCSVdata.Replace("\n", "," + countryCode + "," + "gdp_per_cap" + "\n");
                    sw.Write(outputCSVdata);
                }
            }
            catch (Exception)
            {

            }
        }

        static void processPopulatonData(string countryCode)
        {
            try
            {
                string url = @"https://www.quandl.com/api/v1/datasets/ODA/{0}_LP.csv?auth_token=CzsJ9zvxo3QGrGSucTxn&exclude_headers=true";
                url = String.Format(url, countryCode);
                string path = @"C:\Users\shahnewaz\Documents\GapMinder\IMF Data\allIMFData.txt";

                //if (File.Exists(path))
                //    File.Delete(path);
                var outputCSVdata = wc.DownloadString(url);
                using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
                {
                    outputCSVdata = outputCSVdata.Replace("\n", "," + countryCode + "," + "pop" + "\n");
                    sw.Write(outputCSVdata);
                }
            }
            catch (Exception)
            {

            }
        }

        static void processIMFData()
        {
            System.IO.StreamReader file =
              new System.IO.StreamReader(@"C:\Users\shahnewaz\Documents\GapMinder\CountryCodeList.txt");
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] arr = line.Split('\t');
                processGDPPerCap(arr[0].ToString());
                processPopulatonData(arr[0].ToString());
            }

            file.Close();
        }

        static void Main(string[] args)
        {
            wc = new WebClient();
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
            wc.Headers.Add("DNT", "1");
            wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            wc.Headers.Add("Accept-Encoding", "deflate");
            wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");

            string path = @"C:\Users\shahnewaz\Documents\GapMinder\IMF Data\allIMFData.txt";
            if (File.Exists(path))
                File.Delete(path);

            processIMFData();
        }
    }
}
