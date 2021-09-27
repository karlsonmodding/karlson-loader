using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
// file dialog
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using Keys = System.Windows.Forms.Keys;
using System.Reflection;
using MInject;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KarlsonLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        const uint WM_KEYDOWN = 0x0100;

        public static readonly int[] VERSION = new int[] { 0, 5 };

        private MainWindow splash;
        private Process karlson;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if(Process.GetProcessesByName("KarlsonLoader").Length > 1)
            {
                MessageBox.Show("KarlsonLoader is already running.\nOnly one instance can run at a time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            splash = new MainWindow();

            Random rand = new Random();
            if (rand.Next(0, 100) < 2)
            {
                splash.Image.Source = new BitmapImage(new Uri(@"/Swagg.png", UriKind.Relative));
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            }
            splash.Show();
            splash.Image.Opacity = 0;
            while(splash.Image.Opacity < 1)
            {
                splash.Image.Opacity += 0.01;
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                Thread.Sleep(10);
            }
            splash.SetStatus("Cleaning up updater..");
            foreach(var p in Process.GetProcessesByName("Command Prompt"))
            {
                if (p.MainWindowTitle == "KarlsonLoader Updater")
                {
                    p.Kill();
                    break;
                }
            }
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater.bat")))
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater.bat"));
            if(Environment.GetCommandLineArgs().Length > 2 && Environment.GetCommandLineArgs()[1] == "-installed")
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetCommandLineArgs()[2])))
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetCommandLineArgs()[2]));
            }
            splash.SetStatus("Checking for updates..");
            dynamic newVer;
            using(var client = new HttpClient())
            {
                var response = await client.GetAsync("https://redline2.go.ro/karlsonloader/api/version");
                newVer = Json.Decode(response.Content.ReadAsStringAsync().Result);
            }
            if(int.Parse(newVer.major) > VERSION[0] || (int.Parse(newVer.major) == VERSION[0] && int.Parse(newVer.minor) > VERSION[1]))
            {
                splash.SetStatus("Downloading update [                    ]");
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri("https://redline2.go.ro/karlsonloader/update.exe"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.exe"));
                return;
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"));
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll")))
            {
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                isDepDownloading = true;
                filesToDownload = 1;
                filesDownloaded = 0;
                await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/karlsonloaderasm.dll"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll"));
                isDepDownloading = false;
            }
            /*else
            {
                string currenthash;
                using (var stream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll")))
                {
                    var sha = new SHA256Managed();
                    byte[] checksum = sha.ComputeHash(stream);
                    currenthash = BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
                string newHash;
                using(var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://redline2.go.ro/karlsonloader/karlsonloaderasm.hash");
                    newHash = response.Content.ReadAsStringAsync().Result;
                }
                if(newHash != currenthash)
                {
                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                    isDepDownloading = true;
                    filesToDownload = 1;
                    filesDownloaded = 0;
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll"));
                    await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/karlsonloaderasm.dll"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll"));
                    isDepDownloading = false;
                }
            }*/
            if(!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderbundle")))
            {
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                isDepDownloading = true;
                filesToDownload = 1;
                filesDownloaded = 0;
                await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/karlsonloaderbundle"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderbundle"));
                isDepDownloading = false;
            }
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "icon.ico")))
            {
                isDepDownloading = true;
                filesToDownload = 1;
                filesDownloaded = 0;
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri("https://redline2.go.ro/karlsonloader/libs/data/icon.ico"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "icon.ico"));
                isDepDownloading = false;
            }
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "cfg")))
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select Your Karlson Installation (Make sure it's fresh from itch.io)";
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    ofd.Filter = "Karlson Executable|Karlson.exe|Custom Executable|*.exe";
                    ofd.FilterIndex = 0;
                    ofd.RestoreDirectory = true;
                    splash.Topmost = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "cfg"), Path.GetDirectoryName(ofd.FileName));
                        splash.Topmost = true;
                    }
                    else
                    {
                        Environment.Exit(0);
                        return;
                    }
                }
            }
            string karlsonDir = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "cfg"));
            // launch karlson
            if(File.Exists(Path.Combine(karlsonDir, "UnityCrashHandler32.exe")))
            {
                MessageBox.Show("Your karlson installation is x32 but KarlsonLoader is x64. Install karlson x64 or install KarlsonLoader x32 (TBA)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            karlson = new Process();
            karlson.StartInfo = new ProcessStartInfo(Path.Combine(karlsonDir, "Karlson.exe"));
            karlson.StartInfo.UseShellExecute = false;
            karlson.StartInfo.EnvironmentVariables.Add("KarlsonLoaderDir", AppDomain.CurrentDomain.BaseDirectory);
            karlson.Start();
            splash.Hide();
            while(karlson.MainWindowHandle == (IntPtr)0)
                Thread.Sleep(0);
            PostMessage(karlson.MainWindowHandle, WM_KEYDOWN, (int)Keys.Enter, 0);
            System.Windows.Forms.NotifyIcon trayIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipText = "KarlsonLoader",
                BalloonTipTitle = "KarlsonLoader",
                Text = "KarlsonLoader",
                Icon = new System.Drawing.Icon("data/icon.ico")
            };
            trayIcon.MouseClick += TrayIcon_MouseClick;
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(0, "KarlsonLoader", "KarlsonLoader minimised to tray.\nIt will shutdown when you exit karlson.", System.Windows.Forms.ToolTipIcon.None);
            Thread.Sleep(100);

            // run MInject with karlsonloaderasm.dll
            // Method: LoaderAsm.Loader.Init()
            if(MonoProcess.Attach(karlson, out MonoProcess m_karlson))
            {
                byte[] assemblyBytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll"));
                IntPtr monoDomain = m_karlson.GetRootDomain();
                m_karlson.ThreadAttach(monoDomain);
                m_karlson.SecuritySetMode(0);
                m_karlson.DisableAssemblyLoadCallback();

                IntPtr rawAssemblyImage = m_karlson.ImageOpenFromDataFull(assemblyBytes);
                IntPtr assemblyPointer = m_karlson.AssemblyLoadFromFull(rawAssemblyImage);
                IntPtr assemblyImage = m_karlson.AssemblyGetImage(assemblyPointer);
                IntPtr classPointer = m_karlson.ClassFromName(assemblyImage, "LoaderAsm", "Loader");
                IntPtr methodPointer = m_karlson.ClassGetMethodFromName(classPointer, "Init");

                m_karlson.RuntimeInvoke(methodPointer);
                m_karlson.EnableAssemblyLoadCallback();
                m_karlson.Dispose();
            }
            else
            {
                karlson.Kill();
                MessageBox.Show("Couldn't execute MInject.\nPlease retry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            karlson.WaitForExit();
            trayIcon.Visible = false;
            Environment.Exit(0);
            return;
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                karlson.Kill();
            }
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(isDepDownloading)
            {
                filesDownloaded++;
            }
            else
            {
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.exe"));
                Environment.Exit(0);
            }
        }

        bool isDepDownloading = false;
        int filesDownloaded = 0;
        int filesToDownload = 0;

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string temp = "Downloading update [";
            if (isDepDownloading)
                temp = $"Downloading files ({filesDownloaded + 1}/{filesToDownload}) [";
            for(int i = 1; i <= e.ProgressPercentage / 5; i++)
                temp += "I";
            for (int i = 1; i <= 20 - e.ProgressPercentage / 5; i++)
                temp += " ";
            temp += $"] ({e.TotalBytesToReceive - e.BytesReceived} bytes left)";
            splash.SetStatus(temp);
        }
    }
}
