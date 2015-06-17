using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Shell32;
using System.IO;
using System.Globalization;

namespace HandleWDI
{
    static class Program
    {
        public static string WDIurl = "http://databank.worldbank.org/data/download/WDI_csv.zip";

        public static string SubNationalurl = "http://databank.worldbank.org/data/download/Subnational-Poverty_csv.zip";

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

        public static Shell32.Folder GetShell32NameSpaceFolder(Object folder)
        {
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");

            Object shell = Activator.CreateInstance(shellAppType);
            return (Shell32.Folder)shellAppType.InvokeMember("NameSpace",
            System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folder });
        }

        public static bool unzipWDIFiles(string zipFile, string destinationFolder)
        {
            try
            {
                //Shell32.Folder SrcFlder = GetShell32NameSpaceFolder(from);
                //Shell32.Folder DestFlder = GetShell32NameSpaceFolder(to);
                //FolderItems items = SrcFlder.Items();

                //Shell shell = new Shell();
                Shell32.Folder archive = GetShell32NameSpaceFolder(zipFile);
                Shell32.Folder extractFolder = GetShell32NameSpaceFolder(destinationFolder);

                // Copy each item one-by-one
                foreach (Shell32.FolderItem f in archive.Items())
                {
                    extractFolder.CopyHere(f, 20);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void downloadWDIData()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(WDIurl, @"C:\Users\shahnewaz\Documents\GapMinder\WDI Data\WDI.zip");
            }

        }
    
        public static void processWDIDataFile()
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\allWDIRawData.txt";
            string filePath = @"C:\Users\shahnewaz\Documents\GapMinder\WDI Data\Data\WDI_Data.csv";
            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(filePath);
                string[] headerTokens = getTokens(file.ReadLine());
                string line = null;

                while ((line = file.ReadLine()) != null)
                {
                    string[] tokens = getTokens(line);
                    for (int i = 4; i < tokens.Length; i++)
                    {
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
                        string str = tokens[i].Replace(",", "").Replace("\"", "")
                            .Replace("(", "").Replace(")", "").Trim();

                        double value = 0.00;
                        bool b = Double.TryParse(str
                            , System.Globalization.NumberStyles.Float, culture, out value);
                        if (!b)
                            value = 0.00;

                        sw.WriteLine(tokens[0].Replace(',', ';') + ","
                                    + tokens[1].Replace(',', ';') + ","
                                    + tokens[2].Replace(',', ';') + ","
                                    + tokens[3].Replace(',', ';') + ","
                                    + headerTokens[i] + ","
                                    + value);
                    }
                }
                file.Close();
                file.Dispose();
                //break;
            }
        }

        public static void downloadSubNationalData()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(SubNationalurl, @"C:\Users\shahnewaz\Documents\GapMinder\SubNational\poverty.zip");
            }
        }

        public static void processSubNationalData()
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\allSubnationalData.txt";
            string filePath = @"C:\Users\shahnewaz\Documents\GapMinder\SubNational\poverty\Subnational-Poverty_Data.csv";
            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(filePath);
                string[] headerTokens = getTokens(file.ReadLine());
                string line = null;

                while ((line = file.ReadLine()) != null)
                {
                    string[] tokens = getTokens(line);
                    for (int i = 4; i < tokens.Length; i++)
                    {
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
                        string str = tokens[i].Replace(",", "").Replace("\"", "")
                            .Replace("(", "").Replace(")", "").Trim();

                        double value = 0.00;
                        bool b = Double.TryParse(str
                            , System.Globalization.NumberStyles.Float, culture, out value);
                        if (!b)
                            value = 0.00;

                        sw.WriteLine(tokens[0].Replace(',', ';') + ","
                                    + tokens[1].Replace(',', ';') + ","
                                    + tokens[2].Replace(',', ';') + ","
                                    + tokens[3].Replace(',', ';') + ","
                                    + headerTokens[i] + ","
                                    + value);
                    }
                }
                file.Close();
                file.Dispose();
                //break;
            }
        }

        static void Main(string[] args)
        {
            //downloadWDIData();
            //unzipWDIFiles(@"C:\Users\shahnewaz\Documents\GapMinder\WDI Data\WDI.zip", @"C:\Users\shahnewaz\Documents\GapMinder\WDI Data\Data");
            //processWDIDataFile();
            //downloadSubNationalData();
            unzipWDIFiles(@"C:\Users\shahnewaz\Documents\GapMinder\SubNational\poverty.zip", @"C:\Users\shahnewaz\Documents\GapMinder\SubNational\poverty");
            processSubNationalData();
        }
    }
}
