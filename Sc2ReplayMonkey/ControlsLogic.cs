﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUpsideDownLemur;
using TheMonkeyWhoPlayedWithFire;
using System.Windows.Controls;
using SC2ParserApe;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;

namespace Sc2ReplayMonkey
{
    public class ControlsLogic
    {
        public ControlsLogic(MainWindow main)
        {
            InitSc2ToWPFColorsDict();
            InitSc2MapsLink();
            InitSc2Workers();

            m_Main = main;
            m_IMonkeyDeserializer = IoC.FindMonkey<IMonkeyDeserializer>();
            m_IFileHandlingBaboon = IoC.FindMonkey<IFileHandlingBaboon>();
            m_Config = IoC.FindMonkey<IConfig>();

            if (m_IMonkeyDeserializer != null)
            {
                m_CurrentData = m_IMonkeyDeserializer.CurrentReplayData;
            }
        }

        #region Public methods
        public void ClearInfos()
        {
            if (m_Main != null)
            {
                m_Main.labelPlayer1Team1.Content = String.Empty;
                m_Main.labelPlayer2Team1.Content = String.Empty;
                m_Main.labelPlayer1Team2.Content = String.Empty;
                m_Main.labelPlayer2Team2.Content = String.Empty;
                m_Main.LabelRealmValue.Content = String.Empty;
                m_Main.labelRealTeamSizeValue.Content = String.Empty;
                m_Main.labelGameSpeedValue.Content = String.Empty;
                m_Main.labelVersionValue.Content = String.Empty;
                m_Main.labelRecorderValue.Content = String.Empty;
                m_Main.labelGameTypeValue.Content = String.Empty;
                m_Main.imageWorkerP1T1.Source = null;
                m_Main.imageWorkerP2T1.Source = null;
                m_Main.imageWorkerP1T2.Source = null;
                m_Main.imageWorkerP2T2.Source = null;
                m_Main.randomImageP1T1.Source = null;
                m_Main.randomImageP2T1.Source = null;
                m_Main.randomImageP1T2.Source = null;
                m_Main.randomImageP2T2.Source = null;
                m_Main.labelWinner.Content = "?";
                m_Main.labelWinner.Foreground = Brushes.White;
            }
        }

        public void ShowOptions()
        {
            OptionsWindow optionsWindow = new OptionsWindow(m_Main);
            optionsWindow.Owner = m_Main;
            optionsWindow.ShowDialog();
        }

        public void RelocateReplay()
        {
            if (!String.IsNullOrEmpty(m_CurrentData.ReplayPath))
            {
                String newPath = String.Empty;
                Boolean relocate = false;
                if (!m_Config.AutoRelocate)
                {
                    SaveFileDialog sfDialog = new SaveFileDialog();
                    sfDialog.DefaultExt = ".sc2replay";
                    sfDialog.Filter = "Starcraft II Replay|*.sc2replay";
                    sfDialog.InitialDirectory = m_Config.RelocatePath;
                    sfDialog.Title = "Chose the path you want to relocate the current replay to";
                    sfDialog.FileName = m_Config.RelocatePath + "\\" + Path.GetFileName(m_CurrentData.ReplayPath);
                    Nullable<bool> sfResult = sfDialog.ShowDialog(m_Main);

                    if (sfResult == true)
                    {
                        newPath = sfDialog.FileName;
                        relocate = true;
                    }
                }
                else
                {
                    newPath = m_Config.RelocatePath + "\\" + Path.GetFileName(m_CurrentData.ReplayPath);
                    relocate = true;
                }

                if (relocate)
                {
                    String dataPath = m_IFileHandlingBaboon.AvailableReplays[m_CurrentData.ReplayPath];
                    m_IFileHandlingBaboon.RemoveReplayFromAvailables(m_CurrentData.ReplayPath);
                    File.Move(m_CurrentData.ReplayPath, newPath);
                    m_IFileHandlingBaboon.AddParsedReplayToAvailables(newPath, dataPath);
                    m_IFileHandlingBaboon.ChangeParsedReplayPath(dataPath, newPath);
                    m_CurrentData.ReplayPath = newPath;
                    RefreshListBox();
                }
            }
        }

        public void DeleteReplay()
        {
            if (!String.IsNullOrEmpty(m_CurrentData.ReplayPath))
            {
                File.Delete(m_IFileHandlingBaboon.AvailableReplays[m_CurrentData.ReplayPath]);
                m_IFileHandlingBaboon.RemoveReplayFromAvailables(m_CurrentData.ReplayPath);
            }

            RefreshListBox();
        }

