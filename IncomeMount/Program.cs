using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace IncomeMount
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\mountain.txt";
            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                using (StreamReader file = File.OpenText(@"C:\Users\shahnewaz\Documents\GapMinder\mountain.json"))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);
                        foreach (var x in o2)
                        {
                            string indicator = x.Key;
                            JToken indicatorValue = x.Value;
                            foreach (var y in indicatorValue)
                            {
                                var property = y as JProperty;
                                string period = property.Name;
                                JToken shape = property.Value;
                                sw.WriteLine(indicator + ";" + period + ";" + shape.ToString().Replace("\r\n","").Replace("  ",""));
                            }
                        }
                    }
                }
            }
        }
    }
}
