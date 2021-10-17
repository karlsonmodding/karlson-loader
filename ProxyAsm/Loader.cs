using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace ProxyAsm
{
    public class Loader
    {
        public static void Init()
        {
            // load the assembly from within the game, so LoaderAsm can be used a sa library
            Assembly.LoadFrom(Path.Combine(Environment.GetEnvironmentVariable("KarlsonLoaderDir", EnvironmentVariableTarget.Process), "data", "LoaderAsm.dll")).GetType("LoaderAsm.Loader").GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { });
        }
    }
}
