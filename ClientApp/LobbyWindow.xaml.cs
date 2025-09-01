using InterfaceLibrary;                
using ServerDLL;                       
using System;
using System.ComponentModel;          
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;                 
using System.Collections.ObjectModel;  
using System.IO;                       

namespace ClientApp
{
    public partial class LobbyWindow : Window
    {
        private readonly ClientServices _client;
        private readonly string _lobbyName;
        private readonly Lobbies _parent;         
        private int _lastMsgId = 0;
        private CancellationTokenSource _cts;
        
        // Files
        private int _lastFileId = 0;                                   
        private readonly ObservableCollection<LobbyFileInfo> _files
            = new ObservableCollection<LobbyFileInfo>();  // bind to SharedFilesList



        public LobbyWindow(ClientServices client, string lobbyName, Lobbies parent)
        {
            InitializeComponent();
            Loaded += (_, __) => PlayersList.MouseDoubleClick += PlayersList_MouseDoubleClick; // hook doubleclick handler for the Players list
            SharedFilesList.ItemsSource = _files;   // bind once

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _lobbyName = lobbyName ?? throw new ArgumentNullException(nameof(lobbyName));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            Title = $"Lobby: {_lobbyName} — You: {_client.Username}";

            _cts = new CancellationTokenSource();
            Loaded += (_, __) => _ = StartPollingAsync(_cts.Token);
            Loaded += (_, __) => _ = StartPlayersPollingAsync(_cts.Token);  //loaded hook for players
            Loaded += (_, __) => RefreshSharedFilesOnce();   // initial fetch so the list isn’t empty
            Loaded += (_, __) => _ = StartFilesPollingAsync(_cts.Token);   // files polling
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

        // Share a file (images/text only)
        private void ShareFileBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Let user pick a file
                var dlg = new OpenFileDialog
                {
                    Title = "Select a file to share",
                    Filter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|"+
                                "Text files (*.txt;*.log;*.csv)|*.txt;*.log;*.csv|" +
                                "All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };

                //Stop if user cancels
                if (dlg.ShowDialog(this) != true) return;

                var path = dlg.FileName;
                var fileName = System.IO.Path.GetFileName(path);
                var bytes = File.ReadAllBytes(path);
                var contentType = GetContentTypeFromPath(path);

                //Server only accepts image/* or text/*
                if (!(contentType.StartsWith("image/") || contentType.StartsWith("text/")))
                {
                    Status.Text = "Only image/text files are allowed.";
                    return;
                }

                //Try to upload
                bool ok = _client.UploadLobbyFile(_lobbyName, _client.Username, fileName, bytes, contentType);
                if (!ok)
                {
                    Status.Text = "Upload failed.";
                    return;
                }
                Status.Text = $"Shared: {fileName}";
                RefreshSharedFilesOnce();  // quick pull so it appears immediately
            }
            catch (Exception ex)
            {
                Status.Text = $"Share error: {ex.Message}";
            }
        }

        //Figure out content type from file extension 
        private string GetContentTypeFromPath(string path)
        {
            var ext = (System.IO.Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();

            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";

                case ".txt":
                case ".log":
                case ".csv": return "text/plain";

                default:
                    // Unknown types are rejected by server unless they start with image/ or text/
                    return "application/octet-stream";
            }
        }

        //Refresh file list once
        private void RefreshSharedFilesOnce()
        {
            try
            {
                var newFiles = _client.GetLobbyFilesSince(_lobbyName, _lastFileId, 100);
                if (newFiles != null && newFiles.Length > 0)
                {
                    foreach (var f in newFiles)
                        _files.Add(f);

                    //Update last seen fileID
                    if (_files.Count > 0)
                        _lastFileId = _files[_files.Count - 1].Id;
                }
            }
            catch (Exception ex)
            {
                Status.Text = $"Files refresh error: {ex.Message}";
            }
        }


        // PRIVATE MESSAGING
        // Open PM on double-click
        private void PlayersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var peer = PlayersList.SelectedItem as string;
            OpenPrivateChat(peer);
        }

