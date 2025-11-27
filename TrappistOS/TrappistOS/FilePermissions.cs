using Cosmos.System.FileSystem.VFS;
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
            Array.Resize(ref requiredSystemPaths, requiredSystemPaths.Length + 1); //array.addpend geht nicht, in zukunft Liste mehr nutzen.
            requiredSystemPaths[requiredSystemPaths.Length - 1] = filepath;

            fileRightTable = new Hashtable();
            if (!File.Exists(filepath))
            {
                try
                {
                    File.Create(filepath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in File Permission loading:" + e.Message + "\nPlease restart your machine with the \"force-shutdown\" or \"force-reboot\" Option");
                    Cosmos.HAL.Global.PIT.Wait((uint)3000); //Output normally gets cleared right after putting it out;
                    return false;
                }
            }

            string[] filePermissions = File.ReadAllLines(filepath);

            foreach (string line in filePermissions)
            {
                string[] permissionDetails = line.Split(' ');
                if (permissionDetails.Length < 4) //Structure: [ownerID] [ReaderID1,ReaderID2....] [WriterID1,WriterID2,...] [path]
                {
                    Console.WriteLine(line + "too short");
                    continue; 
                }

                if (permissionDetails.Length > 4) //handle spaces in Paths
                {
                    for (int i = 4; i < permissionDetails.Length; i++)
                    {
                        permissionDetails[3] = permissionDetails[3] + " " + permissionDetails[i];
                    }
                }

                int owner = 0;
                if (int.TryParse(permissionDetails[0], out owner)){ } //try to get int from ownerID
                else 
                {
                    Console.WriteLine(line + " owner not int " + permissionDetails[0]);
                    continue; 
                }

                int[] readRights = Array.Empty<int>();
                string[] readerList = permissionDetails[1].Split(","); //Split readerlist into array to use
                foreach (string reader in readerList)
                {
                    Array.Resize(ref readRights, readRights.Length + 1);    //convert string array to int array
                    if (int.TryParse(reader, out readRights[readRights.Length-1])) { }
                    else 
                    {
                        Console.WriteLine(line + " reader not int: " + reader);
                        continue; 
                    }
                }

                int[] writeRights = Array.Empty<int>();
                string[] writerList = permissionDetails[2].Split(","); //Split Writerlist into array to use
                foreach (string writer in writerList)
                {
                    Array.Resize(ref writeRights, writeRights.Length + 1); //convert string array to int array
                    if (int.TryParse(writer, out writeRights[writeRights.Length-1])) { }
                    else 
                    {
                        Console.WriteLine(line + " writer not int: " + writer);
                        continue; 
                    }
                }

                FileRights currentFileRights = new FileRights(owner,readRights,writeRights); //create new object with got data

                try
                {
                    fileRightTable.Add(permissionDetails[3].ToLower(), currentFileRights); //Add new object to hastable for quick access
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message + " when loading permissions for file: " + permissionDetails[3] + ".\nPermissions for that File might not be correct.");
                    Cosmos.HAL.Global.PIT.Wait((uint)3000);
                }
            }
            //Cosmos.HAL.Global.PIT.Wait((uint)5000);
            foreach (string path in requiredSystemPaths) //Set Systemfiles to be owned by System
            {
                
                if (!fileRightTable.ContainsKey(path.ToLower()))
                {
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

            visitorID = user.visitorid; //Set root to be owned by Visitor, needed for consistent recursion ending
            if (!fileRightTable.ContainsKey(rootdir))
            {
                int[] visitor = { visitorID };
                FileRights rootFile = new FileRights(visitor[0], visitor, visitor);
                fileRightTable.Add(rootdir, rootFile);
            }
            cleanup();
            //Cosmos.HAL.Global.PIT.Wait((uint)10000);
            return true;
        }

        public bool cleanup()
        {
            try
            {
                List<string> pathsToRemove = new List<string>();
                foreach (string key in fileRightTable.Keys)
                {
                    if (!File.Exists(key) && !Directory.Exists(key))
                    {
                        pathsToRemove.Add(key);
                    }
                }
                foreach (string key in pathsToRemove)
                { fileRightTable.Remove(key); }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        //Init permissions without User to initialize to
        public bool InitPermissions(string path, bool overwrite = false, bool shouldsave = true)
        {
            try
            {
                if (path is null)
                {
                    return false;
                }

                
                string lowerpath = path.ToLower(); //hashtable only works with lowercase paths
                
                if (!fileRightTable.ContainsKey(lowerpath)|| overwrite) //check if path already exists in Hashtable, if it does, it can't be initialized or it is overwritten
                {
                    if (fileRightTable.ContainsKey(lowerpath)) //deletion in case of overwrite
                    {
                        if (((FileRights)fileRightTable[filepath]).owner == 0)
                        {
                            return false;
                        }
                        fileRightTable.Remove(lowerpath);
                    }

                    

                    //standard: initialize to visitor
                    FileRights SystemFile = new FileRights(visitorID, new[] { visitorID }, new[] { visitorID });

                    //if path is root, end here
                    if (path == rootdir) 
                    {
                        fileRightTable.Add(path.ToLower(), SystemFile);
                        if (shouldsave)
                        {
                            AppendPermission(path);
                        }
                        return true;
                    }
                    //if parent directory somehow doesn't exist, end here
                    if (Directory.GetParent(path) == null) 
                    {
                        fileRightTable.Add(path.ToLower(), SystemFile);
                        if (shouldsave)
                        {
                            AppendPermission(path);
                        }

                        return true;
                    }

                    //if parent directory exists and is in database, use the permissions from that
                    if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName.ToLower())) 
                    {
                        SystemFile = (FileRights)fileRightTable[(Directory.GetParent(path)).FullName.ToLower()];
                    }

                    //if parent directory exists, but isn't in the database, call this function with the parent directory.
                    //-> recursion until root or first directory with permissions
                    else
                    {
                        if (InitPermissions((Directory.GetParent(path)).FullName))
                        {
                            SystemFile = (FileRights)fileRightTable[(Directory.GetParent(path)).FullName.ToLower()];
                        }
                        else return false;
                    }

                    //finally, add new directory with rights to database
                    fileRightTable.Add(path.ToLower(), SystemFile);
                    if (shouldsave)
                    {
                        AppendPermission(path);
                    }
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


        public bool removePath(string path)
        {
            try
            {
                fileRightTable.Remove(path.ToLower());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        //init permissions to specified user
        public bool InitPermissions(string path, int userID, bool overwrite = false, bool shouldsave = true)
        {
            try
            {
                //if empty path, stop here
                if (path is null) 
                {
                    return false;
                }

                //if path is not valid, end here
                if (!Directory.Exists(path) && !File.Exists(path))
                {
                    return false;
                }

                FileRights SystemFile;

                //if database doesn'#'t know this file, make new object
                if (!fileRightTable.ContainsKey(path.ToLower())) 
                {
                    SystemFile = new FileRights(userID, new[] { userID }, new[] { userID });
                }
                else
                {
                    //if path exists and database knows this path, check if overwrite
                    //if overwrite -> delete path from database and make new object
                    if (overwrite)
                    {
                        fileRightTable.Remove(path.ToLower());
                        SystemFile = new FileRights(userID, new[] { userID }, new[] { userID });
                    }
                    else
                    {
                        return false;
                    }
                        
                }

                //add to database
                fileRightTable.Add(path.ToLower(), SystemFile);
                if (shouldsave)
                {
                    AppendPermission(path);
                }
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error when creating filepermissions " + e.Message);
                return false;
            };
        }

        public bool SetWriter(string path, int userID, bool quiet = false)
        {
            //valid path check
            if (!PathValidation(path,quiet))
            {
                return false;
            }

            if (path == rootdir)
            {
                if(!quiet)
                {
                    Console.WriteLine("You cannot change permissions for the root directory");
                }
                
                return false;
            }

            //Only add to permissions if it isn't already in there
            if (!((FileRights)fileRightTable[path.ToLower()]).writer.Contains(userID))
            {
                ((FileRights)fileRightTable[path.ToLower()]).writer.Add(userID);
            }

            //if Writer permission is given, the person has to be able to view this file. Thus, he gets reading permission on the parent directory.
            if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName.ToLower()))
            {
                if(!IsReader(Directory.GetParent(path).FullName.ToLower(), userID))
                {
                    SetReader(Directory.GetParent(path).FullName.ToLower(), userID);
                }
            }
            else
            {
                if (!quiet)
                {
                    Console.WriteLine($"parent directory {Directory.GetParent(path).FullName} has no rights, creating new...");
                }
                InitPermissions(Directory.GetParent(path).FullName);
                if (!IsReader(Directory.GetParent(path).FullName, userID))
                {
                    SetReader(Directory.GetParent(path).FullName, userID);
                }
            }
            return true;
        }

        public bool SetReader(string path, int userID, bool quiet = false)
        {
            //valid path check
            if (!PathValidation(path,quiet))
            {
                return false;
            }

            if (path == rootdir)
            {
                if (!quiet)
                {
                    Console.WriteLine("You cannot change permissions for the root directory");
                }
                return false;
            }

            //check if the user already has reading rights to avoid duplications
            if (!((FileRights)fileRightTable[path.ToLower()]).reader.Contains(userID))
            {
                ((FileRights)fileRightTable[path.ToLower()]).reader.Add(userID);
            }

            //if user is able to read file or directory, he hs to be able to read the parent directory
            if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName.ToLower()))
            {
                if (!IsReader(Directory.GetParent(path).FullName.ToLower(), userID))
                {
                    SetReader(Directory.GetParent(path).FullName.ToLower(), userID);
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

        //remove writer rights from file
        public bool RemoveWriter(string path, int userID,bool quiet = false)
        {
            //valid path check
            if (!PathValidation(path, quiet))
            {
                return false;
            }


            if (path == rootdir)
            {
                if (!quiet)
                {
                    Console.WriteLine("You cannot change permissions for the root directory");
                }
                return false;
            }

            ((FileRights)fileRightTable[path.ToLower()]).writer.Remove(userID);
            return true;
        }

        //remove reader rights from file with userinterface for dictionaries
        public bool RemoveReader(string path, int userID, string username, FileSystemManager fsManager, bool quiet = false)
        {
            //valid path check
            if (!PathValidation(path, quiet))
            {
                return false;
            }


            if (path == rootdir)
            {
                if (!quiet)
                {
                    Console.WriteLine("You cannot change permissions for the root directory");
                }
                return false;
            }

            //if directory, check if anything below needs rights
            if (Directory.Exists(path)) 
            {

                List<string> pathsToCheck = fsManager.getAllPaths(path).ToList();
                
                //value if user needs to be asked to confirm
                bool confirm = false;
                int rmindex = 0;
                for (int i = 0; i < pathsToCheck.Count; i++)
                {
                    pathsToCheck[i] = pathsToCheck[i].ToLower();
                    if (pathsToCheck[i] == fsManager.getFullPath(path).ToLower())
                    {
                        rmindex = i;
                    }
                }

                pathsToCheck.RemoveAt(rmindex);

                //check if user is owner or reader of any of the files below (writer alone shouldn't be possible and is as such ignored)
                foreach (string potentialpath in pathsToCheck) {
                    if(IsReader(fsManager.getFullPath(potentialpath), userID) || IsOwner(potentialpath,userID))
                    {
                        confirm = true; break; 
                    }
                }

                //if user has to confirm, something has been found
                if (confirm)
                {
                    if (username!="system" && !quiet)
                    {
                        Console.WriteLine("There are files or Directories within this " + path + " that " + username + " has access to.\nAre you sure you want to remove his access?\n(y)es/(n)o");
                    }

                    

                    //If user agrees go through every path do remove permissions if they exist.
                    if (Kernel.WaitForConfirmation() || username == "system" || quiet)
                    {
                        foreach (string potentialpath in pathsToCheck)
                        {
                            if (IsWriter(potentialpath, userID))
                            {
                                RemoveWriter(potentialpath, userID);
                            }
                            internalRemoveReader(potentialpath, userID);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Deletion Aborted.");
                        return false;
                    }
                }
            }

            //remove reading permission from file or dir
            ((FileRights)fileRightTable[path.ToLower()]).reader.Remove(userID);


            //if user or visitor is the owner, there is no need to look if parent rights have to be removed as he need access
            if (IsOwner(path, userID)||IsOwner(path,visitorID)) { return true; }

            //if user or visitor is writer in parent dir, there is no need to look up parent rights as he needs read access
            if (IsWriter(Directory.GetParent(path).FullName, userID) || IsWriter(Directory.GetParent(path).FullName, visitorID)) { return true; }

            //get all subdiretories of Parent, if he is reader in 1, removing reading rights is wrong
            var subdirectories = Directory.GetDirectories(Directory.GetParent(path).FullName); //check if rights are still needed somewhere
            foreach (var subdirectory in subdirectories)
            {
                if(IsReader(Path.Combine(Directory.GetParent(path).FullName, subdirectory), userID))
                {
                    return true;
                }
            }

            //same for files
            var files = Directory.GetFiles(Directory.GetParent(path).FullName);
            foreach (var file in files)
            {
                if (IsReader(Path.Combine(Directory.GetParent(path).FullName, file), userID))
                {
                    return true;
                }
            }

            //remove parent reading permissions if none of the above applies
            internalRemoveReader(Directory.GetParent(path).FullName, userID);

            return true;
        }

        //simplified remover with less protections
        private bool internalRemoveReader(string path,int userID)
        {
            if (IsReader(path, userID))
            {
                ((FileRights)fileRightTable[path.ToLower()]).reader.Remove(userID);
                return true;
            }
            else
                { return false; }
            
        }


        public bool SetOwner(string path, int userID, bool quiet = false)
        {
            if (!PathValidation(path, quiet))
            {
                return false;
            }


            if (path == rootdir)
            {
                if (!quiet)
                {
                    Console.WriteLine("You cannot change permissions for the root directory");
                }
                return false;
            }

            //check if previous owner stil needs rights access
            int prevOwner = ((FileRights)fileRightTable[path.ToLower()]).owner;

            if (prevOwner == userID)
            {
                return true;
            }

            //check if the previous owner should get reding rights on directory removed
            if(!IsOwner(Directory.GetParent(path).FullName.ToLower(), prevOwner) && !IsWriter(Directory.GetParent(path).FullName.ToLower(), prevOwner))
            {
                bool needsRight = false;
                //get all subdiretories of Parent, if he is reader in 1, removing reading rights is wrong
                var subdirectories = Directory.GetDirectories(Directory.GetParent(path).FullName);

                //check if previous owner has rights in subdirectories
                foreach (var subdirectory in subdirectories)
                {
                    if (IsReader(Path.Combine(path, subdirectory), userID))
                    {
                        needsRight = true;
                    }
                }

                //same for files
                var files = Directory.GetFiles(Directory.GetParent(path).FullName);
                foreach (var file in files)
                {
                    if (IsReader(Path.Combine(path, file), userID))
                    {
                        needsRight = true;
                    }
                }

                if(!needsRight)
                {
                    internalRemoveReader(Directory.GetParent(path).FullName.ToLower(),prevOwner);
                }
            }



            //set owner
            ((FileRights)fileRightTable[path.ToLower()]).owner = userID;

            //he is no set the owner, he needs read access
            if (fileRightTable.ContainsKey((Directory.GetParent(path)).FullName.ToLower()))
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

        public bool IsOwner(string path, int userID)
        {
            if(!PathValidation(path,true))
            {
                return false;
            }

            if(fileRightTable.ContainsKey(path) && ((FileRights)fileRightTable[path.ToLower()]).owner == userID && ((FileRights)fileRightTable[path.ToLower()]).owner != visitorID)
            { return true; }
            return false;
        }

        public bool IsReader(string path, int userID, bool acceptVisitor = true)
        {
            if (!PathValidation(path, true))
            {
                return false;
            }
            if (fileRightTable.ContainsKey(path) && ((FileRights)fileRightTable[path.ToLower()]).reader.Contains(userID) || ( ((FileRights)fileRightTable[path.ToLower()]).reader.Contains(visitorID) && acceptVisitor))
            { return true; }
            return false;
        }

        public bool IsWriter(string path, int userID, bool acceptVisitor = true)
        {
            if (!PathValidation(path, true))
            {
                return false;
            }
            if (fileRightTable.ContainsKey(path) && ((FileRights)fileRightTable[path.ToLower()]).writer.Contains(userID) || (((FileRights)fileRightTable[path.ToLower()]).writer.Contains(visitorID) && acceptVisitor))
            { return true; }
            return false;
        }

        public int GetOwnerID(string path)
        {
            if (!PathValidation(path, true))
            {
                return 0;
            }
            return ((FileRights)fileRightTable[path.ToLower()]).owner;
        }

        public int[] GetReaderIDs(string path)
        {
            if (!PathValidation(path, true))
            {
                return new[] { 0 };
            }
            return ((FileRights)fileRightTable[path.ToLower()]).reader.ToArray();
        }

        public bool deletePath(string path)
        {
            try
            {
                if(fileRightTable.ContainsKey(path))
                {
                    fileRightTable.Remove(path.ToLower());
                }
                return true;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return false; }
        }

        public int[] GetWriterIDs(string path)
        {
            if (!PathValidation(path, true))
            {
                return new[] { 0 };
            }
            return ((FileRights)fileRightTable[path.ToLower()]).writer.ToArray();
        }

        public bool EmptyPerms()
        {
            fileRightTable.Clear();
            return true;
        }

        public bool switchPermissionPath(string oldpath, string newPath, bool quiet = true)
        {
            if (!fileRightTable.ContainsKey(oldpath.ToLower()) || fileRightTable.ContainsKey(newPath.ToLower()))
            {
                if(!quiet)
                {
                    Console.WriteLine($"Containskey old {oldpath}: {fileRightTable.ContainsKey(oldpath.ToLower())} \nContainskey new {newPath}: {fileRightTable.ContainsKey(newPath.ToLower())}");
                }
                return false;
            }
            try
            {
                fileRightTable.Add(newPath.ToLower(), fileRightTable[oldpath.ToLower()]);
                fileRightTable.Remove(oldpath.ToLower());
                return true;
            }
            catch (Exception e)
            {
                if (!quiet)
                {
                    Console.WriteLine($"{e.Message}");
                }
                return false; 
                }
            
        }

        public bool SavePermissions()
        {
            try
            {
                //always create a new file to eliminate saving errors
                File.Create(filepath);


                string toSave = "";
                foreach (DictionaryEntry file in fileRightTable)
                {
                    int[] writers = ((FileRights)file.Value).writer.ToArray();
                    int[] readers = ((FileRights)file.Value).reader.ToArray();
                    int owner = ((FileRights)file.Value).owner;

                    //put owners first
                    toSave = toSave + Convert.ToString(owner) + ' ';


                    for (int i = 0; i < readers.Length; i++)
                    {
                        //put int list into string with , seperating them and a space at the end
                        int reader = readers[i];
                        if (i < readers.Length - 1)
                        {
                            toSave = toSave + Convert.ToString(reader) + ',';
                        }
                        else
                        {
                            toSave = toSave + Convert.ToString(reader) + ' ';
                        }
                    }

                    for (int i = 0; i < writers.Length; i++)
                    {
                        //put int list into string with , seperating them and a space at the end
                        int writer = writers[i];
                        if (i > writers.Length - 1)
                        {
                            toSave = toSave + Convert.ToString(writer) + ',';
                        }
                        else
                        {
                            toSave = toSave + Convert.ToString(writer) + ' ';
                        }
                    }
                    //add path and newline
                    toSave = toSave + file.Key.ToString() + Environment.NewLine;
                }

                //add everything at once to file
                File.AppendAllText(filepath, toSave);
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("Error Encountered: " + e.Message);
                return false; 
            }
        }

        public bool AppendPermission(string path)
        {
            try
            {
                string newPermLine = "";
                FileRights newFile = (FileRights)fileRightTable[path];
                newPermLine += newFile.owner.ToString();
                newPermLine += " " + String.Join(",", newFile.reader.ToList());
                newPermLine += " " + String.Join(",", newFile.writer.ToList());
                newPermLine += " " + path;
                File.AppendAllText(filepath, newPermLine);
                return true;
            }
            catch (Exception e) { return false; }
        }

        private bool PathValidation(string path,bool quiet = false)
        {
            //if path is null someting went very wrong
            if (path == null)
            {
                return false;
            }
            //File or dir needs to exist to work with it
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                if(!quiet)
                {
                    Console.WriteLine(path + " does not exist");
                }
                
                return false;
            }
            //init permissions for legacy files
            if (!fileRightTable.ContainsKey(path.ToLower()))
            {
                if (!quiet)
                {
                    Console.WriteLine($"unknown permissions for " + path + ", creating new");
                }
                InitPermissions(path);
            }
            //If this happens, something fundamentally broke.
            if (fileRightTable[path.ToLower()].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            return true;
        }

    }
}
