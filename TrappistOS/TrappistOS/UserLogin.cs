using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Sys = Cosmos.System;

namespace TrappistOS
{
    
    internal class UserLogin : ProgramClass
    {
        string filePath = "users.json";
        public User currentUser = null;
        internal class User
        {
            public int id { get; private set; }
            public string username { get; private set; }
            private string password { get; set; }

            public User(string username, int id, string password = "")
            {
                this.id = id;
                this.username = username;
                this.password = password;
            }
        }

        public override void Run()
        {
            System.Console.WriteLine("3 +4 = 7");
        }

        public int get_id()
        {
            return currentUser.id;
        }

        public string get_name()
        {
            return currentUser.username;
        }

        public void VisitorLogin()
        {
            currentUser = _GetUser("visitor");
        }

        private User _GetUser(string username)
        {
            var Users = ReadJsonFromFile(filePath);
            foreach (var User in Users)
            {
                if (User.username == username)
                {
                    return User;
                }
            }
            return new User("Visitor", 200);
        }

        private void WriteJsonToFile(string filePath, List<User> Users)
        {
            string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Method to read data from a JSON file
        private List<User> ReadJsonFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json);
        }
    }
}
