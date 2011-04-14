using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC2ParserApe;
using TheUpsideDownLemur;

namespace TheMonkeyWhoPlayedWithFire
{
    /// <summary>
    /// This class is central, it deserializes data from an XML, and casts values into CurrentReplayData.
    /// That Object is then sent to the IoC, that dispatches the informations all around the software.
    /// </summary>
    public class MonkeyDeserializer : IMonkeyDeserializer
    {
        public MonkeyDeserializer()
        {
            CurrentReplayData = new ParsedData();
            IoC.AddMonkey<IMonkeyDeserializer>(this);
        }

        public void DeserializeReplay(String path)
        {
            CurrentReplayData = new ParsedData();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
                        
            XmlNode gameVariables = doc.SelectSingleNode("Game_Variables"); //Main node

            //Deserializing the general variables (game length, date etc).
            CurrentReplayData.ReplayPath = gameVariables.SelectSingleNode("ReplayPath").InnerXml;
            CurrentReplayData.Build = Convert.ToInt32(gameVariables.SelectSingleNode("Build").InnerXml);
            CurrentReplayData.GameTime = DateTime.Parse(gameVariables.SelectSingleNode("GameTime").InnerXml);
            CurrentReplayData.GameLength = Convert.ToInt32(gameVariables.SelectSingleNode("GameLength").InnerXml);
            CurrentReplayData.GamePublic = Convert.ToBoolean(gameVariables.SelectSingleNode("GamePublic").InnerXml);
            CurrentReplayData.GameSpeed = gameVariables.SelectSingleNode("GameSpeed").InnerXml;
            CurrentReplayData.MapName = gameVariables.SelectSingleNode("MapName").InnerXml;
            CurrentReplayData.Realm = gameVariables.SelectSingleNode("Realm").InnerXml;
            CurrentReplayData.RealTeamSize = gameVariables.SelectSingleNode("RealTeamSize").InnerXml;
            CurrentReplayData.RecorderID = Convert.ToInt32(gameVariables.SelectSingleNode("RecorderID").InnerXml);
            CurrentReplayData.Version = gameVariables.SelectSingleNode("Version").InnerXml;
            CurrentReplayData.WinnerKnown = Convert.ToBoolean(gameVariables.SelectSingleNode("WinnerKnown").InnerXml);

            //Deserializing chat messages.
            XmlNode chatNode = gameVariables.SelectSingleNode("ChatMessages");
            foreach (XmlNode message in chatNode.ChildNodes)
            {
                Message newMessage = new Message();
                newMessage.MessageID = Convert.ToInt32(message.SelectSingleNode("MessageID").InnerXml);
                newMessage.MessageName = message.SelectSingleNode("MessageName").InnerXml;
                newMessage.MessageTime = Convert.ToInt32(message.SelectSingleNode("MessageTime").InnerXml);
                newMessage.MessageContent = message.SelectSingleNode("MessageContent").InnerXml;
                newMessage.MessageTarget = Convert.ToInt32(message.SelectSingleNode("MessageTarget").InnerXml);

                CurrentReplayData.ChatMessages.Add(newMessage);
            }

            //Deserializing events.
            XmlNode eventsNode = gameVariables.SelectSingleNode("Events");
            foreach (XmlNode singleEvent in eventsNode.ChildNodes)
            {
                Sc2Event newEvent = new Sc2Event();
                newEvent.PlayerID = Convert.ToInt32(singleEvent.SelectSingleNode("PlayerID").InnerXml);
                newEvent.EventTime = Convert.ToDouble(singleEvent.SelectSingleNode("EventTime").InnerXml);
                newEvent.EventAbilityID = Convert.ToInt32(singleEvent.SelectSingleNode("EventAbilityID").InnerXml);              
                newEvent.EventAbility = DeserializeAbility(singleEvent.SelectSingleNode("EventAbility"));

                CurrentReplayData.Events.Add(newEvent);
            }

            //Deserializing players infos.
            XmlNode playersNode = gameVariables.SelectSingleNode("Players");
            foreach (XmlNode singlePlayer in playersNode.ChildNodes)
            {
                //General player info
                PlayerInfo newPlayer = new PlayerInfo();
                newPlayer.Name = singlePlayer.SelectSingleNode("Name").InnerXml;
                newPlayer.UID = Convert.ToInt32(singlePlayer.SelectSingleNode("UID").InnerXml);
                newPlayer.UIDIndex = Convert.ToInt32(singlePlayer.SelectSingleNode("UIDIndex").InnerXml);
                newPlayer.Color = singlePlayer.SelectSingleNode("Color").InnerXml;
                newPlayer.ApmTotal = Convert.ToInt32(singlePlayer.SelectSingleNode("ApmTotal").InnerXml);
                newPlayer.Handicap = Convert.ToInt32(singlePlayer.SelectSingleNode("Handicap").InnerXml);
                newPlayer.Team = Convert.ToInt32(singlePlayer.SelectSingleNode("Team").InnerXml);
                newPlayer.LRace = singlePlayer.SelectSingleNode("LRace").InnerXml;
                newPlayer.Race = singlePlayer.SelectSingleNode("Race").InnerXml;
                newPlayer.ID = Convert.ToInt32(singlePlayer.SelectSingleNode("ID").InnerXml);
                newPlayer.isComputer = Convert.ToBoolean(singlePlayer.SelectSingleNode("isComputer").InnerXml);
                newPlayer.isObserver = Convert.ToBoolean(singlePlayer.SelectSingleNode("isObserver").InnerXml);
                newPlayer.Difficulty = singlePlayer.SelectSingleNode("Difficulty").InnerXml;
                newPlayer.sColor = singlePlayer.SelectSingleNode("sColor").InnerXml;
                newPlayer.sRace = singlePlayer.SelectSingleNode("sRace").InnerXml;
                newPlayer.ColorIndex = Convert.ToInt32(singlePlayer.SelectSingleNode("ColorIndex").InnerXml);
                newPlayer.Won = Convert.ToBoolean(singlePlayer.SelectSingleNode("Won").InnerXml);

                XmlNode apmNode = singlePlayer.SelectSingleNode("Apm");
                foreach (XmlNode singleApmValue in apmNode.ChildNodes)
                {
                    String[] apmValues = singleApmValue.InnerXml.Split(';');
                    newPlayer.Apm.Add(Convert.ToInt32(apmValues[0]), Convert.ToInt32(apmValues[1]));
                }

                XmlNode numEventsNode = singlePlayer.SelectSingleNode("NumEvents");
                foreach (XmlNode numEvent in numEventsNode.ChildNodes)
                {
                    Int32 timesOccured = Convert.ToInt32(numEvent.SelectSingleNode("TimesOccured").InnerXml);
                    Sc2Ability ability = DeserializeAbility(numEvent.SelectSingleNode("EventAbility"));
                    newPlayer.NumEvents.Add(ability, timesOccured);
                }

                XmlNode firstEventsNode = singlePlayer.SelectSingleNode("FirstEvents");
                foreach (XmlNode firstEvent in firstEventsNode.ChildNodes)
                {
                    Int32 gameTime = Convert.ToInt32(firstEvent.SelectSingleNode("Time").InnerXml);
                    Sc2Ability ability = DeserializeAbility(firstEvent.SelectSingleNode("EventAbility"));
                    newPlayer.FirstEvents.Add(ability, gameTime);
                }

                CurrentReplayData.PlayersInfo.Add(newPlayer);
            }
        }

