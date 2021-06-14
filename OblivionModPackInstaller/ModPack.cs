using System;
using System.Collections.Generic;
using System.Text;

namespace OblivionModPackInstaller
{
    class ModPack
    {
        public string ModName { get; set; }
        public string ArchiveName { get; set; }
        public string InstallLocation { get; set; }
        public List<string> Files { get; set; }

        public ModPack() : this("","","",new List<string>())
        {

        }

        public ModPack(string modName, string archiveName, string install, List<string> files)
        {
            this.ModName = modName;
            this.ArchiveName = archiveName;
            this.InstallLocation = install;
            this.Files = files;
        }

    }
}
