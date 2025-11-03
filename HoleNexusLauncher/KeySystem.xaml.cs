using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Newtonsoft.Json.Linq;

namespace HoleNexusLauncher
{
    /// <summary>
    /// Interaction logic for KeySystem.xaml
    /// </summary>
    public partial class KeySystem : Window
    {

        public KeySystem()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #region anims
        Storyboard storyboard = new Storyboard();

        TimeSpan halfsecond = TimeSpan.FromMilliseconds(500);
        TimeSpan second = TimeSpan.FromSeconds(1);

        private IEasingFunction Smooth
        {
            get;
            set;
        }
        = new QuarticEase
        {
            EasingMode = EasingMode.EaseOut
        };

        public void Fade(DependencyObject Object)
        {
            DoubleAnimation FadeIn = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(halfsecond),
            };
            Storyboard.SetTarget(FadeIn, Object);
            Storyboard.SetTargetProperty(FadeIn, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(FadeIn);
            storyboard.Begin();
        }

        public void FadeOut(DependencyObject Object)
        {
            DoubleAnimation FadeOut = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(halfsecond),
            };
            Storyboard.SetTarget(FadeOut, Object);
            Storyboard.SetTargetProperty(FadeOut, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(FadeOut);
            storyboard.Begin();
        }
        #endregion anims

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Fade(this.MainBorder);
        }

        private async void close_Click(object sender, RoutedEventArgs e)
        {
            FadeOut(this.MainBorder);
            await Task.Delay(1000);
            Close();
        }

        private async void _Login_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void _GetKey_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
