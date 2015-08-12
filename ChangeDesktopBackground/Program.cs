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
                Console.WriteLine("");
                Console.WriteLine("--Help Dialog--");
                Console.WriteLine("You may use either a - or " + @"/" + " to declare your switches.");
                Console.WriteLine("If no switches are passed, the program will search for a file titled wallpaper.bmp (or .jpg) and attempt to set the background to that file.");
                Console.WriteLine("");
                Console.WriteLine("--Switches--");
                Console.WriteLine("     -p:<Path>      Full path to the image file.");
                Console.WriteLine("     -s:<Style>     Style type.  Must be one the following:  Tiled, Centered, Stretched, Fit, Fill, Span.  Span is only applicable on Windows 8 and higher.  Default is Fill.");
                Console.WriteLine("     -Help          Displays help.");
                Console.WriteLine("     -h             Displays help.");
                Console.WriteLine("     -?             Displays help.");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("--Examples--");
                Console.WriteLine("     Shows this help menu.");
                Console.WriteLine("     changedesktopbackground.exe");
                Console.WriteLine("");
                Console.WriteLine("     Makes the selected file the background.  Default style of Fill is used.");
                Console.WriteLine("     changedesktopbackground.exe /p:C:\\Users\\Admin\\Pictures\\wallpaper.jpg");
                Console.WriteLine("");
                Console.WriteLine("     Searches for wallpaper.bmp in the program directory and attempts to set it to Centered style.");
                Console.WriteLine("     changedesktopbackground.exe /s:Centered");
                Console.WriteLine("");
                Console.WriteLine("     Sets the specified file as the wallpaper with the Tiled style.");
                Console.WriteLine("     changedesktopbackground.exe /p:C:\\Windows\\Temp\\pic.bmp /s:Tiled");
                Console.WriteLine("");
                Console.WriteLine("     Shows this help menu.");
                Console.WriteLine("     changedesktopbackground.exe /help");
                Console.WriteLine("");
                Environment.Exit(0);
            }
            catch(IOException)
            {
                Console.WriteLine("");
                Console.Write("An error occured displaying help.  Ensure this module is not corrupt and that Windows is functioning properly.");
                Console.WriteLine("");
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
            Centered = 1,
            Stretched = 2,
            Fit = 6,
            Fill = 10,
            Span = 22
        }

        public void Set(string FilePath, Style style)
        {
            //Modified from StackOverflow answer to this question:  http://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net
            //Thank you!!
            bool Retry = false;
            FileInfo file = new FileInfo(FilePath);
            string tempFile = Path.Combine(Path.GetTempPath(), file.Name);
            string AppDataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Windows\Themes\TranscodedWallpaper.jpg");
            do
            {
                try
                {                    
                    //Attempt to read file.
                    Stream stream = new FileStream(FilePath, FileMode.Open);
                    Image img = Image.FromStream(stream);

                    //Check if the given file name exists in the user's %Temp% directory.  The file will be deleted if it exists.
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    //Reset the tempFile variable for .jpg usage.  Check for that file in the %Temp% directory and delete if it exists.
                    tempFile = (tempFile.Remove((tempFile.Length - 3), 3) + "jpg");
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    //Save the image stream to the %Temp% directory as a .jpg.
                    img.Save(tempFile, ImageFormat.Jpeg);

                    //Check for the default wallpaper file name in the default path.  If a file exists, it will be renamed so that it is not overwritten.  (In this case default = Microsoft default) 
                    if(File.Exists(AppDataFilePath))
                    {
                        File.Move(AppDataFilePath, (AppDataFilePath.Replace("jpeg", "").Replace("jpg", "") + DateTime.Now.ToString().Replace(@"/","-").Replace(":",".") + ".jpg"));
                    }

                    //Copy new wallpaper file to the Microsoft default path.  Existing file will the same name be overwritten.
                    File.Copy(tempFile, AppDataFilePath, true);

                    //Set deskotp style in registry.
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                    if (style == Style.Tiled)
                    {
                        key.SetValue(@"WallpaperStyle", 0.ToString());
                        key.SetValue(@"TileWallpaper", 1.ToString());
                    }
                    if (style == Style.Centered)
                    {
                        key.SetValue(@"WallpaperStyle", 0.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }
                    if (style == Style.Stretched)
                    {
                        key.SetValue(@"WallpaperStyle", 2.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }
                    if (style == Style.Fit)
                    {
                        key.SetValue(@"WallpaperStyle", 6.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }
                    if (style == Style.Fill)
                    {
                        key.SetValue(@"WallpaperStyle", 10.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }
                    if (style == Style.Span)
                    {
                        key.SetValue(@"WallpaperStyle", 22.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }

                    //Set Wallpaper and Update UI
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, AppDataFilePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                }
                catch (UnauthorizedAccessException a)
                {
                    if (Retry)
                    {
                        //Error re-occured, program failed.
                        Console.WriteLine("");
                        Console.WriteLine("The previous error re-occured: " + a.Message);
                        Console.WriteLine("The program will exit with a failure code.  Please check permissions on the desired file and try again.");
                        Console.WriteLine("");
                        Environment.Exit(1);
                    }
                    else
                    {
                        //File permission error occured, but the program will retry once.
                        Console.WriteLine("");
                        Console.WriteLine("An error occured: " + a.Message);
                        Console.WriteLine("Attempting to copy file to the temp directory and try again.");
                        Console.WriteLine("");
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                        tempFile = Path.Combine(Path.GetTempPath(), file.Name);
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                        File.Copy(FilePath, tempFile);
                        file = new FileInfo(tempFile);
                        FilePath = file.FullName;
                        Retry = true;
                    }
                }
                catch (SecurityException s)
                {
                    Console.WriteLine("");
                    Console.WriteLine(s.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch (ArgumentNullException n)
                {
                    Console.WriteLine("");
                    Console.WriteLine(n);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch (ArgumentOutOfRangeException r)
                {
                    Console.WriteLine("");
                    Console.WriteLine(r.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch (ArgumentException arg)
                {
                    Console.WriteLine("");
                    Console.WriteLine(arg.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch (ObjectDisposedException d)
                {
                    Console.WriteLine("");
                    Console.WriteLine(d.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch (IOException IO)
                {
                    Console.WriteLine("");
                    Console.WriteLine(IO.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
            } while (Retry);
         }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Refrence Ref = new Refrence();
            Wallpaper WP = new Wallpaper();
            //Check for Help Conditions or argument overload.  If any of these conditions evaluation to yes and proceed with their execution, the end result will be the programming closing.
            if (args.Length ==0)
            {
                Ref.Help();
                //The following exit command is only used as a fallback in case the help menu returns without exiting.  This should never happen.
                Environment.Exit(0);
            }
            else if (args.Length == 1)
            {
                foreach (string arg in args)
                {
                    switch (arg.ToUpper())
                    {
                        case "/H":
                        case "-H":
                        case "/?":
                        case "-?":
                        case "/HELP":
                        case "-HELP":
                            Ref.Help();
                            //The following exit command is only used as a fallback in case the help menu returns without exiting.  This should never happen.
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            else if (args.Length > 1)
            {
                if (args.Length > 2)
                {
                    Console.WriteLine("");
                    Console.WriteLine(@"Too many arguments passsed, or arguments not passed in the proper format.  Please check the help options (-h, -help, -?) for more information.");
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                else
                {
                    foreach (string arg in args)
                    {
                        switch (arg.ToUpper())
                        {
                            case "/H":
                            case "-H":
                            case "/?":
                            case "-?":
                            case "/HELP":
                            case "-HELP":
                                Console.WriteLine("");
                                Console.WriteLine("Please do not pass help switches with other switches.");
                                Console.WriteLine("");
                                Environment.Exit(1);
                                break;
                        }
                    }
                }
            }



            //If the previous checks pass, begin the primary work of the program.  
            //This first "if" block sets the variables from the switches supplied in the command line.  If an incorrect switch is detected in this section, the program will detect this and exit.
            if (args.Length >= 1 && args.Length <= 2)
            {
                Wallpaper.Style style = new Wallpaper.Style();
                style = Wallpaper.Style.Fill;
                string FilePath = "";
                foreach (string arg in args)
                {
                    string s = arg.Substring(3).ToLower();
                    switch(arg.Substring(0,3).ToUpper())
                    {
                        case "/S:":case "-S:":
                            if(s == "tiled")
                            {
                                style = Wallpaper.Style.Tiled;
                            }
                            else if (s == "tile")
                            {
                                style = Wallpaper.Style.Tiled;
                            }
                            else if(s == "centered")
                            {
                                style = Wallpaper.Style.Centered;
                            }
                            else if (s == "center")
                            {
                                style = Wallpaper.Style.Centered;
                            }
                            else if(s == "stretched")
                            {
                                style = Wallpaper.Style.Stretched;
                            }
                            else if (s == "stretch")
                            {
                                style = Wallpaper.Style.Stretched;
                            }
                            else if(s == "fit")
                            {
                                style = Wallpaper.Style.Fit;
                            }
                        else if(s == "fill")
                            {
                                style = Wallpaper.Style.Fill;
                            }
                        else if(s == "span")
                            {
                                if(Version.Parse(Environment.OSVersion.Version.ToString()) >= Version.Parse("6.2.9200.0"))
                                {
                                    style = Wallpaper.Style.Span;
                                }
                                else
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine("Span style is not supported on Windows 7.  Defaulting to Fill style.");
                                    Console.WriteLine("");
                                    style = Wallpaper.Style.Fill;
                                }
                            }
                        else
                            {
                                style = Wallpaper.Style.Fill;
                                Console.WriteLine("");
                                Console.WriteLine("An incorrect value for style was supplied.  The default of Fill will be used.");
                                Console.WriteLine("");
                            }
                            break;
                        case "/P:":case "-P:":
                            FilePath = arg.Substring(3);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("An incorect switch was detected.  Please make sure you are passing the proper switches in the proper format.");
                            Console.WriteLine("You may run this program with no switches/parameters to see help options, or use /h , -help , or /? to show help text.");
                            Console.WriteLine("");
                            Environment.Exit(1);
                        break;
                    }
                }

                //If all switches are set properly, begin this section, which sets the wallpaper and style.
                if (!string.IsNullOrEmpty(FilePath))
                {
                    Console.WriteLine("");
                    Console.WriteLine("Changing wallpaper to " + FilePath);
                    Console.WriteLine("Selected style is " + "\"" +style.ToString() + "\"");
                    Console.WriteLine("");
                    WP.Set(FilePath, style);
                    Console.WriteLine("No execution errors detected. Please check your desktop to verify results.");
                    Console.WriteLine("");
                    Environment.Exit(0);
                }
                else
                {
                    char[] c = { '\'' };
                    string DefaultWallpaper = "";
                    if (File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).TrimEnd(c) + @"\wallpaper.bmp"))
                    {
                        DefaultWallpaper = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).TrimEnd(c) + @"\wallpaper.bmp";
                        Console.WriteLine("");
                        Console.WriteLine("Changing wallpaper to " + DefaultWallpaper);
                        Console.WriteLine("Selected style is " + "\"" + style.ToString() + "\"");
                        Console.WriteLine("");
                        WP.Set(DefaultWallpaper, style);
                        Console.WriteLine("No execution errors detected. Please check your desktop to verify results.");
                        Console.WriteLine("");
                        Environment.Exit(0);
                    }
                    else if(File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).TrimEnd(c) + @"\wallpaper.jpg"))
                    {
                        DefaultWallpaper = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).TrimEnd(c) + @"\wallpaper.jpg";
                        Console.WriteLine("");
                        Console.WriteLine("Changing wallpaper to " + DefaultWallpaper);
                        Console.WriteLine("Selected style is " + "\"" + style.ToString() + "\"");
                        Console.WriteLine("");
                        WP.Set(DefaultWallpaper, style);
                        Console.WriteLine("No execution errors detected. Please check your desktop to verify results.");
                        Console.WriteLine("");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("No wallpaper path was specified and wallpaper.bmp/jpg could not be found in the same directory. Failed to change wallpaper.");
                        Console.WriteLine("");
                        Environment.Exit(1);
                    }
                }
            }
        }
    }
}
