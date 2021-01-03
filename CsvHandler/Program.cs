using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsvHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            var result1 = new List<Csv1>();
            using (var reader = new StreamReader(@"..\..\CsvFiles\Input1.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
            {
                csv.Configuration.Delimiter = ";";

                var data = new List<List<Csv1>>();

                while (csv.Read())
                {
                    if (csv.Context.RawRecord.Trim() == string.Empty)
                    {
                        data.Add(result1);
                        result1 = new List<Csv1>();
                        continue;
                    }
                    var record = csv.GetRecord<Csv1>();
                    result1.Add(record);
                }
                if (result1.Count > 0)
                {
                    data.Add(result1);
                }
            }

            var result2 = new List<Csv2>();
            using (var reader = new StreamReader(@"..\..\CsvFiles\Input2.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
            {
                csv.Configuration.Delimiter = ";";

                var data = new List<List<Csv2>>();

                while (csv.Read())
                {
                    if (csv.Context.RawRecord.Trim() == string.Empty)
                    {
                        data.Add(result2);
                        result2 = new List<Csv2>();
                        continue;
                    }
                    var record = csv.GetRecord<Csv2>();
                    result2.Add(record);
                }
                if (result2.Count > 0)
                {
                    data.Add(result2);
                }
            }

            int numInput;
            Console.WriteLine("Visa data för månad (1-12): ");
            var val = Console.ReadLine();
            numInput = Convert.ToInt32(val);

            var query = (from a in result1
                         group new
                         {
                             a.CustomerId,
                             a.Year,
                             a.Month,
                             a.Sales,
                             a.Quantity
                         } by a.CustomerId into y
                         join b in result2 on y.First().CustomerId equals b.CustomerId into g
                         select new
                         {
                             y.Key,
                             SalesThisYear = y.Sum(x => { if (x.Year.ToString() == "2019") return x.Sales; else return 0; }),
                             SalesLastYear = y.Sum(x => { if (x.Year.ToString() == "2018") return x.Sales; else return 0; }),
                             SalesPeriodThisYear = y.Sum(x => { if (x.Year.ToString() == "2019" && x.Month.Equals(numInput)) return x.Sales; else return 0; }),
                             SalesPeriodLastYear = y.Sum(x => { if (x.Year.ToString() == "2018" && x.Month.Equals(numInput)) return x.Sales; else return 0; }),
                             QuantityThisYear = y.Sum(x => { if (x.Year.ToString() == "2019") return x.Quantity; else return 0; }),
                             QuantityLastYear = y.Sum(x => { if (x.Year.ToString() == "2018") return x.Quantity; else return 0; }),
                             QuantityPeriodThisYear = y.Sum(x => { if (x.Year.ToString() == "2019" && x.Month.Equals(numInput)) return x.Quantity; else return 0; }),
                             QuantityPeriodLastYear = y.Sum(x => { if (x.Year.ToString() == "2018" && x.Month.Equals(numInput)) return x.Quantity; else return 0; }),
                             g.First().LastChanged,
                         }).OrderByDescending(x => x.SalesThisYear).ToList();

            void WriteCsv()
            {
                using (var writer = new StreamWriter(@"..\..\CsvFiles\newfile.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.Delimiter = "\t";
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField("CustomerId;SalesThisYear;SalesLastYear;SalesPeriodThisYear;SalesPeriodLastYear;QuantityThisYear;QuantityLastYear;QuantityPeriodThisYear;QuantityPeriodLastYear;LastChanged");
                    csv.NextRecord();
                    
                    foreach (var x in query)
                    {
                        csv.WriteField(x.Key + ";" + x.SalesThisYear + ";" + x.SalesLastYear + ";" + x.SalesPeriodThisYear + ";" + x.SalesPeriodLastYear + ";"
                            + x.QuantityThisYear + ";" + x.QuantityLastYear + ";" + x.QuantityPeriodThisYear + ";" + x.QuantityPeriodLastYear + ";" + x.LastChanged);
                        csv.NextRecord();
                    }
                }
            }

            if (numInput.ToString().Any())
            {
                WriteCsv();
                Console.WriteLine("Fil skapad! Tryck valfri knapp för att avsluta.");
                Console.ReadKey();
            }
        }

        public class Csv1
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int CustomerId { get; set; }
            public decimal Sales { get; set; }
            public int Quantity { get; set; }
        }

        public class Csv2
        {
            public int CustomerId { get; set; }
            public string LastChanged { get; set; }
        }
    }
}

