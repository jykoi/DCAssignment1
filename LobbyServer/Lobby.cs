using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public class Lobby
    {
        private readonly List<string> _players = new List<string>();
        private readonly object _playersLock = new object();
        private string _name;

        public string Name
        {
            get => _name;
            set => _name = (value ?? string.Empty).Trim();
        }

        public Lobby(string name)
        {
            //_players = new List<string>();
            //_name = name;
            Name = name;
        }

        public bool AddPlayer(string username)
        {
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username)) return false;

            lock (_playersLock)
            {
                if (!_players.Contains(username)) return false;
                _players.Add(username);
                return true;
            }
        }

        public bool RemovePlayer(string username)
        {
            username = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username)) return false;

            lock (_playersLock)
            {
                return _players.Remove(username);
            }
        }

        public string[] GetPlayersSnapshot()
        {
            lock (_playersLock)
            {
                return _players.ToArray();
            }
        }
    }

}
