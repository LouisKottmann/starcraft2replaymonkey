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
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace TheMonkeyWhoPlayedWithFire
{
    public class FileHandlingBaboon: IFileHandlingBaboon
    {
        public FileHandlingBaboon()
        {
            DeserializeAvailables();
            DeserializeComments();
            ReadVersionNumber();
            IoC.AddMonkey<IFileHandlingBaboon>(this);
        }

        /// <summary>
        /// This calls Sc2ParserApe to parse the replays.
        /// </summary>
        /// <param name="ReplayPaths">List of replays to parse.</param>
        public void HandleFiles(List<String> ReplayPaths)
        {
            if (AvailableReplays != null && AvailableReplays.Count > 0)
            {
                foreach (String availableReplay in AvailableReplays.Keys)
                {
                    if (ReplayPaths.Contains(availableReplay))
                    {
                        ReplayPaths.Remove(availableReplay);
                    }
                }
            }

            if (ReplayPaths != null && ReplayPaths.Count > 0)
            {
                //Getting the path of Sc2ParserApe
                String Sc2ParserApeExePath = System.IO.Directory.GetCurrentDirectory() + @"\TheApeWriter.exe";

                //Preparing the arguments for TheApeWriter 
                String arguments = String.Empty;
                foreach (String ReplayPath in ReplayPaths)
                {
                    arguments += "-rp:\"" + ReplayPath + "\" ";
                }

                //Reinitialize the log file.
                String logPath = Directory.GetCurrentDirectory() + "\\parseApeLog.txt";
                if (File.Exists(logPath))
                {
                    File.Delete(logPath);
                }
                m_Writer = File.CreateText(logPath);

                Process newProcess = new Process();

                // StartInfo contains the startup information of the new process
                newProcess.StartInfo.FileName = Sc2ParserApeExePath;
                newProcess.StartInfo.Arguments = arguments;

                // These two optional flags ensure that no DOS window appears
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true;

                // This ensures that you get the output from the DOS application
                newProcess.StartInfo.RedirectStandardOutput = true;
                newProcess.StartInfo.RedirectStandardError = true;
                newProcess.EnableRaisingEvents = true;
                newProcess.ErrorDataReceived += parserApe_DataRecieved;
                newProcess.OutputDataReceived += parserApe_DataRecieved;

                newProcess.Start();
                newProcess.BeginErrorReadLine();
                newProcess.BeginOutputReadLine();
                newProcess.WaitForExit();

                m_Writer.Close();
                m_Writer = null;

                String outputDir = Directory.GetCurrentDirectory() + "\\Replays parsed";
                foreach (String ReplayPath in ReplayPaths)
                {
                    String dataPath = outputDir;
                    dataPath += @"\";
                    dataPath += Path.GetFileNameWithoutExtension(ReplayPath);
                    dataPath += ".xml";
                    AddParsedReplayToAvailables(ReplayPath, dataPath);
                }
            }
        }

        public void SaveComment(String Comment, String ReplayPath)
        {
            CheckAdditionalInfoXMLExistence();

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlAdditionalInfoPath);
            XmlNode rootNode = doc.SelectSingleNode("Root");

            Boolean entryExists = false;
            foreach (XmlNode comment in rootNode.ChildNodes)
            {
                if (comment["ReplayPath"].InnerXml == ReplayPath)
                {
                    //The entry already exists, update it.
                    entryExists = true;
                    if (comment["Comment"] != null)
                    {
                        comment["Comment"].InnerXml = Comment;
                    }
                    else
                    {
                        XmlElement commentElement = doc.CreateElement("Comment");
                        comment.InnerXml = Comment;
                        comment.AppendChild(commentElement);
                    }
                    break;
                }
            }

            //The entry doesn't exist, create it from scratch.
            if (!entryExists)
            {
                XmlElement availableComment = doc.CreateElement("AvailableComment");
                XmlElement replayPathElement = doc.CreateElement("ReplayPath");
                XmlElement commentElement = doc.CreateElement("Comment");
                replayPathElement.InnerXml = ReplayPath;
                commentElement.InnerXml = Comment;
                availableComment.AppendChild(replayPathElement);
                availableComment.AppendChild(commentElement);
                rootNode.AppendChild(availableComment);
            }

            doc.Save(xmlAdditionalInfoPath);

            if (Comments.ContainsKey(ReplayPath))
            {
                Comments[ReplayPath] = Comment;
            }

            else 
            {
                Comments.Add(ReplayPath, Comment);
            }
        }

        private void CheckAdditionalInfoXMLExistence()
        {            
            if (!File.Exists(xmlAdditionalInfoPath))
            {
                XmlDocument doc = new XmlDocument();

                //XML version declaration
                XmlNode xmlnode = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                doc.AppendChild(xmlnode);

                //Adding a root element, cannot have empty XML.
                XmlElement rootElement = SerializeElement(doc, "Root", "");
                doc.AppendChild(rootElement);
                doc.Save(xmlAdditionalInfoPath);
            }
        }

        //Event handler that writes console's output to a logfile.
        private void parserApe_DataRecieved(Object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                m_Writer.WriteLine(e.Data);
            }
        }

        /// <summary>
        /// Adds a newly parsed replay to the list of available files to display in MainWindow.
        /// </summary>
        /// <param name="ReplayPath">The path of the *.sc2replay</param>
        /// <param name="DataPath">The path of the *.xml</param>
        public void AddParsedReplayToAvailables(String ReplayPath, String DataPath)
        {
            //Adding the newly parsed replays to the XML.
            if (File.Exists(ReplayPath) && File.Exists(DataPath))
            {                
                XmlDocument doc = new XmlDocument();

                if (!File.Exists(xmlPath))
                {
                    //XML version declaration
                    XmlNode xmlnode = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                    doc.AppendChild(xmlnode);
                    //Adding a root element, cannot have empty XML.
                    XmlElement rootElement = SerializeElement(doc, "Root", "");
                    doc.AppendChild(rootElement);

                    doc.Save(xmlPath);
                }

                doc.Load(xmlPath);
                XmlNode rootNode = doc.SelectSingleNode("Root");
                XmlElement availableReplayNode = doc.CreateElement("AvailableReplay");
                XmlElement replayPathNode = SerializeElement(doc, "ReplayPath", ReplayPath);
                XmlElement dataPathNode = SerializeElement(doc, "DataPath", DataPath);
                availableReplayNode.AppendChild(replayPathNode);
                availableReplayNode.AppendChild(dataPathNode);
                rootNode.AppendChild(availableReplayNode);
                doc.Save(xmlPath);

                //Adding the newly available replays to the dictionary.
                if (AvailableReplays == null)
                {
                    AvailableReplays = new Dictionary<String, String>();
                }
                if (!AvailableReplays.ContainsKey(ReplayPath))
                {
                    AvailableReplays.Add(ReplayPath, DataPath);
                }                
            }
        }

        public void ChangeParsedReplayPath(String ReplayXmlPath, String NewPath, String OldPath)
        {
            //Update the parsed replay's XML.
            XmlDocument doc = new XmlDocument();
            doc.Load(ReplayXmlPath);
            XmlNode gameVariables = doc.SelectSingleNode("Game_Variables");
            gameVariables.SelectSingleNode("ReplayPath").InnerXml = NewPath;
            doc.Save(ReplayXmlPath);

            //Update the comments' XML.
            CheckAdditionalInfoXMLExistence();
            doc.Load(xmlAdditionalInfoPath);
            XmlNode rootNode = doc.SelectSingleNode("Root");
            foreach (XmlNode comment in rootNode.ChildNodes)
            {
                if (comment["ReplayPath"].InnerXml == OldPath)
                {
                    comment["ReplayPath"].InnerXml = NewPath;
                    doc.Save(xmlAdditionalInfoPath);
                    break;
                }
            }            
        }

        public void RemoveReplayFromAvailables(String ReplayPath)
        {
            //Remove from available replays
            if (AvailableReplays.ContainsKey(ReplayPath))
            {
                //Remove from dictionary
                AvailableReplays.Remove(ReplayPath);

                //Remove from XML
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlNode rootNode = doc.SelectSingleNode("Root");
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    if (node["ReplayPath"].InnerXml == ReplayPath)
                    {
                        rootNode.RemoveChild(node);
                        doc.Save(xmlPath);
                        break;
                    }
                }              
            }

            //Remove from available comments
            if (Comments.ContainsKey(ReplayPath))
            {
                //Remove from dictionary
                Comments.Remove(ReplayPath);

                //Remove from XML
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlAdditionalInfoPath);
                XmlNode rootNode = doc.SelectSingleNode("Root");
                foreach (XmlNode comment in rootNode.ChildNodes)
                {
                    if (comment["ReplayPath"].InnerXml == ReplayPath)
                    {
                        rootNode.RemoveChild(comment);
                        doc.Save(xmlAdditionalInfoPath);
                        break;
                    }
                }
            }
        }

        private void ReadVersionNumber()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Directory.GetCurrentDirectory() + @"\CurrentVersion.xml");
            XmlNode rootNode = doc.SelectSingleNode("Root");
            VersionNumber = rootNode.SelectSingleNode("CurrentVersion").InnerXml;
            VersionNumber = VersionNumber[0] + "." + VersionNumber[1] + "." + VersionNumber[2];
        }

        private void DeserializeAvailables()
        {
            AvailableReplays = new Dictionary<String, String>();
            if (File.Exists(xmlPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlNode rootNode = doc.SelectSingleNode("Root");

                foreach (XmlNode availableReplay in rootNode.ChildNodes)
                {
                    String replayPath = availableReplay.SelectSingleNode("ReplayPath").InnerXml;
                    String dataPath = availableReplay.SelectSingleNode("DataPath").InnerXml;
                    if (!AvailableReplays.ContainsKey(replayPath))
                    {
                        AvailableReplays.Add(replayPath, dataPath);
                    }
                }
            }
        }

        private void DeserializeComments()
        {
            Comments = new Dictionary<String, String>();
            if (File.Exists(xmlAdditionalInfoPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlAdditionalInfoPath);
                XmlNode rootNote = doc.SelectSingleNode("Root");

                foreach (XmlNode comment in rootNote.ChildNodes)
                {
                    String replayPath = comment.SelectSingleNode("ReplayPath").InnerXml;
                    String commentText = comment.SelectSingleNode("Comment").InnerXml;
                    if (!Comments.ContainsKey(replayPath))
                    {
                        Comments.Add(replayPath, commentText);
                    }
                }
            }
        }

        /// <summary>
        /// Prepares an element for serialization, this is to prevent code duplication.
        /// </summary>
        /// <param name="doc">The target XmlDocument</param>
        /// <param name="nodeName">The name of the new node to create</param>
        /// <param name="nodeValue">The value for that node</param>
        /// <returns></returns>
        public static XmlElement SerializeElement(XmlDocument doc, String nodeName, String nodeValue)
        {
            XmlElement newElement = doc.CreateElement(nodeName);

            if (nodeValue == null)
            {
                nodeValue = String.Empty;
            }

            newElement.InnerXml = nodeValue;
            return newElement;
        }

        String xmlAdditionalInfoPath = Directory.GetCurrentDirectory() + @"\AdditionalInfos.xml";
        String xmlPath = Directory.GetCurrentDirectory() + @"\AvailableReplays.xml";
        public Dictionary<String, String> AvailableReplays { get; set; }
        public Dictionary<String, String> Comments { get; set; }
        public String VersionNumber { get; set; }
        StreamWriter m_Writer = null;
    }
}
