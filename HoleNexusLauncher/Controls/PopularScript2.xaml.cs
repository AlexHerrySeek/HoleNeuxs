using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HoleNexusLauncher.Controls
{
    public partial class PopularScript2 : UserControl
    {
        private readonly string _scriptContent;
        private readonly string _imageUrl;
        public MainWindow MainInstance { get; private set; }

        public PopularScript2(string imgurl, string scriptname, string script)
        {
            InitializeComponent();
            _imageUrl = imgurl;
            _scriptContent = script;
            ScriptNameLabel.Text = scriptname;

            // Tìm cửa sổ MainWindow
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    MainInstance = mainWindow;
                    break;
                }
            }

            Loaded += PopularScript2_Loaded;
        }

        private void PopularScript2_Loaded(object sender, RoutedEventArgs e)
        {
            AddImageToGrid();
        }

        internal static ImageSource ToImage(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return null;

                // Nếu là đường dẫn tương đối từ ScriptBlox
                if (!url.StartsWith("http"))
                    url = "https://scriptblox.com" + url;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(url, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch
            {
                // fallback nếu ảnh lỗi
                return new BitmapImage(new Uri("https://tr.rbxcdn.com/3e86507fbb9beb6431c5747e5596b06d/768/432/Image/Png"));
            }
        }

        private void AddImageToGrid()
        {
            BackImage.ImageSource = ToImage(_imageUrl);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_scriptContent);
                MessageBox.Show("✅ Script copied to clipboard!", "HoleNexusLauncher", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy script: " + ex.Message);
            }
        }
    }
}
