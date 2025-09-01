using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using InterfaceLibrary; // ChatMessage, MessagesPage
using ServerDLL;  // ServerInterface

namespace DuplexClient
{
    public partial class PrivateChatWindow : Window
    {
        
        private readonly ClientServices _client;
        private readonly string _me;
        private readonly string _peer;
        private readonly ObservableCollection<ChatMessage> _items = new ObservableCollection<ChatMessage>();

        public PrivateChatWindow(string me, string peer, ClientServices client)
        {
            InitializeComponent();
            
            _me = me;
            _peer = peer;
            _client = client;
            _client.Peer = _peer;

            HeaderText.Text = $"Private: {_me} to {_peer}";
            MessagesList.ItemsSource = _items;

            Loaded += Init;

        }

        private void Init(object sender, RoutedEventArgs e)
        {
            _client.OnDMSent = () =>
            {
                Dispatcher.Invoke(() => RefreshMessages());
            };
            _client.FetchPrivateMessages();
        }

        private void RefreshMessages()
        {
            try
            {
                var page = _client.CurrentDMs;
                if (page?.Items != null)
                {
                    foreach (var m in page.Items)
                        _items.Add(m);
                    _client.LastDMId = page.LastId;
                }
            }
            catch
            {
                
            }

        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var text = InputBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                if (_client.serverChannel.SendPrivateMessage(_me, _peer, text))
                {
                    InputBox.Clear();
                    //RefreshMessages(); // optimistic refresh
                }
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            _client.Peer = "";
            _client.LastDMId = 0;
            _client.CurrentDMs = null;
            base.OnClosed(e);
        }
    }
}