        // Open PM on message button click
        private void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            var peer = PlayersList.SelectedItem as string;
            OpenPrivateChat(peer);
        }

        // Open a priv chat window for selected user
        private void OpenPrivateChat(string peerUser)
        {
            if (string.IsNullOrWhiteSpace(peerUser) ||
                string.Equals(peerUser, _client.Username, StringComparison.OrdinalIgnoreCase))
                return;

            var win = new PrivateChatWindow(_client.serverChannel, _client.Username, peerUser);
            win.Owner = this;      
            win.Show();
        }


        //File sharing click handlers
        // Open selected shared file
        private void SharedFilesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = SharedFilesList.SelectedItem as LobbyFileInfo;
            if (item == null) return;

            try
            {
                var bytes = _client.DownloadLobbyFile(_lobbyName, item.Id);
                if (bytes == null || bytes.Length == 0)
                {
                    Status.Text = "Download failed.";
                    return;
                }

                if (item.ContentType != null && item.ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                {
                    ShowTextFile(item.FileName, bytes);
                }
                else if (item.ContentType != null && item.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    ShowImageFile(item.FileName, bytes);
                }
                else
                {
                    Status.Text = "Unsupported file type (only image/text).";
                }
            }
            catch (Exception ex)
            {
                Status.Text = $"Open error: {ex.Message}";
            }
        }

        // Show a text file in a simple read-only window
        private void ShowTextFile(string title, byte[] bytes)
        {
            string text;
            try { text = System.Text.Encoding.UTF8.GetString(bytes); }
            catch { text = "[Unable to decode as UTF-8]"; }

            var win = new Window
            {
                Title = title,
                Width = 600,
                Height = 500,
                Content = new System.Windows.Controls.ScrollViewer
                {
                    Content = new System.Windows.Controls.TextBox
                    {
                        Text = text,
                        IsReadOnly = true,
                        AcceptsReturn = true,
                        TextWrapping = System.Windows.TextWrapping.Wrap,
                        VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
                    }
                }
            };
            win.Owner = this;
            win.Show();
        }

        // Show an image file in a simple viewer window
        private void ShowImageFile(string title, byte[] bytes)
        {
            var bmp = new System.Windows.Media.Imaging.BitmapImage();
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                bmp.BeginInit();
                bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
            }

            var win = new Window
            {
                Title = title,
                Width = 700,
                Height = 500,
                Content = new System.Windows.Controls.ScrollViewer
                {
                    Content = new System.Windows.Controls.Image
                    {
                        Source = bmp,
                        Stretch = System.Windows.Media.Stretch.Uniform
                    }
                }
            };
            win.Owner = this;
            win.Show();
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
                        Status.Text = $"Loaded up to #{_lastMsgId}");
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

        // Poll players list (runs alongside chat polling)
        private async Task StartPlayersPollingAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _client.IsConnected())
            {
                try
                {
                    var players = _client.serverChannel.GetPlayers(_lobbyName) ?? Array.Empty<string>();

                    await Dispatcher.InvokeAsync(() =>
                    {
                        PlayersList.ItemsSource = players;  // bind latest names
                    });
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Status.Text = $"Players poll error: {ex.Message}";
                    });
                }

                try { await Task.Delay(2000, token); } catch { break; } 
            }
        }

        // Poll shared files list (keeps other clients in sync)
        private async Task StartFilesPollingAsync(System.Threading.CancellationToken token)
        {
            while (!token.IsCancellationRequested && _client.IsConnected())
            {
                try
                {
                    var page = _client.GetLobbyFilesSince(_lobbyName, _lastFileId, 100);
                    if (page != null && page.Length > 0)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var f in page)
                                _files.Add(f);
                        });
                        _lastFileId = _files.Count > 0 ? _files[_files.Count - 1].Id : _lastFileId;

                        await Dispatcher.InvokeAsync(() =>
                            Status.Text = $"Files loaded up to #{_lastFileId}");
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                        Status.Text = $"Files poll error: {ex.Message}");
                }

                try { await Task.Delay(2000, token); } catch { break; } //2s 
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
