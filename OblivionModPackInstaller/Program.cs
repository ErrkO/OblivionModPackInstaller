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
            Stopwatch timer = Stopwatch.StartNew();

            // List of file types we don't want
            List<string> skippables = new List<string>()
            {
                 ".rtf"
                ,".pdf"
                ,".doc"
                ,".html"
                //,".txt"
            };

            //string modList = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\ModList.txt");
            string modList = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\ModList2.txt");

            List<ModPack> mods = OblivionRemasteredParser.ParseDoc(modList);

            //string oblivionPath = @"F:\SteamLibrary\steamapps\common\";
            string oblivionPath = @"F:\Users\erico\Downloads\Oblivion Mods\Common2";
            string oblivionMods = @"F:\Users\erico\Downloads\Oblivion Mods";
            string modPath = "";
            string installPath;
            string extractPath;

            List<string> modArchives = new List<string>();
            List<string> modContents = new List<string>();
            List<ModPack> modErrors = new List<ModPack>();
            modArchives.AddRange(Directory.GetFiles(oblivionMods));

            bool proceed;
            int counter = 0;
            
            foreach (ModPack mod in mods)
            {
                proceed = true;
                counter++;

                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(string.Format("Starting Mod #{0} Name: {1}\n", counter,mod.ModName));

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
                else if (mod.ModName == "Ivellon 1.8")
                {
                    Console.Write("");
                }
                else if (mod.ModName == "Oblivion Uncut vX (6/20)")
                {
                    Console.Write("");
                }
                else if (mod.ModName == "Dibella's Watch")
                {
                    Console.Write("");
                }
                else if (mod.ArchiveName == "ATE181_1_beta-44146-1beta.zip")
                {
                    Console.Write("");
                }
                else if (mod.ModName == "Leviathan Soulgems v1.0")
                {
                    Console.Write("");
                }

                // Path we are installing too
                installPath = Path.Combine(oblivionPath, mod.InstallLocation);
                // Path we are extracting the archive too
                extractPath = Path.Combine(oblivionMods, Path.GetFileNameWithoutExtension(mod.ArchiveName));
                string tempFileName;
                int filesCopied = 0;
                bool isMatched;

                // if we can't find the downloaded archive at the specified
                // location, print an error message and move on to the next
                // mod
                if (modPath == "")
                {
                    Console.WriteLine(string.Format("System did not find archive ({0}) Skipping\n\n",mod.ArchiveName));
                    modErrors.Add(mod);
                }
                else
                {
                    // Unzip the archive
                    proceed = UnzipFile(modPath, extractPath);

                    if (!proceed)
                    {
                        Console.WriteLine("ERROR: Skipping\n\n");
                        modErrors.Add(mod);
                    }
                    else
                    {
                        modContents = new List<string>();
                        modContents.AddRange(Directory.GetDirectories(extractPath));
                        modContents.AddRange(Directory.GetFiles(extractPath));

                        foreach (string file in mod.Files)
                        {
                            isMatched = false;

                            if (file.Contains(","))
                            {
                                try
                                {
                                    modContents.AddRange(Directory.GetDirectories(Path.Combine(extractPath, file.Split(",")[0]).Replace("?","-")));
                                }
                                catch (Exception Ex)
                                {
                                    Console.WriteLine(string.Format("ERROR: Cannot add location(s) because {0}",Ex.Message));
                                }

                                try
                                {
                                    modContents.AddRange(Directory.GetFiles(Path.Combine(extractPath, file.Split(",")[0]).Replace("?", "-")));
                                }
                                catch (Exception Ex)
                                {
                                    Console.WriteLine(string.Format("ERROR: Cannot add location(s) because {0}", Ex.Message));
                                }
                            }

                            foreach (string content in modContents)
                            {
                                // If the content has an extension in the ignoredFileTypes
                                // Skip it
                                if (!skippables.Any(x => content.EndsWith(x)))
                                {
                                    string modFile;
                                    string contentFile = "";

                                    if (file.Contains(","))
                                    {
                                        modFile = file.Replace(",", "\\");

                                        var mFiles = modFile.Split("\\");
                                        var cFiles = content.Split("\\");

                                        for (int i = 0; i < mFiles.Count(); i++)
                                        {
                                            contentFile = Path.Combine(cFiles[cFiles.Count() - 1 - i],contentFile);
                                        }

                                        //contentFile = Path.Combine(Directory.GetParent(content).Name,Path.GetFileName(content));
                                    }
                                    else
                                    {
                                        modFile = Path.GetFileName(file.Replace(",", "\\"));
                                        contentFile = Path.GetFileName(content);
                                    }
                                    
                                    if (contentFile.ToLower() == modFile.ToLower())
                                    {
                                        if (file.Contains(","))
                                        {
                                            tempFileName = Path.GetFileName(modFile);
                                        }
                                        else
                                        {
                                            tempFileName = Path.GetFileName(contentFile);
                                        }

                                        Console.WriteLine(string.Format("\nCOPY: {0}", content));

                                        // Copy the directory
                                        if (Path.GetExtension(tempFileName) == "")
                                        {
                                            try
                                            {
                                                CopyFilesRecursively(content, Path.Combine(installPath, tempFileName));
                                                filesCopied++;
                                                Console.WriteLine(string.Format("SUCCESSS: copied to {0}", Path.Combine(installPath, tempFileName)));
                                            }
                                            catch (Exception Ex)
                                            {
                                                Console.WriteLine(string.Format("FAILURE: {0}", Ex.Message));
                                            }
                                        } // Copy the file
                                        else
                                        {
                                            try
                                            {
                                                File.Copy(content, Path.Combine(installPath, tempFileName), true);
                                                Console.WriteLine(string.Format("SUCCESSS: copied to {0}", Path.Combine(installPath, tempFileName)));
                                                filesCopied++;
                                            }
                                            catch (Exception Ex)
                                            {
                                                Console.WriteLine(string.Format("FAILURE: {0}", Ex.Message));
                                            }
                                        }

                                        isMatched = true;
                                        break;
                                    }
                                }
                            }

                            if (!isMatched)
                            {
                                Console.WriteLine(string.Format("ERROR: Could not match file {0}", file));
                                modErrors.Add(mod);
                            }
                        }

                        Console.WriteLine(string.Format("\nFiles to Copy {0}\nFiles Actually Copied {1}",mod.Files.Count(),filesCopied));

                        if (mod.Files.Count() < filesCopied)
                        {
                            modErrors.Add(mod);
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
                }
            }

            if (modErrors.Count() > 0)
            {
                Console.WriteLine("\n\n-----------------------------------\nErrors\n-----------------------------------\n");
                
                foreach (ModPack mod in modErrors)
                {
                    Console.WriteLine(string.Format("Error with {0}", mod.ModName));
                }
            }

            timer.Stop();
            TimeSpan timespan = timer.Elapsed;

            Console.WriteLine(string.Format("Elapsed Time: {0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));

            //JObject fileDump = JObject.Parse(File.ReadAllText(@"..\..\..\TarotDeck.json"));
            Console.Write("\nPress any Key to close...");
            Console.ReadKey();
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

        private static bool UnzipFilesRecursively(string sourcePath, string targetPath,bool proceed)
        {
            List<string> files = new List<string>();
            List<string> archives = new List<string>()
            {
                 ".zip"
                ,".7z"
                ,".rar"
            };

            if (archives.Any(x => Path.GetExtension(sourcePath) == x))
            {
                proceed = UnzipFile(sourcePath, targetPath);
            }

            sourcePath = targetPath;

            if (!proceed)
            {
                return proceed;
            }
            else if (!Directory.GetFiles(sourcePath).Any(x => x.EndsWith(".7z")) &&
                Directory.GetDirectories(sourcePath).Count() < 1)
            {
                return proceed;
            }
            else
            {
                foreach (string zip in Directory.GetFiles(sourcePath).Where(x => x.EndsWith(".7z")))
                {
                    proceed = proceed & UnzipFile(sourcePath, Path.Combine(sourcePath, Path.GetFileNameWithoutExtension(zip)));

                    try
                    {
                        Directory.Delete(sourcePath, true);
                    }
                    catch (System.Exception Ex)
                    {
                        Console.WriteLine(string.Format("Cannot delete {0} because {1}", sourcePath, Ex.Data));
                    }

                }

                foreach (string dir in Directory.GetDirectories(sourcePath))
                {
                    proceed = proceed & UnzipFilesRecursively(dir,Path.Combine(targetPath,dir),proceed);
                }
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
