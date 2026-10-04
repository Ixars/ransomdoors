using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WpfApplication = System.Windows.Application;

namespace rans0m
{
    public class Global
    {
        public static Overlay? overlayWindow;
        // Titles used by the pop up windows
        public static readonly List<string> tauntTitles = new() {
            "RANS0M",
            "MOSNAR",
            "RANSOM",
            "M0NARS",
            "YOU ARE AN IDIOT",
            "Untitled",
            "Untitled (3)",
            "I FOUND YOU",
            "RANSOM.exe",
            "RAANNNSSSSOOOOOMMMMMM",
            "times up",
            "GIVE MONEY",
            "ERROR",
            "DHAUFGH",
            "_________",
            "IMG.JPG"
        };

        // Images used by the pop up windows
        private static List<BitmapImage> _tauntImages;
        public static List<BitmapImage> tauntImages
        {
            get
            {
                if (_tauntImages == null)
                {
                    _tauntImages = new()
                    {
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch1.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch2.jpeg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch3.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch4.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch5.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/idiot.png"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/tauntface.png"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/tauntflower.png"),
                    };
                }
                return _tauntImages;
            }
        }





        // ----------------- GLOBAL VARIABLES -----------------

        public static int ransomLeft = 0; // Cash to pay
        public static int ransomTimeLeft = 0; // 3rd phase countdown
        public static bool underRansom = false;
        public static Action? RansomPayed;
        public static List<string> usedCoins = new(); // this is to avoid people from copy pasting coins, not that secure tho

        public static bool crucifixUsed = false;
        public static bool canAttack = true;
        public static System.Drawing.Point lastRegisteredMousePos;
        public static bool spyingMouse = false;

        public static Random rng = new Random();

        /// <summary>
        /// Size of the overlay in DIPs. Falls back to the primary work area (also DIPs)
        /// if the overlay isn't laid out yet, so callers never mix pixel and DIP units.
        /// </summary>
        public static (double Width, double Height) OverlaySizeDips()
        {
            Overlay? o = overlayWindow;
            if (o == null || o.ActualWidth <= 0 || o.ActualHeight <= 0)
            {
                return (SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            }
            return (o.ActualWidth, o.ActualHeight);
        }

        private static double ElementWidth(FrameworkElement e) => e.ActualWidth > 0 ? e.ActualWidth : e.Width;
        private static double ElementHeight(FrameworkElement e) => e.ActualHeight > 0 ? e.ActualHeight : e.Height;





        // ---------------------- PUBLIC METHODS ----------------------

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        private static BitmapImage LoadBitmapImage(string uri)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        static public byte[] GetBytesFromResource(string uri)
        {
            System.Windows.Resources.StreamResourceInfo streamInfo = WpfApplication.GetResourceStream(new Uri($"pack://application:,,,/Assets/{uri}"));
            byte[] bytes = new byte[streamInfo.Stream.Length];
            streamInfo.Stream.Read(bytes, 0, (int)streamInfo.Stream.Length);

            return bytes;
        }

        static public Stream GetResourceSteam(string uri)
        {
            return WpfApplication.GetResourceStream(new Uri($"pack://application:,,,/Assets/{uri}")).Stream;
        }

        static public Thickness CombineThickness(Thickness a, Thickness b)
        {
            return new Thickness(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
        }

        public static void KeyPressed(Keys key)
        {
            if (spyingMouse)
            {
                lastRegisteredMousePos = new System.Drawing.Point(-1, -1); // Invalidate the last registered mouse position if a key is pressed during the spy phase so it also triggers the ransom
            }
        }

        /// <summary>
        /// Randomly positions a window within the overlay's DIP bounds.
        /// </summary>
        public static void RandomPosWindow(Window window)
        {
            var (w, h) = OverlaySizeDips();

            int maxX = (int)Math.Max(0, w - ElementWidth(window));
            int maxY = (int)Math.Max(0, h - ElementHeight(window));

            window.Left = rng.Next(0, Math.Max(1, maxX));
            window.Top = rng.Next(0, Math.Max(1, maxY));
        }

        /// <summary>
        /// Randomly positions a control within the overlay's DIP bounds.
        /// </summary>
        public static void RandomPosControl(FrameworkElement element)
        {
            var (w, h) = OverlaySizeDips();

            int maxX = (int)Math.Max(0, w - ElementWidth(element));
            int maxY = (int)Math.Max(0, h - ElementHeight(element));

            int x = rng.Next(0, Math.Max(1, maxX));
            int y = rng.Next(0, Math.Max(1, maxY));

            element.Margin = new Thickness(x, y, 0, 0);
        }

        /// <summary>
        /// Centers a control within the overlay's DIP bounds.
        /// </summary>
        public static void CenterControl(FrameworkElement element)
        {
            var (w, h) = OverlaySizeDips();

            double x = (w - ElementWidth(element)) / 2;
            double y = (h - ElementHeight(element)) / 2;

            element.Margin = new Thickness(x, y, 0, 0);
        }

        /// <summary>
        /// Centers a window within the overlay's DIP bounds.
        /// </summary>
        public static void CenterWindow(Window window)
        {
            var (w, h) = OverlaySizeDips();

            window.Left = (w - ElementWidth(window)) / 2;
            window.Top = (h - ElementHeight(window)) / 2;
        }

        /// <summary>
        /// Cool glitch idle animation, used for the ransom pop ups
        /// </summary>
        public static async void GlitchIdle(Window control, bool divideAndTaunt = false)
        {
            (double x, double y) = await control.Dispatcher.InvokeAsync(() =>
                (control.Left, control.Top));

            while (true)
            {
                await Task.Delay(200);
                try
                {
                    if (!Global.underRansom)
                    {
                        await control.Dispatcher.InvokeAsync(() =>
                        {
                            control.Close();
                        });
                        break;
                    }

                    await control.Dispatcher.InvokeAsync(() =>
                    {
                        if (divideAndTaunt)
                        {
                            if (Global.rng.Next(1, 100) <= 2)
                            {
                                var (w, h) = OverlaySizeDips();
                                x = Global.rng.Next(0, (int)Math.Max(1, w - ElementWidth(control)));
                                y = Global.rng.Next(0, (int)Math.Max(1, h - ElementHeight(control)));
                                TauntWindow tauntWindow = new TauntWindow();
                                tauntWindow.Show();
                            }
                        }

                        control.Left = x + Global.rng.Next(-5, 5);
                        control.Top = y + Global.rng.Next(-5, 5);
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        public static void HideSystemMenu(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & ~WS_SYSMENU);
        }
    }
}