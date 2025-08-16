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
        public static List<string> usernames
        {
            get { return _usernames; }
            private set { _usernames = value; }
        }


        public static bool AddUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || _usernames.Contains(username))
            {
                return false; 
            }
            _usernames.Add(username);
            return true; 
        }
    }
}
