using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Sys = Cosmos.System;

namespace TrappistOS
{


    internal class UserLogin
    {
        public int maxAdminID { private set; get; } = 20; //TODO: move into systemfile
        public int visitorid { private set; get; } = 200;
        private string filepath;
        private UserClass currentUser = null;
        private class UserClass
        {
            public int id { get; private set; }
            public string username { get; private set; }
            public string password { get; set; }

            public UserClass(string username, int id, string password)
            {
                this.id = id;
                this.username = username;
                this.password = password;
            }
        }


        public void BeforeRun(string userpath) //intitialize and login into Visitor
        {
            this.filepath = userpath;
            if (!File.Exists(filepath))
            {
                string[] intitial_users = { $"{maxAdminID+200} Visitor Visitor", "1 Admin Admin"}
            ;
                File.WriteAllLines(filepath, intitial_users);
            }
            currentUser = VisitorLogin();
        }

        public bool IsAdmin() //Admincheck
        {
            if (currentUser == null)
            {
                currentUser = VisitorLogin();
            }
            return currentUser.id < maxAdminID;
        }

        public bool IsVisitor() //Admincheck
        {
            if (currentUser == null)
            {
                currentUser = VisitorLogin();
            }
            return currentUser.id == visitorid;
        }

        private bool UserIsAdmin(UserClass user)
        {
            return user.id < maxAdminID;
        }

        public bool Logout()
        {
            currentUser = VisitorLogin();
            return true;
        }

        public bool Changepassword()
        {
            List<string> new_users = new List<string>();
            Console.Write("Please put in your password:");
            string password = null; //hides the input
            password = TypePassword();

            if (password == null)
            {
                Console.WriteLine("Change Aborted");
                return false;
            }

            Console.WriteLine();
            if (password != currentUser.password)
            {
                Console.WriteLine("wrong Password");
                return false;
            }
            while (true)
            {
                Console.Write("Password: ");
                password = null;
                password = TypePassword();

                if (password == null)
                {
                    Console.WriteLine("Change Aborted");
                    return false;
                }
                Console.WriteLine();
                Console.Write("Confirm Password: ");
                string repeatPassword = null;
                repeatPassword = TypePassword();

                if (password == null)
                {
                    Console.WriteLine("Change Aborted");
                    return false;
                }
                Console.WriteLine();
                if (password == repeatPassword)
                { break; }
                Console.WriteLine("Passwords don't match");
            } //make sure password is correct
            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users) // move up every user by the amount added, to stop users from becomng admins
            {
                string[] elements = user.Split(' '); //Arrangement: id name password
                if (elements.Length != 3)
                {
                    continue;
                } // list of users with new ids
                if (elements[1] != currentUser.username)
                {
                    new_users.Add(elements[0] + " " + elements[1] + " " + elements[2]);
                }
                else
                {
                    new_users.Add(elements[0] + " " + elements[1] + " " + password); // change password
                }
            }
            Console.WriteLine($"Are you sure you want to Change you Password? (y)es/(n)o");
            
            if (Kernel.WaitForConfirmation())
            {
                File.WriteAllLines(filepath, new_users.ToArray()); //overwrite old users
                currentUser.password = password;
                Console.WriteLine($"Change successful");
                return true;
            }

            Console.WriteLine("Change aborted");
            return false;
        }

        public bool Login()
        {
            Console.Write("username: ");
            string username = Console.ReadLine();
            if (username == "Visitor")
            {
                currentUser = VisitorLogin();
                return true;
            }
            Console.Write("Password: ");
            string password = null; //hides the input

            password = TypePassword();

            if (password == null)
            {
                Console.WriteLine("Login Aborted");
                return false;
            }
            Console.WriteLine();

            var user = _Login(username, password);

            if (user != null)
            {
                currentUser = user;
                Console.WriteLine("Login successful");
                return true;
            }
            Console.WriteLine("Login failed");
            return false;
        }

