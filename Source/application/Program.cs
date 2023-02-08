using Common.LoggerManager;
using Common.XO.Requests;
using Config.Application;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DEVICE_CORE
{
    class Program
    {
        #region --- Win32 API ---
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;
        // window position
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        #endregion --- Win32 API ---

        const int STARTUP_WAIT_DELAY = 2048;
        const int COMMAND_WAIT_DELAY = 4096;
        const int CONFIGURATION_UPDATE_DELAY = 6144;
        static readonly DeviceActivator activator = new DeviceActivator();

        static readonly string[] MENU = new string[]
        {
            " ",
            "============ [ MENU ] ============",
            " m => menu",
            " q => QUIT",
            " <ALT><M> => Manual Entry",
            "  "
        };

        static private AppConfig configuration;

        static async Task Main(string[] args)
        {
            SetupEnvironment();

            // Device discovery
            string pluginPath = Path.Combine(Environment.CurrentDirectory, "DevicePlugins");

            IDeviceApplication application = activator.Start(pluginPath);

            await application.Run().ConfigureAwait(false);

            await ConsoleModeOperation(application);

            application.Shutdown();
        }

        static private void SetupEnvironment()
        {
            try
            {
                // Get appsettings.json config - AddEnvironmentVariables() requires package: Microsoft.Extensions.Configuration.EnvironmentVariables
                configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build()
                    .Get<AppConfig>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application Exception: [{ex}].");
            }

            // logger manager
            SetLogging();

            // Screen Colors
            SetScreenColors();

            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            SetupWindow();
        }

        static private void SetupWindow()
        {
            Console.BufferHeight = Int16.MaxValue - 1;
            //Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            Console.CursorVisible = false;

            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                //DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }

        static async Task ConsoleModeOperation(IDeviceApplication application)
        {
            // GET STATUS
            //await application.Command(LinkDeviceActionType.GetStatus).ConfigureAwait(false);

            DisplayMenu();

            ConsoleKeyInfo keypressed = GetKeyPressed(false);

            while (keypressed.Key != ConsoleKey.Q)
            {
                bool redisplay = false;

                // Check for <ALT> key combinations
                if ((keypressed.Modifiers & ConsoleModifiers.Alt) != 0)
                {
                    switch (keypressed.Key)
                    {
                        case ConsoleKey.A:
                        {
                            break;
                        }
                        case ConsoleKey.D:
                        {
                            break;
                        }
                        case ConsoleKey.F:
                        {
                            break;
                        }
                        case ConsoleKey.M:
                        {
                            await application.Command(LinkDeviceActionType.ManualCardEntry).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.R:
                        {
                            break;
                        }
                        case ConsoleKey.T:
                        {
                            break;
                        }
                        case ConsoleKey.U:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [UPDATE]");
                            break;
                        }
                        case ConsoleKey.V:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [VERSION]");
                            break;
                        }
                    }
                }
                else
                {
                    switch (keypressed.Key)
                    {
                        case ConsoleKey.M:
                        {
                            Console.WriteLine("");
                            DisplayMenu();
                            break;
                        }
                        case ConsoleKey.C:
                        {
                            break;
                        }
                        case ConsoleKey.H:
                        {
                            break;
                        }
                        case ConsoleKey.I:
                        {
                            break;
                        }
                        case ConsoleKey.O:
                        {
                            break;
                        }
                        case ConsoleKey.K:
                        {
                            break;
                        }
                        case ConsoleKey.R:
                        {
                            break;
                        }
                        case ConsoleKey.S:
                        {
                            break;
                        }
                        case ConsoleKey.V:
                        {
                            break;
                        }
                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                        {
                            break;
                        }
                        case ConsoleKey.D8:
                        case ConsoleKey.NumPad8:
                        {
                            break;
                        }
                        case ConsoleKey.X:
                        {
                            break;
                        }
                        default:
                        {
                            redisplay = false;
                            break;
                        }
                    }
                }

                keypressed = GetKeyPressed(redisplay);
            }

            Console.WriteLine("\r\nCOMMAND: [QUIT]\r\n");
        }

        static private ConsoleKeyInfo GetKeyPressed(bool redisplay)
        {
            if (redisplay)
            {
                Console.Write("SELECT COMMAND: ");
            }
            return Console.ReadKey(true);
        }

        static private void DisplayMenu()
        {
            foreach (string value in MENU)
            {
                Console.WriteLine(value);
            }

            Console.Write("SELECT COMMAND: ");
        }

        static AppConfig GetApplicationConfiguration()
        {
            AppConfig configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build()
                .Get<AppConfig>();

            return configuration;
        }

        static void SetLogging()
        {
            try
            {
                //string[] logLevels = GetLoggingLevels(0);
                string[] logLevels = configuration.LoggerManager.Logging.Levels.Split("|");

                if (logLevels.Length > 0)
                {
                    string fullName = Assembly.GetEntryAssembly().Location;
                    string logname = Path.GetFileNameWithoutExtension(fullName) + ".log";
                    string path = Directory.GetCurrentDirectory();
                    string filepath = path + "\\logs\\" + logname;

                    int levels = 0;
                    foreach (string item in logLevels)
                    {
                        foreach (LOGLEVELS level in LogLevels.LogLevelsDictonary.Where(x => x.Value.Equals(item)).Select(x => x.Key))
                        {
                            levels += (int)level;
                        }
                    }

                    Logger.SetFileLoggerConfiguration(filepath, levels);

                    Logger.info($"{Assembly.GetEntryAssembly().GetName().Name} ({Assembly.GetEntryAssembly().GetName().Version}) - LOGGING INITIALIZED.");
                }
            }
            catch (Exception e)
            {
                Logger.error("main: SetupLogging() - exception={0}", e.Message);
            }
        }

        static void SetScreenColors()
        {
            try
            {
                // Set Foreground color
                //Console.ForegroundColor = GetColor(configuration.GetSection("Application:Colors").GetValue<string>("ForeGround"));
                Console.ForegroundColor = GetColor(configuration.Application.Colors.ForeGround);

                // Set Background color
                //Console.BackgroundColor = GetColor(configuration.GetSection("Application:Colors").GetValue<string>("BackGround"));
                Console.BackgroundColor = GetColor(configuration.Application.Colors.BackGround);

                Console.Clear();
            }
            catch (Exception ex)
            {
                Logger.error("main: SetScreenColors() - exception={0}", ex.Message);
            }
        }

        static ConsoleColor GetColor(string color) => color switch
        {
            "BLACK" => ConsoleColor.Black,
            "DARKBLUE" => ConsoleColor.DarkBlue,
            "DARKGREEEN" => ConsoleColor.DarkGreen,
            "DARKCYAN" => ConsoleColor.DarkCyan,
            "DARKRED" => ConsoleColor.DarkRed,
            "DARKMAGENTA" => ConsoleColor.DarkMagenta,
            "DARKYELLOW" => ConsoleColor.DarkYellow,
            "GRAY" => ConsoleColor.Gray,
            "DARKGRAY" => ConsoleColor.DarkGray,
            "BLUE" => ConsoleColor.Blue,
            "GREEN" => ConsoleColor.Green,
            "CYAN" => ConsoleColor.Cyan,
            "RED" => ConsoleColor.Red,
            "MAGENTA" => ConsoleColor.Magenta,
            "YELLOW" => ConsoleColor.Yellow,
            "WHITE" => ConsoleColor.White,
            _ => throw new Exception($"Invalid color identifier '{color}'.")
        };
    }
}
