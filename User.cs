using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WRing
{
    public static class User
    {
        public static Rectangle GetScreenBounds()
        {
            return Screen.PrimaryScreen.Bounds;
        }

        public static Rectangle GetScreenBounds(int index)
        {
            return Screen.AllScreens[index].Bounds;
        }

        public static string GetName()
        {
            return Environment.UserName;
        }

        public static string GetDesktopWallapersPath()
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
            var wallpaperPath = wpReg.GetValue("WallpaperSource").ToString();
            wpReg.Close();
            return wallpaperPath;
        }

        [Obsolete]
        public static string GetDesktopWallapersPathOld()
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            var wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            return wallpaperPath;
        }

        public static void SetDesktopWallapers(string path)
        {
            WinAPI.SystemParametersInfo(
                WinAPI.SPI_SETDESKWALLPAPER, 0, path,
                WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDCHANGE
            );
        }

        public static void PrintDesktopWallapers(IntPtr hdc)
        {
            WinAPI.PaintDesktop(hdc);
        }

        public static void PrintDesktopWallapers(Window wnd)
        {
            WinAPI.PaintDesktop(wnd.GetDC());
        }
    }
}
