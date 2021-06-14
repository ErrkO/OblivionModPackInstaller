using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace OblivionModPackInstaller
{
    class OblivionRemasteredParser
    {

        public static List<ModPack> ParseDoc()
        {

            List<string> folders = new List<string>()
            {
                 "Data"
                ,"Textures"
                ,"Meshes"
                ,"Src"
                ,"OBSE"
                ,"Shaders"
                ,"Essential"
                ,"Music"
                ,"Sound"
                ,"Prefabs"
                ,"Fonts"
                ,"Ini"
                ,"Menus"
            };

            string line = "";
            List<ModPack> mods = new List<ModPack>();

            string pattern_ModName = @"-+\s+\d+\)\s(.*)";
            string pattern_FileName = @"File.*\s=\s(.*)";
            string pattern_Location = @"Contents\s=\s\((.*)\)";
            Regex ModName = new Regex(pattern_ModName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex FileName = new Regex(pattern_FileName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex Location = new Regex(pattern_Location, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match mc;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\ModList.txt");
            
            string fileName = "";
            string archive = "";
            string location = "";
            List<string> files = new List<string>();

            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {                

                if (ModName.IsMatch(line))
                {
                    mc = ModName.Match(line);

                    fileName = mc.Groups[1].Value;
                }
                else if (FileName.IsMatch(line))
                {
                    mc = FileName.Match(line);

                    archive = mc.Groups[1].Value;
                }
                else if (Location.IsMatch(line))
                {
                    mc = Location.Match(line);

                    location = mc.Groups[1].Value;

                    location = location.Replace(">", "\\");
                }
                else if (line != "")
                {
                    files.Add(line);
                }
                else
                {
                    mods.Add(new ModPack(fileName, archive, location, ParseFiles(files)));
                    files.Clear();
                }
            }

            file.Close();

            return mods;
        }

        public static List<string> ParseFiles(List<string> files)
        {
            List<string> newFiles = new List<string>();
            string pattern_Extension = @"\..{3,4}$";
            string pattern_Folder = @"\((.*)\)\s(.*)";
            string pattern_Location = @"Contents\s=\s\((.*)\)";
            Regex re_Extension = new Regex(pattern_Extension, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex re_Folder = new Regex(pattern_Folder, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex Location = new Regex(pattern_Location, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Match match;
            string tempFile = "";

            foreach (string file in files)
            {
                tempFile = file;

                if (re_Folder.IsMatch(file))
                {
                    match = re_Folder.Match(file);
                    tempFile = "";

                    tempFile += match.Groups[1].Value + "\\" + (match.Groups[2].Value.Contains("*") ? "" : match.Groups[2].Value);

                    tempFile = tempFile.Replace(">", "\\");

                }
                
                if (tempFile.Contains("*All"))
                {
                    tempFile = ".";
                }

                newFiles.Add(tempFile);

            }

            return newFiles;
        }

    }
}
