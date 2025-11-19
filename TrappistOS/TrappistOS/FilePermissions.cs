using Cosmos.System.Graphics.Fonts;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        Hashtable fileRightTable;
        string filepath = @"0:\sysperms";
        FilePermissions() 
        {
            fileRightTable = new Hashtable();
            bool createdNew = false;
            if (!File.Exists(filepath))
            {
                createdNew = true;
                File.Create(filepath);
            }
            string[] filePermissions = File.ReadAllLines(filepath);
            foreach (string line in filePermissions)
            {
                string[] permissionDetails = line.Split(' ');

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
                    if (int.TryParse(reader, out readRights[readRights.Length])) { }
                    else { continue; }
                }

                int[] writeRights = Array.Empty<int>();
                string[] writerList = permissionDetails[1].Split(",");
                foreach (string writer in writerList)
                {
                    if (int.TryParse(writer, out writeRights[writeRights.Length])) { }
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
            if (createdNew || !fileRightTable.ContainsKey(filepath))
            {
                if (fileRightTable[filepath].GetType() != typeof(FileRights))
                { Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");}
                else
                {
                    int[] system = { 0 };
                    ((FileRights)fileRightTable[filepath]).writer = system;
                    ((FileRights)fileRightTable[filepath]).owner = system[0];
                    ((FileRights)fileRightTable[filepath]).reader = system;
                }
                    
            }
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
            if (fileRightTable[filepath].GetType() != typeof(FileRights))
            {
                Console.WriteLine("Error in file Permission Hashtable, please restart the machine. If this message appears again, please contact an Administrator.");
                return 0;
            }
            else
            {
                return ((FileRights)fileRightTable[filepath]).owner;
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
                File.Create(filepath);
                foreach (DictionaryEntry file in fileRightTable)
                {
                    int[] writers = ((FileRights)file.Value).writer;
                    int[] readers = ((FileRights)file.Value).reader;
                    int owner = ((FileRights)file.Value).owner;

                    File.AppendAllText(filepath, Convert.ToString(owner) + ' ');
                    int last = writers.Last();
                    foreach(int  writer in writers)
                    {
                        if (writer == last)
                        {
                            File.AppendAllText(filepath, Convert.ToString(writer) + ',');
                        }
                        else
                        {
                            File.AppendAllText(filepath, Convert.ToString(writer) + ' ');
                        }
                    }
                    last = readers.Last();
                    foreach (int reader in readers)
                    {
                        if (reader == last)
                        {
                            File.AppendAllText(filepath, Convert.ToString(reader) + ',');
                        }
                        else
                        {
                            File.AppendAllText(filepath, Convert.ToString(reader) + ' ');
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
