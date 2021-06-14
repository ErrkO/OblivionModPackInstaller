using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OblivionModPackInstaller
{
    class Program
    {
        static void Main(string[] args)
        {

            List<string> skippables = new List<string>()
            {
                 ".txt"
                ,".rtf"
                ,".pdf"
                ,".doc"
                ,".html"
            };

            List<ModPack> mods = OblivionRemasteredParser.ParseDoc();

            //string oblivionPath = @"F:\SteamLibrary\steamapps\common\";
            string oblivionPath = @"F:\Users\erico\Downloads\Oblivion Mods\Common";
            string oblivionMods = @"F:\Users\erico\Downloads\Oblivion Mods";
            string modPath = "";
            string installPath;
            string extractPath;

            List<string> modArchives = new List<string>();
            List<string> modContents = new List<string>();
            modArchives.AddRange(Directory.GetFiles(oblivionMods));

            bool proceed;
            int counter = 0;
            
            foreach (ModPack mod in mods)
            {
                proceed = true;
                counter++;

                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(string.Format("Starting Mod #{0} Name: {1}\n", counter,mod.ModName));

                modContents = new List<string>();

                // If any of the archives in the list contain the mod name 
                // then grab the path, otherwise clear it
                if (modArchives.Any(x => x.Contains(mod.ArchiveName)))
                {
                    modPath = modArchives.First(x => x.Contains(mod.ArchiveName));
                }
                else
                {
                    modPath = "";
                }
                
                // debug statements
                if (mod.ModName == "The Dark Tower v9 Essential")
                {
                    Console.Write("");
                }

                // Path we are installing too
                installPath = Path.Combine(oblivionPath, mod.InstallLocation);
                // Path we are extracting the archive too
                extractPath = Path.Combine(oblivionMods, mod.ArchiveName);

                if (modPath == "")
                {
                    Console.WriteLine(string.Format("System did not find archive ({0}) Skipping\n\n",mod.ArchiveName));
                }
                else
                {
                    // Unzip the archive
                    proceed = UnzipFilesRecursively(modPath, extractPath);

                    if (proceed)
                    {

                        modContents.AddRange(Directory.GetDirectories(extractPath));

                        foreach (string content in modContents)
                        {
                            if (mod.Files.Any(x => content.Contains(x)))
                            {
                                Console.WriteLine(Path.Combine(installPath, Path.GetFileName(content)));

                                CopyFilesRecursively(content, Path.Combine(installPath, Path.GetFileName(content)));

                            }
                        }

                        modContents.Clear();

                        modContents.AddRange(Directory.GetFiles(extractPath));

                        foreach (string content in modContents)
                        {
                            if (!skippables.Any(x => content.EndsWith(x)) && mod.Files.Any(x => content.Contains(x)))
                            {
                                Console.WriteLine(Path.Combine(installPath, Path.GetFileName(content)));

                                File.Copy(content, Path.Combine(installPath, Path.GetFileName(content)), true);

                            }
                        }

                        try
                        {
                            Directory.Delete(extractPath, true);
                        }
                        catch (System.Exception Ex)
                        {
                            Console.WriteLine(string.Format("Cannot delete {0} because {1}", extractPath, Ex.Data));
                        }

                        Console.WriteLine("\n\n");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Skipping\n\n");
                    }
                }               

            }

            //JObject fileDump = JObject.Parse(File.ReadAllText(@"..\..\..\TarotDeck.json"));

            Console.WriteLine("Hello World!");
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                string destPath = newPath.Replace(sourcePath, targetPath);
                File.Copy(newPath, destPath, true);
            }
        }

        private static bool UnzipFilesRecursively(string sourcePath, string targetPath)
        {
            bool proceed;
            List<string> files = new List<string>();

            proceed = UnzipFile(sourcePath, targetPath);

            sourcePath = targetPath;

            if (!Directory.GetFiles(sourcePath).Any(x => x.EndsWith(".7z")) &&
                Directory.GetDirectories(sourcePath).Count() < 1)
            {
                return proceed;
            }
            else
            {

            }

            foreach (string zip in Directory.GetFiles(sourcePath).Where(x => x.EndsWith(".7z")))
            {
                proceed = proceed & UnzipFile(sourcePath,Path.Combine(sourcePath,Path.GetFileName(zip)));
            }


























            bool end = false;
            while(!end)
            {
                if (Directory.GetDirectories(sourcePath).Count() > 0)
                {
                    foreach (string dir in Directory.GetDirectories(sourcePath))
                    {
                        files.AddRange(Directory.GetFiles(dir).Where(x => x.EndsWith(".7z")));
                    }
                }
                else
                {
                    end = true;
                }
            }

            files.AddRange(Directory.GetFiles(targetPath).Where(x => x.EndsWith(".7z")));
            
            foreach (string zip in files)
            {
                proceed = proceed & UnzipFile(zip, Path.Combine(targetPath,Path.GetFileName(zip)));
            }

            return proceed;
        }

        private static bool UnzipFile(string sourcePath, string targetPath)
        {
            bool proceed = true;
            string zPath = "7za.exe"; //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = zPath;
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourcePath, targetPath);
                Process x = Process.Start(pro);
                x.WaitForExit();

                if (x.ExitCode == 2)
                {
                    proceed = false;
                }
            }
            catch (System.Exception Ex)
            {
                Console.WriteLine(Ex);
                proceed = false;
            }

            return proceed;
        }
        
    }
}
