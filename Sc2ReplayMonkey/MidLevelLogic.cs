using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUpsideDownLemur;
using TheMonkeyWhoPlayedWithFire;
using SC2ParserApe;
using System.Text.RegularExpressions;
using System.IO;

namespace Sc2ReplayMonkey
{
    public class MidLevelLogic
    {
        public MidLevelLogic(MainWindow main, ControlsLogic logic)
        {
            m_Main = main;
            m_ControlsLogic = logic;
            m_Config = IoC.FindMonkey<IConfig>();
            m_MonkeyDeserializer = IoC.FindMonkey<IMonkeyDeserializer>();
        }

        public void CallRelocate(String newPath = "")
        {
            if (!String.IsNullOrEmpty(newPath))
            {
                m_ControlsLogic.RelocateReplay(Path.GetDirectoryName(m_MonkeyDeserializer.CurrentReplayData.ReplayPath) + "\\" + newPath + ".sc2replay");
            }
        }

        public Boolean GenerateNewName(out String NewName)
        {
            NewName = String.Empty;
            ParsedData data = m_MonkeyDeserializer.CurrentReplayData;

            try
            {
                switch (m_Config.ChosenRenamingType)
                {
                    case RenamingType.Personalized:
                        NewName = m_Config.RenameFormat;
                        NewName = ReplaceInvalidChars(NewName);
                        break;
                    case RenamingType.Formatted1: //Player1(Race) vs Player2(Race)
                        String playerNameF1 = m_Main.labelPlayer1Team1.Content.ToString();
                        NewName += playerNameF1 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF1)].LRace + ") ";
                        playerNameF1 = m_Main.labelPlayer2Team1.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF1))
                        {
                            NewName += playerNameF1 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF1)].LRace + ") ";
                        }
                        NewName += "VS ";
                        playerNameF1 = m_Main.labelPlayer1Team2.Content.ToString();
                        NewName += playerNameF1 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF1)].LRace + ") ";
                        playerNameF1 = m_Main.labelPlayer2Team2.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF1))
                        {
                            NewName += playerNameF1 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF1)].LRace + ")";
                        }
                        NewName = ReplaceInvalidChars(NewName);
                        return true;
                    case RenamingType.Formatted2: //Player1 vs Player2 Date
                        String playerNameF2 = m_Main.labelPlayer1Team1.Content.ToString();
                        NewName += playerNameF2;
                        playerNameF2 = m_Main.labelPlayer2Team1.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF2))
                        {
                            NewName += " " + playerNameF2;
                        }
                        NewName += " VS ";
                        playerNameF2 = m_Main.labelPlayer1Team2.Content.ToString();
                        NewName += playerNameF2;
                        playerNameF2 = m_Main.labelPlayer2Team2.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF2))
                        {
                            NewName += " " + playerNameF2;
                        }
                        NewName += " " + data.GameTime.ToShortDateString() + " " + data.GameTime.ToShortTimeString();
                        NewName = ReplaceInvalidChars(NewName);
                        return true;
                    case RenamingType.Formatted3: //Player1(Race) vs Player2(Race) Date 
                        String playerNameF3 = m_Main.labelPlayer1Team1.Content.ToString();
                        NewName += playerNameF3 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF3)].LRace + ") ";
                        playerNameF3 = m_Main.labelPlayer2Team1.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF3))
                        {
                            NewName += playerNameF3 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF3)].LRace + ") ";
                        }
                        NewName += "VS ";
                        playerNameF3 = m_Main.labelPlayer1Team2.Content.ToString();
                        NewName += playerNameF3 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF3)].LRace + ") ";
                        playerNameF3 = m_Main.labelPlayer2Team2.Content.ToString();
                        if (!String.IsNullOrEmpty(playerNameF3))
                        {
                            NewName += playerNameF3 + "(" + data.PlayersInfo[m_ControlsLogic.GetPlayerIDFromName(playerNameF3)].LRace + ") ";
                        }
                        NewName += " " + data.GameTime.ToShortDateString() + " " + data.GameTime.ToShortTimeString();
                        NewName = ReplaceInvalidChars(NewName);
                        return true;
                    case RenamingType.Custom: //Customized, keywords enabled
                        GetFormattedNewName(ref NewName, m_Config.RenameFormat);
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void GetFormattedNewName(ref String stringToFormat, String Format)
        {
            stringToFormat = Format;
            ParsedData data = m_MonkeyDeserializer.CurrentReplayData;
            //Matching the {Player#} keyword
            Regex playerRegex = new Regex(@"\{Player[0-9]\}");
            Regex playerNr = new Regex(@"[0-9]");
            foreach (Match match in playerRegex.Matches(stringToFormat))
            {
                Int32 playerNumber = Convert.ToInt32(playerNr.Match(match.Value).Value);
                if (playerNumber < data.PlayersInfo.Count)
                {
                    stringToFormat = stringToFormat.Replace(match.Value, data.PlayersInfo[playerNumber].Name);
                }
            }

            //Matching the {Race#} keyword
            Regex raceRegex = new Regex(@"\{Race[0-9]\}");
            foreach (Match match in raceRegex.Matches(stringToFormat))
            {
                Int32 playerNumber = Convert.ToInt32(playerNr.Match(match.Value).Value);
                if (playerNumber < data.PlayersInfo.Count)
                {
                    stringToFormat = stringToFormat.Replace(match.Value, data.PlayersInfo[playerNumber].LRace);
                }
            }

            //Matching the {Date HH:mm:ss} keyword
            Regex dateRegex = new Regex(@"\{Date (HH)?:?(mm)?:?(ss)?\}");
            Regex dateFormatRegex = new Regex(@" (HH)?:?(mm)?:?(ss)?");
            foreach (Match match in dateRegex.Matches(stringToFormat))
            {
                String dateFormat = dateFormatRegex.Match(match.Value).Value;
                stringToFormat = stringToFormat.Replace(match.Value, data.GameTime.ToShortDateString() + " " + data.GameTime.ToString(dateFormat));
            }

            //Matching the {Length HH:mm:ss} keyword
            Regex lengthRegex = new Regex(@"\{Length (HH)?:?(mm)?:?(ss)?\}");
            foreach (Match match in lengthRegex.Matches(stringToFormat))
            {
                String dateFormat = dateFormatRegex.Match(match.Value).Value;
                stringToFormat = stringToFormat.Replace(match.Value, new DateTime(new TimeSpan(0, 0, data.GameLength).Ticks).ToString(dateFormat));
            }

            //Matching the {Realm} keyword
            Regex realmRegex = new Regex(@"\{Realm\}");
            foreach (Match match in realmRegex.Matches(stringToFormat))
            {
                stringToFormat = stringToFormat.Replace(match.Value, data.Realm);
            }

            //Matching the {GameType} keyword
            Regex gameTypeRegex = new Regex(@"\{GameType\}");
            foreach (Match match in gameTypeRegex.Matches(stringToFormat))
            {
                stringToFormat = stringToFormat.Replace(match.Value, data.RealTeamSize);
            }

            //Matching the {Comment} keyword
            Regex commentRegex = new Regex(@"\{Comment\}");
            foreach (Match match in commentRegex.Matches(stringToFormat))
            {
                stringToFormat = stringToFormat.Replace(match.Value, m_MonkeyDeserializer.CurrentReplayComment);
            }

            stringToFormat = ReplaceInvalidChars(stringToFormat);
        }

        private String ReplaceInvalidChars(String inputString)
        {
            //Handling invalid characters and backslash
            foreach (Char invalidChar in Path.GetInvalidFileNameChars())
            {
                inputString = inputString.Replace(invalidChar, '_');
            }

            inputString = inputString.Replace(@"\", @"_");
            inputString = inputString.Replace(@"/", @"_");

            return inputString;
        }

        IConfig m_Config = null;
        IMonkeyDeserializer m_MonkeyDeserializer = null;
        MainWindow m_Main = null;
        ControlsLogic m_ControlsLogic = null;
    }
}
