using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using System.Drawing.Imaging;

namespace ChangeDesktopBackground
{
    public class Refrence
    {
        public void Help()
        {
            try
            {
                Console.WriteLine("Available Switches (Select only 1)");
                Console.WriteLine("You may use either a " + "-" + "or " + @"/" + " to declare your switches.");
                Console.WriteLine("If no switches are passed, the program will search for a file titled wallpaper.bmp and attempt to set the background to that file.");
                Console.WriteLine("");
                Console.WriteLine("--Switches--");
                Console.WriteLine("     -p:<Path>      Path to the File Name");
                Console.WriteLine("     -s:<Style>     Style type.  Must be one the following:  Tiled, Centered, Stretched, Fit, Fill, Span.  Span is only applicable on Windows 8 and higher.  Default is Fill.");
                Console.WriteLine("     -Help          Displays help.");
                Console.WriteLine("     -h             Displays help.");
                Console.WriteLine("     -?             Displays help.");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("--Examples--");
                Console.WriteLine("     changedesktopbackground.exe                                                     Shows this help menu.");
                Console.WriteLine("     changedesktopbackground.exe /p:C:\\Users\\Admin\\Pictures\\wallpaper.jpg        Makes the selected file the background.  Default style of Fill is used.");
                Console.WriteLine("     changedesktopbackground.exe /s:Centered                                         Searches for wallpaper.bmp in the program directory and attempts to set it to Centered style.");
                Console.WriteLine("     changedesktopbackground.exe /p:C:\\Windows\\Temp\\pic.bmp /s:Tiled              Sets the specified file as the wallpaper with the Tiled style.");
                Console.WriteLine("     changedesktopbackground.exe /help                                               Shows this help menu.");
            }
            catch(IOException)
            {
                Console.Write("An error occured displaying help.  Ensure this module is not corrupt and that Windows is functioning properly.");
                Environment.Exit(1);
            }
        }
    }
    public class Wallpaper
    {
        //Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled = 0,
            Centered = 0,
            Stretched = 2,
            Fit = 6,
            Fill = 10,
            Span = 22
        }

        public void Set(String FilePath, Style style)
        {
            //Modified from StackOverflow answer to this question:  http://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net
            //Thank you!!
            try
            {
                FileInfo file = new FileInfo(FilePath);
                Stream stream = new FileStream(FilePath, FileMode.Open);
      
                Image img = Image.FromStream(stream);
                string tempPath = Path.Combine(Path.GetTempPath(), file.Name);
                img.Save(tempPath, ImageFormat.Bmp);

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch(SecurityException s)
            {
                Console.WriteLine(s.Message);
                Environment.Exit(1);
            }
            catch(ArgumentNullException n)
            {
                Console.WriteLine(n);
                Environment.Exit(1);
            }
            catch(ObjectDisposedException d)
            {
                Console.WriteLine(d.Message);
                Environment.Exit(1);
            }
            catch (IOException IO)
            {
                Console.WriteLine(IO.Message);
                Environment.Exit(1);
            }
            catch (ArgumentOutOfRangeException r)
            {
                Console.WriteLine(r.Message);
                Environment.Exit(1);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Refrence Ref = new Refrence();
            Wallpaper WP = new Wallpaper();
            if (args.Length ==0)
            {
                Ref.Help();
            }
            else if (args.Length >= 1 && args.Length <= 2)
            {
                Wallpaper.Style style = new Wallpaper.Style();
                style = Wallpaper.Style.Fill;
                string FilePath = "";
                foreach (string arg in args)
                {
                    switch(arg.Substring(0,2).ToUpper())
                    {
                        case "/S":case "-S":
                        if(arg.Substring(3) == "Tiled")
                            {
                                style = Wallpaper.Style.Tiled;
                            }
                        else if(arg.Substring(3) == "Centered")
                            {
                                style = Wallpaper.Style.Centered;
                            }
                        else if(arg.Substring(3) == "Stretched")
                            {
                                style = Wallpaper.Style.Stretched;
                            }
                        else if(arg.Substring(3) == "Fit")
                            {
                                style = Wallpaper.Style.Fit;
                            }
                        else if(arg.Substring(3) == "Fill")
                            {
                                style = Wallpaper.Style.Fill;
                            }
                        else if(arg.Substring(3) == "Span")
                            {
                                if(Version.Parse(Environment.OSVersion.Version.ToString()) > Version.Parse("6.1.7601"))
                                {
                                    style = Wallpaper.Style.Span;
                                }
                                else
                                {
                                    Console.WriteLine("Span style is not supported on Windows 7.  Defaulting to Fill style.");
                                    style = Wallpaper.Style.Fill;
                                }
                            }
                        else
                            {
                                style = Wallpaper.Style.Fill;
                                Console.WriteLine("An incorrect value for style was supplied.  The default of Fill will be used.");
                            }
                            break;
                        case "/P":case "-P":
                            FilePath = arg.Substring(3);
                            break;
                        case "/H":case "-H":case "/?":case "-?":case "/HELP":case "-HELP":
                            Ref.Help();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(FilePath))
                {
                    Console.WriteLine("Changing wallpaper to " + FilePath);
                    Console.WriteLine("Selected style is " + "\"" +style.ToString() + "\"");
                    WP.Set(FilePath, style);
                    Console.WriteLine("No errors detected during execution. Please check your desktop to verify results.");
                    Environment.Exit(0);
                }
                else
                {
                    char[] c = { '\'' };
                    string DefaultWallpaper = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).TrimEnd(c) + @"\wallpaper.bmp";
                    if (File.Exists(DefaultWallpaper))
                    {
                        Console.WriteLine("Changing wallpaper to " + FilePath);
                        Console.WriteLine("Selected style is " + "\"" + style.ToString() + "\"");
                        WP.Set(DefaultWallpaper, style);
                        Console.WriteLine("No errors detected during execution. Please check your desktop to verify results.");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("No wallpaper path was specified and wallpaper.bmp could not be found in the same directory. Failed to change wallpaper.");
                        Environment.Exit(1);
                    }
                }
            }
            else
            {
                Console.WriteLine(@"Too many arguments passsed.  Please check the help options (-h, -help, -?) for more information.");
                Environment.Exit(1);
            }
        }
    }
}
