using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using static HoleNexusLauncher.MainWindow;

namespace HoleNexusLauncher
{
    public partial class Loading : Window
    {
        WebClient WebStuff = new WebClient();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // show = 5, hide = 0

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole(); // Tạo mới console nếu chưa có

        public Loading()
        {
            InitializeComponent();
            ShowConsole();
            Console.Title = "HoleNexus | Loading...";
            Console.WriteLine(" ██░ ██  ▒█████   ██▓    ▓█████  ███▄    █ ▓█████ ▒██   ██▒ █    ██   ██████ \r\n▓██░ ██▒▒██▒  ██▒▓██▒    ▓█   ▀  ██ ▀█   █ ▓█   ▀ ▒▒ █ █ ▒░ ██  ▓██▒▒██    ▒ \r\n▒██▀▀██░▒██░  ██▒▒██░    ▒███   ▓██  ▀█ ██▒▒███   ░░  █   ░▓██  ▒██░░ ▓██▄   \r\n░▓█ ░██ ▒██   ██░▒██░    ▒▓█  ▄ ▓██▒  ▐▌██▒▒▓█  ▄  ░ █ █ ▒ ▓▓█  ░██░  ▒   ██▒\r\n░▓█▒░██▓░ ████▓▒░░██████▒░▒████▒▒██░   ▓██░░▒████▒▒██▒ ▒██▒▒▒█████▓ ▒██████▒▒\r\n ▒ ░░▒░▒░ ▒░▒░▒░ ░ ▒░▓  ░░░ ▒░ ░░ ▒░   ▒ ▒ ░░ ▒░ ░▒▒ ░ ░▓ ░░▒▓▒ ▒ ▒ ▒ ▒▓▒ ▒ ░\r\n ▒ ░▒░ ░  ░ ▒ ▒░ ░ ░ ▒  ░ ░ ░  ░░ ░░   ░ ▒░ ░ ░  ░░░   ░▒ ░░░▒░ ░ ░ ░ ░▒  ░ ░\r\n ░  ░░ ░░ ░ ░ ▒    ░ ░      ░      ░   ░ ░    ░    ░    ░   ░░░ ░ ░ ░  ░  ░  \r\n ░  ░  ░    ░ ░      ░  ░   ░  ░         ░    ░  ░ ░    ░     ░           ░  \r\n Welcome, To HoleNexus! Wait Download Assets To Using...");
        }

        public static void ShowConsole()
        {
            IntPtr handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole(); // Nếu app chưa có console -> tạo mới
            }
            else
            {
                ShowWindow(handle, 5); // 5 = SW_SHOW
            }
        }

        public static void HideConsole()
        {
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, 0); // 0 = SW_HIDE
            }
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
        private async Task KiemTraTaiNguyenMonacoAsync()
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

        #region Kiểm Tra Tài Nguyên API
        private async Task KiemTraTaiNguyenAPIAsync()
        {
            try
            {
                RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\HoleNexusWRDWrapper");
                string WRDVer = SettingReg?.GetValue("WrapperVersion")?.ToString();

                if (WRDVer != null)
                {
                    string VersionWRDWrapper = WebStuff.DownloadString("https://raw.githubusercontent.com/AlexHerrySeek/HoleNexus/refs/heads/main/backend/VersionWRDWrapper");
                    if (WRDVer != VersionWRDWrapper.Split(new[] { '\r', '\n' }).FirstOrDefault())
                    {
                        Console.WriteLine("Wrapper not up to date, downloading new version");

                        string[] filesToDelete = new[]
                        {
                    "HoleNexusWRDWrapper.deps.json",
                    "HoleNexusWRDWrapper.dll",
                    "HoleNexusWRDWrapper.exe",
                    "HoleNexusWRDWrapper.pdb",
                    "HoleNexusWRDWrapper.runtimeconfig.json",
                    "WRDFakeServer.exe"
                };

                        foreach (string file in filesToDelete)
                        {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }

                        Console.WriteLine("Deleted old files");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt to check WRD wrapper version resulted in error: {ex}");
            }

            // Tải file nếu chưa có
            if (!File.Exists("HoleNexusWRDWrapper.deps.json"))
            {
                Console.WriteLine("Downloading HoleNexusWRDWrapper.deps.json...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/WRDWrapper/HoleNexusWRDWrapper.deps.json", "HoleNexusWRDWrapper.deps.json");
            }
            if (!File.Exists("HoleNexusWRDWrapper.dll"))
            {
                Console.WriteLine("Downloading HoleNexusWRDWrapper.dll...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/WRDWrapper/HoleNexusWRDWrapper.dll", "HoleNexusWRDWrapper.dll");
            }
            if (!File.Exists("HoleNexusWRDWrapper.exe"))
            {
                Console.WriteLine("Downloading HoleNexusWRDWrapper.exe...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/WRDWrapper/HoleNexusWRDWrapper.exe", "HoleNexusWRDWrapper.exe");
            }
            if (!File.Exists("HoleNexusWRDWrapper.pdb"))
            {
                Console.WriteLine("Downloading HoleNexusWRDWrapper.pdb...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/WRDWrapper/HoleNexusWRDWrapper.pdb", "HoleNexusWRDWrapper.pdb");
            }
            if (!File.Exists("HoleNexusWRDWrapper.runtimeconfig.json"))
            {
                Console.WriteLine("Downloading HoleNexusWRDWrapper.runtimeconfig.json...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/WRDWrapper/HoleNexusWRDWrapper.runtimeconfig.json", "HoleNexusWRDWrapper.runtimeconfig.json");
            }
            if (!File.Exists("WRDFakeServer.exe"))
            {
                Console.WriteLine("Downloading WRDFakeServer.exe...");
                WebStuff.DownloadFile("https://github.com/AlexHerrySeek/HoleNexus/releases/download/AlexHerry/WRDFakeServer.exe", "WRDFakeServer.exe");
            }

            if (!File.Exists("wearedevs_exploit_api.dll"))
            {
                Console.WriteLine("Downloading wearedevs_exploit_api.dll...");
                WebStuff.DownloadFile("https://wrdcdn.net/r/2/exploit%20api/wearedevs_exploit_api.dll", "wearedevs_exploit_api.dll");
            }

            // OpenSSL
            Console.WriteLine("Checking OpenSSL dependencies...");
            if (!Directory.Exists("OpenSSL"))
                Directory.CreateDirectory("OpenSSL");

            string[] opensslFiles = {
        "msys-2.0.dll",
        "msys-crypto-3.dll",
        "msys-ssl-3.dll",
        "openssl.exe"
    };
            foreach (var file in opensslFiles)
            {
                string path = $"OpenSSL\\{file}";
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Downloading {file}...");
                    WebStuff.DownloadFile($"https://github.com/AlexHerrySeek/HoleNexus/raw/refs/heads/main/backend/OpenSSL/{file}", path);
                }
            }

            Console.WriteLine("All done Install!");
        }
        #endregion

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra tài nguyên song song với animation
            var checkMonaco = KiemTraTaiNguyenMonacoAsync();
            var checkAPI = KiemTraTaiNguyenAPIAsync();
            await Task.WhenAll(checkMonaco, checkAPI);

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

            await checkMonaco;
            await checkAPI;

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
