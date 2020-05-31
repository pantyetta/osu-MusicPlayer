using osu_MusicPlayer.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WMPLib;

namespace osu_MusicPlayer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowsMediaPlayer mediaPlayer = new WindowsMediaPlayer();

        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.SystemIdle);

        NotifyIcon notifyIcon = new NotifyIcon();

        ContextMenuStrip menuStrip = new ContextMenuStrip();

        ToolStripLabel toolStripTitle = new ToolStripLabel();
        ToolStripLabel toolStripArtist = new ToolStripLabel();
        ToolStripMenuItem toolStripControl = new ToolStripMenuItem();
        ToolStripMenuItem toolStripBack = new ToolStripMenuItem();
        ToolStripMenuItem toolStripPlay = new ToolStripMenuItem();
        ToolStripMenuItem toolStripNext = new ToolStripMenuItem();
        ToolStripMenuItem toolStripPlaylist = new ToolStripMenuItem();
        ToolStripMenuItem toolStripExit = new ToolStripMenuItem();


        //アプリの保存先を取得
        string appPath = Directory.GetCurrentDirectory();

        //テキスト用変数
        string textfilePath = null;


        //参照先URLの関数及びインデックス設定
        List<string> PlayerURL = new List<string>();
        List<string> PlayerTitle = new List<string>();
        List<string> PlayerArtist = new List<string>();
        List<string> SearchTitle = new List<string>();
        List<string> SearchArtist = new List<string>();
        List<string> SearchTags = new List<string>();

        //プレイリスト用
        string SelectPlaylist;
        List<string> PlaylistURL = new List<string>();
        List<string> PlaylistTitle = new List<string>();
        List<string> PlaylistArtist = new List<string>();

        List<int> PlaylistHistory = new List<int>();
        List<int> PlaylistNext = new List<int>();


        int NowPlay;

        List<int> PlayHistory = new List<int>();
        List<int> PlayNext = new List<int>();



        public MainWindow()
        {
            InitializeComponent();

            //NotifyIconの初期化
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/2.ico", UriKind.RelativeOrAbsolute)).Stream;

            notifyIcon.Text = "Osu!MusicPlayer";
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);

            //menustrip関係
            toolStripTitle.Text = "タイトル";
            toolStripArtist.Text = "アーティスト";
            toolStripControl.Text = "コントロール";
            toolStripBack.Text = "Back";
            toolStripPlay.Text = "Play";
            toolStripNext.Text = "Next";
            toolStripPlaylist.Text = "プレイリストに追加";
            toolStripExit.Text = "終了";

            menuStrip.Items.Add(toolStripTitle);
            menuStrip.Items.Add(toolStripArtist);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toolStripControl);
            toolStripControl.DropDownItems.Add(toolStripBack);
            toolStripControl.DropDownItems.Add(toolStripPlay);
            toolStripControl.DropDownItems.Add(toolStripNext);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toolStripPlaylist);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toolStripExit);

            notifyIcon.ContextMenuStrip = menuStrip;

            //色々設定
            textfilePath = appPath + @"\music.osuplayer";

            MenuItem_Delete.IsEnabled = false;

            while (Settings.Default.osuURL == "")
            {
                SetosuPath();
            }


            //曲リスト作成
            //ファイルがあれば曲を読み込む
            if (File.Exists(textfilePath))
            {
                NewReadMusic(textfilePath);
            }
            //ファイルを生成する
            else
            {
                NewSaveMusic(textfilePath);
            }

            Debug.WriteLine(PlayerURL.Count + "曲");


            //プレイリストを読み込む
            PlaylistUpdate();


            //音量の初期設定
            Slider_Volume.Maximum = Settings.Default.maxvolume;
            Slider_Volume.Value = Settings.Default.volume;
            mediaPlayer.settings.volume = Settings.Default.volume;


            //音量バーを変えたら音量変更
            Slider_Volume.ValueChanged += delegate
            {
                Settings.Default.volume = (int)Slider_Volume.Value;
                mediaPlayer.settings.volume = Settings.Default.volume;
                Settings.Default.Save();
            };


            //イベント発生300ms
            timer.Interval = TimeSpan.FromMilliseconds(300);


            //タイマーイベント
            timer.Tick += delegate
            {
                
                //再生シークバーを移動させる
                if (mediaPlayer.playState == WMPPlayState.wmppsPlaying || mediaPlayer.playState == WMPPlayState.wmppsPaused)
                {
                    try
                    {
                        Slider_Time.Maximum = (int)(mediaPlayer.controls.currentItem.duration * 100);
                        Slider_Time.Value = (int)(mediaPlayer.controls.currentPosition * 100);
                        Label_Time.Content = mediaPlayer.controls.currentPositionString;
                    }
                    catch (Exception)
                    {

                    }

                }


                //自動で次の曲を再生する
                if (mediaPlayer.playState == WMPPlayState.wmppsStopped)
                {
                    SelectedNowPlaylist();
                }
            };

            timer.Start();
        }


        /// <summary>
        /// ×ボタンを押された時にタスクトレイに収納
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //閉じないアプリを
            e.Cancel = true;

            //タスクバーとウィンドウ表示を消す
            ShowInTaskbar = false;
            Visibility = Visibility.Collapsed;

            //バック
            toolStripBack.Click += delegate
            {
                NewPlayBack();
            };

            //再生
            toolStripPlay.Click += delegate
            {
                if (mediaPlayer.playState == WMPPlayState.wmppsPlaying)
                {
                    mediaPlayer.controls.pause();
                    Button_Play.Content = "Play";
                    toolStripPlay.Text = "Play";
                }
                //一時停止だったら再生
                else if (mediaPlayer.playState == WMPPlayState.wmppsPaused)
                {
                    mediaPlayer.controls.play();
                    Button_Play.Content = "Pause";
                    toolStripPlay.Text = "Pause";
                }
                //停止だったら再生
                else
                {
                    SelectedNowPlaylist();
                }
            };

            //スキップ
            toolStripNext.Click += delegate
            {
                SelectedNowPlaylist();
            };

            //playlistクリック
            toolStripPlaylist.DropDownItemClicked += ToolStripPlaylist_DropDownItemClicked;

            //toolStripExitクリック
            toolStripExit.Click += delegate
            {
                notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            };

            //タスクトレイに表示
            notifyIcon.Visible = true;

            notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_click);
        }

        //notifyIconプレイリスト登録
        private void ToolStripPlaylist_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(Label_Title.Content.ToString() != "タイトル")
            {
                string playlistindex = PlaylistURL.Find(x => x.Contains(mediaPlayer.URL));
                if (playlistindex == null)
                {

                    StreamWriter streamWriter = new StreamWriter(appPath + @"\Playlist\" + e.ClickedItem + @".Playlist", true, System.Text.Encoding.UTF8);
                    streamWriter.WriteLine("{0},/ {1},/ {2}", mediaPlayer.URL, Label_Title.Content, Label_Artist.Content);
                    streamWriter.Close();

                    //初期化
                    PlaylistURL.Clear();
                    PlaylistTitle.Clear();
                    PlaylistArtist.Clear();


                    StreamReader streamReader = new StreamReader(appPath + @"\Playlist\" + e.ClickedItem + @".Playlist", System.Text.Encoding.UTF8);
                    var count = File.ReadLines(appPath + @"\Playlist\" + e.ClickedItem + @".Playlist").Count();

                    string[] triger = { ",/ " };

                    //各値に代入
                    for (int i = 0; i < count; i++)
                    {
                        string[] readtemp = streamReader.ReadLine().Split(triger, StringSplitOptions.None);
                        PlaylistURL.Add(readtemp[0]);
                        PlaylistTitle.Add(readtemp[1]);
                        PlaylistArtist.Add(readtemp[2]);
                    }
                    streamReader.Close();

                    Debug.WriteLine("追加完了");
                }
            } 
        }

        /// <summary>
        /// notifyiconを押したときのやつ
        /// </summary>
        private void notifyIcon_click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                notifyIcon.Visible = false;
                ShowInTaskbar = true;
                Visibility = Visibility.Visible;
                Topmost = true;
                Topmost = false;

            }
        }


        ///<summary>
        ///new曲データ取得・セーブメソッド
        ///取得したらプログラム内データも更新
        /// </summary>
        private void NewSaveMusic(string osuplayerFilePath)
        {
            var messagebox = new messagebox();
            messagebox.Show();

            //譜面データをリストアップ
            IEnumerable<string> osuFiles = Directory.GetFiles(Settings.Default.osuURL, "*.osu", SearchOption.AllDirectories);

            StreamWriter streamWriter = new StreamWriter(osuplayerFilePath, false, System.Text.Encoding.UTF8);

            //bool mapflag;

            //内容読み込む？
            foreach (string osufile in osuFiles)
            {

                bool mapflag = true;

                string[] beatmap = new string[6];
                beatmap[5] = "";

                StreamReader streamReader = new StreamReader(osufile);

                //Artistまで1行ずつ読み込む
                while (mapflag)
                {
                    string temp = streamReader.ReadLine();

                    if (temp.Contains("AudioFilename:"))
                    {
                        beatmap[0] = Path.GetDirectoryName(osufile) + @"\" + temp.Remove(0, 14).Trim();
                        continue;
                    }
                    else if (temp.Contains("Title:"))
                    {
                        beatmap[1] = temp.Remove(0, 6);
                        continue;
                    }
                    else if (temp.Contains("TitleUnicode:"))
                    {
                        beatmap[2] = temp.Remove(0, 13);
                        continue;
                    }
                    else if (temp.Contains("Artist:"))
                    {
                        beatmap[3] = temp.Remove(0, 7);
                        continue;
                    }
                    else if (temp.Contains("ArtistUnicode:"))
                    {
                        beatmap[4] = temp.Remove(0, 14);
                        continue;
                    }
                    else if (temp.Contains("Tags:"))
                    {
                        beatmap[5] = temp.Remove(0, 5);
                        continue;
                    }
                    else if (temp.Contains("[Difficulty]"))
                    {
                        mapflag = false;
                        continue;
                    }
                }

                if (beatmap[2] == "")
                    beatmap[2] = beatmap[1];

                if (beatmap[4] == "")
                    beatmap[4] = beatmap[3];


                if (PlayerURL.LastOrDefault() != beatmap[0])
                {
                    streamWriter.WriteLine("{0},/ {1},/ {2},/ {3},/ {4},/ {5}", beatmap[0], beatmap[1], beatmap[2], beatmap[3], beatmap[4], beatmap[5]);
                    PlayerURL.Add(beatmap[0]);
                    SearchTitle.Add(beatmap[1]);
                    PlayerTitle.Add(beatmap[2]);
                    SearchArtist.Add(beatmap[3]);
                    PlayerArtist.Add(beatmap[4]);
                    SearchTags.Add(beatmap[5]);
                }   
            }
            streamWriter.Close();
            messagebox.Close();
        }


        ///<summary>
        ///new曲読み込みメソッド
        /// </summary>
        public void NewReadMusic(string osuplayerFilePath)
        {
            StreamReader streamReader = new StreamReader(osuplayerFilePath, System.Text.Encoding.UTF8);
            var count = File.ReadLines(osuplayerFilePath).Count();

            string[] triger = { ",/ " };

            //各値に代入
            for (int i = 0; i < count; i++)
            {
                string[] readtemp = streamReader.ReadLine().Split(triger, StringSplitOptions.None);
                PlayerURL.Add(readtemp[0]);
                SearchTitle.Add(readtemp[1]);
                PlayerTitle.Add(readtemp[2]);
                SearchArtist.Add(readtemp[3]);
                PlayerArtist.Add(readtemp[4]);
                SearchTags.Add(readtemp[5]);
            }
            streamReader.Close();
        }


        /// <summary>
        /// NewPlayNextメソッド
        /// 次の曲に行きます
        /// </summary>
        public void NewPlayNext(int number, bool playlistbool)
        {
            if(number > 0)
            {
                if (playlistbool)
                {
                    //プレイリスト
                    //PlayNextがあったらそれを読み込む
                    if (PlaylistNext.Count > 0)
                    {
                        number = PlaylistNext[0];
                        PlaylistNext.RemoveAt(0);
                    }

                    //PlayHistryにindexを追加する
                    PlaylistHistory.Add(number);


                    //URLをセットして曲を流す
                    NowPlay = number;
                    mediaPlayer.URL = PlaylistURL[number];
                    Label_Title.Content = PlaylistTitle[number];
                    Label_Artist.Content = PlaylistArtist[number];

                }
                else
                {
                    //普通の再生
                    //PlayNextがあったらそれを読み込む
                    if (PlayNext.Count > 0)
                    {
                        number = PlayNext[0];
                        PlayNext.RemoveAt(0);
                    }

                    //PlayHistryにindexを追加する
                    PlayHistory.Add(number);


                    //URLをセットして曲を流す
                    NowPlay = number;

                    mediaPlayer.URL = PlayerURL[number];
                    Label_Title.Content = PlayerTitle[number];
                    Label_Artist.Content = PlayerArtist[number];

                    int index = PlaylistURL.FindIndex(x => x.Contains(mediaPlayer.URL));
                    if (index == -1)
                        CheckBox_PLaylist.IsChecked = false;
                    else
                        CheckBox_PLaylist.IsChecked = true;
                }
                
                mediaPlayer.controls.play();

                toolStripTitle.Text = Label_Title.Content.ToString();
                toolStripArtist.Text = Label_Artist.Content.ToString();

                Button_Play.Content = "Pause";
                toolStripPlay.Text = "Pause";

                Debug.WriteLine(PlayerURL[number]);
            }
        }


        /// <summary>
        /// NewPLayBackメソッド
        /// 前の曲に戻ります
        /// </summary>
        public void NewPlayBack()
        {
            if (SelectPlaylist == null)
            {
                if (PlayHistory.Count > 1)
                {

                    //変数の定義
                    int number;

                    //現在の曲をPLayNextの0に入れる
                    PlayNext.Insert(0, NowPlay);

                    //PlayHistryから読み込んでそれを再生
                    number = PlayHistory[PlayHistory.Count - 2];
                    PlayHistory.RemoveAt(PlayHistory.Count - 1);

                    //URLをセットして曲を流す
                    NowPlay = number;
                    mediaPlayer.URL = PlayerURL[number];
                    Label_Title.Content = PlayerTitle[number];
                    Label_Artist.Content = PlayerArtist[number];
                    mediaPlayer.controls.play();

                    toolStripTitle.Text = Label_Title.Content.ToString();
                    toolStripArtist.Text = Label_Artist.Content.ToString();

                    Button_Play.Content = "Pause";
                    toolStripPlay.Text = "Pause";

                    Debug.WriteLine(PlayerURL[number]);
                }
            }
            else
            {
                if (PlaylistHistory.Count > 1)
                {
                    //変数の定義
                    int number;

                    //現在の曲をPLayNextの0に入れる
                    PlaylistNext.Insert(0, NowPlay);
    
                    //PlayHistryから読み込んでそれを再生
                    number = PlaylistHistory[PlaylistHistory.Count - 2];
                    PlaylistHistory.RemoveAt(PlaylistHistory.Count - 1);
    
                    //URLをセットして曲を流す
                    NowPlay = number;
                    mediaPlayer.URL = PlaylistURL[number];
                    Label_Title.Content = PlaylistTitle[number];
                    Label_Artist.Content = PlaylistArtist[number];
                    mediaPlayer.controls.play();

                    toolStripTitle.Text = Label_Title.Content.ToString();
                    toolStripArtist.Text = Label_Artist.Content.ToString();

                    Button_Play.Content = "Pause";
                    toolStripPlay.Text = "Pause";

                    Debug.WriteLine(PlaylistURL[number]);
                }
            }
        }


        /// <summary>
        /// newNextIndexメソッド
        /// 次の曲をランダムに決める
        /// </summary>
        public int NewNextIndex(int Max)
        {
            if (PlayerURL.Count == 0)
                return -1;

            Random random = new Random();
            int index;

            //同じ曲(曲名)だったらもう一度
            while (true)
            {
                index = random.Next(0, Max);

                if (Label_Title.Content.ToString() == PlayerTitle[index])
                    continue;

                break;
            }

            return index;
        }


        /// <summary>
        /// osuのファイルパスの設定
        /// </summary>
        public void SetosuPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            folderBrowserDialog.Description = "osu!.exeがあるファイルを選択してください。";
            folderBrowserDialog.SelectedPath = @"C:";

            //ダイヤログを表示する
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(folderBrowserDialog.SelectedPath + @"\osu!.exe"))
                {
                    Settings.Default.osuURL = folderBrowserDialog.SelectedPath + @"\Songs";
                    
                    Debug.WriteLine("osuパス：" + Settings.Default.osuURL);
                }
                else
                {
                    Settings.Default.osuURL = "";
                    System.Windows.MessageBox.Show("パスが違います。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }

            Settings.Default.Save();

            folderBrowserDialog.Dispose();
        }


        //音量の変更時にmaxを超えないように制御
        private void ChangeVolume()
        {
            if (Settings.Default.maxvolume < Settings.Default.volume)
                Settings.Default.volume = Settings.Default.maxvolume;

            Slider_Volume.Maximum = Settings.Default.maxvolume;
            Slider_Volume.Value = Settings.Default.volume;
        }

        
        /// <summary>
        /// プレイリストファイルの更新
        /// </summary>
        private void PlaylistUpdate()
        {
            //選択メニューに4つ以上あったらデフォの3つを残して消す
            int SelectItemCount = MenuItem_Select.Items.Count;

            //元からあるのをコピーしclearした後に貼り付ける
            Object[] menuitemselects = new Object[3];
            menuitemselects[0] = MenuItem_Select.Items[0];
            menuitemselects[1] = MenuItem_Select.Items[1];
            menuitemselects[2] = MenuItem_Select.Items[2];

            //削除
            MenuItem_Select.Items.Clear();
            MenuItem_Delete.Items.Clear();
            toolStripPlaylist.DropDownItems.Clear();

            //貼り付け
            MenuItem_Select.Items.Add(menuitemselects[0]);
            MenuItem_Select.Items.Add(menuitemselects[1]);
            MenuItem_Select.Items.Add(menuitemselects[2]);
            

            //メニューに追加
            if (Directory.Exists(appPath + @"\Playlist"))
            {
                String[] Playlist = Directory.GetFiles(appPath + @"\Playlist", "*.Playlist");
                foreach (string t in Playlist)
                {
                    System.Windows.Controls.MenuItem AddMune = new System.Windows.Controls.MenuItem();
                    System.Windows.Controls.MenuItem delMune = new System.Windows.Controls.MenuItem();
                    string PlaylistString;
                    PlaylistString = t.Replace(appPath + @"\Playlist\", "");
                    PlaylistString = PlaylistString.Replace(".Playlist", "");
                    AddMune.Header = PlaylistString;
                    delMune.Header = PlaylistString;
                    MenuItem_Select.Items.Add(AddMune);
                    MenuItem_Delete.Items.Add(delMune);
                    toolStripPlaylist.DropDownItems.Add(PlaylistString);
                    toolStripPlaylist.Enabled = true;
                    MenuItem_Delete.IsEnabled = true;
                }
                Debug.WriteLine("playlist更新完了。");

                if (MenuItem_Select.Items.Count == 3)
                {
                    MenuItem_Delete.IsEnabled = false;
                    toolStripPlaylist.Enabled = false;
                }
            }
            else
            {
                MenuItem_Delete.IsEnabled = false;
                toolStripPlaylist.Enabled = false;
            }
        }


        /// <summary>
        /// 再生時に関するプレイリストの挙動
        /// </summary>
        private void SelectedNowPlaylist()
        {
            if (SelectPlaylist == null)
            {
                CheckBox_PLaylist.IsChecked = false;
                NewPlayNext(NewNextIndex(PlayerURL.Count), false);
            }
            else
            {
                NewPlayNext(NewNextIndex(PlaylistURL.Count), true);

                int index = PlaylistURL.FindIndex(x => x.Contains(mediaPlayer.URL));
                if (index == -1)
                    CheckBox_PLaylist.IsChecked = false;
                else
                    CheckBox_PLaylist.IsChecked = true;
            }


        }


        //windowでの処理------------------------------------------------------------------------------------

        /// <summary>
        /// スキップ
        /// </summary>
        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            SelectedNowPlaylist();
        }


        /// <summary>
        /// 再生・一時停止
        /// </summary>
        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            //再生していたら停止
            if (mediaPlayer.playState == WMPPlayState.wmppsPlaying)
            {
                mediaPlayer.controls.pause();
                Button_Play.Content = "Play";
                toolStripPlay.Text = "Play";
            }
            //一時停止だったら再生
            else if (mediaPlayer.playState == WMPPlayState.wmppsPaused)
            {
                mediaPlayer.controls.play();
                Button_Play.Content = "Pause";
                toolStripPlay.Text = "Pause";
            }
            //停止だったら再生
            else
            {
                SelectedNowPlaylist();
            }
        }


        /// <summary>
        /// 前の曲に戻る
        /// </summary>
        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            NewPlayBack();
        }


        /// <summary>
        /// プレイリストに曲を入れる
        /// </summary>
        private void CheckBox_PLaylist_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectPlaylist == null || Label_Title.Content.ToString() == "タイトル")
                CheckBox_PLaylist.IsChecked = false;
            else
            {
                if(CheckBox_PLaylist.IsChecked == true)
                {
                    //登録
                    string playlistindex = PlaylistURL.Find(x => x.Contains(mediaPlayer.URL));
                    if (playlistindex == null)
                    {

                        StreamWriter streamWriter = new StreamWriter(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist", true, System.Text.Encoding.UTF8);
                        streamWriter.WriteLine("{0},/ {1},/ {2}", mediaPlayer.URL, Label_Title.Content, Label_Artist.Content);
                        streamWriter.Close();

                        //初期化
                        PlaylistURL.Clear();
                        PlaylistTitle.Clear();
                        PlaylistArtist.Clear();


                        StreamReader streamReader = new StreamReader(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist", System.Text.Encoding.UTF8);
                        var count = File.ReadLines(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist").Count();

                        string[] triger = { ",/ " };

                        //各値に代入
                        for (int i = 0; i < count; i++)
                        {
                            string[] readtemp = streamReader.ReadLine().Split(triger, StringSplitOptions.None);
                            PlaylistURL.Add(readtemp[0]);
                            PlaylistTitle.Add(readtemp[1]);
                            PlaylistArtist.Add(readtemp[2]);
                        }
                        streamReader.Close();

                        Debug.WriteLine("追加完了");
                    }
                   
                }
                else
                {
                    //削除
                    int playlistindex = PlaylistURL.FindIndex(x => x.Contains(mediaPlayer.URL));
                    PlaylistURL.RemoveAt(playlistindex);
                    PlaylistTitle.RemoveAt(playlistindex);
                    PlaylistArtist.RemoveAt(playlistindex);

                    //書き込み治す
                    StreamWriter streamWriter = new StreamWriter(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist", false, System.Text.Encoding.UTF8);

                    for (int i = 0; i < PlaylistURL.Count; i++)
                    {
                        streamWriter.WriteLine("{0},/ {1},/ {2}", PlaylistURL[i], PlaylistTitle[i], PlaylistArtist[i]);
                    }
                    streamWriter.Close();

                    //初期化
                    PlaylistURL.Clear();
                    PlaylistTitle.Clear();
                    PlaylistArtist.Clear();

                    //読み込み治し
                    StreamReader streamReader = new StreamReader(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist", System.Text.Encoding.UTF8);
                    var count = File.ReadLines(appPath + @"\Playlist\" + SelectPlaylist + @".Playlist").Count();

                    string[] triger = { ",/ " };

                    //各値に代入
                    for (int i = 0; i < count; i++)
                    {
                        string[] readtemp = streamReader.ReadLine().Split(triger, StringSplitOptions.None);
                        PlaylistURL.Add(readtemp[0]);
                        PlaylistTitle.Add(readtemp[1]);
                        PlaylistArtist.Add(readtemp[2]);
                    }
                    streamReader.Close();

                    Debug.WriteLine("削除完了");
                }

            }
        }


        //ここからMenuのやつ----------------------------------------------------------------------------------


        //アプリを終了する。 
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        //曲の再読み込み
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("本当に読み込みますか？\r\n曲の数によっては時間がかかる場合があります。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                //曲をセーブしてから読み込みなおす
                try
                {
                    NewSaveMusic(textfilePath);

                    System.Windows.MessageBox.Show("読み込みが終わりました。", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("エラーが起きました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /// <summary>
        ///このアプリについて 
        /// </summary>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Osu! Music Player\r\nversion: 0.5\r\n制作: pantyetta", "アプリについて");
        }


        /// <summary>
        /// osuのパスを変更する
        /// </summary>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            SetosuPath();

        }


        /// <summary>
        /// 設定ウィンドウを開く
        /// </summary>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            var appsettings = new Window1();
            appsettings.Show();

            appsettings.Closed += delegate
            {
                Debug.WriteLine("設定フォーム閉を閉じました。");
                ChangeVolume();

                Debug.WriteLine("MAX音量：" + Settings.Default.maxvolume);

            };

        }


        /// <summary>
        /// 曲検索メソッド
        /// </summary>
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            var window2 = new Window2();
            
            window2.Show();

            window2.Button_serch.Click += delegate
            {
                window2.ListBox_Result.Items.Clear();

                //タイトル(ローマ字・英語)での検索
                foreach (string t in SearchTitle.FindAll(x => x.IndexOf(window2.Text.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    int SearchIndex = SearchTitle.FindIndex(x => x.Contains(t));
                    window2.ListBox_Result.Items.Add(PlayerTitle[SearchIndex] + " -- " + PlayerArtist[SearchIndex]);
                }

                //タイトル(日本語)での検索
                foreach (string t in PlayerTitle.FindAll(x => x.IndexOf(window2.Text.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    int SearchIndex = PlayerTitle.FindIndex(x => x.Contains(t));
                    string clicktemp = PlayerTitle[SearchIndex] + " -- " + PlayerArtist[SearchIndex];
                    bool clickflag = true;

                    foreach (string t2 in window2.ListBox_Result.Items)
                    {
                        if (t2 == clicktemp)
                            clickflag = false;
                    }
                    if (clickflag)
                        window2.ListBox_Result.Items.Add(clicktemp);
                    
                }

                //アーティスト(ローマ字・英語)での検索
                foreach (string t in SearchArtist.FindAll(x => x.IndexOf(window2.Text.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    int SearchIndex = SearchArtist.FindIndex(x => x.Contains(t));
                    string clicktemp = PlayerTitle[SearchIndex] + " -- " + PlayerArtist[SearchIndex];
                    bool clickflag = true;

                    foreach (string t2 in window2.ListBox_Result.Items)
                    {
                        if (t2 == clicktemp)
                            clickflag = false;
                    }
                    if (clickflag)
                        window2.ListBox_Result.Items.Add(clicktemp);
                }

                //アーティスト(日本語)での検索
                foreach (string t in PlayerArtist.FindAll(x => x.IndexOf(window2.Text.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    int SearchIndex = PlayerArtist.FindIndex(x => x.Contains(t));
                    string clicktemp = PlayerTitle[SearchIndex] + " -- " + PlayerArtist[SearchIndex];
                    bool clickflag = true;

                    foreach (string t2 in window2.ListBox_Result.Items)
                    {
                        if (t2 == clicktemp)
                            clickflag = false;
                    }
                    if (clickflag)
                        window2.ListBox_Result.Items.Add(clicktemp);
                }

                //タグでの検索
                foreach (string t in SearchTags.FindAll(x => x.IndexOf(window2.Text.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    int SearchIndex = SearchTags.FindIndex(x => x.Contains(t));
                    string clicktemp = PlayerTitle[SearchIndex] + " -- " + PlayerArtist[SearchIndex];
                    bool clickflag = true;

                    foreach (string t2 in window2.ListBox_Result.Items)
                    {
                        if (t2 == clicktemp)
                            clickflag = false;
                    }
                    if (clickflag)
                        window2.ListBox_Result.Items.Add(clicktemp);
                }
            };

            window2.ListBox_Result.SelectionChanged += delegate
            {
                string[] triger = { " -- " };
                var search = window2.ListBox_Result.SelectedItem;
                if(search != null)
                {
                    string[] SearchTemp =  search.ToString().Split(triger, StringSplitOptions.RemoveEmptyEntries);
                    NewPlayNext(PlayerTitle.FindIndex(x => x.Contains(SearchTemp[0])), false);
                }
            };

        }


        /// <summary>
        ///プレイリストを手動で更新するメソッド 
        /// </summary>
        private void MenuItem_Update_Click(object sender, RoutedEventArgs e)
        {
            PlaylistUpdate();
        }


        /// <summary>
        /// プレイリストを追加するメソッド
        /// </summary>
        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            //フォルダーがあるか調べ無ければ作る
            if (Directory.Exists(appPath + @"\Playlist"))
            {
                //else処理のみ
            }
            else
            {
                Directory.CreateDirectory(appPath + @"\Playlist");
            }

            //ここから保存ダイヤログの設定
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            //初期の保存ネーム
            saveFileDialog.FileName = ".Playlist";

            //保存先の指定
            saveFileDialog.InitialDirectory = @appPath + @"\Playlist";

            //フィルター(ファイル形式の指定)
            saveFileDialog.Filter = "Osu!MusicPlayerファイル(*.Playlist)|*.Playlist|すべてのファイル(*.*)|*.*";

            //タイトル
            saveFileDialog.Title = "プレイリストの保存";

            //初期ディレクトリに戻す
            saveFileDialog.RestoreDirectory = true;

            if(saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //ファイルを作る
                StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8);
                //ここで書き込む
                streamWriter.Close();
            }

            //内容を破棄
            saveFileDialog.Dispose();

            //更新
            PlaylistUpdate();
        }


        /// <summary>
        /// プレイリストを選択する
        /// </summary>
        private void MenuItem_Select_Click(object sender, RoutedEventArgs e)
        {
            
            string[] triger1 = { " Header:" };
            string[] triger2 = { " Items.Count:" };
            string test = e.Source.ToString();
            string[] name;
            string[] header;

            name = test.Split(triger1, StringSplitOptions.None);

            header = name[1].Split(triger2, StringSplitOptions.None);

            if (header[0] == "選択しない")
            {
                SelectPlaylist = null;
                CheckBox_PLaylist.IsChecked = false;
                PlaylistURL.Clear();
                PlaylistTitle.Clear();
                PlaylistArtist.Clear();
                Debug.WriteLine("選択解除");
                return;
            }

            if (header[0] == "追加...")
                return;

            //初期化
            PlaylistURL.Clear();
            PlaylistTitle.Clear();
            PlaylistArtist.Clear();

            if(SelectPlaylist == header[0])
            {
                PlaylistHistory.Clear();
                PlaylistNext.Clear();
            }


            StreamReader streamReader = new StreamReader(appPath + @"\Playlist\" + header[0] + @".Playlist", System.Text.Encoding.UTF8);
            var count = File.ReadLines(appPath + @"\Playlist\" + header[0] + @".Playlist").Count();

            string[] triger3 = { ",/ " };

            //各値に代入
            for (int i = 0; i < count; i++)
            {
                string[] readtemp = streamReader.ReadLine().Split(triger3, StringSplitOptions.None);
                PlaylistURL.Add(readtemp[0]);
                PlaylistTitle.Add(readtemp[1]);
                PlaylistArtist.Add(readtemp[2]);

                if (mediaPlayer.URL == readtemp[0])
                    CheckBox_PLaylist.IsChecked = true;

            }
            streamReader.Close();

            SelectPlaylist = header[0];

            Debug.WriteLine("プレイリスト" + header[0] + "を選択中");
        }


        /// <summary>
        /// プレイリストの削除
        /// </summary>
        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            string[] triger1 = { " Header:" };
            string[] triger2 = { " Items.Count:" };
            string test = e.Source.ToString();
            string[] name;
            string[] header;

            name = test.Split(triger1, StringSplitOptions.None);

            header = name[1].Split(triger2, StringSplitOptions.None);

            if(System.Windows.MessageBox.Show(header[0] + "を削除しますか？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                File.Delete(appPath + @"\Playlist\" + header[0] + @".Playlist");

                PlaylistUpdate();

                System.Windows.MessageBox.Show("プレイリスト" + header[0] + "を削除しました。", "連絡");
            }
        }
    }
}
