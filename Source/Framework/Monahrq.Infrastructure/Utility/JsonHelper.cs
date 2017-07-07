using System.IO;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Monahrq.Infrastructure.Utility
{
    public static class JsonHelper
    {


        public static void GenerateJsonFile(object item, string fileName, string fileheader = null, bool unquotedPropertyNames = false)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                StringBuilder output = new StringBuilder();
                if (!string.IsNullOrEmpty(fileheader))
                    output.Append(fileheader);

                if (!unquotedPropertyNames)
                    output.Append(JsonConvert.SerializeObject(item, Formatting.None));
                else
                {
                    var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Include };
                    using (var stringWriter = new StringWriter())
                    {
                        using (var writer = new JsonTextWriter(stringWriter))
                        {
                            writer.QuoteName = false;
                            serializer.Serialize(writer, item);
                        }

                        output.Append(stringWriter);
                    }
                }
                output.Append(";");
                File.WriteAllText(fileName, output.ToString());
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void GenerateWingJsonFile(object item, string fileName, string fileheader = null, string fileHeaderMapping = null)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            StringBuilder output = new StringBuilder();

            if (!string.IsNullOrEmpty(fileHeaderMapping)) // First Add fileheader namespace mapping if available.
                output.AppendLine(fileHeaderMapping);

            if (!string.IsNullOrEmpty(fileheader)) // Add fileheader if available 
                output.Append(fileheader + "{ ");

            output.Append("\"Data\":");

            output.Append(JsonConvert.SerializeObject(item, Formatting.None)); // Serialize item(s) to json

            output.Append(" };");
            File.WriteAllText(fileName, output.ToString());
        }

        public static T Deserialize<T>(string jsonString) where T : class, new()
        {
            if (string.IsNullOrEmpty(jsonString)) return default(T);

            return JsonConvert.DeserializeObject<T>(jsonString, new JsonSerializerSettings());
        }

        public static string Serialize<T>(T objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
        }

        public static string Serialize<T>(T objectToSerialize, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(objectToSerialize, Formatting.None, settings);
        }

    }
}
