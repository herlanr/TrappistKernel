using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Sys = Cosmos.System;
using System;

namespace TrappistOS
{
    
    internal class UserLogin : ProgramClass
    {
        string filePath = "users.json";
        private UserClass currentUser = null;
        private class UserClass
        {
            public int id { get; private set; }
            public string username { get; private set; }
            public string password { get; set; }

            public UserClass(string username, int id, string password = "")
            {
                this.id = id;
                this.username = username;
                this.password = password;
            }
        }

        public void BeforeRun()
        {
            Console.WriteLine("test");
        }

        public override void Run()
        {
            Console.WriteLine("username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();
            
            var user = Login(username, password);
            
            //currentUser = user;
        }

        private UserClass Login(string name, string pw)
        {
            UserClass user = GetUser(name);
            if (user == null)
            {
                Console.WriteLine($"{name} is not registered.");
                return VisitorLogin();
            }
            if (user.password == pw)
            {
                Console.WriteLine($"Password Correct, welcome {name}");
                return user;
            }
            Console.WriteLine("Password incorrect");
            return VisitorLogin();
        }


        public int get_id()
        {
            return currentUser.id;
        }

        public string get_name()
        {
            return currentUser.username;
        }

        private UserClass VisitorLogin()
        {
            return GetUser("visitor");
        }

        private UserClass GetUser(string username)
        {
            var users = ReadJsonFromFile(filePath);
            foreach (var user in users)
            {
                if (user.username == username)
                {
                    return user;
                }
            }
            return new UserClass("Visitor", 200);
        }

        private void WriteJsonToFile(string filePath, List<UserClass> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Method to read data from a JSON file
        private List<UserClass> ReadJsonFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<UserClass>>(json);
        }
    }
}
