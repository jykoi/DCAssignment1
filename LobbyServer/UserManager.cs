using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public static class UserManager
    {
        private static List<string> _usernames = new List<string>();
        public static List<string> Usernames
        {
            get { return _usernames; }
            private set { _usernames = value; }
        }

        public static bool AddUser(string username)
        {
            // Check for null, empty, whitespace, or duplicate usernames
            if (string.IsNullOrWhiteSpace(username) || _usernames.Contains(username))
            {
                return false; 
            }
            //add the user
            _usernames.Add(username);
            return true; 
        }
    }
}
