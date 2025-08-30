using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ServerDLL;

namespace LobbyServer
{
    
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Welcome");
            var tcp = new NetTcpBinding();

            var host = new ServiceHost(typeof(ServerImplementation));
            host.AddServiceEndpoint(typeof(ServerInterface), tcp, "net.tcp://0.0.0.0:8100/DataService");
            host.AddServiceEndpoint(typeof(ServerInterface), tcp, "net.tcp://0.0.0.0:8200/DataService");
            host.Open();

            Console.WriteLine("System Online");
            Console.ReadLine();

            host.Close();
        }
    }
}
