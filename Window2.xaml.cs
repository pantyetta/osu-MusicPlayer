using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace osu_MusicPlayer
{
    /// <summary>
    /// Window2.xaml の相互作用ロジック
    /// </summary>
    public partial class Window2 : Window
    {
        MainWindow mainWindow = new MainWindow();

        public Window2()
        {
            InitializeComponent();
            Text.Focus();
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            var bc = new BrushConverter();
            if (Text.Text == "")
                Text.Background = (Brush)bc.ConvertFrom("#00FFFFFF");
            else
                Text.Background = (Brush)bc.ConvertFrom("#FFFFFFFF");
        }
    }
}
