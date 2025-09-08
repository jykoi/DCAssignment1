using ClientFunctions;
using InterfaceLibrary;
using Microsoft.Win32;
using ServerDLL;                       
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Net.WebRequestMethods;

namespace DuplexClient
{
    public partial class LobbyWindow : Window
    {
        private readonly ClientServices _client;
        private readonly string _lobbyName;
        private readonly Lobbies _parent;         
        private CancellationTokenSource _cts;

        private readonly ObservableCollection<LobbyFileInfo> _files
            = new ObservableCollection<LobbyFileInfo>();  // bind to SharedFilesList


        public LobbyWindow(ClientServices client, string lobbyName, Lobbies parent)
        {
            InitializeComponent();
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _lobbyName = lobbyName ?? throw new ArgumentNullException(nameof(lobbyName));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SharedFilesList.ItemsSource = _files;
            SharedFilesList.DisplayMemberPath = nameof(LobbyFileInfo.FileName); //Shows the correct file name.

            Title = $"Lobby: {_lobbyName} — You: {_client.Username}";

            _cts = new CancellationTokenSource();
            //subscribe to Init when the window loads   
            Loaded += Init;
            //subscribe to Closing event
            Closing += LobbyWindow_Closing;
        }

        private void Init(object sender, RoutedEventArgs e)
        {
            //set functions that will be called when events happen on the server
            _client.OnMessageSent = () =>
            {
                Dispatcher.Invoke(() => LoadMessages());
            };

            _client.OnPlayerJoined = () =>
            {
                Dispatcher.Invoke(() => LoadPlayers());
            };

            _client.OnFileSent = () =>
            {
                Dispatcher.Invoke(() => RefreshSharedFilesOnceAsync());
            };

            // call the fetch functions to load any data on window load
            _client.FetchLobbyMessages();
            _client.FetchPlayersList();
            _client.FetchLobbyFiles();
        }

        // opens private messages with the selected player
        private void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            var peer = PlayersList.SelectedItem as string;
            OpenPrivateChat(peer);
        }

        //opens private messages with the double-clicked player
        private void PlayersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var peer = PlayersList.SelectedItem as string;
            OpenPrivateChat(peer);
        }

        private void SharedFilesList_MouseDoubleClick(object sender, RoutedEventArgs e)
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

        private void ShareFileBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Let user pick a file
                var dlg = new OpenFileDialog
                {
                    Title = "Select a file to share",
                    Filter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|" +
                                "Text files (*.txt;*.log;*.csv)|*.txt;*.log;*.csv|" +
                                "All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };

                //Stop if user cancels
                if (dlg.ShowDialog(this) != true) return;

                var path = dlg.FileName;
                var fileName = System.IO.Path.GetFileName(path);

                var ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant();
                if (ext != ".txt" && ext != ".bmp" && ext != ".png")
                {
                    Status.Text = "Only .txt and .bmp and .png files are allowed.";
                    return;
                }

                var bytes = System.IO.File.ReadAllBytes(path);

                const int MaxPayload = 60 * 1024; // 60 KB to stay under the default 64KB. so large files won't cause channel fault/crash

                if (bytes.Length > MaxPayload)
                {
                    MessageBox.Show("File is too large!! Please use a smaller .txt or .bmp. or .png");
                    return;
                }

                var contentType = Functions.GetContentTypeFromPath(path);

                //Server only accepts image/* or text/*
                //if (!(contentType.StartsWith("image/") || contentType.StartsWith("text/")))
                //{
                //    Status.Text = "Only image/text files are allowed.";
                //    return;
                //}

                //Try to upload
                bool ok = _client.UploadLobbyFile(_lobbyName, _client.Username, fileName, bytes, contentType);
                if (!ok)
                {
                    Status.Text = "Upload failed.";
                    return;
                }
                Status.Text = $"Shared: {fileName}";
                  
            }
            catch (Exception ex)
            {
                Status.Text = $"Share error: {ex.Message}";

            }
        }

        //fetches lobby files and adds them to the collection
        private void RefreshSharedFilesOnceAsync()
        {
            try
            {
                var newFiles = _client.CurrentLobbyFiles;
                if (newFiles != null && newFiles.Length > 0)
                {
                    
                    foreach (var f in newFiles)
                        _files.Add(f);

                    // keep track of the last file id. If files are found, set ID to count-1. Otherwise, set to the ID
                    //which is 0
                    _client.LastFileId = _files.Count > 0 ? _files[_files.Count - 1].Id : _client.LastFileId;
                }  
            }
            catch (Exception ex)
            {
                Status.Text = $"Files refresh error: {ex.Message}";
            }
        }

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

        private void OpenPrivateChat(string peerUser)
        {
            if (string.IsNullOrWhiteSpace(peerUser) ||
                string.Equals(peerUser, _client.Username, StringComparison.OrdinalIgnoreCase))
                return;

            var win = new PrivateChatWindow(_client.Username, peerUser, _client);
            win.Owner = this;
            win.Show();
        }

        private async Task LoadPlayers()
        {
            var players = _client.CurrentPlayers;
            await Dispatcher.InvokeAsync(() =>
            {
                PlayersList.ItemsSource = players;
            });
        }


        // Send message to lobby
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            //trim the string firstKK
            var text = (MessageInput.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                //check that the message was posted successfully
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

        

        private void LoadMessages()
        {
            //fetch the list of messages
            var page = _client.CurrentLobbyMessages;
            foreach (var m in page.Items)
            {
                //add each message to the listbox
                ChatList.Items.Add($"[{m.Timestamp:t}] {m.FromUser}: {m.Text}");
            }
            //update status text
            Status.Text = $"Loaded {page.Items.Count} message(s).";

        }

        // Refresh 
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatList.Items.Clear();
            _client.LastMsgId = 0;
            _client.FetchLobbyMessages();
        }

        //  Leave / Close 
        private async void LeaveBtn_Click(object sender, RoutedEventArgs e)
        {
            await LeaveAndReturnAsync();
            _client.CurrentLobbyName = "";
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
            //clean up after leaving the lobby.
            _client.CurrentLobbyName = "";
            _client.LastMsgId = 0;
            _client.OnMessageSent = null;
            _client.LastFileId = 0;
            _client.OnFileSent = null;
            if (!_cts.IsCancellationRequested)
            {
                e.Cancel = true;              
                _ = LeaveAndReturnAsync();
            }
        }

        
    }
}