        public void RefreshListBox()
        {
            if (m_Main != null)
            {
                ItemCollection items = m_Main.listBoxAvailableReplays.Items;
                items.Clear();

                if (m_IFileHandlingBaboon != null)
                {
                    foreach (KeyValuePair<String, String> availableReplay in m_IFileHandlingBaboon.AvailableReplays)
                    {
                        items.Add(availableReplay.Key);
                    }
                }
            }
        }

        public void UpdateDisplay(String XMLFilePath)
        {
            ClearInfos();
            if (m_IMonkeyDeserializer != null)
            {
                m_IMonkeyDeserializer.DeserializeReplay(XMLFilePath);
                m_CurrentData = m_IMonkeyDeserializer.CurrentReplayData;
                GenerateTargetStringDict();
                UpdateChatLogContent();
                UpdateCurrentMap();
                UpdatePlayersInfo();
                UpdateGameInfo();
                UpdateOtherInfo();
            }
        }

        public void ShowWinner()
        {
            if (m_Main != null)
            {
                if (m_Main.labelWinner.Content == "?")
                {
                    foreach (PlayerInfo player in m_CurrentData.PlayersInfo)
                    {
                        if (player.Won)
                        {
                            m_Main.labelWinner.Content = player.Name;
                            m_Main.labelWinner.Foreground = m_Sc2ToWPFColors[player.sColor];
                            m_Main.buttonShowWinner.Content = "Hide Winner";
                        }
                    }
                }
                else
                {
                    m_Main.labelWinner.Content = "?";
                    m_Main.labelWinner.Foreground = Brushes.White;
                    m_Main.buttonShowWinner.Content = "Show Winner";
                }
            }
        }

        public void EnlargeMapImage()
        {
            WindowEnlargeMapImage EnlargedMap = new WindowEnlargeMapImage(m_Main);
            EnlargedMap.Owner = m_Main;
            EnlargedMap.largeImage.Source = m_Main.currentMapImage.Source;
            EnlargedMap.ShowDialog();
        }

