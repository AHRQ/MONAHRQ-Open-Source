using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monahrq.Infrastructure.Generators
{
    /// <summary>
    /// The csv file generator.
    /// </summary>
    public class CSVGenerator
    {
        /// <summary>
        /// Export list into file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputList">The input list.</param>
        /// <param name="filePath">The file path.</param>
        public static void Create<T>(IEnumerable<T> inputList, string filePath)
        {
            var modelType = typeof(T);
            var map = CSVMap.GetMap(modelType);
            var orderedMap = map.OrderBy(m => m.Key).Select(m => m.Value);
            var headersList = orderedMap.Select(m => m.Name).ToList();
            var membersList = orderedMap.Select(m => m.MemberName).ToList();

            //Create file if it doesn't exist
            if (!File.Exists(filePath)) File.Create(filePath).Close();
            
            using (var stream = new StreamWriter(filePath))
            {
                //Write out CSV file header
                var header = string.Join(",", headersList);
                stream.WriteLine(header);

                //Write out all the data in the InputList collection
                foreach (var dataRow in inputList)
                {
                    //Prepare to collect values to be written out
                    var propValues = new List<Object>();
                    //Get CSVMap tags from dataRow
                    foreach (var csvMap in orderedMap)
                    {
                        //if output value requires a Transform delegate invoke it..
                        if (csvMap.Transform)
                        {
                            var CSVModel = (ICSVMap)dataRow;
                            propValues.Add(CSVModel.Transform(csvMap.MemberName));
                        }
                        else //..otherwise get the properties values
                        {
                            var propValue = csvMap.MemberInfo.GetValue(dataRow);
                            //Convert Boolean values to "1"(true) or "0"(false)
                            if (csvMap.MemberType == typeof(bool))
                                propValue = propValue.ToString() == "True" ? "1" : "0";

                            //..wrap quotes around it, if type is Alphanumeric
                            if (csvMap.Type == FieldType.Alphanumeric)
                                propValues.Add(string.Format("\"" + propValue) + "\"");
                            else
                                propValues.Add(propValue);
                        }
                    }

                    //Convert all values in a comma delimited string  and write this string out 
                    stream.WriteLine(string.Join(",", propValues));
                }
                stream.Close();
            }
        }
    }
}