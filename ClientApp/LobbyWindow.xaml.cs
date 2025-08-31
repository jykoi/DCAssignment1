using InterfaceLibrary;                
using ServerDLL;                       
using System;
using System.ComponentModel;          
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClientApp
{
    public partial class LobbyWindow : Window
    {
        private readonly ClientServices _client;
        private readonly string _lobbyName;
        private readonly Lobbies _parent;         
        private int _lastMsgId = 0;
        private CancellationTokenSource _cts;

        
        public LobbyWindow(ClientServices client, string lobbyName, Lobbies parent)
        {
            InitializeComponent();
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _lobbyName = lobbyName ?? throw new ArgumentNullException(nameof(lobbyName));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            Title = $"Lobby: {_lobbyName} — You: {_client.Username}";

            _cts = new CancellationTokenSource();
            Loaded += (_, __) => _ = StartPollingAsync(_cts.Token);
            Closing += LobbyWindow_Closing;
        }

        // Send
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var text = (MessageInput.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                bool ok = _client.serverChannel.PostLobbyMessage(_lobbyName, _client.Username, text);
                if (!ok)
                {
                    System.Windows.MessageBox.Show("Message failed to send.");
                    return;
                }
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Send error: {ex.Message}");
            }
        }

        // Poll 
        private async Task StartPollingAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _client.IsConnected())
            {
                try
                {
                    var page = _client.serverChannel.GetLobbyMessagesSince(_lobbyName, _lastMsgId, 100);
                    if (page?.Items != null && page.Items.Count > 0)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var m in page.Items)
                                ChatList.Items.Add($"[{m.Timestamp:t}] {m.FromUser}: {m.Text}");
                        });
                        _lastMsgId = page.LastId;
                    }

                    await Dispatcher.InvokeAsync(() =>
                        Status.Text = $"Loaded {_lastMsgId} Message(s).");
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                        Status.Text = $"Poll error: {ex.Message}");
                }

                try { await Task.Delay(1500, token); } catch { break; }
            }
        }

        // Refresh 
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            _lastMsgId = 0;
            ChatList.Items.Clear();
            Status.Text = "Refreshing…";
        }

        //  Leave / Close 
        private async void LeaveBtn_Click(object sender, RoutedEventArgs e)
        {
            await LeaveAndReturnAsync();
        }

        private async Task LeaveAndReturnAsync()
        {
            try
            {
                // Tell server we left the lobby
                _client.serverChannel.LeaveLobby(_lobbyName, _client.Username);
            }
            catch
            {
                
            }

            _cts.Cancel();

            if (_parent != null && !_parent.IsVisible)
                _parent.Show();

            //
            await Dispatcher.InvokeAsync(() => { });
            Close();
        }

        private void LobbyWindow_Closing(object sender, CancelEventArgs e)
        {
            
            if (!_cts.IsCancellationRequested)
            {
                e.Cancel = true;              
                _ = LeaveAndReturnAsync();
            }
        }
    }
}
