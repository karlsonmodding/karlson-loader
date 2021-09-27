using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonLoader
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if(Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase) != "KarlsonLoader.exe")
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe")))
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"));
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase)), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"));
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KarlsonLoader.exe"), "-old " + Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase));
                Environment.Exit(0);
                return;
            }
            if(Environment.GetCommandLineArgs().Length > 2 && Environment.GetCommandLineArgs()[1] == "-old")
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetCommandLineArgs()[2])))
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetCommandLineArgs()[2]));
            }
            // set to load assemblies from another directory
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}
