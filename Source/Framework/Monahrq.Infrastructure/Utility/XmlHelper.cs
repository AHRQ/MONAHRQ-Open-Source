using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Infrastructure.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class XmlHelper
    {
        //public static bool IsValidXml(string xmlFilePath, string xsdFilePath)
        //{
        //    var xdoc = XDocument.Load(xmlFilePath);
            
        //    var schemas = new XmlSchemaSet();
        //    schemas.Add("", xsdFilePath);

        //    Boolean result = true;
        //    xdoc.Validate(schemas, (sender, e) =>
        //    {
        //        result = false;
        //    });

        //    return result;
        //}

        public static bool IsValidXml(string xmlFilePath, string xsdFileRelFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlFilePath) || string.IsNullOrWhiteSpace(xmlFilePath) || string.IsNullOrEmpty(xsdFileRelFilePath))
                    return true;

                var xdoc = XDocument.Load(xmlFilePath);
                var schema = LoadSchemaFromFile(xsdFileRelFilePath);

                var schemas = new XmlSchemaSet();
                schemas.Add(schema);

                var result = true;
                xdoc.Validate(schemas, (sender, e) =>
                {
                    result = false;
                });

                return result;
            }
            catch (Exception exc)
            {
                ILogWriter logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates the ns MGR.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        public static XmlNamespaceManager CreateNsMgr(XmlDocument doc)
        {
            if (doc == null)
                return null;

            var rootNode = doc.SelectSingleNode("/*");

            if (rootNode == null)
                return null;

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            foreach (XmlAttribute attr in rootNode.Attributes)
            {
                if (attr.Prefix == "xmlns")
                {
                    nsmgr.AddNamespace(attr.LocalName, attr.Value);
                }
            }
                
            return nsmgr;
        }



        /// <summary>
        /// Generic method for deserializing an object from a XmlDocument
        /// </summary>
        /// <param name="t">Type to deserialize</param>
        /// <param name="source">XmlDocument containing the Xml to deserialize.</param>
        /// <returns></returns>
        public static object Deserialize(Type t, XmlReader source)
        {
            using (source)
            {
                XmlSerializer serializer = GetSerializerForType(t);
                object result = serializer.Deserialize(source);
                source.Close();

                return result;
            }
        }

        private static readonly Dictionary<Type, XmlSerializer> _serializerDictionary = new Dictionary<Type, XmlSerializer>();

        /// <summary>
        /// Gets the serializer for a type, caching it for future uses and performance
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static XmlSerializer GetSerializerForType(Type t)
        {
            XmlSerializer xsr;
            lock (_serializerDictionary)
            {
                if (!_serializerDictionary.TryGetValue(t, out xsr))
                {
                    xsr = new XmlSerializer(t);
                    _serializerDictionary.Add(t, xsr);
                }
            }

            return xsr;
        }

        /// <summary>
        /// Generic method for deserializing an object from a XmlDocument
        /// </summary>
        /// <param name="t">Type to deserialize</param>
        /// <param name="source">XmlDocument containing the Xml to deserialize.</param>
        /// <returns></returns>
        public static object Deserialize(Type t, XmlDocument source)
        {
            if (source == null)
                return null;

            using (XmlNodeReader nr = new XmlNodeReader(source))
            {
                return Deserialize(t, nr);
            }
        }

        /// <summary>
        /// Generic method for deserializing an object from a Xml file and Xsd file 
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="xmlPath">Path to the Xml document containing the Xml to deserialize</param>
        /// <param name="schemaPath">Optional Xsd schema to use to validate Xml</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlPath, string schemaPath)
        {
            return (T)Deserialize(typeof(T), xmlPath, schemaPath);
        }

        /// <summary>
        /// Generic method for deserializing an object from a Xml file 
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="xmlPath">Path to the Xml document containing the Xml to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlPath)
        {
            return (T)Deserialize(typeof(T), xmlPath, null);
        }

        /// <summary>
        /// Generic method for deserializing an object from a XmlDocument
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="source">XmlDocument containing the Xml to deserialize.</param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlDocument source)
        {
            return (T)Deserialize(typeof(T), source);
        }

        /// <summary>
        /// Generic method for deserializing an object from a XmlDocument
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="source">XmlDocument containing the Xml to deserialize.</param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlReader source)
        {
            return (T)Deserialize(typeof(T), source);
        }


        /// <summary>
        /// Generic method for deserializing an object from a XmlElement
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="source">XmlElement containing the Xml to deserialize.</param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlElement source)
        {
            using (XmlNodeReader nr = new XmlNodeReader(source))
            {
                return (T) Deserialize(typeof (T), nr);
            }
        }

        /// <summary>
        /// Generic method for deserializing an object from a XmlDocument
        /// </summary>
        /// <param name="t">Type to deserialize</param>
        /// <param name="xmlPath">Path to the Xml document containing the Xml to deserialize</param>
        /// <param name="schemaPath">Optional Xsd schema to use to validate Xml</param>
        /// <returns></returns>
        public static object Deserialize(Type t, string xmlPath, string schemaPath)
        {
            XmlDocument source = new XmlDocument();
            using (XmlReader xr = CreateReader(xmlPath, schemaPath))
            {
                source.Load(xr);
                xr.Close();

                //Deserialize from the xmlreader
                object result = Deserialize(t, source);
                return result;
            }
        }

        /// <summary>
        /// Creates the reader.
        /// </summary>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="schemaPath">The schema path.</param>
        /// <returns></returns>
        public static XmlReader CreateReader(string xmlPath, string schemaPath)
        {
            XmlReader xr;

            if (schemaPath != null)
            {
                //Create a new XmlReader to read the Schema file defined by the schemapath
                XmlReaderSettings xrs = new XmlReaderSettings();

                //Read the schema into a XmlSchema variable - throw any errors to callback method
                XmlSchema xs = LoadSchemaFromFile(schemaPath);

                //Throw any voilations when loading the xmlfile and verifying it conforms to the schema to the callback method
                xrs.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                //Validate the xml file conforms to the schema
                xrs.ValidationType = ValidationType.Schema;

                //Add the schema we loaded from the file above to the schemacollection for the reader we're about to create
                xrs.Schemas.Add(xs);

                //Create a reader for the xmlfile specified - add the schema we loaded to validate it.
                xr = XmlReader.Create(File.OpenRead(xmlPath), xrs);
            }
            else
                //Create a reader for the xmlfile specified with no schema
                xr = XmlReader.Create(File.OpenRead(xmlPath));

            return xr;
        }

        /// <summary>
        /// Loads the schema from file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static XmlSchema LoadSchemaFromFile(string path)
        {
            using (XmlReader xr = XmlReader.Create(File.OpenRead(path)))
            {
                //Read the schema into a XmlSchema variable - throw any errors to callback method
                XmlSchema xs = XmlSchema.Read(xr, ValidationCallBack);

                return xs;
            }
        }

        /// <summary>
        /// Convert a XmlSchema into a byte array
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static string GetStringFromSchema(XmlSchema schema)
        {
            //Get the bytes from the schema
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                schema.Write(sw);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Convert a XmlSchema into a byte array
        /// </summary>
        /// <returns></returns>
        public static XmlSchema GetSchemaFromString(string source)
        {
            //Get the bytes from the schema
            using (var sr = new StringReader(source))
            {
                XmlSchema result = XmlSchema.Read(sr, ValidationCallBack);

                return result;
            }
        }


        /// <summary>
        /// This method will take an object and serialize it into xml and write it to the specified file.
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="xmlPath">The path in the filesystem to write a new xml file containing serialized xml from the object.</param>
        public static void Serialize(object source, string xmlPath)
        {   
            using (XmlWriter xw = XmlWriter.Create(xmlPath))
            {
                XmlDocument doc = Serialize(source);
                doc.Save(xw);
                xw.Flush();
                xw.Close();
            }
        }

        /// <summary>
        /// Deserializers the async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="xmlPath">The XML path.</param>
        public static T DeserializerAsync<T>(string xmlPath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (XmlReader xr = new XmlTextReader(xmlPath))
            {
                //Deserialize from the xmlreader
                T result = (T)xs.Deserialize(xr);
                return result;
            }
        }

        /// <summary>
        /// This method will take an object and serialize it in memory into a XmlDocument.
        /// </summary>
        /// <param name="source">The object to serialize into Xml.</param>
        /// <param name="removeNamespaces">if set to <c>true</c> [remove namespaces].</param>
        /// <returns>
        /// A XmlDocument containing the serialized Xml from the specified object.
        /// </returns>
        public static XmlDocument Serialize(object source, bool removeNamespaces = false)
        {
            var result = source == null ? default(XmlDocument) : Serialize(source, source.GetType(), removeNamespaces);

            //if (removeNamespaces)
            //{
            //    RemoveXmlns(result);
            //}

            return result;
        }

        /// <summary>
        /// This method will take an object and serialize it in memory into a XmlDocument.
        /// </summary>
        /// <param name="source">The object to serialize into Xml.</param>
        /// <param name="t">The t.</param>
        /// <param name="removeNamespaces">if set to <c>true</c> [remove namespaces].</param>
        /// <returns>
        /// A XmlDocument containing the serialized Xml from the specified object.
        /// </returns>
        public static XmlDocument Serialize(object source, Type t,  bool removeNamespaces = false)
        {
            if (source == null)
                return null;

            //Initialize the XmlDocument we're going to load the serialized object into
            XmlDocument result = new XmlDocument();

            //Initialize the serializer for the object's type
            XmlSerializer serializer = GetSerializerForType(t);

            

            //We need a place for the XmlWriter to write the serialized Xml - use a memorystream because we're going to do this 
            //in memory and return a xmldocument
            using (var memstream = new MemoryStream())
            {

                //Create a XmlWriter and point it at the stringbuilder for the location to write the serialized Xml
                using (XmlWriter xw = XmlWriter.Create(memstream))
                {

                    //Serialize the object into Xml and write the Xml to the in memory stringbuilder
                    if (removeNamespaces)
                    {
                        //Create our own namespaces for the output
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                        //Add an empty namespace and empty value
                        ns.Add("", "");

                        serializer.Serialize(xw, source, ns);
                    }
                    else
                        serializer.Serialize(xw, source);

                    memstream.Position = 0;

                    //Load the Xml from memory to the XmlDocument and return
                    result.Load(memstream);
                    return result;
                }
            }
        }

        /// <summary>
        /// Callback for Xml/Xsd validation errors - this should travel with the generic deserialization function above.
        /// </summary>
        /// <param name="sender">Object which caused the validation event</param>
        /// <param name="args"></param>
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                throw new ApplicationException(
                    string.Format("\tWarning: Matching schema not found. No validation occurred.{0}", args.Message));
            
            throw new ApplicationException(string.Format("\tValidation error: {0}", args.Message));
        }

        public static XmlDocument RemoveXmlns(XmlDocument doc)
        {
            if (doc == null) return doc;

            XDocument d;
            using (var nodeReader = new XmlNodeReader(doc))
                d = XDocument.Load(nodeReader);

            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            using (var xmlReader = d.CreateReader())
                xmlDocument.Load(xmlReader);

            return xmlDocument;
        }

        public static XmlDocument RemoveXmlns(string xml)
        {
            var d = XDocument.Parse(xml);

            if (d.Root == null) return null;

            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(d.CreateReader());

            return xmlDocument;
        }
    }
}