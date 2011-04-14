using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUpsideDownLemur;
using System.IO;
using System.Xml;
using TheMonkeyWhoPlayedWithFire;

namespace Sc2ReplayMonkey
{
    public class Config : IConfig
    {
        public Config()
        {
            IoC.AddMonkey<IConfig>(this);
            DeserializeConfig();
        }

        public void DeserializeConfig()
        {
            if (File.Exists(m_XmlPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(m_XmlPath);

                XmlNode rootNode = doc.SelectSingleNode("Root");
                RelocatePath = rootNode.SelectSingleNode("RelocatePath").InnerXml;
                AutoRelocate = Convert.ToBoolean(rootNode.SelectSingleNode("AutoRelocate").InnerXml);
            }
        }

        public void SerializeConfig()
        {
            XmlDocument doc = new XmlDocument();

            if(File.Exists(m_XmlPath))
            {
                File.Delete(m_XmlPath);
            }

            //XML version declaration
            XmlNode xmlnode = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            doc.AppendChild(xmlnode);

            //Adding a root element, cannot have empty XML.
            XmlElement rootElement = FileHandlingBaboon.SerializeElement(doc, "Root", "");
            XmlElement relocatePathNode = FileHandlingBaboon.SerializeElement(doc, "RelocatePath", RelocatePath);
            XmlElement autoRelocateNode = FileHandlingBaboon.SerializeElement(doc, "AutoRelocate", AutoRelocate.ToString());
            rootElement.AppendChild(relocatePathNode);
            rootElement.AppendChild(autoRelocateNode);

            doc.AppendChild(rootElement);

            doc.Save(m_XmlPath);
        }

        String m_XmlPath = Directory.GetCurrentDirectory() + @"\Config.xml";
        public String RelocatePath { get; set; }
        public Boolean AutoRelocate { get; set; }
    }
}