        /// <summary>
        /// Deserializes an Sc2Ability.
        /// </summary>
        /// <param name="abilityNode">The XML Node that contains the ability to deserialize.</param>
        /// <returns>An Sc2Ability object, containing the informations.</returns>
        private Sc2Ability DeserializeAbility(XmlNode abilityNode)
        {
            Sc2Ability newAbility = new Sc2Ability();
            newAbility.AbilityCode = Convert.ToInt32(abilityNode.SelectSingleNode("AbilityCode").InnerXml);
            newAbility.Description = abilityNode.SelectSingleNode("Description").InnerXml;
            newAbility.Name = abilityNode.SelectSingleNode("Name").InnerXml;
            newAbility.Type = Convert.ToInt32(abilityNode.SelectSingleNode("Type").InnerXml);
            newAbility.TypeString = abilityNode.SelectSingleNode("TypeString").InnerXml;
            newAbility.SubType = Convert.ToInt32(abilityNode.SelectSingleNode("SubType").InnerXml);
            newAbility.SubTypeString = abilityNode.SelectSingleNode("SubTypeString").InnerXml;
            newAbility.Mineral = Convert.ToInt32(abilityNode.SelectSingleNode("Mineral").InnerXml);
            newAbility.Gas = Convert.ToInt32(abilityNode.SelectSingleNode("Gas").InnerXml);
            newAbility.Supply = Convert.ToInt32(abilityNode.SelectSingleNode("Supply").InnerXml);

            return newAbility;
        }

        public ParsedData CurrentReplayData { get; set; }
    }
}