        public void WatchReplay()
        {            
            if (!String.IsNullOrEmpty(m_CurrentData.ReplayPath))
            {                
                RegistryKey hkLocal = Registry.LocalMachine;
                if (hkLocal != null)
                {                    
                    RegistryKey sc2Key = hkLocal.OpenSubKey("SOFTWARE");
                    if(sc2Key != null)
                    {
                        sc2Key = sc2Key.OpenSubKey("Blizzard Entertainment");
                        if (sc2Key != null)
                        {
                            sc2Key = sc2Key.OpenSubKey("Starcraft II Retail");
                            if (sc2Key != null)
                            {
                                String sc2Path = sc2Key.GetValue("InstallPath").ToString();
                                if (!String.IsNullOrEmpty(sc2Path))
                                {
                                    sc2Path += "Versions\\";
                                    if (m_CurrentData != null && m_CurrentData.Build != null)
                                    {
                                        sc2Path += "Base" + m_CurrentData.Build + "\\";
                                        sc2Path += "SC2.exe";

                                        if (File.Exists(sc2Path))
                                        {
                                            Process.Start(m_CurrentData.ReplayPath).WaitForExit();
                                        }
                                        else
                                        {
                                            MessageBox.Show(m_Main, "You do not have the required game version to watch this replay (version: " + m_CurrentData.Version + ")", "SC2 version is missing");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Private methods
        private void UpdateOtherInfo()
        {
            m_Main.LabelRealmValue.Content = m_CurrentData.Realm;
            m_Main.labelRealTeamSizeValue.Content = m_CurrentData.RealTeamSize;
            m_Main.labelGameSpeedValue.Content = m_CurrentData.GameSpeed;
            m_Main.labelVersionValue.Content = m_CurrentData.Version;
            m_Main.labelRecorderValue.Content = m_TargetStringDict[m_CurrentData.RecorderID + 1];
            m_Main.labelGameTypeValue.Content = m_CurrentData.GamePublic ? "Public" : "Private";
        }

        private void UpdateCurrentMap()
        {
            String mapUriPath = GetMapKey(m_CurrentData.MapName);

            BitmapImage mapImage = new BitmapImage();
            mapImage.BeginInit();
            mapImage.UriSource = new Uri(mapUriPath);
            mapImage.EndInit();

            m_Main.currentMapImage.Source = mapImage;
            m_Main.labelMapName.Content = m_CurrentData.MapName;
        }

        private void FillPlayerInfoControl(Label labelToFill, Image imageToFill, Image randomImageControl, Brush labelColor, String playerName, String SRace, String LRace)
        {
            labelToFill.Content = playerName;
            labelToFill.Foreground = labelColor;

            if (SRace == "RAND")
            {
                BitmapImage randomImage = new BitmapImage();
                randomImage.BeginInit();                
                randomImage.UriSource = new Uri(m_Workers["RAND"]);
                randomImage.EndInit();
                randomImageControl.Source = randomImage;
                randomImageControl.ToolTip = "This player randomed!";

                BitmapImage workerImage = new BitmapImage();
                workerImage.BeginInit();
                workerImage.UriSource = new Uri(m_Workers[LRace]);
                workerImage.EndInit();
                imageToFill.Source = workerImage;
            }

            else
            {
                BitmapImage workerImage = new BitmapImage();
                workerImage.BeginInit();
                workerImage.UriSource = new Uri(m_Workers[SRace]);
                workerImage.EndInit();
                imageToFill.Source = workerImage;
            }
        }

        private void UpdateGameInfo()
        {
            m_Main.labelDate.Content = m_CurrentData.GameTime.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            TimeSpan gameLength = new TimeSpan(0, 0, m_CurrentData.GameLength);
            m_Main.labelLength.Content = new DateTime(gameLength.Ticks).ToString("HH:mm:ss");
        }

        private void UpdateChatLogContent()
        {
            List<Message> chatLog = m_CurrentData.ChatMessages;
            RichTextBox RTB = m_Main.richTextBoxChatLog;
            RTB.Document.Blocks.Clear();
            TextPointer docStart = RTB.CaretPosition;

            foreach (Message mess in chatLog)
            {
                TimeSpan newTimeSpan = new TimeSpan(0, 0, mess.MessageTime);
                TextRange tr = new TextRange(RTB.Document.ContentEnd, RTB.Document.ContentEnd);
                tr.Text = "(" + newTimeSpan.ToString() + ") ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                TextRange tr2 = new TextRange(RTB.Document.ContentEnd, RTB.Document.ContentEnd);
                tr2.Text = mess.MessageName;
                tr2.ApplyPropertyValue(TextElement.ForegroundProperty, m_Sc2ToWPFColors[m_CurrentData.PlayersInfo[mess.MessageID - 1].sColor]);

                TextRange tr3 = new TextRange(RTB.Document.ContentEnd, RTB.Document.ContentEnd);
                tr3.Text = " (" + m_TargetStringDict[mess.MessageTarget] + "):" + mess.MessageContent + Environment.NewLine;
                tr3.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }

            RTB.ScrollToHome();
        }

        private void UpdatePlayersInfo()
        {
            switch (m_CurrentData.RealTeamSize)
            {
                case "1v1":
                    m_Main.labelPlayer2Team1.Content = "";
                    m_Main.labelPlayer2Team2.Content = "";
                    foreach (PlayerInfo playerInfo in m_CurrentData.PlayersInfo)
                    {
                        if(playerInfo.isObserver)
                        {
                            break;
                        }

                        String playerName = playerInfo.Name;
                        if (playerInfo.isComputer)
                        {
                            playerName += " (" + playerInfo.Difficulty + ")";
                        }
                        if (playerInfo.Team == 1)
                        {
                            FillPlayerInfoControl(m_Main.labelPlayer1Team1, 
                                                  m_Main.imageWorkerP1T1, 
                                                  m_Main.randomImageP1T1,
                                                  m_Sc2ToWPFColors[playerInfo.sColor], 
                                                  playerName, 
                                                  playerInfo.sRace,
                                                  playerInfo.LRace);
                        }
                        if (playerInfo.Team == 2)
                        {
                            FillPlayerInfoControl(m_Main.labelPlayer1Team2,
                                                  m_Main.imageWorkerP1T2,
                                                  m_Main.randomImageP1T2,
                                                  m_Sc2ToWPFColors[playerInfo.sColor], 
                                                  playerName,
                                                  playerInfo.sRace,
                                                  playerInfo.LRace);
                        }
                    }
                    break;
                case "2v2":
                    Boolean team1player1 = true;
                    Boolean team2player1 = true;
                    foreach (PlayerInfo playerInfo in m_CurrentData.PlayersInfo)
                    {
                        if (playerInfo.isObserver)
                        {
                            break;
                        }

                        String playerName = playerInfo.Name;
                        if (playerInfo.isComputer)
                        {
                            playerName += " (" + playerInfo.Difficulty + ")";
                        }

                        if (playerInfo.Team == 1)
                        {
                            if (team1player1)
                            {
                                FillPlayerInfoControl(m_Main.labelPlayer1Team1,
                                                      m_Main.imageWorkerP1T1,
                                                      m_Main.randomImageP1T1,
                                                      m_Sc2ToWPFColors[playerInfo.sColor],
                                                      playerName,
                                                      playerInfo.sRace,
                                                      playerInfo.LRace);
                                team1player1 = false;
                            }
                            else
                            {
                                FillPlayerInfoControl(m_Main.labelPlayer2Team1,
                                                      m_Main.imageWorkerP2T1,
                                                      m_Main.randomImageP2T1,
                                                      m_Sc2ToWPFColors[playerInfo.sColor],
                                                      playerName,
                                                      playerInfo.sRace,
                                                      playerInfo.LRace);
                            }
                        }
                        if (playerInfo.Team == 2)
                        {
                            if (team2player1)
                            {
                                FillPlayerInfoControl(m_Main.labelPlayer1Team2,
                                                     m_Main.imageWorkerP1T2,
                                                     m_Main.randomImageP1T2,
                                                     m_Sc2ToWPFColors[playerInfo.sColor],
                                                     playerName,
                                                     playerInfo.sRace,
                                                     playerInfo.LRace);
                                team2player1 = false;
                            }
                            else
                            {
                                FillPlayerInfoControl(m_Main.labelPlayer2Team2,
                                                      m_Main.imageWorkerP2T2,
                                                      m_Main.randomImageP2T2,
                                                      m_Sc2ToWPFColors[playerInfo.sColor],
                                                      playerName,
                                                      playerInfo.sRace,
                                                      playerInfo.LRace);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void InitSc2Workers()
        {
            m_Workers = new Dictionary<String, String>();
            m_Workers.Add("Terr", m_CurrentDirectory + @"\Resources\Pictures\SC2 Workers\scv.png");
            m_Workers.Add("Terran", m_CurrentDirectory + @"\Resources\Pictures\SC2 Workers\scv.png");
            m_Workers.Add("Prot", m_CurrentDirectory + @"\Resources\Pictures\SC2 Workers\probe.png");
            m_Workers.Add("Protoss", m_CurrentDirectory + @"\Resources\Pictures\probe.png");
            m_Workers.Add("Zerg", m_CurrentDirectory + @"\Resources\Pictures\SC2 Workers\drone.png");
            m_Workers.Add("RAND", m_CurrentDirectory + @"\Resources\Pictures\SC2 workers\random.png");
        }

        private void InitSc2ToWPFColorsDict()
        {
            m_Sc2ToWPFColors = new Dictionary<String, Brush>();
            m_Sc2ToWPFColors.Add("Blue", Brushes.Blue);
            m_Sc2ToWPFColors.Add("Red", Brushes.Red);
            m_Sc2ToWPFColors.Add("Green", Brushes.Green);
            m_Sc2ToWPFColors.Add("Orange", Brushes.Orange);
            m_Sc2ToWPFColors.Add("Teal", Brushes.Teal);
            m_Sc2ToWPFColors.Add("Yellow", Brushes.Yellow);
            m_Sc2ToWPFColors.Add("Purple", Brushes.Purple);
            m_Sc2ToWPFColors.Add("Violet", Brushes.Violet);
            m_Sc2ToWPFColors.Add("Brown", Brushes.Brown);
            m_Sc2ToWPFColors.Add("Pink", Brushes.Pink);
            m_Sc2ToWPFColors.Add("Light Pink", Brushes.LightPink);
            m_Sc2ToWPFColors.Add("Light Gray", Brushes.LightGray);
            m_Sc2ToWPFColors.Add("Light Green", Brushes.LightGreen);
            m_Sc2ToWPFColors.Add("Dark Green", Brushes.DarkGreen);            
            m_Sc2ToWPFColors.Add("Dark Gray", Brushes.DarkGray);
            m_Sc2ToWPFColors.Add("Unknown", Brushes.White);
            m_Sc2ToWPFColors.Add("", Brushes.White);
        }

        private String GetMapKey(String mapName)
        {
            foreach (KeyValuePair<String, String> mapPair in m_Maps)
            {
                if (mapName.Contains(mapPair.Key))
                {
                    return mapPair.Value;
                }
            }
            return m_Maps["Unknown"];
        }

        private void InitSc2MapsLink()
        {            
            m_Maps = new Dictionary<String, String>();
            m_Maps.Add("Abyss", m_CurrentDirectory + @"\SC2 Official Maps\Abyss.jpg");
            m_Maps.Add("Agria Valley", m_CurrentDirectory + @"\SC2 Official Maps\Agria_Valley.jpg");
            m_Maps.Add("Arakan Citadel", m_CurrentDirectory + @"\SC2 Official Maps\Arakan_Citadel.jpg");
            m_Maps.Add("Arid Wastes", m_CurrentDirectory + @"\SC2 Official Maps\Arid_Wastes.jpg");
            m_Maps.Add("Blistering Sands", m_CurrentDirectory + @"\SC2 Official Maps\Blistering_Sands.jpg");
            m_Maps.Add("Burial Grounds", m_CurrentDirectory + @"\SC2 Official Maps\Burial_Grounds.jpg");
            m_Maps.Add("Challenges", m_CurrentDirectory + @"\SC2 Official Maps\Unknown.jpg");
            m_Maps.Add("Colony 426", m_CurrentDirectory + @"\SC2 Official Maps\Colony_426.jpg");
            m_Maps.Add("Crossfire", m_CurrentDirectory + @"\SC2 Official Maps\Crossfire.jpg");
            m_Maps.Add("Debris Field", m_CurrentDirectory + @"\SC2 Official Maps\Debris_Field.jpg");
            m_Maps.Add("Delta Quadrant", m_CurrentDirectory + @"\SC2 Official Maps\Delta_Quadrant.jpg");
            m_Maps.Add("Desert Oasis", m_CurrentDirectory + @"\SC2 Official Maps\Desert_Oasis.jpg");
            m_Maps.Add("Dig Site", m_CurrentDirectory + @"\SC2 Official Maps\Dig_Site.jpg");
            m_Maps.Add("Dirt Side", m_CurrentDirectory + @"\SC2 Official Maps\Dirt_Side.jpg");
            m_Maps.Add("Discord IV", m_CurrentDirectory + @"\SC2 Official Maps\Discord_IV.jpg");
            m_Maps.Add("Elysium", m_CurrentDirectory + @"\SC2 Official Maps\Elysium.jpg");
            m_Maps.Add("Extinction", m_CurrentDirectory + @"\SC2 Official Maps\Extinction.jpg");
            m_Maps.Add("Forbidden Planet", m_CurrentDirectory + @"\SC2 Official Maps\Forbidden_Planet.jpg");
            m_Maps.Add("Frontier", m_CurrentDirectory + @"\SC2 Official Maps\Frontier.jpg");
            m_Maps.Add("High Ground", m_CurrentDirectory + @"\SC2 Official Maps\High_Ground.jpg");
            m_Maps.Add("High Orbit", m_CurrentDirectory + @"\SC2 Official Maps\High_Orbit.jpg");
            m_Maps.Add("Incineration Zone", m_CurrentDirectory + @"\SC2 Official Maps\Incineration_Zone.jpg");
            m_Maps.Add("Jungle Basin", m_CurrentDirectory + @"\SC2 Official Maps\Jungle_Basin.jpg");
            m_Maps.Add("Junk Yard", m_CurrentDirectory + @"\SC2 Official Maps\Junk_yard.jpg");
            m_Maps.Add("Kulas Ravine", m_CurrentDirectory + @"\SC2 Official Maps\Kulas Ravine.jpg");
            m_Maps.Add("Lava Flow", m_CurrentDirectory + @"\SC2 Official Maps\Lava_Flow.jpg");
            m_Maps.Add("Lost Temple", m_CurrentDirectory + @"\SC2 Official Maps\Lost_Temple.jpg");
            m_Maps.Add("Megaton", m_CurrentDirectory + @"\SC2 Official Maps\Megaton.jpg");
            m_Maps.Add("Metalopolis", m_CurrentDirectory + @"\SC2 Official Maps\Metalopolis.jpg");
            m_Maps.Add("Monlyth Ridge", m_CurrentDirectory + @"\SC2 Official Maps\Monlyth_Ridge.jpg");
            m_Maps.Add("Monsoon", m_CurrentDirectory + @"\SC2 Official Maps\Monsoon.jpg");
            m_Maps.Add("New Antioch", m_CurrentDirectory + @"\SC2 Official Maps\New_Antioch.jpg");
            m_Maps.Add("Nightmare", m_CurrentDirectory + @"\SC2 Official Maps\Nightmare.jpg");
            m_Maps.Add("Outpost", m_CurrentDirectory + @"\SC2 Official Maps\Outpost.jpg");
            m_Maps.Add("Primeval", m_CurrentDirectory + @"\SC2 Official Maps\Primeval.jpg");
            m_Maps.Add("Quicksand", m_CurrentDirectory + @"\SC2 Official Maps\Quicksand.jpg");
            m_Maps.Add("Red Stone Gulch", m_CurrentDirectory + @"\SC2 Official Maps\Red_Stone_Gulch.jpg");
            m_Maps.Add("Sacred Ground", m_CurrentDirectory + @"\SC2 Official Maps\Sacred grounds.jpg");
            m_Maps.Add("Sand Canyon", m_CurrentDirectory + @"\SC2 Official Maps\Sand_Canyon.jpg");
            m_Maps.Add("Scorched Haven", m_CurrentDirectory + @"\SC2 Official Maps\Scorched_Haven.jpg");
            m_Maps.Add("Scrap Station", m_CurrentDirectory + @"\SC2 Official Maps\Scrap_Station.jpg");
            m_Maps.Add("Shakuras Plateau", m_CurrentDirectory + @"\SC2 Official Maps\Shakuras_Plateau.jpg");
            m_Maps.Add("Steppes of War", m_CurrentDirectory + @"\SC2 Official Maps\Steppes_of_War.jpg");
            m_Maps.Add("Tarsonis Assault", m_CurrentDirectory + @"\SC2 Official Maps\Tarsonis_Assault.jpg");
            m_Maps.Add("Tectonic Rift", m_CurrentDirectory + @"\SC2 Official Maps\Tectonic_Rift.jpg");
            m_Maps.Add("Tempest", m_CurrentDirectory + @"\SC2 Official Maps\Tempest.jpg");
            m_Maps.Add("Terminus", m_CurrentDirectory + @"\SC2 Official Maps\Terminus.jpg");
            m_Maps.Add("The Bio Lab", m_CurrentDirectory + @"\SC2 Official Maps\The_Bio_Lab.jpg");
            m_Maps.Add("Toxic Slums", m_CurrentDirectory + @"\SC2 Official Maps\Toxic_Slums.jpg");
            m_Maps.Add("Twilight Fortress", m_CurrentDirectory + @"\SC2 Official Maps\Twilight_Fortress.jpg");
            m_Maps.Add("Typhon", m_CurrentDirectory + @"\SC2 Official Maps\Typhon.jpg");
            m_Maps.Add("Ulaan Deeps", m_CurrentDirectory + @"\SC2 Official Maps\Ulaan_Deeps.jpg");
            m_Maps.Add("War Zone", m_CurrentDirectory + @"\SC2 Official Maps\War_Zone.jpg");
            m_Maps.Add("Worldship", m_CurrentDirectory + @"\SC2 Official Maps\Worldship.jpg");
            m_Maps.Add(@"Xel'Naga Caverns", m_CurrentDirectory + @"\SC2 Official Maps\Xel'Naga_Caverns.jpg");
            m_Maps.Add("Unknown", m_CurrentDirectory + @"\SC2 Official Maps\Unknown.jpg");
        }

        private void GenerateTargetStringDict()
        {
            m_TargetStringDict = new Dictionary<Int32, String>();
            m_TargetStringDict.Add(0, "All");

            foreach (PlayerInfo player in m_CurrentData.PlayersInfo)
            {
                m_TargetStringDict.Add(player.ID, player.Name);
            }
        }
        #endregion

        #region Member variables
        private MainWindow m_Main = null;
        private IMonkeyDeserializer m_IMonkeyDeserializer = null;
        private IFileHandlingBaboon m_IFileHandlingBaboon = null;
        private IConfig m_Config = null;
        private ParsedData m_CurrentData = null;
        private Dictionary<String, Brush> m_Sc2ToWPFColors = null;
        private Dictionary<Int32, String> m_TargetStringDict = null;
        private Dictionary<String, String> m_Maps = null;
        private Dictionary<String, String> m_Workers = null;
        String m_CurrentDirectory = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString();
        #endregion

    }
}