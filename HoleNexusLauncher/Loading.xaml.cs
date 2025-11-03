using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using static HoleNexusLauncher.MainWindow;

namespace HoleNexusLauncher
{
    public partial class Loading : Window
    {
        public Loading()
        {
            InitializeComponent();
        }

        #region Hiệu Ứng
        Storyboard storyboard = new Storyboard();
        Storyboard logostoryboard = new Storyboard();
        Storyboard logostoryboard2 = new Storyboard();

        TimeSpan halfsecond = TimeSpan.FromMilliseconds(500);
        TimeSpan second = TimeSpan.FromSeconds(1);

        private IEasingFunction Smooth { get; set; } = new QuarticEase
        {
            EasingMode = EasingMode.EaseOut
        };

        public void Fade(DependencyObject obj)
        {
            DoubleAnimation fadeIn = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(halfsecond),
            };
            Storyboard.SetTarget(fadeIn, obj);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(fadeIn);
            storyboard.Begin();
        }

        public void FadeOut(DependencyObject obj)
        {
            DoubleAnimation fadeOut = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(halfsecond),
            };
            Storyboard.SetTarget(fadeOut, obj);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(fadeOut);
            storyboard.Begin();
        }

        public void ObjectShiftPos(DependencyObject obj, Thickness get, Thickness set)
        {
            ThicknessAnimation shift = new ThicknessAnimation()
            {
                From = get,
                To = set,
                Duration = second,
                EasingFunction = Smooth,
            };
            Storyboard.SetTarget(shift, obj);
            Storyboard.SetTargetProperty(shift, new PropertyPath(MarginProperty));
            storyboard.Children.Add(shift);
            storyboard.Begin();
        }

        public void Resize()
        {
            double newLeft = 30;
            double targetWidth = 205.526666666667;
            double targetWidth2 = 238.563333333333;
            Canvas.SetLeft(name, 142);
            Canvas.SetTop(name, 47);
            Canvas.SetLeft(name2, 142);
            Canvas.SetTop(name2, 90);

            DoubleAnimation danimationX = new DoubleAnimation
            {
                From = MainBorder.Width,
                To = 450,
                Duration = second,
                EasingFunction = Smooth
            };
            MainBorder.BeginAnimation(WidthProperty, danimationX);

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(widthAnimation, name);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(WidthProperty));
            logostoryboard.Children.Add(widthAnimation);

            DoubleAnimation widthAnimation2 = new DoubleAnimation
            {
                To = targetWidth2,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(widthAnimation2, name2);
            Storyboard.SetTargetProperty(widthAnimation2, new PropertyPath(WidthProperty));
            logostoryboard.Children.Add(widthAnimation2);

            DoubleAnimation leftAnimation = new DoubleAnimation
            {
                To = newLeft,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(leftAnimation, logo);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));
            logostoryboard.Children.Add(leftAnimation);

            logostoryboard.Begin();

            DoubleAnimation danimationY = new DoubleAnimation
            {
                From = MainBorder.Height,
                To = 200,
                Duration = second,
                EasingFunction = Smooth
            };
            MainBorder.BeginAnimation(HeightProperty, danimationY);
        }

        public void Resize2()
        {
            Panel.SetZIndex(name, -1);
            Panel.SetZIndex(name2, -1);
            double originalWidth = 0;
            double originalLogoLeft = 135;

            DoubleAnimation danimationX = new DoubleAnimation
            {
                From = MainBorder.Width,
                To = 700,
                Duration = second,
                EasingFunction = Smooth
            };
            MainBorder.BeginAnimation(WidthProperty, danimationX);

            DoubleAnimation danimationY = new DoubleAnimation
            {
                From = MainBorder.Height,
                To = 452,
                Duration = second,
                EasingFunction = Smooth
            };
            MainBorder.BeginAnimation(HeightProperty, danimationY);

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = originalWidth,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(widthAnimation, name);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(WidthProperty));
            logostoryboard2.Children.Add(widthAnimation);

            DoubleAnimation widthAnimation2 = new DoubleAnimation
            {
                To = originalWidth,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(widthAnimation2, name2);
            Storyboard.SetTargetProperty(widthAnimation2, new PropertyPath(WidthProperty));
            logostoryboard2.Children.Add(widthAnimation2);

            DoubleAnimation leftAnimation = new DoubleAnimation
            {
                To = originalLogoLeft,
                Duration = second,
                EasingFunction = Smooth
            };
            Storyboard.SetTarget(leftAnimation, logo);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));
            logostoryboard2.Children.Add(leftAnimation);

            logostoryboard2.Begin();
        }
        #endregion

        #region Kiểm Tra Tài Nguyên Monaco
        private async Task KiemTraTaiNguyenAsync()
        {
            string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Monaco");
            string zipUrl = "https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/Monaco.zip";
            string tempZip = Path.Combine(Path.GetTempPath(), "MonacoQuynhAnh.zip");

            try
            {
                if (!Directory.Exists(assetsPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(assetsPath));
                    Console.WriteLine("Downloading Monaco assets...");

                    using (HttpClient client = new HttpClient())
                    {
                        var data = await client.GetByteArrayAsync(zipUrl);
                        await File.WriteAllBytesAsync(tempZip, data);
                    }

                    Console.WriteLine("Extracting assets...");
                    string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
                    ZipFile.ExtractToDirectory(tempZip, extractPath, true);

                    File.Delete(tempZip);
                    Console.WriteLine("Monaco setup complete. Restarting application...");

                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });

                    Environment.Exit(0);
                }
                else
                {
                //    Console.WriteLine("Monaco assets found.");
                }

                await UpdateManager.CheckForUpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking resources: {ex.Message}");
            }
        }
        #endregion

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra tài nguyên song song với animation
            var checkTask = KiemTraTaiNguyenAsync();

            Fade(MainBorder);
            ObjectShiftPos(MainBorder, MainBorder.Margin, new Thickness(0));
            await Task.Delay(2000);

            Resize();
            await Task.Delay(2000);

            Resize2();
            await Task.Delay(2000);

            FadeOut(this.MainBorder);
            await Task.Delay(1000);
            MainBorder.Visibility = Visibility.Collapsed;
            await Task.Delay(500);

            await checkTask;

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}
