using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfApplication = System.Windows.Application;

namespace rans0m
{
    public partial class Overlay : Window
    {
        private DispatcherTimer topMostTimer;
        private KeyboardHook keyboardHook = new KeyboardHook();
        private Border[] dpbSegments;
        private NotifyIcon? trayIcon;

        public Overlay()
        {
            InitializeComponent();
            Global.overlayWindow = this;
            dpbSegments = new Border[] { Seg0, Seg1, Seg2, Seg3, Seg4, Seg5, Seg6, Seg7, Seg8, Seg9 };
            
            topMostTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

            keyboardHook.KeyPressed += Global.KeyPressed;
            keyboardHook.Hook();

            this.Closing += (_, _) =>
            {
                // Hidden here instead of in Closed, otherwise it lingers as a ghost icon or smthing
                trayIcon?.Visible = false;
                FullCleanup();
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ResetRansom();
            MakeClickThrough();

            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.WindowState = WindowState.Maximized;

            // Register file types and set the app to it's ready state
            FileTypeRegister.RegisterIconForExtension(".gold1", Global.GetBytesFromResource("Gold/Gold1.ico"), "GoldFile1");
            FileTypeRegister.RegisterIconForExtension(".gold2", Global.GetBytesFromResource("Gold/Gold2.ico"), "GoldFile2");
            FileTypeRegister.RegisterIconForExtension(".gold3", Global.GetBytesFromResource("Gold/Gold3.ico"), "GoldFile3");
            FileTypeRegister.RegisterIconForExtension(".gold4", Global.GetBytesFromResource("Gold/Gold4.ico"), "GoldFile4");
            FileTypeRegister.RegisterIconForExtension(".gold5", Global.GetBytesFromResource("Gold/Gold5.ico"), "GoldFile5");
            FileTypeRegister.RegisterIconForExtension(".gold6", Global.GetBytesFromResource("Gold/HoneyPot.ico"), "GoldFile6");
            FileTypeRegister.RegisterIconForExtension(".crucifix", Global.GetBytesFromResource("crucifix.ico"), "CrucifixFile");

            await ResetRansom();
            SetupTrayIcon();
            SetupTopMostTimer();

            if (!Config.LoadConfig())
            {
                new ConfigWindow().ShowDialog();
            }

            // Starts the RansomLoop (async void, yields back to the UI thread immediately)
            RansomLoop();
        }

        /// <summary>
        /// Removes every trace RANS0M leaves on the system: gold coin files, the drawer temp folder,
        /// the file types registrations and their icons, and the desktop wallpaper.
        /// </summary>
        private void FullCleanup()
        {
            GoldCoinManager.DeleteAllCoins();
            FileTypeRegister.UnregisterAllGoldFileTypes();
            WallpaperUpdater.RestoreWallpaper();
            CursorUpdater.RestoreCursor();
        }

        /// <summary>
        /// TopMost alone doesn't stick, other windows (especially elevated ones) can still cover the overlay
        /// so this keeps shoving it back to the front without stealing focus/keyboard input
        /// </summary>
        private void SetupTopMostTimer()
        {
            topMostTimer.Tick += (s, e) =>
            {
                NativeMethods.SetWindowPos(new WindowInteropHelper(this).Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            };
            topMostTimer.Start();

            this.Closed += (s, e) => topMostTimer.Stop();
        }

        /// <summary>
        /// Setup the TrayIcon to close the app and open configuration menu
        /// </summary>
        private void SetupTrayIcon()
        {
            ContextMenuStrip trayMenu = new ContextMenuStrip();

            trayMenu.Items.Add("Configuration").Click += (s, e) => new ConfigWindow().Show();
            trayMenu.Items.Add("Close").Click += (s, e) => WpfApplication.Current.Shutdown();

            trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath) ?? System.Drawing.SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Text = "RANS0M",
                Visible = true
            };
        }

        private static class NativeMethods
        {
            public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOACTIVATE = 0x0010;

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private void MakeClickThrough()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, -20);
            SetWindowLong(hwnd, -20, extendedStyle |
                0x00000020 | // WS_EX_TRANSPARENT
                0x00080000 // WS_EX_LAYERED
                );
        }












        // -------------------- CORE --------------------

