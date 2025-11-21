using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace TrappistOS
{
    internal class FileSystemManager
    {
        private CosmosVFS fs;
        string currentDir = @"0:\";
        Stack<string> dirHistory = new Stack<string>();

        public void fsInitialize()
        {
            fs = new CosmosVFS();
            Cosmos.System.FileSystem.VFS.VFSManager.RegisterVFS(fs);
        }

        public void showFreeSpace()
        {
            var available_space = fs.GetAvailableFreeSpace(@"0:\");
            Console.WriteLine("Available Free Space: " + available_space);
        }

        public void listFiles()
        {

            var dirs = Directory.GetDirectories(currentDir);
            var files = Directory.GetFiles(currentDir);

            try
            {
                foreach(var dir in dirs)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(@"\" + dirInfo.Name + @"\");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                foreach (var file in files)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    var fileInfo = new DirectoryInfo(file);
                    Console.WriteLine(fileInfo.Name);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public bool createFile(string filename)
        {
            string path = Path.Combine(currentDir, filename);

            if (filename.Length > 8)
            {
                Console.WriteLine("File or Directory name can't be longer than 8 characters");
                return false;
            }

            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    File.Create(path);
                    Console.WriteLine("File successfully created: " + path);
                    return true;
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine("There is already a file or directory with this name.");
                        return false;
                    } else
                    {
                        Console.WriteLine("File already exist: " + path);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating file: " + e.Message);
                return false;
            }
        }

        public bool createDirectory(string dirName)
        {
            string path = currentDir + @"\" + dirName;

            try
            {
                if (!Directory.Exists(path) && !File.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine("Directory successfully created: " + path);
                    return true;
                }
                else
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine("There is already a file or directory with this name.");
                        return false;

                    } else
                    {
                        Console.WriteLine("Directory already exist or " + path);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating directory: " + e.Message);
                return false;
            }
        }

        public bool deleteFile(string name)
        {
            string path = Path.Combine(currentDir, name);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Console.WriteLine("File " + path + " deleted");
                    return true;
                }

                else
                {
                    Console.WriteLine("File doesn't exist");
                    return false;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public bool deleteDir(string name)    
        {

            string path = Path.Combine(currentDir, name);

            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                    Console.WriteLine("Directory " + path + " deleted");
                    return true;
                }

                else 
                {
                    Console.WriteLine("Directory doesn't exist");
                    return false;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public bool readFromFile(String filename)
        {
            string path = currentDir + @"\" + filename;

            try
            {
                Console.WriteLine(File.ReadAllText(path));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;

            }
        }

        public bool changeDirectory(string newDir)
        {

            if(newDir == "..")
            {
                if (dirHistory.Count > 0) 
                {
                    currentDir = dirHistory.Pop();
                    return true;
                }
                else
                {
                    Console.WriteLine("You are on the home directory");
                    return false;
                }
            }

            string path = Path.Combine(currentDir, newDir);

            if (Directory.Exists(path))
            {
                dirHistory.Push(currentDir);
                currentDir = path;
                return true;
            }
            else
            {
                Console.WriteLine("Directory doesn't exist");
                return false;
            }

        }
        
        public bool moveFile(string filename, string dest)
        {

            string formatedFileName = filename.Replace("/", @"\");
            string formatedDestName = dest.Replace("/", @"\");

            string filePath = filename.StartsWith(@"0:\") ? formatedFileName : Path.Combine(currentDir, formatedFileName);
            string newPath = dest.StartsWith(@"0:\") ? formatedDestName : Path.Combine(@"0:\", formatedDestName);

            if (File.Exists(filePath) && Directory.Exists(newPath))
                {
                    try
                    {
                        File.Copy(filePath, newPath);
                        File.Delete(filePath);
                        Console.WriteLine("File: " + filePath + " moved to " + newPath);
                        return true;
                    } catch (Exception ex) 
                    {
                        Console.WriteLine(ex.ToString());
                        return false;
                }
            } 
                else 
                {
                    Console.WriteLine("File or Dir doesn't exist. File: " + filePath + ". New Path: " + newPath);
                    return false;
                }

        }

        public string getCurrentDir()
        {
            return currentDir;
        }

    }
}
