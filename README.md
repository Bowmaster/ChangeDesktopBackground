# ChangeDesktopBackground
ChangeDesktopBackground

*Note*  This program is still a work in progress, please use and test with your own risk.  Any feedback, suggestions, or advice is most welcome.

This program is designed to allow for simpler, programtic changing of desktop backgrounds in much the same way you'd run an "xcopy" command.

Initially, the end goal was to use this to remove bginfo.exe wallpaper brandings from a customer enviornment, but it has morphed to become much more flexibile.

In addition to being ran through the command or run line, this can also be pushed using an enterprise management suite like SCCM, Altiris, or ManageSoft.

-Switches-

All switches may be used in this format -<switch>:<value> OR /<switch>:<value>

<No Switches> = Help menu

/? OR /h OR /help = Help menu

/s:<style> = The style to set the background too. Blank values will use the "Fill" style.  Acceptable styles are: Titled, Centered, Stretched, Fit, Fill, Span (Windows 8 and higher).  Selecting "Span" on a Windows 7 device will result in a default value of "Fill" being used.

/p:<path to file> = The path to the desired wallpaper file.  Formats can be either .bmp or jpg.  If this value is not specified, a file titled wallpaper.bmp will be searched for in the same directory as the program.  If this file is found, it will be used for the background.  If not, an error code will be returned (1).

-Usage-
Running the program with no switches will display the help message.

To run, you must declare, at a minumun either a switch for background file path (-p), or a switch for style (-s).  You may also declare both.

ChangeDesktopBackground.exe
#Displays Help

ChangeDesktopBackground.exe -help
#Displays Help

ChangeDesktopBackground.exe /s:Span
#Searchs for wallpaper.bmp in the program's current directory and sets that as the background using the Span style (assuming Windows 8 or higher, otherwise Fill is used for Windows 7 (see above)).

ChangeDesktopBackground.exe /p:"C:\Users\Test Pics\Background.jpg"
#Sets the specified file as the background.  Since /s is not specified, the default style of "Fill" is used.

ChangeDesktopBackground.exe -p:"C:\NewBackground.bmp" -s:Centered
#Sets the specified file as the background and uses the "Centered" style.

-Testing-
This has currently been tested on the following platforms:
Windows 7
Windows 10
Future testing will occur on:
Windows 8.1
Server 2012, Server 2012 R2 (possibly)

-To Do List-
  1)Test on more devices and combinations.
  2)Optimize code as needed.
  3)Add support for web-based url's.

-Permissions-
This is free for all to use, modify, incorporate as needed.  Credit need not be given, though I certainly wouldn't be mad if you did in some way.  Please test thoroughly in a dev/test environment before using on any production machines or your own device.

-Thanks-
Special thanks to the answering poster of this StackOverflow question - http://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net
The code was essential in creating the main worker class used in this programs.
