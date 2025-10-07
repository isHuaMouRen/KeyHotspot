using ToolLib.AreaSelectorLib;
using ToolLib.ErrorReportLib;
using ToolLib.PosSelectorLib;
using ToolLib.AutoStartLib;
using ToolLib.CmdLib;
using ToolLib.DownloaderLib;
using ToolLib.GdiToolLib;
using ToolLib.HashLib;
using ToolLib.HexLib;
using ToolLib.HotkeyManagerLib;
using ToolLib.IniLib;
using ToolLib.InputLib;
using ToolLib.JsonLib;
using ToolLib.KeyboardHookLib;
using ToolLib.LogLib;
using ToolLib.MemoryLib;
using ToolLib.RegistryLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyHotspot.Forms;

namespace KeyHotspot
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (sender, e) => GlobalExceptionHandler(sender, e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => GlobalExceptionHandler(sender, e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (sender, e) => GlobalExceptionHandler(sender, e.Exception);

            try { Application.Run(new Main_Form()); } catch (Exception ex) { GlobalExceptionHandler(null, ex); }


        }

        private static void GlobalExceptionHandler(object sender, Exception ex)
        {
            if (ErrorReportBox.Show("Error", "未捕获的异常", ex) == DialogResult.Abort) Environment.Exit(1);
        }
    }
}
