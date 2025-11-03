using HoleNexusLauncher.Controls;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HoleNexusLauncher.Execution;
using Path = System.IO.Path;

namespace HoleNexusLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public bool IsVisible = true;
    public WebView2 browser;
    private HttpClient HttpClient = new HttpClient();
    private static readonly HttpClient httpClient = new HttpClient();
    private int currentPage = 1;
    private bool isLoading = false;
    private const int maxPages = 10;
    public class DataModel
    {
        public List<string> changelogs { get; set; }
        public List<string> news { get; set; }
    }

    bool IsInjected = false;
    bool InjectionInProgress = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UserNameLabel.Content = Environment.UserName;
        Information("Welcome back, " + Environment.UserName + "!", 4);
        LoginLabel.Content = "Last Login: " + DateTime.Now;
        LoadGitHubData();
        GetScripts();
        _ = LoadPopularScriptsAsync();
        initbrowser();
        EditorHolder.Children.Add(browser);
        FillTreeView(MainScriptsHolder, MainScriptsHolder, BaseSscriptDirectory);
    }

    #region Kiểm Tra Cập Nhật
    public static class UpdateManager
    {
        private const string VersionUrl = "https://raw.githubusercontent.com/AlexHerrySeek/HoleNeuxs/refs/heads/main/backend/vesion";
        private const string UpdateUrl = "https://raw.githubusercontent.com/YourUser/YourRepo/main/Setup.exe"; 
        private const string CurrentVersion = "0.0.4"; // <-- Thay đổi phiên bản hiện tại ở đây

        public static async Task CheckForUpdateAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string latestVersion = await client.GetStringAsync(VersionUrl);
                    latestVersion = latestVersion.Trim();

                    if (IsNewerVersion(latestVersion, CurrentVersion))
                    {
                        var result = MessageBox.Show(
                            $"A new update is available ({latestVersion})!\nWould you like to download and install it now?","Update Available",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            await DownloadAndInstallUpdateAsync();
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to check for updates.\n{ex.Message}\n\nThe application will now close.","Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private static bool IsNewerVersion(string online, string local)
        {
            try
            {
                Version vOnline = new Version(online);
                Version vLocal = new Version(local);
                return vOnline > vLocal;
            }
            catch
            {
                return false;
            }
        }

        private static async Task DownloadAndInstallUpdateAsync()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "HoleNexusBootstrapper.exe");

                using (HttpClient client = new HttpClient())
                using (var response = await client.GetAsync(UpdateUrl))
                {
                    response.EnsureSuccessStatusCode();
                    await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                MessageBox.Show("Installing the latest update...", "Updating", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download or install the update.\n{ex.Message}\n\nThe application will now close.","Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
    #endregion

    #region Lấy Thông Tin Từ Github
    private async void LoadGitHubData()
    {
        try
        {
            string url = "https://raw.githubusercontent.com/NguyenNhatIT/HoleNeuxs/refs/heads/main/backend/info.json";
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(url);
                DataModel data = JsonConvert.DeserializeObject<DataModel>(json);
                ChangelogsPanel1.Children.Clear();
                NewPanel1.Children.Clear();
                if (data.changelogs != null)
                {
                    foreach (string log in data.changelogs)
                    {
                        Changelog changelogItem = new Changelog(log);
                        ChangelogsPanel1.Children.Add(changelogItem);
                    }
                }

                if (data.news != null)
                {
                    foreach (string n in data.news)
                    {
                        News newsItem = new News(n);
                        NewPanel1.Children.Add(newsItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Window Buttons
    private void CloseClick(object sender, MouseButtonEventArgs e)
    {
        this.Close();
    }
    private void MaxClick(object sender, MouseButtonEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
            return;
        }
        WindowState = WindowState.Normal;
    }
    private void MinClick(object sender, MouseButtonEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
    #endregion

    #region Navigation Animations
    void AnimateButtonSwap(Border expandTarget, Border shrinkTarget, Border shrinkTarget2, Border shrinkTarget3, Border shrinkTarget4)
    {
        var expandWidth = new DoubleAnimation(80, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuadraticEase() };
        var expandHeight = new DoubleAnimation(29, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuadraticEase() };
        expandTarget.BeginAnimation(FrameworkElement.WidthProperty, expandWidth);
        expandTarget.BeginAnimation(FrameworkElement.HeightProperty, expandHeight);

        var shrinkWidth = new DoubleAnimation(33, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuadraticEase() };
        var shrinkHeight = new DoubleAnimation(29, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuadraticEase() };
        shrinkTarget.BeginAnimation(FrameworkElement.WidthProperty, shrinkWidth);
        shrinkTarget.BeginAnimation(FrameworkElement.HeightProperty, shrinkHeight);
        shrinkTarget2.BeginAnimation(FrameworkElement.WidthProperty, shrinkWidth);
        shrinkTarget2.BeginAnimation(FrameworkElement.HeightProperty, shrinkHeight);
        shrinkTarget3.BeginAnimation(FrameworkElement.WidthProperty, shrinkWidth);
        shrinkTarget3.BeginAnimation(FrameworkElement.HeightProperty, shrinkHeight);
        shrinkTarget4.BeginAnimation(FrameworkElement.WidthProperty, shrinkWidth);
        shrinkTarget4.BeginAnimation(FrameworkElement.HeightProperty, shrinkHeight);

        AnimateGradient(expandTarget.Background as LinearGradientBrush, "#FF06080D", "#FF1F2B55");
        AnimateGradient(expandTarget.BorderBrush as LinearGradientBrush, "#FF232A42", "#FF586BA8");

        AnimateGradient(shrinkTarget.Background as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget.BorderBrush as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget2.Background as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget2.BorderBrush as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget3.Background as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget3.BorderBrush as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget4.Background as LinearGradientBrush, "#0A0D16", "#0A0D16");
        AnimateGradient(shrinkTarget4.BorderBrush as LinearGradientBrush, "#0A0D16", "#0A0D16");

        var expandLabel = FindLabel(expandTarget);
        if (expandLabel != null)
        {
            expandLabel.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
            expandLabel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        var shrinkLabel = FindLabel(shrinkTarget);
        if (shrinkLabel != null)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => shrinkLabel.Visibility = Visibility.Hidden;
            shrinkLabel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        var shrinkLabel2 = FindLabel(shrinkTarget2);
        if (shrinkLabel2 != null)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => shrinkLabel2.Visibility = Visibility.Hidden;
            shrinkLabel2.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        var shrinkLabel3 = FindLabel(shrinkTarget3);
        if (shrinkLabel3 != null)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => shrinkLabel3.Visibility = Visibility.Hidden;
            shrinkLabel3.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        var shrinkLabel4 = FindLabel(shrinkTarget3);
        if (shrinkLabel4 != null)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => shrinkLabel4.Visibility = Visibility.Hidden;
            shrinkLabel4.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    void AnimateGradient(LinearGradientBrush brush, string color1Hex, string color2Hex)
    {
        if (brush == null || brush.GradientStops.Count < 2) return;

        var color1 = (Color)ColorConverter.ConvertFromString(color1Hex);
        var color2 = (Color)ColorConverter.ConvertFromString(color2Hex);

        var anim1 = new ColorAnimation(color1, TimeSpan.FromMilliseconds(400));
        var anim2 = new ColorAnimation(color2, TimeSpan.FromMilliseconds(400));

        brush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, anim1);
        brush.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, anim2);
    }

    Label? FindLabel(Border border)
    {
        foreach (var child in ((Grid)border.Child).Children)
        {
            if (child is Label lbl)
                return lbl;
        }
        return null;
    }
    #endregion

    #region Navigation Buttons
    private void HomeBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        EditorHolder.Visibility = Visibility.Hidden;
        AnimateButtonSwap(HomeBtn, EditorBtn, ScriptsBtn, ChatBtn, SettingsBtn);
        SwitchScriptPages(Homepage, EditorPage, ScriptsPage, ChatPage, SettingsPage);
    }

    private void EditorBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        EditorHolder.Visibility = Visibility.Visible;
        AnimateButtonSwap(EditorBtn, HomeBtn, ScriptsBtn, ChatBtn, SettingsBtn);
        SwitchScriptPages(EditorPage, Homepage, ScriptsPage, ChatPage, SettingsPage);
    }

    private void ScriptsBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        EditorHolder.Visibility = Visibility.Hidden;
        AnimateButtonSwap(ScriptsBtn, HomeBtn, EditorBtn, ChatBtn, SettingsBtn);
        SwitchScriptPages(ScriptsPage, Homepage, EditorPage, ChatPage, SettingsPage);
    }

    private void ChatBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        EditorHolder.Visibility = Visibility.Hidden;
        AnimateButtonSwap(ChatBtn, HomeBtn, EditorBtn, ScriptsBtn, SettingsBtn);
        SwitchScriptPages(ChatPage, Homepage, EditorPage, ScriptsPage, SettingsPage);
    }

    private void SettingsBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        EditorHolder.Visibility = Visibility.Hidden;
        AnimateButtonSwap(SettingsBtn, HomeBtn, EditorBtn, ScriptsBtn, ChatBtn);
        SwitchScriptPages(SettingsPage, Homepage, EditorPage, ScriptsPage, ChatPage);
    }
    #endregion

    #region Scripts
    private async void GetScripts()
    {
        try
        {
            PopularScripts.Children.Clear();
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://scriptblox.com/api/script/trending");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error Connent ScriptBlox!");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            dynamic dynJson = JsonConvert.DeserializeObject(json);

            foreach (dynamic item in dynJson.result.scripts)
            {
                string title = Convert.ToString(item.title);
                string slug = Convert.ToString(item.slug);
                string imageUrl = Convert.ToString(item.game.imageUrl);
                if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/no-script.webp")
                    imageUrl = Convert.ToString(item.image);

                if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http"))
                    imageUrl = "https://scriptblox.com" + imageUrl;
                string script = "";
                try
                {
                    HttpResponseMessage scriptRes = await client.GetAsync($"https://scriptblox.com/api/script/{slug}");
                    if (scriptRes.IsSuccessStatusCode)
                    {
                        string scriptRaw = await scriptRes.Content.ReadAsStringAsync();
                        dynamic scriptJson = JsonConvert.DeserializeObject(scriptRaw);
                        script = Convert.ToString(scriptJson.result.script);
                    }
                    else
                    {
                        script = "-- Script not available --";
                    }
                }
                catch
                {
                    script = "-- Script not available --";
                }
                if (!string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(imageUrl))
                {
                    PopularScript pop = new PopularScript(imageUrl, title, script);
                    pop.MouseWheel += new MouseWheelEventHandler(ScrollPopular);
                    PopularScripts.Children.Add(pop);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Something went wrong fetching scripts!\n{ex.Message}");
        }
    }
    #endregion

    #region Thông Báo Toastr
    private void Success(string text, int time)
    {
        var notification = new HoleNexusLauncher.Controls.ThongBao
        {
            BackgroundColor = Colors.White,
            ProgressColor = Colors.SeaGreen,
            BProgressColor = Colors.LightGreen,
            NotificationImage = new BitmapImage(new Uri("pack://application:,,,/Assets/succeded.png")),
            NotificationText = text,
            Time = time
        };

        NotificationsContainer.Children.Add(notification);
        notification.StartNotification();
    }

    private void Error(string text, int time)
    {
        var notification = new HoleNexusLauncher.Controls.ThongBao
        {
            BackgroundColor = Colors.White,
            ProgressColor = Colors.DarkRed,
            BProgressColor = Colors.LightPink,
            NotificationImage = new BitmapImage(new Uri("pack://application:,,,/Assets/errored.png")),
            NotificationText = text,
            Time = time
        };

        NotificationsContainer.Children.Add(notification);
        notification.StartNotification();
    }

    private void Warning(string text, int time)
    {
        var notification = new HoleNexusLauncher.Controls.ThongBao
        {
            BackgroundColor = Colors.White,
            ProgressColor = Colors.Goldenrod,
            BProgressColor = Colors.LightGoldenrodYellow,
            NotificationImage = new BitmapImage(new Uri("pack://application:,,,/Assets/warned.png")),
            NotificationText = text,
            Time = time
        };

        NotificationsContainer.Children.Add(notification);
        notification.StartNotification();
    }

    private void Information(string text, int time)
    {
        var notification = new HoleNexusLauncher.Controls.ThongBao
        {
            BackgroundColor = Colors.White,
            ProgressColor = Colors.DodgerBlue,
            BProgressColor = Colors.LightBlue,
            NotificationImage = new BitmapImage(new Uri("pack://application:,,,/Assets/informed.png")),
            NotificationText = text,
            Time = time
        };

        NotificationsContainer.Children.Add(notification);
        notification.StartNotification();
    }
    #endregion

    #region Navigation
    private void ScrollNews(object sender, MouseWheelEventArgs e)
    {
        this.NewsPanel.ScrollToHorizontalOffset(this.NewsPanel.VerticalOffset + (double)(e.Delta / 10));
    }
    private void ScrollPopular(object sender, MouseWheelEventArgs e)
    {
        this.PopularScripts1.ScrollToHorizontalOffset(this.PopularScripts1.HorizontalOffset + (double)(e.Delta / 10));
    }
    private void PopularScripts1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;

        if (e.Delta > 0)
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 50);
        else
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 50);

        e.Handled = true;
    }
    public async void SwitchScriptPages(UIElement openElement, UIElement Close1, UIElement Close2, UIElement Close3, UIElement Close4)
    {
        openElement.Visibility = Visibility.Visible;
        Storyboard scriptStoryboard = new Storyboard();

        DoubleAnimation openAnim = new DoubleAnimation()
        {
            To = 1,
            Duration = TimeSpan.FromSeconds(.2)
        };
        DoubleAnimation close1Anim = new DoubleAnimation()
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(.2)
        };
        DoubleAnimation close2Anim = new DoubleAnimation()
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(.3)
        };
        DoubleAnimation close3Anim = new DoubleAnimation()
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(.3)
        };
        DoubleAnimation close4Anim = new DoubleAnimation()
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(.3)
        };

        Storyboard.SetTarget(openAnim, openElement);
        Storyboard.SetTarget(close1Anim, Close1);
        Storyboard.SetTarget(close2Anim, Close2);
        Storyboard.SetTarget(close3Anim, Close3);
        Storyboard.SetTarget(close4Anim, Close4);
        Storyboard.SetTargetProperty(openAnim, new PropertyPath(OpacityProperty));
        Storyboard.SetTargetProperty(close1Anim, new PropertyPath(OpacityProperty));
        Storyboard.SetTargetProperty(close2Anim, new PropertyPath(OpacityProperty));
        Storyboard.SetTargetProperty(close3Anim, new PropertyPath(OpacityProperty));
        Storyboard.SetTargetProperty(close4Anim, new PropertyPath(OpacityProperty));

        scriptStoryboard.Children.Add(openAnim);
        scriptStoryboard.Children.Add(close1Anim);
        scriptStoryboard.Children.Add(close2Anim);
        scriptStoryboard.Children.Add(close3Anim);
        scriptStoryboard.Children.Add(close4Anim);

        scriptStoryboard.Begin();

        await Task.Delay(300);
        Close1.Visibility = Visibility.Hidden;
        Close2.Visibility = Visibility.Hidden;
        Close3.Visibility = Visibility.Hidden;
        Close4.Visibility = Visibility.Hidden;
    }

    #endregion

    #region Monaco Editor
    public void initbrowser()
    {
        browser = new WebView2();
        string localPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Monaco", "index.html");
        browser.Source = new Uri(localPath);
        browser.NavigationCompleted += (sender, e) =>
        {

        };
        if (TabPanel.Children.Count == 0)
        {
            browser.Visibility = Visibility.Hidden;
        }
    }

    internal async Task<string> GetText()
    {
        if (browser?.CoreWebView2 == null)
            return "";

        string response = await browser.ExecuteScriptAsync("GetText()");
        return System.Text.Json.JsonSerializer.Deserialize<string>(response);
    }

    internal async Task SetText(string text)
    {
        if (browser?.CoreWebView2 == null)
            return;

        text = text.Replace("`", "\\`");
        await browser.ExecuteScriptAsync($"SetText(`{text}`);");
    }

    public async Task ClearText()
    {
        if (browser?.CoreWebView2 == null)
            return;

        await browser.ExecuteScriptAsync("SetText(``);");
    }

    public async Task OpenFile()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Lua Script (*.lua)|*.lua|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            string text = await File.ReadAllTextAsync(dlg.FileName);
            await SetText(text);
        }
    }

    public async Task SaveFile()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Lua Script (*.lua)|*.lua|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            FileName = "script.lua"
        };

        if (dlg.ShowDialog() == true)
        {
            string text = await GetText();
            await File.WriteAllTextAsync(dlg.FileName, text);
        }
    }
    #endregion

    #region Tabs
    private void ScrollTabs(object sender, MouseWheelEventArgs e)
    {
        this.ChangelogsPanel.ScrollToHorizontalOffset(this.ChangelogsPanel.VerticalOffset + (double)(e.Delta / 10));
    }

    private void ScriptScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            double newOffset = sv.HorizontalOffset - e.Delta; 
            if (newOffset < 0) newOffset = 0;
            if (newOffset > sv.ScrollableWidth) newOffset = sv.ScrollableWidth;

            sv.ScrollToHorizontalOffset(newOffset);
            e.Handled = true;
        }
    }


    public async void AddTab(string namee = "", string text = "")
    {
        browser.Visibility = Visibility.Visible;

        Controls.TabControl tab = new Controls.TabControl(TabPanel, this);
        this.TabPanel.Children.Add(tab);
        tab.TabLabel.Content = namee;
        tab.script = text;
        ScriptScroller.ScrollToRightEnd();
        tab.MouseWheel += new MouseWheelEventHandler(ScrollTabs);

        await Task.Delay(10);
        tab.Opacity = 0;
        tab.RenderTransform = new TranslateTransform(-50, 0);

        DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
        DoubleAnimation slideIn = new DoubleAnimation(-50, 0, TimeSpan.FromSeconds(0.1));

        tab.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        (tab.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.XProperty, slideIn);

        if (browser.CoreWebView2 == null)
        {
            browser.CoreWebView2InitializationCompleted += (_, e) =>
            {
                if (e.IsSuccess)
                {
                    Dispatcher.Invoke(() => tab.Select());
                }
            };
        }
        else
        {
            tab.Select();
        }
    }

    private void AddTabBTN_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        AddTab($"Tab {TabPanel.Children.Count.ToString()}.lua", "print('Hello Roblox, I Hacker! Using HoleNexus Expoit.')\n--// Made By Alex Herry.");
    }
    #endregion

    #region Script List
    public string BaseSscriptDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Assets\\scripts";

    public string[] GrabbedDirs { get; set; }

    private void AddParents(FolderItemControl lim)
    {
        if (lim.IsSub)
        {
            lim.ParentItems.Add(lim.ParentFolderSub);
        }
    }

    private void AddItemsToAList(List<UIElement> listToAdd, FolderItemControl itemToAdd)
    {
        listToAdd.Add(itemToAdd);
        if (itemToAdd.IsSub)
        {
            AddItemsToAList(listToAdd, itemToAdd.ParentFolderSub);
        }
    }
    public void FillTreeView(StackPanel treeView, StackPanel TopStack, string directory, FolderItemControl ParentSubFolderItem = null)
    {
        if (!Directory.Exists(@"Assets\Scripts"))
            Directory.CreateDirectory(@"Assets\Scripts");

        treeView.Children.Clear();
        string[] directories = Directory.GetDirectories(directory);
        foreach (string path in directories)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FolderItemControl folderItem = new FolderItemControl();

            folderItem.NameText.Text = directoryInfo.Name;

            treeView.Children.Add(folderItem);
            folderItem.Width = (folderItem.Parent as StackPanel).Width;
            folderItem.ParentPanelMian = treeView;
            folderItem.ParentFolderSub = (folderItem.Parent as FolderItemControl);
            if (folderItem.Parent != TopStack)
            {
                ParentSubFolderItem.HasSubs = true;
                ParentSubFolderItem.SubItems.Add(folderItem);
                folderItem.ParentFolderSub = ParentSubFolderItem;
                folderItem.ParentPanelMian = TopStack;
                folderItem.PasrentPanelSub = treeView;
                folderItem.IsSub = true;
                AddItemsToAList(folderItem.ParentItems, folderItem.ParentFolderSub);
            }
            else
            {
                folderItem.ParentFolderSub = null;
                folderItem.ParentPanelMian = TopStack;
                folderItem.IsSub = false;
                folderItem.PasrentPanelSub = null;
            }
            folderItem.FOlderStack.Height = 0;
            FillTreeView(folderItem.FOlderStack, TopStack, path, folderItem);
        }

        foreach (string file in Directory.GetFiles(directory))
        {
            FileInfo fileInfo = new FileInfo(file);
            ScriptItemControl fileItem = new ScriptItemControl();
            fileItem.MainName = fileInfo.Name;
            fileItem.NameText.Text = fileInfo.Name;
            fileItem.FullPath = fileInfo.FullName;

            treeView.Children.Add(fileItem);

        }
    }
    #endregion

    #region ScriptBlox Mục Scripts
    private async Task LoadPopularScriptsAsync(bool append = false)
    {
        if (isLoading) return;
        try
        {
            isLoading = true;
            string apiUrl = $"https://scriptblox.com/api/script/fetch?page={currentPage}";
            string response = await httpClient.GetStringAsync(apiUrl);
            using JsonDocument doc = JsonDocument.Parse(response);
            var scripts = doc.RootElement.GetProperty("result").GetProperty("scripts");

            if (!append)
            {
                PopularScripts2.Children.Clear();
                currentPage = 1;
            }

            foreach (var script in scripts.EnumerateArray())
            {
                string title = script.GetProperty("title").GetString();
                string imageUrl = null;

                if (script.GetProperty("game").TryGetProperty("imageUrl", out var gameImage))
                    imageUrl = gameImage.GetString();

                if (string.IsNullOrEmpty(imageUrl) && script.TryGetProperty("image", out var img))
                    imageUrl = img.GetString();

                string scriptCode = script.TryGetProperty("script", out var codeProp)? codeProp.GetString(): "No script found";

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (imageUrl.StartsWith("/"))
                        imageUrl = "https://scriptblox.com" + imageUrl;

                    if (imageUrl.Contains("/images/no-script.webp") ||
                        imageUrl.EndsWith("/images/no-script.webp"))
                    {
                        if (script.GetProperty("game").TryGetProperty("gameId", out var gameId))
                        {
                            string robloxGameId = gameId.ToString();
                            imageUrl = $"https://tr.rbxcdn.com/180DAY-59af3523ad8898216dbe1043788837bf/480/270/Image/Png/noFilter";
                        }
                        else
                        {
                            imageUrl = "https://tr.rbxcdn.com/180DAY-59af3523ad8898216dbe1043788837bf/480/270/Image/Png/noFilter";
                        }
                    }
                }
                else
                {
                    imageUrl = "https://tr.rbxcdn.com/180DAY-59af3523ad8898216dbe1043788837bf/480/270/Image/Png/noFilter";
                }

                var item = new PopularScript2(imageUrl, title, scriptCode);
                item.Width = 215;
                item.Height = 130;
                item.Margin = new Thickness(2);

                PopularScripts2.Children.Add(item);
            }

            currentPage++;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Error loading scripts:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            isLoading = false;
        }
    }
    private void PopularScriptsPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 100)
        {
            if (currentPage <= maxPages && !isLoading)
            {
                _ = LoadPopularScriptsAsync(append: true);
            }
        }
    }
    #endregion
    private void PopularScripts1_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        
    }
    #region Chức Năng Control UI
    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        string code = await GetText(); 
        Execution.ExecutionHandler.Execute(code); 
    }
    private async void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (browser?.CoreWebView2 == null)
        {
            Error("Editor chưa sẵn sàng!",4);
            return;
        }

        string text = await GetText();
        if (string.IsNullOrWhiteSpace(text))
        {
            Warning("Không có gì để xóa!", 4);
            return;
        }

        await ClearText();
        Success("Đã xóa toàn bộ nội dung!", 4);
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (browser?.CoreWebView2 == null)
        {
            Error("Editor chưa sẵn sàng!", 4);
            return;
        }

        await OpenFile();
    }

    private async void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        if (browser?.CoreWebView2 == null)
        {
            Error("Editor chưa sẵn sàng!",4);
            return;
        }

        await SaveFile();
    }

    private void Attach_Click(object sender, RoutedEventArgs e)
    {
        Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
        if (IsInjected)
        {
            MessageBox.Show("The API has already been injected. Attempting to inject twice will result in a crash");
        }
        else if (InjectionInProgress)
        {
            MessageBox.Show("Injection is already in process");
        }
        else if (pname.Length > 0) // If Roblox is running
        {
            InjectionInProgress = true;

            // wrd
            if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
            {
                // wrd
                if (Execution.SelectedAPI.API == "Selected API: WeAreDevs API")
                {
                    ExecutionHandler.Inject();
                }
            }
        }
        else
        {
            MessageBox.Show("Please open Roblox first before attempting to inject");
        }
    }

    private void KillRoblox_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to kill Roblox?", "HoleNexus", MessageBoxButton.YesNo) == MessageBoxResult.No)
        {
            // Just a basic YesNo thing
        }
        else
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("RobloxPlayerBeta")) // We will loop though each process just to find it
                {
                    proc.Kill();
                    MessageBox.Show("Roblox process killed", "HoleNexus");
                }

            }
            catch
            {
                // Just in case the user is stupid or something
                MessageBox.Show("Roblox process has already been killed, or Roblox isn't running.", "HoleNexus");
            }
        }
    }
    #endregion
}