/*This file is part of Sc2ReplayMonkey
    Sc2ReplayMonkey is a starcraft II replay analyzer tool built upon SC2PArserApe  
 
    Copyright (C) 2011  Louis Kottmann louis.kottmann@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
            AutoRelocate = false;
            AutoRename = false;
            FullDelete = false;
            DoNotShowSc2NotFoundError = false;
            RelocatePath = String.Empty;
            RenameFormat = String.Empty;
            ChosenRenamingType = RenamingType.None;
            DeserializeConfig();

            IoC.AddMonkey<IConfig>(this);
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
                FullDelete = Convert.ToBoolean(rootNode.SelectSingleNode("FullDelete").InnerXml);
                if (rootNode["DoNotShowSc2NotFoundError"] != null)
                {
                    DoNotShowSc2NotFoundError = Convert.ToBoolean(rootNode.SelectSingleNode("DoNotShowSc2NotFoundError").InnerXml);
                }
                if (rootNode["AutoRename"] != null)
                {
                    AutoRename = Convert.ToBoolean(rootNode.SelectSingleNode("AutoRename").InnerXml);
                }
                if (rootNode["RenameFormat"] != null)
                {
                    RenameFormat = rootNode.SelectSingleNode("RenameFormat").InnerXml;
                }
                if (rootNode["ChosenRenamingType"] != null)
                {
                    switch (rootNode.SelectSingleNode("ChosenRenamingType").InnerXml)
                    {
                        case "Personalized":
                            ChosenRenamingType = RenamingType.Personalized;
                            break;
                        case "Formatted1":
                            ChosenRenamingType = RenamingType.Formatted1;
                            break;
                        case "Formatted2":
                            ChosenRenamingType = RenamingType.Formatted2;
                            break;
                        case "Formatted3":
                            ChosenRenamingType = RenamingType.Formatted3;
                            break;
                        case "Custom":
                            ChosenRenamingType = RenamingType.Custom;
                            break;
                        default:
                            ChosenRenamingType = RenamingType.None;
                            break;
                    }
                }
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
            XmlElement fullDeleteNode = FileHandlingBaboon.SerializeElement(doc, "FullDelete", FullDelete.ToString());
            XmlElement doNotShowSc2NotFoundErrorNode = FileHandlingBaboon.SerializeElement(doc, "DoNotShowSc2NotFoundError", DoNotShowSc2NotFoundError.ToString());
            XmlElement renameFormatNode = FileHandlingBaboon.SerializeElement(doc, "RenameFormat", RenameFormat);
            XmlElement autoRenameNode = FileHandlingBaboon.SerializeElement(doc, "AutoRename", AutoRename.ToString());
            XmlElement chosenRenamingTypeNode = FileHandlingBaboon.SerializeElement(doc, "ChosenRenamingType", ChosenRenamingType.ToString());

            rootElement.AppendChild(autoRenameNode);
            rootElement.AppendChild(chosenRenamingTypeNode);
            rootElement.AppendChild(renameFormatNode);
            rootElement.AppendChild(doNotShowSc2NotFoundErrorNode);
            rootElement.AppendChild(fullDeleteNode);
            rootElement.AppendChild(relocatePathNode);
            rootElement.AppendChild(autoRelocateNode);

            doc.AppendChild(rootElement);

            doc.Save(m_XmlPath);
        }

        private String m_XmlPath = Directory.GetCurrentDirectory() + @"\Config.xml";
        public String RelocatePath { get; set; }
        public Boolean AutoRelocate { get; set; }
        public Boolean FullDelete { get; set; }
        public Boolean DoNotShowSc2NotFoundError { get; set; }
        public String RenameFormat { get; set; }
        public Boolean AutoRename { get; set; }
        public RenamingType ChosenRenamingType { get; set; }        
    }
}
