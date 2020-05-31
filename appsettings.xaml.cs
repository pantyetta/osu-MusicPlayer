using osu_MusicPlayer.Properties;
using System;
using System.Windows;
using System.Windows.Controls;

namespace osu_MusicPlayer
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        //音量の最大値
        int maxvolume;

        public Window1()
        {
            InitializeComponent();

            //設定ファイルを読み込む
            maxvolume = Settings.Default.maxvolume;

            //設定音量を代入する
            Slider_MaxVolume.Value = maxvolume;
            TextBox_MaxVolume.Text = maxvolume.ToString();

            //音量をテキストボックスに入れる(int型)
            Slider_MaxVolume.ValueChanged += delegate
            {
                maxvolume = (int)Slider_MaxVolume.Value;
                TextBox_MaxVolume.Text = maxvolume.ToString();
                
            };
        }


        //音量テキストボックスをバーに反映させる
        private void TextBox_MaxVolume_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Slider_MaxVolume.Value = int.Parse(TextBox_MaxVolume.Text);
            }
            catch (Exception)
            {
            }            
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //settingsに書き込んで閉じる
            Settings.Default.maxvolume = maxvolume;
            Settings.Default.Save();
            this.Close();
        }
    }
}
