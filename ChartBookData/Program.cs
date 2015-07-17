using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.IO;

namespace ChartBookData
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\allChartBookData.txt";
            if (File.Exists(path))
                File.Delete(path);

            OleDbConnection connection =
                new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=E:\\AllData.xlsx;Extended Properties='Excel 12.0 xml;HDR=YES;'");
            connection.Open();
            DataTable Sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            foreach (DataRow dr in Sheets.Rows)
            {
                string sht = dr[2].ToString().Replace("'", "");
                if (sht.Equals("Countries$"))
                    continue;
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter("select * from [" + sht + "]", connection);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);
                DataTable d = ds.Tables[0];
                using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
                {
                    foreach (DataRow dataRow in d.Rows)
                    {
                        foreach (DataColumn dc in dataRow.Table.Columns)
                        {
                            if (dc.ColumnName == "F1")
                                continue;
                            sw.WriteLine(sht.Replace("$","") + ";" + dataRow[0].ToString()+ ";" + dc.ColumnName + ";" + dataRow[dc.ColumnName].ToString());
                        }
                    }
                }
            }
            
        }
    }
}
