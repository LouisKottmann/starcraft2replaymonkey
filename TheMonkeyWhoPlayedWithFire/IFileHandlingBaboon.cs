using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheMonkeyWhoPlayedWithFire
{
    public interface IFileHandlingBaboon
    {
        /// <summary>
        /// This calls Sc2ParserApe to parse the replays.
        /// </summary>
        /// <param name="ReplayPaths">List of replays to parse.</param>
        void HandleFiles(List<String> ReplayPaths);

        Dictionary<String, String> AvailableReplays { get; set; }

        void AddParsedReplayToAvailables(String ReplayPath, String DataPath);

        void RemoveReplayFromAvailables(String ReplayPath);

        void ChangeParsedReplayPath(String ReplayXmlPath, String NewPath);

        void DeserializeAvailables();
    }
}
