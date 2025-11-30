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
        string rootdir = @"0:\";
        Stack<string> dirHistory = new Stack<string>();

        public void fsInitialize()
        {
            fs = new CosmosVFS();
            Cosmos.System.FileSystem.VFS.VFSManager.RegisterVFS(fs);
        }

        public void showFreeSpace()
        {
            var available_space = fs.GetAvailableFreeSpace(rootdir);
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
            filename = filename.Replace('/','\\');
            string path = Path.Combine(currentDir, filename);

            int dotlocation = -1;
            dotlocation = path.IndexOf('.');
            while(dotlocation != path.IndexOf("..") && dotlocation != path.IndexOf("..")+1)
            {
                path.Remove(dotlocation);
                dotlocation = path.IndexOf('.', dotlocation+1);
            }
            

            int backLocation = 0;       // .. resolution
            int preLocation = 0;
            backLocation = path.IndexOf(@"..");
            while (backLocation != -1)
            {
                while (path.IndexOf(@"\", preLocation+1)< backLocation-1 && path.IndexOf(@"\", preLocation + 1) != -1)
                {
                    preLocation = path.IndexOf(@"\", preLocation+1);
                }
                if (preLocation == -1 || preLocation == 0)
                {
                    //Console.WriteLine("You are already in the home directory");
                    return null;
                }
                path = path.Remove(preLocation+1, (backLocation - preLocation) + 1);
                backLocation = path.IndexOf(@"..");
            }
            while (path.Contains(@"\\"))
            {
                path = path.Replace(@"\\", @"\");
            }
            if (path.EndsWith(@"\") && path != rootdir)
            {
                path = path.Remove(path.Length - 1);
            }
            return path;
        }

        public string[] getAllPaths(string path)
        {
            //Console.WriteLine($"looking at: {path}");
            var result = new List<string>();
            if (Directory.Exists(path))
            {
                result.Add(getFullPath(path));
                string[] subdirs = Directory.GetDirectories(path);
                
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    //Console.WriteLine($"adding file: {Path.Combine(path, file)}");
                    result.Add(getFullPath(Path.Combine(path, file)));
                }
                foreach (string dir in subdirs)
                {
                   // Console.WriteLine($"adding directory: {Path.Combine(path, dir)}");
                    result.AddRange(getAllPaths(Path.Combine(path,dir)));
                }
            }

            if (File.Exists(path))
            {
                result.Add(getFullPath(path));
            }

            return result.ToArray();
        }

        public string createFile(string filename)
        {
            string path = getFullPath(filename);

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
            string path = getFullPath(dirName);
            Console.WriteLine(path);
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
            string path = getFullPath(name);

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

            string path = getFullPath(name);

            try
            {
                if (Directory.Exists(path))
                {
                    if (currentDir.Contains(path))
                    {
                        Console.WriteLine("You cannot delete a Parent Directory");
                        return false;
                    }
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
            string path = getFullPath(filename);

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
            /*
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
            */
            string path = getFullPath(newDir);

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
        
        public string moveFile(string filename, string dest)
        {

            string formatedFileName = filename.Replace("/", @"\");
            string formatedDestName = dest.Replace("/", @"\");

            string filePath = getFullPath(filename);
            string newPath = getFullPath(dest);

            if (File.Exists(filePath) && Directory.Exists(newPath))
                {
                    try
                    {
                        File.Copy(filePath, newPath);
                        File.Delete(filePath);
                        Console.WriteLine("File: " + filePath + " moved to " + newPath);
                        return Path.Combine(newPath, filename);
                    } catch (Exception ex) 
                    {
                        Console.WriteLine(ex.ToString());
                        return null;
                }
            } 
                else 
                {
                    Console.WriteLine("File or Dir doesn't exist. File: " + filePath + ". New Path: " + newPath);
                    return null;
                }

        }

        public string renameFileOrDir(string filename, string newName)
        {

            if (newName.Length > 8)
            {
                Console.WriteLine("File or Directory name can't be longer than 8 characters");
                return null;
            }
            string path = getFullPath(filename);

            try
            {
                if (currentDir.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Renaming the current directory or its ancestors is not allowed.");
                    return null; 
                }

                if (File.Exists(path))
                {
                    fs.GetFile(path).SetName(newName);
                    Console.WriteLine("File renamed!");
                    int index = path.LastIndexOf(@"\");
                    return Path.Combine(path.Substring(0,index+1),newName);
                }
                else if (Directory.Exists(path))
                {
                    fs.GetDirectory(path).SetName(newName);
                    Console.WriteLine("Directory renamed!");
                    int index = path.LastIndexOf(@"\");
                    return Path.Combine(path.Substring(0, index + 1), newName);
                }
                else
                {
                    Console.WriteLine("File or Dir wasn't found!");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public string getCurrentDir()
        {
            return currentDir;
        }

    }
}
