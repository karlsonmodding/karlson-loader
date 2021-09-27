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
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
// file dialog
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Reflection;

namespace KarlsonLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow splash;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
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
            splash.SetStatus("Checking for empty folder..");
            if (Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase) == "KarlsonLoader.exe" && Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Length == 1)
            {
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installer.exe"));
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installer.exe"), "-cpy");
                Environment.Exit(0);
                return;
            }
            if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "-cpy")
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLauncher.exe")))
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLauncher.exe"));
            }
            else if (Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Length > 1)
            {
                splash.SetStatus("Moving to new folder..");
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader"));
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader", "installer.exe"));
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader", "installer.exe"), "-del");
                Environment.Exit(0);
                return;
            }
            if(Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "-del")
            {
                if (File.Exists(Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "KarlsonLoader.exe")))
                    File.Delete(Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "KarlsonLoader.exe"));
            }
            splash.SetStatus("Downloading files (1/11) [                    ]");
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs"));
            Directory.CreateDirectory(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")));
            WebClient wc = new WebClient();
            wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
            await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/update.exe"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe")); // fetch newest KarlsonLoader
            foreach(string s in new string[] { "Microsoft.Web.Infrastructure.dll", "System.Web.Helpers.dll", "System.Web.Razor.dll", "System.Web.WebPages.Deployment.dll", "System.Web.WebPages.dll", "System.Web.WebPages.Razor.dll" })
            { // download all needed dlls for KarlsonLoader (located on the server)
                await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/libs/" + s), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", s)); // fetch newest KarlsonLoader
            }
            await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/libs/MInject/x64.dll"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "MInject.dll")); // fetch newest KarlsonLoader
            await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/karlsonloaderasm.dll"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderasm.dll"));
            await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/karlsonloaderbundle"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "karlsonloaderbundle"));
            await wc.DownloadFileTaskAsync(new Uri("https://redline2.go.ro/karlsonloader/libs/data/icon.ico"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "icon.ico"));
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"), "-installed " + Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase));
            Environment.Exit(0);
        }

        int filesDownloaded = 0;
        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            filesDownloaded++;
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string temp = $"Downloading files ({filesDownloaded + 1}/11) [";
            for(int i = 1; i <= e.ProgressPercentage / 5; i++)
                temp += "I";
            for (int i = 1; i <= 20 - e.ProgressPercentage / 5; i++)
                temp += " ";
            temp += $"] ({e.TotalBytesToReceive - e.BytesReceived} bytes left)";
            splash.SetStatus(temp);
        }
    }
}
