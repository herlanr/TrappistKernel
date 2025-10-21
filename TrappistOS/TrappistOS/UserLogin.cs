using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Sys = Cosmos.System;
using System;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace TrappistOS
{
    
    
    internal class UserLogin : ProgramClass
    {
        private int maxAdminID = 20;
        private string filepath = @"0:\users";
        private UserClass currentUser = null;
        private class UserClass
        {
            public int id { get; private set; }
            public string username { get; private set; }
            public string password { get; set; }

            public UserClass(string username, int id, string password = "visitor")
            {
                this.id = id;
                this.username = username;
                this.password = password;
            }
        }

        public UserLogin()
        {
            Identifier = "UserLogin";
        }

        public void BeforeRun()
        {
            if (!File.Exists(filepath))
            {
                string[] intitial_users = { "200 Visitor Visitor", "1 Admin Admin", Environment.NewLine}
            ;
                File.WriteAllLines(filepath, intitial_users);
            }
            VisitorLogin();
            Console.WriteLine("File.ReadAllText(filePath)");
        }

        public bool IsAdmin()
        {
            return currentUser.id < maxAdminID;
        }

        private bool UserIsAdmin(UserClass user)
        {
            return user.id < maxAdminID;
        }

        public void Login()
        {
            Console.WriteLine("username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();
            
            var user = _Login(username, password);

            if (user != null)
            {
                currentUser = user;
                Console.WriteLine("Login successful");
            }
            Console.WriteLine("Login failed");
        }

        public void IncreaseAdminRange(int count)
        {
            maxAdminID += count;
            string[] new_users = null;


            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users)
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3)
                {
                    if (elements.Length == 0)
                    {
                        continue;
                    }
                    Console.Write("Invalid user");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    Console.WriteLine();
                    continue;
                }
                new_users.Append(Convert.ToString(Convert.ToInt32(elements[0] + count)) + elements[1] + elements[2]);
            }
            File.WriteAllLines(filepath, new_users);
            Console.WriteLine("increase successful");
        }

        public void CreateUser(bool isAdmin)
        {
            Console.WriteLine("username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Password: ");
            string password = null;
            while (true)
            {
                while (true)
                {
                    var key = System.Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    password += key.KeyChar;
                }
                Console.WriteLine("Confirm Password: ");
                string repeatPassword = null;
                while (true)
                {
                    var key = System.Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    repeatPassword += key.KeyChar;
                }
                if (password == repeatPassword)
                { break; }
                Console.WriteLine("Passwords don't match");
            }

            bool success = SaveUser(username, password,isAdmin);

            if (success)
            {
                Console.WriteLine("Creation successful");
            }
            Console.WriteLine("Creation failed");
        }

        private bool SaveUser(string username, string password, bool isAdmin)
        {
            if (username.Contains(" "))
            {
                Console.WriteLine("Error, Username Contains Space");
                return false;
            }
            if (password.Contains(" "))
            {
                Console.WriteLine("Error, Password Contains Space");
                return false;
            }

            int[] adminNumbers = null;
            int[] usedNumbers = null;

            for (int i=1; i<=maxAdminID; i++)
            {
                adminNumbers.Append(i);
            }

            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users)
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3)
                {
                    if (elements.Length == 0)
                    {
                        continue;
                    }
                    Console.Write("Invalid user");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                        usedNumbers.Append(Convert.ToInt32(elements[0]));
                    }
                    Console.WriteLine();
                    continue;
                }
                usedNumbers.Append(Convert.ToInt32(elements[0]));
                if (elements[1] == username) //id name password
                {
                    Console.WriteLine("Username already taken");
                    return false;
                }
                adminNumbers = adminNumbers.Where(val => val != Convert.ToInt32(elements[0])).ToArray();
            }
            if (isAdmin && adminNumbers.Length == 0)
            {
                Console.WriteLine("No Adminnubers free, increase range with 'increaseAdminRange'");
                return false;
            }
            if (isAdmin)
            {
                File.AppendAllText(filepath, Convert.ToString(adminNumbers[0]) + username + password);
                return true;
            }
            Random rnd = new Random();
            int cnt = 0;
            int new_id = 0;
            while (true)
            {
                new_id = rnd.Next(maxAdminID + 1, 10000);
                if (!usedNumbers.Contains(new_id))
                {
                    break;
                }
                cnt++;
                if (cnt > 500)
                {
                    Console.WriteLine("Couldn't find new valid id");
                    return false;
                }
            }
            File.AppendAllText(filepath, Convert.ToString(new_id) + username + password);
            return true;
        }

        private UserClass _Login(string name, string pw)
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
            return null;
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
            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users)
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3) {
                    Console.Write("Invalid user");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    Console.WriteLine();
                    continue;
                }
                if (elements[1] == username) //id name password
                {
                    return new UserClass(elements[1], Convert.ToInt32(elements[0]), elements[3]);
                }
            }

            return null;
        }

    }
}
