using Cosmos.System.Graphics.Fonts;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    internal class FilePermissions
    {
        private class FileRights
        {
            public int owner;
            public List<int> reader;
            public List<int> writer;
            public FileRights(int iowner, int[] ireader, int[] iwriter) 
            {
                owner = iowner;
                reader = ireader.ToList();
                writer = iwriter.ToList();
            }
        }


        private int visitorID;
        Hashtable fileRightTable;
        string filepath = @"0:\filePerm.sys";
        string rootdir = @"0:\";

        public bool PermInit(UserLogin user, string[] requiredSystemPaths) 
        {
            Array.Resize(ref requiredSystemPaths, requiredSystemPaths.Length + 1);
            requiredSystemPaths[requiredSystemPaths.Length - 1] = filepath;

            fileRightTable = new Hashtable(); 
            //Console.WriteLine("Checking if " + filepath + " exists");
            if (!File.Exists(filepath))
            {
                //Console.WriteLine("Trying to create File " + filepath);
                try
                {
                    File.Create(filepath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in File Permission loading:" + e.Message + "\nPlease restart your machine with the \"force-shutdown\" or \"force-reboot\" Option");
                    Cosmos.HAL.Global.PIT.Wait((uint)3000);
                    return false;
                }
            }
            string[] filePermissions = File.ReadAllLines(filepath);
            foreach (string line in filePermissions)
            {
                string[] permissionDetails = line.Split(' ');
                if (permissionDetails.Length < 4)
                { continue; }
                if (!File.Exists(permissionDetails[3]) || Directory.Exists(permissionDetails[3]))
                {
                    continue;
                }

                int owner = 0;
                if (int.TryParse(permissionDetails[0], out owner)){ }
                else { continue; }

                int[] readRights = Array.Empty<int>();
                string[] readerList = permissionDetails[1].Split(",");
                foreach (string reader in readerList)
                {
                    Array.Resize(ref readRights, readRights.Length + 1);
                    if (int.TryParse(reader, out readRights[readRights.Length-1])) { }
                    else { continue; }
                }

                int[] writeRights = Array.Empty<int>();
                string[] writerList = permissionDetails[1].Split(",");
                foreach (string writer in writerList)
                {
                    Array.Resize(ref writeRights, writeRights.Length + 1);
                    if (int.TryParse(writer, out writeRights[writeRights.Length-1])) { }
                    else { continue; }
                }

                FileRights currentFileRights = new FileRights(owner,readRights,writeRights);

                try
                {
                    fileRightTable.Add(permissionDetails[3], currentFileRights);
                }
                catch
                {
                    Console.WriteLine("Duplicate File " + permissionDetails[3] + " in filepermissions. Permissions for that File might not be correct.");
                }
            }
            foreach (string path in requiredSystemPaths)
            {
                
                if (!fileRightTable.ContainsKey(path.ToLower()))
                {
                    //Console.WriteLine(path);
                    //Cosmos.HAL.Global.PIT.Wait((uint)1000);
                    int[] system = { 0 };
                    FileRights SystemFile = new FileRights(system[0], system, system);
                    fileRightTable.Add(path.ToLower(),SystemFile);  
                }
                if (((FileRights)fileRightTable[path.ToLower()]).owner != 0 || ((FileRights)fileRightTable[path.ToLower()]).reader != new List<int>(0) || ((FileRights)fileRightTable[path.ToLower()]).writer != new List<int>(0))
                {
                    fileRightTable.Remove(path.ToLower());
                    int[] system = { 0 };
                    FileRights SystemFile = new FileRights(system[0], system, system);
                    fileRightTable.Add(path.ToLower(), SystemFile);
                }
            }
            visitorID = user.maxAdminID + user.visitorid;
            if (!fileRightTable.ContainsKey(@"0:\"))
            {
                int[] visitor = { visitorID };
                FileRights rootFile = new FileRights(visitor[0], visitor, visitor);
                fileRightTable.Add(@"0:\", rootFile);
            }

            return true;
        }

        public bool InitPermissions(string path)
        {
            try
            {
                if (path is null)
                {
                    return false;
                }
                string lowerpath = path.ToLower();
                
                if (!fileRightTable.ContainsKey(lowerpath))
                {
                    FileRights SystemFile = new FileRights(visitorID, new[] { visitorID }, new[] { visitorID });
                    //FileRights SystemFile = new FileRights(userID, new[] { userID }, new[] { userID });
                    if (path == rootdir)
                    {
                        //Console.WriteLine("Went down to root");
                    }
                    else if(Directory.GetParent(path) == null)
                    {
                        //Console.WriteLine(path + " has not parent dir and isn't root");
                    }
                    else if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName))
                    {
                        //Console.WriteLine("using rights from parent:" + path);
                        SystemFile = (FileRights)fileRightTable[(Directory.GetParent(path)).FullName];
                    }
                    else
                    {
                        //Console.WriteLine("making rights for parent: " + path);
                        if (InitPermissions((Directory.GetParent(path)).FullName))
                        {
                            SystemFile = (FileRights)fileRightTable[(Directory.GetParent(path)).FullName];
                        }
                        else return false;
                    }
                    fileRightTable.Add(path.ToLower(), SystemFile);
                    return true;
                }
                else
                    return false;
                
            }
            catch (Exception e) {
                Console.WriteLine("Error when creating filepermissions " + e.Message);
                return false; 
            };
        }

        public bool InitPermissions(string path, int userID)
        {
            try
            {
                if (path is null)
                {
                    return false;
                }
                FileRights SystemFile;
                if (!fileRightTable.ContainsKey(path.ToLower()))
                {
                    SystemFile = new FileRights(userID, new[] { userID }, new[] { userID });
                }
                else
                {
                    return false;
                }
                fileRightTable.Add(path.ToLower(), SystemFile);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error when creating filepermissions " + e.Message);
                return false;
            }
            ;
        }

        public bool SetWriter(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if(path == rootdir)
            {
                Console.WriteLine("You cannot change permissions for the root directory");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                InitPermissions(path);
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }

            if(!((FileRights)fileRightTable[path.ToLower()]).writer.Contains(userID))
            {
                ((FileRights)fileRightTable[path.ToLower()]).writer.Add(userID);
            }
            if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName))
            {
                if(!IsReader(Directory.GetParent(path).FullName,userID))
                {
                    SetReader(Directory.GetParent(path).FullName, userID);
                }
            }
            else
            {
                InitPermissions(Directory.GetParent(path).FullName, userID);
                if (!IsReader(Directory.GetParent(path).FullName, userID))
                {
                    SetReader(Directory.GetParent(path).FullName, userID);
                }
            }
            return true;
        }

        public bool SetReader(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (path == rootdir)
            {
                Console.WriteLine("You cannot change permissions for the root directory");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                InitPermissions(path);
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false;
            }

            if(!((FileRights)fileRightTable[path.ToLower()]).reader.Contains(userID))
            {
                ((FileRights)fileRightTable[path.ToLower()]).reader.Add(userID);
            }

            if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName))
            {
                if (!IsReader(Directory.GetParent(path).FullName, userID))
                {
                    SetReader(Directory.GetParent(path).FullName, userID);
                }
            }
            else
            {
                if(path == rootdir)
                {
                    InitPermissions(Directory.GetParent(path).FullName, visitorID);
                    return true;
                }
                InitPermissions(Directory.GetParent(path).FullName, userID);
                if (!IsReader(Directory.GetParent(path).FullName, userID))
                {
                    SetReader(Directory.GetParent(path).FullName, userID);
                }
            }
            return true;
        }

        public bool RemoveWriter(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (path == rootdir)
            {
                Console.WriteLine("You cannot change permissions for the root directory");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                InitPermissions(path);
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            ((FileRights)fileRightTable[path.ToLower()]).writer.Remove(userID);
            return true;
        }

        public bool RemoveReader(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (path == rootdir)
            {
                Console.WriteLine("You cannot change permissions for the root directory");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                InitPermissions(path);
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }

            ((FileRights)fileRightTable[path.ToLower()]).reader.Remove(userID);

            if (IsOwner(path, userID)||IsOwner(path,visitorID)) { return true; }

            var subdirectories = Directory.GetParent(path).GetDirectories();
            foreach (var subdirectory in subdirectories)
            {
                if(IsReader(subdirectory.FullName, userID))
                {
                    return true;
                }
            }

            var files = Directory.GetParent(path).GetDirectories();
            foreach (var file in files)
            {
                if (IsReader(file.FullName, userID))
                {
                    return true;
                }
            }

            RemoveReader(Directory.GetParent(path).FullName, userID);

            return true;
        }

        public bool SetOwner(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (path == rootdir)
            {
                Console.WriteLine("You cannot change permissions for the root directory");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false;
            }
            else
            {
                ((FileRights)fileRightTable[path.ToLower()]).owner = userID;
                if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName))
                {
                    if (!IsReader(Directory.GetParent(path).FullName, userID))
                    {
                        SetReader(Directory.GetParent(path).FullName, userID);
                    }
                }
                else
                {
                    InitPermissions(Directory.GetParent(path).FullName, userID);
                    if (!IsReader(Directory.GetParent(path).FullName, userID))
                    {
                        SetReader(Directory.GetParent(path).FullName, userID);
                    }
                }
                return true;
            }
        }

        public bool IsOwner(string path, int userID)
        {
            if(path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false; 
            }
            else
            {
                if(((FileRights)fileRightTable[path.ToLower()]).owner == userID && ((FileRights)fileRightTable[path.ToLower()]).owner != visitorID)
                { return true; }
                return false;
            }
        }

        public bool IsReader(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            else
            {
                if (((FileRights)fileRightTable[path.ToLower()]).reader.Contains(userID) || ((FileRights)fileRightTable[path.ToLower()]).reader.Contains(visitorID))
                { return true; }
                return false;
            }
        }

        public bool IsWriter(string path, int userID)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return false;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            else
            {
                if (((FileRights)fileRightTable[path.ToLower()]).writer.Contains(userID) || ((FileRights)fileRightTable[path.ToLower()]).writer.Contains(visitorID))
                { return true; }
                return false;
            }
        }

        public int GetOwnerID(string path)
        {
            if (path == null)
            {
                return 0;
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return 0;
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(),new FileRights(visitorID, new[] { visitorID}, new[] { visitorID }));
                return visitorID;
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return 0;
            }
            else
            {
                return ((FileRights)fileRightTable[path.ToLower()]).owner;
            }
        }

        public int[] GetReaderIDs(string path)
        {
            if (path == null)
            {
                return Array.Empty<int>();
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return Array.Empty<int>();
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
                return new[] { visitorID };
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                int[] system = { 0 };
                return system;
            }
            else
            {
                return ((FileRights)fileRightTable[path.ToLower()]).reader.ToArray();
            }
        }

        public bool deletePath(string path)
        {
            try
            {
                fileRightTable.Remove(path);
                return true;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return false; }
        }

        public int[] GetWriterIDs(string path)
        {
            if (path == null)
            {
                return Array.Empty<int>();
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exist");
                return Array.Empty<int>();
            }
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                Console.WriteLine($"unknown permissions for " + path + ", creating new");
                fileRightTable.Add(path.ToLower(), new FileRights(visitorID, new[] { visitorID }, new[] { visitorID }));
                return new[] { visitorID };
            }
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                int[] system = { 0 };
                return system;
            }
            else
            {
                return ((FileRights)fileRightTable[path.ToLower()]).writer.ToArray();
            }
        }

        public bool EmptyPerms()
        {
            fileRightTable.Clear();
            return true;
        }

        public bool SavePermissions()
        {
            try
            {
                //Console.WriteLine("Trying to delete File: " + filepath);
                if (File.Exists(filepath))
                {
                    //Console.WriteLine("Trying to delete existing File: " + filepath);
                    File.Delete(filepath);
                    //Console.WriteLine("Deleted file: " + filepath);
                }
                //Console.WriteLine("Trying to create file: " + filepath);
                File.Create(filepath);
                //Console.WriteLine("Created file: " + filepath);

                foreach (DictionaryEntry file in fileRightTable)
                {
                    int[] writers = ((FileRights)file.Value).writer.ToArray();
                    int[] readers = ((FileRights)file.Value).reader.ToArray();
                    int owner = ((FileRights)file.Value).owner;

                    File.AppendAllText(filepath, Convert.ToString(owner) + ' ');
                    //Console.WriteLine(" added " + Convert.ToString(owner) + ' ' + " to " + filepath);
                    for(int i = 0; i < writers.Length; i++)
                    {
                        int writer = writers[i];
                        if (i > writers.Length-1)
                        {
                            File.AppendAllText(filepath, Convert.ToString(writer) + ',');
                            //Console.WriteLine(" added " + Convert.ToString(writer) + ',' + " to " + filepath);
                        }
                        else
                        {
                            File.AppendAllText(filepath, Convert.ToString(writer) + ' ');
                            //Console.WriteLine(" added " + Convert.ToString(writer) + ' ' + " to " + filepath);
                        }
                    }
                    for (int i = 0; i < readers.Length; i++)
                    {
                        int reader = readers[i];
                        if (i < readers.Length - 1)
                        {
                            File.AppendAllText(filepath, Convert.ToString(reader) + ',');
                            //Console.WriteLine(" added " + Convert.ToString(reader) + ',' + " to " + filepath);
                        }
                        else
                        {
                            File.AppendAllText(filepath, Convert.ToString(reader) + ' ');
                            //Console.WriteLine(" added " + Convert.ToString(reader) + ' ' + " to " + filepath);
                        }
                    }
                    File.AppendAllText(filepath,file.Key.ToString() + Environment.NewLine);

                }
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("Error Encountered: " + e.Message);
                return false; 
            }
        }



    }
}
