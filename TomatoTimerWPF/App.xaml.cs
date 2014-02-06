using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace TomatoTimerWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Custom main method
        [STAThread]
        public static void Main()
        {
            //EmbeddedAssembly.Load("TomatoTimerWPF.Microsoft.WindowsAPICodePack.dll", "Microsoft.WindowsAPICodePack.dll");
            //EmbeddedAssembly.Load("TomatoTimerWPF.Microsoft.WindowsAPICodePack.Shell.dll", "Microsoft.WindowsAPICodePack.Shell.dll");

            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    String resourceName = "TomatoTimerWPF.Microsoft.WindowsAPICodePack.Shell.dll";
            //    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            //    {
            //        Byte[] assemblyData = new Byte[stream.Length];
            //        stream.Read(assemblyData, 0, assemblyData.Length);
            //        return System.Reflection.Assembly.Load(assemblyData);
            //    }
            //};

            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    String resourceName = "TomatoTimerWPF.Microsoft.WindowsAPICodePack.dll";
            //    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            //    {
            //        Byte[] assemblyData = new Byte[stream.Length];
            //        stream.Read(assemblyData, 0, assemblyData.Length);
            //        return System.Reflection.Assembly.Load(assemblyData);
            //    }
            //};

            // Create new instance of application subclass
            App app = new App();

            // Code to register events and set properties that were
            // defined in XAML in the application definition
            app.InitializeComponent();

            // Start running the application
            app.Run();

        }
    }


}
