using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2ReplayMonkey
{
    public interface IConfig
    {
        void DeserializeConfig();

        void SerializeConfig();

        String RelocatePath { get; set; }

        Boolean AutoRelocate { get; set; }
    }
}