        public bool DeleteUser(string username)
        {
            List<string> new_users = new List<string>();
            bool found = false;
            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users) // move up every user by the amount added, to stop users from becomng admins
            {
                string[] elements = user.Split(' '); //Arrangement: id name password
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
                } // list of users with new ids
                if (elements[1] != username)
                {
                    new_users.Add(elements[0] + " " + elements[1] + " " + elements[2]);
                }
                else
                {
                    found = true;
                }
            }
            if (found)
            {
                Console.WriteLine($"Are you sure you want to delete {username}? (y)es/(n)o");
                
                if (Kernel.WaitForConfirmation())
                {
                    File.WriteAllLines(filepath, new_users.ToArray()); //overwrite old users
                    Console.WriteLine($"successfully deleted {username}");
                    return true;
                }
                else
                {
                    Console.WriteLine("deletion aborted");
                    return false;
                }
            }
            else { 
                Console.WriteLine($"{username} does not exist");
                return false;
            }
        }

        public bool IncreaseAdminRange(int count)
        {
            maxAdminID += count; //TODO: move into systemfile
            List<string> new_users = new List<string>();
            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users) // move up every user by the amount added, to stop users from becomng admins
            {
                string[] elements = user.Split(' '); //Arrangement: id name password
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
                } // list of users with new ids
                new_users.Add(Convert.ToString(Convert.ToInt32(elements[0] + count)) + " " + elements[1] + " " + elements[2]);
            }
            File.WriteAllLines(filepath, new_users.ToArray()); //overwrite old users
            Console.WriteLine("increase successful");
            return true;
        }

        public int CreateUser(bool isAdmin)
        {
            Console.Write("username: ");
            string username = Console.ReadLine();
            
            if (GetUser(username) != null)
            {
                Console.WriteLine($"{username} is already exists");
                return 0;
            }

            string password = null;
            while (true)
            {
                Console.Write("Password: ");
                password = null;
                password = TypePassword();

                if (password == null)
                {
                    Console.WriteLine("Creation Aborted");
                    return 0;
                }
                Console.WriteLine();
                Console.Write("Confirm Password: ");
                string repeatPassword = null;
                repeatPassword = TypePassword();

                if (password == null)
                {
                    Console.WriteLine("Change Aborted");
                    return 0;
                }
                Console.WriteLine();
                if (password == repeatPassword)
                { break; }
                Console.WriteLine("Passwords don't match");
            } //make sure password is correct

            bool success = SaveUser(username, password,isAdmin);

            if (success)
            {
                Console.WriteLine("Creation successful");
                return GetId(username);
            }
            Console.WriteLine("Creation failed");
            return 0;
        }

        private bool SaveUser(string username, string password, bool isAdmin)
        {
            if (username == "")
            {
                Console.WriteLine("empty username");
                return false;
            }
            if (username.Contains(" ")) //spaces mess with username saving
            {
                Console.WriteLine("Error, Username Contains Space");
                return false;
            }
            if (password == "")
            {
                Console.WriteLine("empty password");
                return false;
            }
            if (password.Contains(" "))
            {
                Console.WriteLine("Error, Password Contains Space");
                return false;
            }
            int[] adminNumbers = null;
            List<int>  usedNumbers = new List<int>();
            List<int> adminNumList = new List<int>();
            for (int i = 1; i <= maxAdminID; i++)
            {
                adminNumList.Add(i);
            }
            adminNumbers = adminNumList.ToArray();
            string[] users = File.ReadAllLines(filepath);
            foreach (string user in users)
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3)
                {
                    if (elements.Length == 0 || elements[0] == "")
                    {
                        continue;
                    }
                    Console.Write("Invalid user");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                        usedNumbers.Add(Convert.ToInt32(elements[0]));
                    }
                    Console.WriteLine();
                    continue;
                }
                usedNumbers.Add(Convert.ToInt32(elements[0]));
                if (elements[1] == username) //id name password
                {
                    Console.WriteLine("Username already taken");
                    return false;
                }
                List<int> new_admin = new List<int>();
                for (int i = 1; i < adminNumbers.Length; i++)
                {
                    if (adminNumbers[i] != Convert.ToInt32(elements[0]))
                    {
                        new_admin.Add(adminNumbers[i]);
                    }
                }
                adminNumbers = new_admin.ToArray();
            }
            if (isAdmin && adminNumbers.Length == 0)
            {
                Console.WriteLine("No AdminIds free, increase range with 'increaseAdminRange'");
                return false;
            }
            if (isAdmin)
            {
                File.AppendAllText(filepath, Convert.ToString(adminNumbers[0]) + " " + username + " " + password);
                return true;
            }
            Random rnd = new Random();
            int cnt = 0;
            int new_id = 0;
            while (true)
            {// user that isn't admin needs to get matcing ID
                new_id = rnd.Next(maxAdminID + 1, 10000+maxAdminID);
                foreach (int number in usedNumbers) {
                    if (number == new_id)
                    {
                        cnt++;
                        if (cnt > 500) //Contingency for too long a loop
                        {
                            Console.WriteLine("Couldn't find new valid id, try again");
                            return false;
                        }
                        continue; 
                    }
                }
                break;
            }
            File.AppendAllText(filepath, Convert.ToString(new_id) + " " + username + " " + password+ Environment.NewLine);//create new user
            return true;
        }

        private UserClass _Login(string name, string pw)
        {
            UserClass user = GetUser(name);
            if (user == null)
            {
                Console.WriteLine($"{name} is not registered.");
                return null;
            }
            if (user.password == pw)
            {
                Console.WriteLine($"Password Correct, welcome {name}");
                return user;
            }
            Console.WriteLine("Password incorrect");
            return null;
        }

        public bool AutoLogin(int userId)
        {

            currentUser = GetUser(GetName(userId));
            return true;
        }

        public int GetId()
        {
            if (currentUser == null)
            {
                currentUser = VisitorLogin();
            }
            return currentUser.id;
        }

        public string GetName()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Invalid logged in user");
                currentUser = VisitorLogin();
            }
            return currentUser.username;
        }

        public int GetId(string username)
        {
            UserClass thisUser = GetUser(username);
            if (thisUser is null)
            {
                return 0;
            }
            return thisUser.id;
        }

        public string GetName(int userID)
        {
            string[] users = File.ReadAllLines(filepath);

            if (userID == 0)
            {
                return "system";
            }

            foreach (string user in users) //look through file for user
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3)
                {
                    if (elements[0] == "")
                    { continue; }
                    Console.Write($"Invalid user with amount of elements: {elements.Length}");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    Console.WriteLine();
                    continue;
                }
                if (elements[0] == userID.ToString()) //id name password
                {
                    int id = Convert.ToInt32(((string)elements[0]).Trim());
                    return elements[1];
                }
            }
            return "unknown";
        }

        private UserClass VisitorLogin()
        {
            return new UserClass("Visitor", visitorid + maxAdminID,"Visitor");
        }

        private UserClass GetUser(string username)
        {

            string[] users = File.ReadAllLines(filepath);
            
            foreach (string user in users) //look through file for user
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3) {
                    if (elements[0] == "")
                    { continue; }
                    Console.Write($"Invalid user with amount of elements: {elements.Length}");
                    if (elements.Length >= 1)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    Console.WriteLine();
                    continue;
                }
                if (elements[1] == username) //id name password
                {
                    int id = Convert.ToInt32(((string)elements[0]).Trim());
                    UserClass result = new UserClass(elements[1], id, elements[2]);
                    return result;
                }
            }
            return null;
        }

        protected static void Interrupthandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe read operation has been interrupted.");

            Console.WriteLine($"  Key pressed: {args.SpecialKey}");

            Console.WriteLine($"  Cancel property: {args.Cancel}");

            // Set the Cancel property to true to prevent the process from terminating.
            Console.WriteLine("Setting the Cancel property to true...");
            args.Cancel = true;

            // Announce the new value of the Cancel property.
            Console.WriteLine($"  Cancel property: {args.Cancel}");
            
        }
        
        public string[] GetAllUsers()
        {
            string[] users = File.ReadAllLines(filepath);
            List<string> result = new List<string>();

            foreach (string user in users) //look through file for user
            {
                string[] elements = user.Split(' ');

                if (elements.Length != 3)
                {
                    if (elements[0] == "")
                    { continue; }
                    Console.WriteLine();
                    continue;
                }

                result.Add(elements[1]);
            }
            return result.ToArray();
        }

        public string TypePassword()
        {
            string password = "";
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                if (key.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                password += key.KeyChar;
            }
            return password;
        }
    }
}

