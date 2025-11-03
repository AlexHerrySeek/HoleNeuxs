using Microsoft.Win32;
using System;
using System.Drawing;

namespace WRDWrapper
{
    internal class Program
    {
        static string WrapperVersion = "1.0";
        static async Task Main(string[] args)
        {
            Console.Title = "HoleNexus WeAreDevs Wrapper";

            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\HoleNexusWRDWrapper"); // From the settings we saved
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HoleNexusWRDWrapper");
            key.SetValue("WrapperVersion", WrapperVersion);
            key.Close();

            var server = new PipeProcess("WRDFakeServer");
            Console.WriteLine("Starting pipe server, please don't close this window (literally don't)...");
            await server.StartAsync();
        }
    }
}