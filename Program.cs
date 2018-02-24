using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace ConsoleApplication8
{
    public class Triangle
    {
        public string Product { get; set; }
        public int OriginYear { get; set; }
        public Dictionary<int, List<decimal>> Incremental = new Dictionary<int, List<decimal>>();
        public Dictionary<int, List<decimal>> Cumulative = new Dictionary<int, List<decimal>>();
    }
    class Program
    {
        static void Main(string[] args)
        {
            TextReader textReader = new StreamReader(@"Input.csv");
            CsvReader csvreader = new CsvReader(textReader);

            csvreader.Read(); // Read Header line
            List<Triangle> triangles = new List<Triangle>();
            int minimumOriginYear = int.MaxValue;
            int maximumDevelopmentYear = 0;
            while (csvreader.Read())
            {
                string product = string.Empty;
                int originYear = 0;
                int developmentYear = 0;
                decimal incrementalValue = 0;
                bool IsproductReadSuccessful = csvreader.TryGetField<string>(0, out product);
                bool IsoriginYearReadSuccessful = csvreader.TryGetField<int>(1, out originYear);
                bool IsdevelopmentYearReadSuccessful = csvreader.TryGetField<int>(2, out developmentYear);
                bool IsincrementalValueReadSuccessful = csvreader.TryGetField<decimal>(3, out incrementalValue);

                if(!IsproductReadSuccessful || !IsoriginYearReadSuccessful || !IsdevelopmentYearReadSuccessful || !IsincrementalValueReadSuccessful)
                {
                    continue;
                }

                if(originYear < minimumOriginYear)
                {
                    minimumOriginYear = originYear;
                }
                if(developmentYear > maximumDevelopmentYear)
                {
                    maximumDevelopmentYear = developmentYear;
                }


                Triangle triangle = triangles.Find(a => a.Product == product);
                if(triangle == null)
                {
                    Triangle newTriangle = new Triangle();
                    newTriangle.Product = product;
                    newTriangle.OriginYear = originYear;
                    newTriangle.Incremental = new Dictionary<int, List<decimal>>();
                    newTriangle.Cumulative = new Dictionary<int, List<decimal>>();
                    newTriangle.Incremental[originYear] = new List<decimal>();
                    newTriangle.Cumulative[originYear] = new List<decimal>();
                    int index = developmentYear - originYear;
                    if (index > 0)
                    {
                        // Insert 0 for the missing development year
                        int missingDevelopmentYearIndex = 0;
                        while (missingDevelopmentYearIndex < index)
                        {
                            newTriangle.Incremental[originYear].Insert(missingDevelopmentYearIndex, 0);
                            newTriangle.Cumulative[originYear].Insert(missingDevelopmentYearIndex, 0);
                            missingDevelopmentYearIndex++;
                        }
                    }
                    newTriangle.Incremental[originYear].Insert(index, incrementalValue);
                    newTriangle.Cumulative[originYear].Insert(index, incrementalValue);

                    triangles.Add(newTriangle);
                }
                else
                {

                    if(!triangle.Incremental.ContainsKey(originYear))
                    {
                        triangle.Incremental.Add(originYear, new List<decimal>());
                        triangle.Cumulative.Add(originYear, new List<decimal>());
                    }
                    List<decimal> incremental = triangle.Incremental[originYear];
                    List<decimal> cumulative = triangle.Cumulative[originYear];

                    int index = developmentYear - originYear;
                    int count = incremental.Count;
                    if(index > count)
                    {
                        // Insert 0 for the missing development year
                        while (count < index)
                        {
                            incremental.Insert(count, 0);
                            cumulative.Insert(count, cumulative[count - 1]); // Cumulative value will be the last value as no claim this development year
                            count++;
                        }
                    }
                    incremental.Insert(index, incrementalValue);
                    if (index > 0)
                    {
                        cumulative.Insert(index, incrementalValue + cumulative[index - 1]);
                    }
                    else
                    {
                        cumulative.Insert(index, incrementalValue); // This is first development year
                    }
                }
            }

            TextWriter csvWriter = new StreamWriter(@"Output.csv");

            string line = string.Empty;

            line = line + minimumOriginYear + ",";
            line = line + (maximumDevelopmentYear - minimumOriginYear + 1).ToString() + ",";
            csvWriter.WriteLine(line);

            // Find the maximum triange length
            int maximumTriangleLength = 0;
            foreach(Triangle triangle in triangles)
            {
                int subTriangleLength = 0;
                foreach (KeyValuePair<int, List<decimal>> subTriangle in triangle.Incremental)
                {
                    subTriangleLength = subTriangleLength + subTriangle.Value.Count;
                }
                if(subTriangleLength > maximumTriangleLength)
                {
                    maximumTriangleLength = subTriangleLength;
                }
            }


            foreach (Triangle triangle in triangles)
            {
                line = string.Empty;
                line = line + triangle.Product + ",";

                // Pre-fix 0 for the missing origin year
                int subTriangleLength = 0;
                foreach (KeyValuePair<int, List<decimal>> keyValuePair in triangle.Cumulative)
                {
                    subTriangleLength = subTriangleLength + keyValuePair.Value.Count;
                }

                int numberOfZerosToAppend = maximumTriangleLength - subTriangleLength;
                for (int index = 1; index <= numberOfZerosToAppend; index = index + 1)
                {
                    line = line + "0" + ",";
                }

                foreach (KeyValuePair<int, List<decimal>> keyValuePair in triangle.Cumulative)
                {
                    foreach(decimal cumulativeValue in keyValuePair.Value)
                    {
                        line = line + cumulativeValue + ",";
                    }
                }
                csvWriter.WriteLine(line);
            }
            csvWriter.Flush();
        }
    }
}
