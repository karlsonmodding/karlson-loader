using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LoaderAsm
{
    public class Loader
    {
        public static string KarlsonLoaderDir;
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

        public static Texture2D Texture2D_splash;
        public static bool loading = true;
        public static string loadingStatusString = "Loading... Please wait.";

        // entry point of assembly
        public static void Init()
        {
            KarlsonLoaderDir = Environment.GetEnvironmentVariable("KarlsonLoaderDir", EnvironmentVariableTarget.Process);
            //WinConsole.Initialize();
            IntPtr hWnd = FindWindow(null, Application.productName);
            SetWindowText(hWnd, "Karlson (modded)");
            Application.logMessageReceived += Application_logMessageReceived;
            GameObject go = new GameObject("KarlsonLoader_MonoHooks");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<KarlsonLoader.MonoHooks>();
            AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(KarlsonLoaderDir, "data", "karlsonloaderbundle"));
            Texture2D_splash = ab.LoadAsset<Texture2D>("Splash");
            if (!Directory.Exists(Path.Combine(KarlsonLoaderDir, "UML")))
                Directory.CreateDirectory(Path.Combine(KarlsonLoaderDir, "UML"));
            if (!Directory.Exists(Path.Combine(KarlsonLoaderDir, "UML", "mods")))
                Directory.CreateDirectory(Path.Combine(KarlsonLoaderDir, "UML", "mods"));
            if (!Directory.Exists(Path.Combine(KarlsonLoaderDir, "UML", "deps")))
                Directory.CreateDirectory(Path.Combine(KarlsonLoaderDir, "UML", "deps"));
            if (!Directory.Exists(Path.Combine(KarlsonLoaderDir, "UML", "res")))
                Directory.CreateDirectory(Path.Combine(KarlsonLoaderDir, "UML", "res"));
            File.WriteAllText(Path.Combine(KarlsonLoaderDir, "UML", "log"), "");
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            Log(condition + " " + stackTrace);
        }

        public static void Log(string str)
        {
            Console.WriteLine(str);
            File.AppendAllText(Path.Combine(KarlsonLoaderDir, "UML", "log"), str);
        }
    }

    static class WinConsole
    {
        static public void Initialize(bool alwaysCreateNewConsole = true)
        {
            bool consoleAttached = true;
            if (alwaysCreateNewConsole
                || (AttachConsole(ATTACH_PARRENT) == 0
                && Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED))
            {
                consoleAttached = AllocConsole() != 0;
            }

            if (consoleAttached)
            {
                InitializeOutStream();
                InitializeInStream();
                Console.Title = "KarlsonLoader Console";
            }
        }

        private static void InitializeOutStream()
        {
            var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                var writer = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
            }
        }

        private static void InitializeInStream()
        {
            var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                Console.SetIn(new StreamReader(fs));
            }
        }

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
                                FileAccess dotNetFileAccess)
        {
            var file = new SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                var fs = new FileStream(file, dotNetFileAccess);
                return fs;
            }
            return null;
        }

        #region Win API Functions and Constants
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll",
            EntryPoint = "SetConsoleIcon",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        static extern bool SetConsoleIcon(IntPtr hIcon);

        [DllImport("kernel32.dll",
            EntryPoint = "AttachConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 AttachConsole(UInt32 dwProcessId);

        [DllImport("kernel32.dll",
            EntryPoint = "CreateFileW",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
              string lpFileName,
              UInt32 dwDesiredAccess,
              UInt32 dwShareMode,
              IntPtr lpSecurityAttributes,
              UInt32 dwCreationDisposition,
              UInt32 dwFlagsAndAttributes,
              IntPtr hTemplateFile
            );

        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 OPEN_EXISTING = 0x00000003;
        private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        private const UInt32 ERROR_ACCESS_DENIED = 5;

        private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;

        #endregion
    }
}
