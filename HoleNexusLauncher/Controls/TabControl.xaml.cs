using HoleNexus.Classes;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HoleNexusLauncher.Controls
{
    public partial class TabControl : UserControl
    {
        public string script = "-- Tao Thích Hà Quỳnh Anh OK?";
        public bool SelectedTab;
        public StackPanel Parent { get; set; }
        public MainWindow MainInstance { get; set; }

        public TabControl(StackPanel Parent, MainWindow Instance)
        {
            InitializeComponent();
            this.Parent = Parent;
            this.MainInstance = Instance;
        }

        public async void Select()
        {
            try
            {
                this.SelectedTab = true;

                if (this.MainInstance?.browser?.CoreWebView2 == null)
                    return;

                if (Common.SelectedTab != null && Common.SelectedTab != this)
                {
                    Common.SelectedTab.TabLabel.Visibility = Visibility.Visible;

                    if (this.MainInstance?.browser?.CoreWebView2 != null)
                        Common.SelectedTab.script = await this.MainInstance.GetText();

                    Common.SelectedTab.Deselect();
                }

                if (this.MainInstance?.browser?.CoreWebView2 != null)
                    await this.MainInstance.SetText(this.script);

                Common.SelectedTab = this;

                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1),
                    GradientStops = new GradientStopCollection
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#FF06080D"), 0),
                new GradientStop((Color)ColorConverter.ConvertFromString("#FF1F2B55"), 1)
            }
                };

                MainBorder.Background = gradientBrush;
                MainBorder.BorderThickness = new Thickness(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TabControl.Select] Lỗi: {ex.Message}");
            }
        }

        private async void buttonclose_Click(object sender, RoutedEventArgs e)
        {
            if (Common.SelectedTab == this)
                this.script = await this.MainInstance.GetText();

            int index = Parent.Children.IndexOf(this);
            if (Parent.Children.Count > 1)
            {
                if (index > 0)
                    ((TabControl)Parent.Children[index - 1]).Select();
                else if (Parent.Children.Count > 1)
                    ((TabControl)Parent.Children[index + 1]).Select();
            }

            this.MainInstance.TabPanel.Children.Remove(this);

            if (this.MainInstance.TabPanel.Children.Count == 0)
            {
                this.MainInstance.browser.Visibility = Visibility.Hidden;
            }
        }

        public void Deselect()
        {
            this.SelectedTab = false;
            Common.SelectedTab = null;
            Common.PreviousTab = this;
            this.MainBorder.Background = new SolidColorBrush(Color.FromRgb(27, 33, 52));
            MainBorder.BorderThickness = new Thickness(0);
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Select();
        }
    }
}
