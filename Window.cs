using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace WRing
{
    public class Window : IWin32Window
    {
        public static readonly IntPtr NULL = IntPtr.Zero;

        public IntPtr Handle { private set; get; }

        private Window() { } 

        // alt constructors etc

        public static Window[] GetAllWindows()
        {
            List<Window> wins = new List<Window>();
            var a = new WinAPI.EnumWindowsCB(cb);
            WinAPI.EnumWindows(a, 0);
            bool cb(IntPtr hwnd, int lp)
            {
                wins.Add(Find(hwnd));
                return true;
            }
            return wins.ToArray();
        }

        public enum TextSearchConditional
        {
            /// <summary>
            /// Contains any text
            /// </summary>
            WithText,
            /// <summary>
            /// Not contains text
            /// </summary>
            NoText,
            /// <summary>
            /// Text equal argument string with case
            /// </summary>
            Equal,
            /// <summary>
            /// Text equal argument string without case
            /// </summary>
            EqualIgnoreCase,
            /// <summary>
            /// Text contains argument string with case
            /// </summary>
            Contains,
            /// <summary>
            /// Text starts with argument string with case
            /// </summary>
            StartsWith,
            /// <summary>
            /// Text ends with argument string with case
            /// </summary>
            EndsWith,
        }

        public static Window[] GetWindowsByText(TextSearchConditional cond, string arg = "")
        {
            List<Window> wins = new List<Window>();
            var a = new WinAPI.EnumWindowsCB(cb);
            WinAPI.EnumWindows(a, 0);
            bool cb(IntPtr hwnd, int lp)
            {
                wins.Add(Find(hwnd));
                return true;
            }
            switch (cond)
            {
                case TextSearchConditional.WithText:
                    wins = wins.FindAll((w) => w.GetText().Length > 0);
                    break;
                case TextSearchConditional.NoText:
                    wins = wins.FindAll((w) => w.GetText().Length == 0);
                    break;
                case TextSearchConditional.Equal:
                    wins = wins.FindAll((w) => w.GetText() == arg);
                    break;
                case TextSearchConditional.EqualIgnoreCase:
                    wins = wins.FindAll((w) => w.GetText().ToLower() == arg.ToLower());
                    break;
                case TextSearchConditional.Contains:
                    wins = wins.FindAll((w) => w.GetText().Contains(arg));
                    break;
                case TextSearchConditional.StartsWith:
                    wins = wins.FindAll((w) => w.GetText().StartsWith(arg));
                    break;
                case TextSearchConditional.EndsWith:
                    wins = wins.FindAll((w) => w.GetText().EndsWith(arg));
                    break;
                default:
                    throw new Exception(((int)cond).ToString());
            }
            return wins.ToArray();
        }

        public static Window Find(IntPtr hwnd)
        {
            return new Window()
            {
                Handle = hwnd
            };
        }

        public static Window Find(long hwnd)
        {
            return new Window()
            {
                Handle = new IntPtr(hwnd)
            };
        }

        public static Window Find(Control ctrl)
        {
            return new Window()
            {
                Handle = ctrl.Handle
            };
        }

        public static Window Find(string title)
        {
            return new Window()
            {
                Handle = WinAPI.FindWindow(null, title)
            };
        }

        public static Window Find(string clname, string title)
        {
            return new Window()
            {
                Handle = WinAPI.FindWindow(clname, title)
            };
        }

        public static Window Find(IntPtr parent, IntPtr child, string clname, string title)
        {
            return new Window()
            {
                Handle = WinAPI.FindWindowEx(parent, child, clname, title)
            };
        }

        public static Window FromPoint(Point pt)
        {
            return new Window()
            {
                Handle = WinAPI.WindowFromPoint(new WinAPI.POINT() { x = pt.X, y = pt.Y })
            };
        }

        public static Window FromPoint(int x, int y)
        {
            return new Window()
            {
                Handle = WinAPI.WindowFromPoint(new WinAPI.POINT() { x = x, y = y })
            };
        }

        public Window GetParent()
        {
            return new Window()
            {
                Handle = WinAPI.GetParent(Handle)
            };
        }

        public Window[] GetChilds(bool withInner = false)
        {
            List<Window> wins = new List<Window>();
            for (var w = GetChild(); w.IsWindow(); w = w.GetNext())
            {
                wins.Add(w);
                if (withInner)
                {
                    var arr = w.GetChilds(true);
                    wins.AddRange(arr);
                }
            }
            return wins.ToArray();
        }

        public Window GetChild()
        {
            return new Window()
            {
                Handle = WinAPI.GetWindow(Handle, WinAPI.GW_CHILD)
            };
        }

        public Window GetChild(string title)
        {
            return new Window()
            {
                Handle = WinAPI.FindWindowEx(Handle, NULL, null, title)
            };
        }

        public Window GetChild(string clname, string title)
        {
            return new Window()
            {
                Handle = WinAPI.FindWindowEx(Handle, NULL, clname, title)
            };
        }

        public Window GetNext()
        {
            return new Window()
            {
                Handle = WinAPI.GetWindow(Handle, WinAPI.GW_HWNDNEXT)
            };
        }

        public Window GetPrevios()
        {
            return new Window()
            {
                Handle = WinAPI.GetWindow(Handle, WinAPI.GW_HWNDPREV)
            };
        }

        public Window GetFirst()
        {
            return new Window()
            {
                Handle = WinAPI.GetWindow(Handle, WinAPI.GW_HWNDFIRST)
            };
        }

        public Window GetLast()
        {
            return new Window()
            {
                Handle = WinAPI.GetWindow(Handle, WinAPI.GW_HWNDLAST)
            };
        }

        public static Window GetForeground()
        {
            return new Window()
            {
                Handle = WinAPI.GetForegroundWindow()
            };
        }

        public static Window GetDesktopWindow()
        {
            return new Window()
            {
                Handle = WinAPI.GetDesktopWindow()
            };
        }

        //

        public bool IsWindow()
        {
            return WinAPI.IsWindow(Handle);
        }

        public bool NotNull()
        {
            return (Handle != NULL);
        }

        public string GetClassName()
        {
            StringBuilder sb = new StringBuilder(256);
            WinAPI.GetClassName(Handle,sb, 256);
            return sb.ToString();
        }

        // etc

        // need fix
        public bool Screenshot(out Bitmap bmp)
        {
            bmp = null;
            if (!WinAPI.GetWindowRect(Handle, out var rect)) return false;
            using (var image = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    var hdcBitmap = graphics.GetHdc();
                    WinAPI.PrintWindow(Handle, hdcBitmap, 0);
                    graphics.ReleaseHdc(hdcBitmap);
                }
                bmp = new Bitmap(image);
            }
            return true;
        }

        public DialogResult MessageBox(string text, string tilte = null)
        {
            return (DialogResult)WinAPI.MessageBox(Handle, text, tilte, 0);
        }

        public int MessageBox(string text, string tilte, MessageBoxButtons btn, MessageBoxIcon ico)
        {
            return WinAPI.MessageBox(Handle, text, tilte, (uint)btn | (uint)ico);
        }

        public bool SetForeground()
        {
            return WinAPI.SetForegroundWindow(Handle);
        }

        public string GetText()
        {
            int capacity = WinAPI.GetWindowTextLength(new HandleRef(this, Handle)) * 2;
            StringBuilder stringBuilder = new StringBuilder(capacity);
            WinAPI.GetWindowText(new HandleRef(this, Handle), stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }

        // need fix
        public bool SetText(string text)
        {
            return WinAPI.SetWindowText(Handle, text);
        }

        // need fix
        public bool AddText(string text)
        {
            var source = GetText();
            return WinAPI.SetWindowText(Handle, source + text);
        }

        public void ClearSelected()
        {
            WinAPI.SendMessage(Handle, (uint)WinAPI.Message.WM_CLEAR, NULL, NULL);
        }

        public void SelectListItem(int index)
        {
            WinAPI.SendMessage(Handle, (uint)WinAPI.Message.CB_SETCURSEL, (IntPtr)index, NULL);
        }

        // Rectangle, Size, Location

        public RECT GetRECT()
        {
            WinAPI.GetWindowRect(Handle, out var rect);
            return rect;
        }

        public Rectangle GetRectangle()
        {
            WinAPI.GetWindowRect(Handle, out var rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public Size GetSize()
        {
            WinAPI.GetWindowRect(Handle, out var rect);
            return new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public Point GetLocation()
        {
            WinAPI.GetWindowRect(Handle, out var rect);
            return new Point(rect.Left, rect.Top);
        }

        public void SetLocation(int x, int y)
        {
            WinAPI.SetWindowPos(Handle, 0, x, y, 0, 0, WinAPI.SWP_NOSIZE | WinAPI.SWP_NOZORDER);
        }

        public void SetLocation(Point pt)
        {
            WinAPI.SetWindowPos(Handle, 0, pt.X, pt.Y, 0, 0, WinAPI.SWP_NOSIZE | WinAPI.SWP_NOZORDER);
        }

        public void SetSize(int w, int h)
        {
            WinAPI.SetWindowPos(Handle, 0, 0, 0, w, h, WinAPI.SWP_NOZORDER | WinAPI.SWP_NOMOVE);
        }

        public void SetSize(Size sz)
        {
            WinAPI.SetWindowPos(Handle, 0, 0, 0, sz.Width, sz.Height, WinAPI.SWP_NOZORDER | WinAPI.SWP_NOMOVE);
        }

        public void SetRectangle(Rectangle rect)
        {
            WinAPI.SetWindowPos(Handle, 0, rect.X, rect.Y, rect.Width, rect.Height, WinAPI.SWP_NOZORDER);
        }

        public void SetRectangle(int x, int y, int w, int h)
        {
            WinAPI.SetWindowPos(Handle, 0, x, y, w, h, WinAPI.SWP_NOZORDER);
        }

        //

        public void SetParent(IntPtr parent)
        {
            WinAPI.SetParent(Handle, parent);
        }

        public void SetParent(Window parent)
        {
            WinAPI.SetParent(Handle, parent.Handle);
        }

        public void SetFocus()
        {
            WinAPI.SetFocus(Handle);
        }

        public void Close()
        {
            WinAPI.SendMessage(Handle, (uint)WinAPI.Message.WM_CLOSE, NULL, NULL);
        }

        public bool Destroy()
        {
            return WinAPI.DestroyWindow(Handle);
        }

        public void Show(bool flag)
        {
            WinAPI.ShowWindow(Handle, ((flag)?WinAPI.SW_SHOW:WinAPI.SW_HIDE));
        }

        public bool IsVisible()
        {
            return WinAPI.IsWindowVisible(Handle);
        }

        public bool IsEnabled()
        {
            return WinAPI.IsWindowEnabled(Handle);
        }

        public void SetRedraw(bool flag)
        {
            if (flag)
            {
                WinAPI.SendMessage(Handle, (uint)WinAPI.Message.WM_SETREDRAW, (IntPtr)1, NULL);
            }
            else
            {
                WinAPI.SendMessage(Handle, (uint)WinAPI.Message.WM_SETREDRAW, NULL, NULL);
                WinAPI.RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                    0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/ | 0x0001/*RDW_INVALIDATE*/);
            }
        }

        // need finish
        public void SetWindowState(FormWindowState state)
        {
            WinAPI.Message msg = 0;
            switch (state)
            {
                case FormWindowState.Normal:
                    msg = WinAPI.Message.SC_RESTORE;
                    break;
                case FormWindowState.Minimized:
                    msg = WinAPI.Message.SC_MINIMIZE;
                    break;
                case FormWindowState.Maximized:
                    msg = WinAPI.Message.SC_MAXIMIZE;
                    break;
                default:
                    throw new Exception(((uint)state).ToString());
            }
            WinAPI.SendMessage(Handle, (uint)WinAPI.Message.WM_SYSCOMMAND, (IntPtr)(uint)msg, NULL);
        }

        public void Click()
        {
            WinAPI.SendMessage(Handle, (uint)WinAPI.Message.BM_CLICK, NULL, NULL);
        }

        public IntPtr GetDC()
        {
            return WinAPI.GetWindowDC(Handle);
        }

        public void Enable(bool flag)
        {
            WinAPI.EnableWindow(Handle, flag);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }
}