        /// <summary>
        /// Reset the app to it's ready state
        /// </summary>
        public async Task ResetRansom()
        {
            await Task.Run(() => GoldCoinManager.DeleteAllCoins());

            Global.canAttack = true;
            Global.RansomPayed = null;
            Global.underRansom = false;
            Global.ransomLeft = 0;
            Global.usedCoins.Clear();
            Global.crucifixUsed = false;

            foreach (TauntWindow tauntWindow in WpfApplication.Current.Windows.OfType<TauntWindow>().ToList())
            {
                tauntWindow.Close();
            }

            redVignette.Opacity = 0;
            staticBg.Opacity = 0;

            img_idle.Opacity = 0;
            img_attack.Opacity = 0;
            img_stopsign.Opacity = 0; 
            vb_download.Opacity = 0;
            vb_download.Width = 668;
            vb_download.Height = 88;
            foreach (Border seg in dpbSegments)
            {
                seg.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            WallpaperUpdater.RestoreWallpaper();
            CursorUpdater.RestoreCursor();
        }

        /// <summary>
        /// Shakes img_attack around its center position for the given number of ticks (20ms apart)
        /// </summary>
        private async Task ShakeAsync(Thickness center, int ticks)
        {
            for (int i = 0; i <= ticks; i++)
            {
                await Task.Delay(20);
                try { img_attack.Margin = Global.CombineThickness(center, new Thickness(Global.rng.Next(-40, 40), Global.rng.Next(-40, 40), 0, 0)); }
                catch { break; }
            }
        }

        /// <summary>
        /// Ransom loop, keeps summoning ransom randomly
        /// </summary>
        private async void RansomLoop()
        {
            while (true)
            {
                if (!Config.SpawnAutomatically)
                {
                    await InterruptibleDelay(250);
                    continue;
                }

                int minDelay = Math.Min(Config.MinSpawnDelay, Config.MaxSpawnDelay) * 1000;
                int maxDelay = Math.Max(Config.MinSpawnDelay, Config.MaxSpawnDelay) * 1000;
                await InterruptibleDelay(Global.rng.Next(minDelay, maxDelay)); // Ransom Debounce

                try { await this.Dispatcher.Invoke(SpawnRansom); }
                catch { break; }
            }
        }

        private CancellationTokenSource? _spawnDelayCts;

        /// <summary>
        /// Cuts the current wait short so a config change applies right away.
        /// </summary>
        public void RetriggerSpawnLoop() => _spawnDelayCts?.Cancel();

        private async Task InterruptibleDelay(int ms)
        {
            // Fresh CTS each call so an old Cancel() can't carry over into a later delay
            _spawnDelayCts?.Dispose(); 
            _spawnDelayCts = new CancellationTokenSource();
            try { await Task.Delay(ms, _spawnDelayCts.Token); }
            catch { }
        }

        /// <summary>
        /// Summons Ransom, triggers each step of the ransom process
        /// </summary>
        public async Task SpawnRansom()
        {
            if (!Global.canAttack) return;
            Global.canAttack = false;

            bool mouseMoved = await RansomWarning();
            if (mouseMoved) // User moved the mouse
            {
                int generatedGold = 0;
                try { generatedGold = await Task.Run(() => GoldCoinManager.GenerateCoins()); }
                catch { }

                try
                {
                    if (generatedGold <= 0) // hopefully doesn't happen
                    {
                        await CrashJumpscare();
                    }
                    else
                    {
                        await DownloadJumpscare();

                        if (await Ransomed(generatedGold)) // If user didn't pay the ransom in time
                        {
                            Global.underRansom = false;
                            await CrashJumpscare();
                        }
                    }
                }
                catch
                {
                    await ResetRansom();
                }
            }
            else await ResetRansom(); // User didn't move the mouse, dodged the ransom
        }











        // ------------------- RANSOM PHASES -------------------

        /// <summary>
        /// First phase of Ransom
        /// </summary>
        /// <returns>true if the mouse moved during the warning phase</returns>
        public async Task<bool> RansomWarning()
        {
            Uri uri = new Uri("pack://application:,,,/Assets/Sounds/spawn.wav");
            System.Windows.Resources.StreamResourceInfo streamResourceInfo = WpfApplication.GetResourceStream(uri);
            SoundHandle spawnSound = SoundHelper.Create(streamResourceInfo.Stream);
            spawnSound.Play();

            // Shows Ransom's face randomly on the screen
            Global.RandomPosControl(img_idle);
            img_idle.Opacity = 100;

            await Task.Delay(500); // Mouse spy phase

            Global.lastRegisteredMousePos = System.Windows.Forms.Cursor.Position;
            Global.spyingMouse = true;

            // Center Ransom's face and show the warning sign
            img_stopsign.Opacity = 100;
            Global.CenterControl(img_idle);
            img_idle.Margin = new Thickness(img_idle.Margin.Left, img_idle.Margin.Top+50, 0, 0);
            Background = new SolidColorBrush(Color.FromRgb(40, 0, 0));

            await Task.Delay(500); // End of spy phase

            Global.spyingMouse = false;
            bool mouseMoved = System.Windows.Forms.Cursor.Position != Global.lastRegisteredMousePos;

            // Hide the warning sign, show Ransom face's on the center with a red background
            img_stopsign.Opacity = 0;
            Background = new SolidColorBrush(Color.FromArgb(100, 100, 0, 0));

            int waitTime = mouseMoved ? 25 : 100;
            await Task.Delay(waitTime);
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            img_idle.Opacity = 0;

            return mouseMoved;
        }

        /// <summary>
        /// Second phase of Ransom, Jumpscare+Downloading effect
        /// </summary>
        public async Task DownloadJumpscare()
        {
            SoundHandle attackSound = SoundHelper.Create(Global.GetResourceSteam("Sounds/attack.wav"));
            attackSound.Play();

            Global.CenterControl(img_attack);
            Thickness imgAttackCenter = img_attack.Margin; // Store the center position of the attack image for the shake effect
            img_attack.Opacity = 100;
            Background = new SolidColorBrush(Color.FromRgb(120, 0, 0));
            staticBg.Opacity = 0.05;

            // Ransom Face Shake Effect
            _ = ShakeAsync(imgAttackCenter, 40);

            await Task.Delay(800);

            // Downloading screen
            img_attack.Opacity = 0;

            SoundHandle installSound = SoundHelper.Create(Global.GetResourceSteam("Sounds/install.wav"));
            installSound.Play();

            vb_download.Opacity = 100;

            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };

            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 204, 0, 0), 0));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 150, 0, 0), 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 204, 0, 0), 1));

            bool effect = true;
            _ = Dispatcher.BeginInvoke(async () =>
            {
                while (effect)
                {
                    txt_download.Content = "DOWNLOADING";
                    txt_download.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    await Task.Delay(120);
                    txt_download.Content = "DOWNLOADING.";
                    await Task.Delay(120);
                    txt_download.Content = "DOWNLOADING..";
                    await Task.Delay(120);
                    txt_download.Content = "DOWNLOADING...";
                    await Task.Delay(120);
                    txt_download.Content = "DOWNLOADING";
                    txt_download.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    await Task.Delay(120);
                }
                
            });

            _ = Dispatcher.BeginInvoke(async () =>
            {
                foreach (Border seg in dpbSegments)
                {
                    await Task.Delay(120);
                    vb_download.Width = vb_download.Width + 20;
                    vb_download.Height = vb_download.Height + 20;
                    seg.Background = gradientBrush;
                }
            });
            

            // Text and download bar shake/glitch effect
            new Thread(() =>
            {
                for (int i = 0; i <= 40; i++)
                {
                    Thread.Sleep(40);
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            txt_download.Margin = new Thickness(Global.rng.Next(-5, 5), Global.rng.Next(-5, 5), 0, 0);
                            downloadProgressBar.Margin = new Thickness(Global.rng.Next(-20, 20), Global.rng.Next(-20, 20), 0, 0);
                        });
                    }
                    catch { break; }
                }
            })
            { IsBackground = true }.Start();

            await Task.Delay(1200);
            effect = false;

            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            img_attack.Opacity = 0;
            vb_download.Opacity = 0;
            foreach (Border seg in dpbSegments) seg.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            staticBg.Opacity = 0;
        }

        /// <summary>
        /// Third phase of Ransom, the actual ransom, plays the music, shows the Ransomed window, etc...
        /// </summary>
        /// <param name="generatedGold">Total value of the coins actually generated on disk for this run</param>
        public async Task<bool> Ransomed(int generatedGold)
        {
            WallpaperUpdater.SetDarkRedWallpaper();
            CursorUpdater.SetInfectedCursor();

            // Red Flash
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            Background = brush;

            ColorAnimation animation = new ColorAnimation
            {
                From = Color.FromArgb(255, 255, 0, 0),
                To = Color.FromArgb(0, 255, 0, 0),
                Duration = TimeSpan.FromMilliseconds(500)
            };

            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            redVignette.Opacity = 100;

            // OST
            SoundHandle layer1 = SoundHelper.Create(Global.GetResourceSteam("Sounds/layer1.wav"));
            SoundHandle layer2 = SoundHelper.Create(Global.GetResourceSteam("Sounds/layer2.wav"));
            SoundHandle layer3 = SoundHelper.Create(Global.GetResourceSteam("Sounds/layer3.wav"));

            // Clamp so an unlucky generation run can never leave the ransom unpayable
            Global.ransomLeft = Math.Min(Config.RansomAmount, generatedGold);
            Global.underRansom = true;

            // Lets payment wake the wait below immediately instead of only at the next layer boundary
            TaskCompletionSource<bool> paidSignal = new TaskCompletionSource<bool>();
            Global.RansomPayed = () => // Ransom payed event
            {
                layer1.Stop();
                layer2.Stop();
                layer3.Stop();

                this.Dispatcher.Invoke(() => ResetRansom());
                paidSignal.TrySetResult(true);
            };

            // Shows the main ransom window
            RansomNotification? ransomNotification = null;

            _ = Dispatcher.BeginInvoke(() =>
            {
                ransomNotification = new RansomNotification();
                ransomNotification.Show();
            }, DispatcherPriority.Normal);

            int layer3Seconds = 26;
            int remainingSeconds = Math.Max(0, Config.InfectionDuration - layer3Seconds);
            int layer1Seconds = remainingSeconds / 2;
            int layer2Seconds = remainingSeconds - layer1Seconds;

            Global.ransomTimeLeft = Config.InfectionDuration;

            layer1.PlayLooping();
            bool startedLayer2 = false;
            bool startedLayer3 = false;

            int elapsed = 0;
            while (elapsed < Config.InfectionDuration)
            {
                // Ticking every second keeps ransomTimeLeft accurate, still wakes early on payment
                if (await Task.WhenAny(Task.Delay(1000), paidSignal.Task) == paidSignal.Task) return false;
                if (!Global.underRansom) return false;

                elapsed++;
                Global.ransomTimeLeft = Math.Max(0, Config.InfectionDuration - elapsed);

                if (!startedLayer2 && elapsed >= layer1Seconds)
                {
                    startedLayer2 = true;
                    layer1.Stop();
                    layer2.PlayLooping();
                }
                else if (startedLayer2 && !startedLayer3 && elapsed >= layer1Seconds + layer2Seconds)
                {
                    startedLayer3 = true;
                    layer2.Stop();
                    layer3.Play();
                }
            }

            // If the code reaches here, this means the user didn't pay the ransom in time 
            if (ransomNotification != null)
            {
                ransomNotification.Close();
            }

            redVignette.Opacity = 0;
            return true;
        }

        /// <summary>
        /// Shows the jumpscare and shutdowns/exec a command depending on the config
        /// </summary>
        public async Task CrashJumpscare()
        {
            SoundHandle attackSound = SoundHelper.Create(Global.GetResourceSteam("Sounds/attack.wav"));
            attackSound.Play();

            Background = new SolidColorBrush(Color.FromRgb(100, 0, 0));
            staticBg.Opacity = 0.05;

            Global.CenterControl(img_attack);
            Thickness imgAttackCenter = img_attack.Margin; // Store the center position of the attack image for the shake effect
            img_attack.Opacity = 100;
            // Ransom Face Shake Effect
            _ = ShakeAsync(imgAttackCenter, 25);

            await Task.Delay(1000);

            if (Config.ExecCMDOnDeath)
            {
                string command = Config.CMDOnDeath.Trim();

                if (Uri.TryCreate(command, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = uri.ToString(),
                        UseShellExecute = true
                    });
                }
            }

            if (Config.CrashOnDeath)
            {
                Process.Start("shutdown", "/s /t 0");
            }

            await ResetRansom();
        }

    }
}