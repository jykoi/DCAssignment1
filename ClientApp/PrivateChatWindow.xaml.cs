using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using InterfaceLibrary; // ChatMessage, MessagesPage
using ServerDLL;  // ServerInterface

namespace ClientApp
{
    public partial class PrivateChatWindow : Window
    {
        private readonly ServerInterface _channel;
        private readonly string _me;
        private readonly string _peer;
        private int _lastId = 0;
        private readonly DispatcherTimer _timer;
        private readonly ObservableCollection<ChatMessage> _items = new ObservableCollection<ChatMessage>();

        public PrivateChatWindow(ServerInterface channel, string me, string peer)
        {
            InitializeComponent();
            _channel = channel;
            _me = me;
            _peer = peer;

            HeaderText.Text = $"Private: {_me} to {_peer}";
            MessagesList.ItemsSource = _items;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            _timer.Tick += (s, e) => RefreshMessages();
            _timer.Start();

            RefreshMessages();
        }

        private void RefreshMessages()
        {
            try
            {
                //get the list of messages for this chat window
                var page = _channel.GetPrivateMessagesSince(_me, _peer, _lastId, 100);
                if (page?.Items != null)
                {
                    //add the messages to the collection
                    foreach (var m in page.Items)
                        _items.Add(m);

                    _lastId = page.LastId;
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
                if (_channel.SendPrivateMessage(_me, _peer, text))
                {
                    InputBox.Clear();
                    RefreshMessages(); // optimistic refresh
                }
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
