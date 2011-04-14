using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC2ParserApe;

namespace TheMonkeyWhoPlayedWithFire
{
    public interface IMonkeyDeserializer
    {
        void DeserializeReplay(String path);

        ParsedData CurrentReplayData { get; set; }
    }
}
