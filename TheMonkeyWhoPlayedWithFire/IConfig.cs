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

        Boolean FullDelete { get; set; }

        Boolean DoNotShowSc2NotFoundError { get; set; }

        String RenameFormat { get; set; }

        Boolean AutoRename { get; set; }

        RenamingType ChosenRenamingType { get; set; }
    }

    public enum RenamingType
    {
        Personalized,
        Formatted1,
        Formatted2,
        Formatted3,
        Custom,
        None,
    }
}
