using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace TrappistOS
{
    internal class UserLogin
    {
        static string filePath = "users.json";
        static public User currentUser = null;
        internal class User
        {
            public int id { get; private set;}
            public string username { get; private set; }
            private string password { get; set; }

            public User ( string username, int id, string password = "")
            {
                this.id = id;
                this.username = username;
                this.password = password;
            }
        }

        static public int get_id()
        {
            return currentUser.id;
        }

        static public string get_name()
        {
            return currentUser.username;
        }

        static public User VisitorLogin() {
            return _GetUser("visitor");
        }

        static private User _GetUser(string username)
        {
            var Users = ReadJsonFromFile(filePath);
            foreach (var User in Users) {
                if (User.username == username)
                {
                    return User;
                }
            }
            return new User("Visitor",200);
        }

        static private void WriteJsonToFile(string filePath, List<User> Users)
        {
            string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Method to read data from a JSON file
        static private List<User> ReadJsonFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json);
        }
    }
}
