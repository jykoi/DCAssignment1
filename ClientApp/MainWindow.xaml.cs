using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServerInterface serverChannel;
        private NetTcpBinding tcp;
        private string URL = "net.tcp://localhost:8100/DataService";
        private ChannelFactory<ServerInterface> chanFactory;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            tcp = new NetTcpBinding();
            chanFactory = new ChannelFactory<ServerInterface>(tcp, URL);
            serverChannel = chanFactory.CreateChannel();
            if (!serverChannel.CheckUsername(usernameField.Text))
            {
                ((ICommunicationObject)serverChannel).Close();
                chanFactory.Close();
                usernameField.Text = "pick valid and unique username";
            }
            else
            {
                usernameField.Text = "Username accepted";
                loginBtn.IsEnabled = false;
            }
        }
    }
}
