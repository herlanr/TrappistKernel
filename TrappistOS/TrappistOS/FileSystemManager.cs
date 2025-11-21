using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool listFiles()
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
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine(fileInfo.Name);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public string getFullPath(string filename)
        {
            string formatedFileName = filename.Replace("/", @"\");
            string path = Path.Combine(currentDir, formatedFileName);
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return null;
            }
            return path.ToLower();
        }

        public string[] getAllPaths(string path)
        {
            var result = new List<string>();
            if (Directory.Exists(path))
            {
                result.Add(path);
                string[] subdirs = Directory.GetDirectories(path);
                result.AddRange(subdirs);
                string[] files = Directory.GetFiles(path);
                result.AddRange(files);
                foreach (string dir in subdirs)
                {
                    result.AddRange(getAllPaths(dir));
                }
            }

            if (File.Exists(path))
            {
                result.Add(path);
            }

            return result.ToArray();
        }

        public string createFile(string filename)
        {
            string path = Path.Combine(currentDir, filename);

            if (filename.Length > 8)
            {
                Console.WriteLine("File or Directory name can't be longer than 8 characters");
                return null;
            }

            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    FileStream newfile = File.Create(path);



                    Console.WriteLine("File successfully created: " + path);
                    return path;
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine("There is already a file or directory with this name.");
                        return null;
                    } else
                    {
                        Console.WriteLine("File already exist: " + path);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating file: " + e.Message);
                return null;
            }
        }

        public string createDirectory(string dirName)
        {
            string path = currentDir + dirName;

            try
            {
                if (!Directory.Exists(path) && !File.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine("Directory successfully created: " + path);
                    return path;
                }
                else
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine("There is already a file or directory with this name.");
                        return null;

                    } else
                    {
                        Console.WriteLine("Directory already exist or " + path);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating directory: " + e.Message);
                return null;
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

        public bool renameFileOrDir(string filename, string newName)
        {

            if (newName.Length > 8)
            {
                Console.WriteLine("File or Directory name can't be longer than 8 characters");
                return false;
            }

            string formatedFileName = filename.Replace("/", @"\");
            string path = filename.StartsWith(@"0:\") ? formatedFileName : Path.Combine(currentDir, formatedFileName);

            try
            {
                if (currentDir.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Renaming the current directory or its ancestors is not allowed.");
                    return false; 
                }

                if (File.Exists(path))
                {
                    fs.GetFile(path).SetName(newName);
                    Console.WriteLine("File renamed!");
                    return true;
                }
                else if (Directory.Exists(path))
                {
                    fs.GetDirectory(path).SetName(newName);
                    Console.WriteLine("Directory renamed!");
                    return true;
                }
                else
                {
                    Console.WriteLine("File or Dir wasn't found!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public string getCurrentDir()
        {
            return currentDir;
        }

    }
}
