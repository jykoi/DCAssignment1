using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using ClassLibrary1;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for LobbyWindow.xaml
    /// </summary>
    public partial class LobbyWindow : Window
    {
        private readonly ServerInterface _proxy;
        public LobbyWindow(ServerInterface proxy)
        {
            InitializeComponent();
            _proxy = proxy;
            Loaded += (_, __) => LoadLobbies();
        }

        private void LoadLobbies()
        {
            try
            {
                var names = _proxy.ListLobbies() ?? Array.Empty<string>();
                LobbiesList.ItemsSource = names;
                Status.Text = $"Loaded {names.Length} lobby(ies).";
                
            }
            catch (Exception ex)
            {
                Status.Text = "Failed in loading lobbies: " + ex.Message;
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadLobbies();
        }

    }
}
