using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace ReconSCHARPClient.Helpers
{
    public class Serializer
    {
        public T Deserialize<T>(XDocument doc, string rootAttribute) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T), new XmlRootAttribute(rootAttribute));

            XmlReader reader = doc.CreateReader();
            //StreamReader file = new StreamReader(filePath);

            T result = (T)ser.Deserialize(reader);
            reader.Close();

            //T result = (T)ser.Deserialize(file);
            //file.Close();

            return result;         
        }

        public string Serialize<T>(T ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
    }
}
