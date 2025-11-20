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
            public int[] reader;
            public int[] writer;
            public FileRights(int iowner, int[] ireader, int[] iwriter) 
            {
                owner = iowner;
                reader = ireader;
                writer = iwriter;
            }
        }

        private int visitorID;
        Hashtable fileRightTable;
        string filepath = @"0:\filePerm";
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
                
                if (!fileRightTable.ContainsKey(path) || ((FileRights)fileRightTable[path]).owner != 0)
                {
                    //Console.WriteLine(path);
                    //Cosmos.HAL.Global.PIT.Wait((uint)3000);
                    int[] system = { 0 };
                    FileRights SystemFile = new FileRights(system[0], system, system);
                    fileRightTable.Add(path,SystemFile);  
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

        public bool InitPermissions(string path, int userID)
        {
            try
            {
                FileRights SystemFile = new FileRights(userID,new[] { userID }, new[] { userID });
                fileRightTable.Add(path, SystemFile);
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("Error when creating filepermissions " + e.Message);
                return false; 
            };
        }

        public bool SetWriter(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            else
            {
                ((FileRights)fileRightTable[filepath]).writer.Append(userID);
                return true;
            }
        }

        public bool SetReader(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false;
            }
            else
            {
                ((FileRights)fileRightTable[filepath]).reader.Append(userID);
                return true;
            }
        }

        public bool SetOwner(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false;
            }
            else
            {
                ((FileRights)fileRightTable[filepath]).owner = userID;
                return true;
            }
        }

        public bool IsOwner(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            { 
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator."); 
                return false; 
            }
            else
            {
                if(((FileRights)fileRightTable[filepath]).owner == userID)
                { return true; }
                return false;
            }
        }

        public bool IsReader(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            else
            {
                if (((FileRights)fileRightTable[filepath]).reader.Contains(userID))
                { return true; }
                return false;
            }
        }

        public bool IsWriter(string path, int userID)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return false;
            }
            else
            {
                if (((FileRights)fileRightTable[filepath]).writer.Contains(userID))
                { return true; }
                return false;
            }
        }

        public int GetOwnerID(string path)
        {
            if (!fileRightTable.ContainsKey(path))
            {
                Console.WriteLine("unknown permissions, creating new");
                fileRightTable.Add(path,new FileRights(visitorID, new[] { visitorID}, new[] { visitorID }));
                return visitorID;
            }
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return 0;
            }
            else
            {
                return ((FileRights)fileRightTable[path]).owner;
            }
        }

        public int[] GetReaderIDs(string path)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                int[] system = { 0 };
                return system;
            }
            else
            {
                return ((FileRights)fileRightTable[filepath]).reader;
            }
        }

        public int[] GetWriterIDs(string path)
        {
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                int[] system = { 0 };
                return system;
            }
            else
            {
                return ((FileRights)fileRightTable[filepath]).writer;
            }
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
                    int[] writers = ((FileRights)file.Value).writer;
                    int[] readers = ((FileRights)file.Value).reader;
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
