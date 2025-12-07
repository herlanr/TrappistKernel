using System;
using System.Collections.Generic;
using System.IO;

namespace TrappistOS
{
    public class UserLogin
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
                string[] intitial_users = { $"{maxAdminID+ visitorid} Visitor Visitor", "1 Admin Admin"};
                File.WriteAllLines(filepath, intitial_users);
            }
            visitorid = visitorid + maxAdminID;
            currentUser = VisitorLogin();
        }

        public bool IsAdmin() //Admincheck
        {
            if (currentUser == null)
            {
                currentUser = VisitorLogin();
            }
            return currentUser.id <= maxAdminID;
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

        public bool DeleteUser(string username,bool quiet = false)
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
                    if(!quiet)
                    {
                        Console.Write("Invalid user");
                    }
                    
                    if (elements.Length >= 1 && !quiet)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    if(!quiet)
                    {
                        Console.WriteLine();
                    }
                    
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
                
                File.WriteAllLines(filepath, new_users.ToArray()); //overwrite old users
                if(!quiet)
                {
                    Console.WriteLine($"successfully deleted {username}");
                }
                
                return true;
            }
            else { 
                if(!quiet)
                {
                    Console.WriteLine($"{username} does not exist");
                }
                
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
            string username = string.Empty;
            while (true)
            {
                Console.Write("username: ");
                username = Console.ReadLine();
                if(username.Length > 8)
                {
                    Console.WriteLine("username too long, it can't be longer than 8 characters.");
                }
                else
                {
                    break;
                }
            }

            if (GetUser(username,true) != null)
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

        public int CreateUser(bool isAdmin,string username, string password, bool quiet = false)
        {
            Console.Write("username: ");

            if (GetUser(username, true) != null)
            {
                if(!quiet)
                {
                    Console.WriteLine($"{username} is already exists");
                }
                
                return 0;
            }

            bool success = SaveUser(username, password, isAdmin);

            if (success)
            {
                Console.WriteLine("Creation successful");
                return GetId(username);
            }
            Console.WriteLine("Creation failed");
            return 0;
        }

        private bool SaveUser(string username, string password, bool isAdmin, bool quiet = false)
        {
            if (username == "")
            {
                if (!quiet)
                {
                    Console.WriteLine("empty username");
                }
                return false;
            }
            if (username.Contains(" ")) //spaces mess with username saving
            {
                if(!quiet)
                {
                    Console.WriteLine("Error, Username Contains Space");
                }
                
                return false;
            }
            if (password == "")
            {
                if (!quiet)
                {
                    Console.WriteLine("empty password");
                }
                
                return false;
            }
            if (password.Contains(" "))
            {
                if (!quiet)
                {
                    Console.WriteLine("Error, Password Contains Space");
                }
                
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
                    if(!quiet)
                    {
                        Console.Write("Invalid user");
                    }
                    
                    if (elements.Length >= 1)
                    {
                        if (!quiet)
                        {
                            Console.Write(" with id" + elements[0]);
                        }
                        usedNumbers.Add(Convert.ToInt32(elements[0]));
                    }
                    if(!quiet)
                    {
                        Console.WriteLine();
                    }
                    
                    continue;
                }
                usedNumbers.Add(Convert.ToInt32(elements[0]));
                if (elements[1] == username) //id name password
                {
                    if(!quiet)
                    {
                        Console.WriteLine("Username already taken");
                    }
                    
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
                if(!quiet)
                {
                    Console.WriteLine("No AdminIds free, increase range with 'increaseAdminRange'");
                }
                
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
            {// user that isn't admin needs to get matching ID
                new_id = rnd.Next(maxAdminID + 1, 10000+maxAdminID);
                if (usedNumbers.Contains(new_id))
                {
                    cnt++;
                    if (cnt > 500) //Contingency for too long a loop
                    {
                        if(!quiet)
                        {
                            Console.WriteLine("Couldn't find new valid id, try again");
                        }
                        
                        return false;
                    }
                    continue;
                }
                break;
            }
            File.AppendAllText(filepath, Convert.ToString(new_id) + " " + username + " " + password+ Environment.NewLine);//create new user
            return true;
        }

        private UserClass _Login(string name, string pw, bool quiet = false)
        {
            UserClass user = GetUser(name,true);
            if (user == null)
            {
                if(!quiet)
                {
                    Console.WriteLine($"{name} is not registered.");
                }
                
                return null;
            }
            if (user.password == pw)
            {
                if(!quiet)
                {
                    Console.WriteLine($"Password Correct, welcome {name}");
                }
                
                return user;
            }
            if(!quiet)
            {
                Console.WriteLine("Password incorrect");
            }
            
            return null;
        }

        public bool AutoLogin(int userId)
        {
            string username = GetName(userId);
            currentUser = GetUser(username,true);
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

        public string GetName(bool quiet = false)
        {
            if (currentUser == null)
            {
                if(!quiet)
                {
                    Console.WriteLine("Invalid logged in user");
                }
                
                currentUser = VisitorLogin();
            }
            return currentUser.username;
        }

        public int GetId(string username)
        {
            UserClass thisUser = GetUser(username,true);
            if (thisUser is null)
            {
                return 0;
            }
            return thisUser.id;
        }

        public string GetName(int userID,bool quiet = false)
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
                    if(!quiet)
                    {
                        Console.Write($"Invalid user with amount of elements: {elements.Length}");
                    }
                    
                    if (elements.Length >= 1 && !quiet)
                    {
                        Console.Write(" with id" + elements[0]);
                    }
                    if(!quiet)
                    {
                        Console.WriteLine();
                    }
                    
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
            return new UserClass("Visitor", visitorid,"Visitor");
        }

        private UserClass GetUser(string username,bool quiet = false)
        {

            string[] users = File.ReadAllLines(filepath);
            
            foreach (string user in users) //look through file for user
            {
                string[] elements = user.Split(' ');
                if (elements.Length != 3) {
                    if (elements[0] == "")
                    { continue; }
                    if (!quiet)
                    { 
                        Console.Write($"Invalid user with amount of elements: {elements.Length}");
                        if (elements.Length >= 1)
                        {
                            Console.Write(" with id" + elements[0]);
                        }
                        Console.WriteLine();
                    }
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
                if(key.Key == ConsoleKey.Backspace)
                {
                    if(password.Length - 1 < 0)
                    {
                        continue;
                    }
                    password = password.Substring(0, password.Length - 1);
                    continue;
                }
                password += key.KeyChar;
            }
            return password;
        }
    }
}

