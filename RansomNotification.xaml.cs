using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace rans0m
{
    public partial class RansomNotification : Window
    {
        private DispatcherTimer _timer;

        public RansomNotification()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Closed += (_, _) => _timer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HideSystemMenu(this);

            // Init the window
            txt_timer.Text = $"TIME: {Global.ransomTimeLeft / 60:D2}:{Global.ransomTimeLeft % 60:D2}";
            Global.RandomPosWindow(this);

            // Init 9 taunt windows
            for (int i = 0; i < 9; i++)
            {
                TauntWindow tauntWindow = new TauntWindow();
                tauntWindow.Show();
            }

            Global.GlitchIdle(this, true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txt_ransomLeft.Text = Global.ransomLeft.ToString();
            txt_timer.Text = $"TIME: {Global.ransomTimeLeft / 60:D2}:{Global.ransomTimeLeft % 60:D2}";
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // TODO : Check if file is a valid .gold file
                e.Effects = DragDropEffects.Link;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            bool sfxPlayed = false; // Prevent multiple sfx to play when user drags multiple coins
            foreach (string file in files) // technically it's easily cheat but who cares
            {
                string extension = Path.GetExtension(file);

                if (extension.StartsWith(".gold"))
                {
                    try
                    {
                        int goldType = Int32.Parse(extension.Replace(".gold", ""));
                        Dictionary<string, string> goldFileData = GoldCoinManager.DecryptCoinFile(file);

                        if (goldFileData == null || !goldFileData.TryGetValue("RANSOM_COIN", out string coinId))
                            continue; // Corrupt/foreign file

                        if (!Global.usedCoins.Contains(coinId)) // If coin hasn't been used yet (to prevent peoples from just copy pasting coins
                        {
                            if (!sfxPlayed)
                            {
                                // Use the coin
                                SoundHandle cashSfx = SoundHandle.Create(Global.GetResourceSteam("Sounds/cash.wav")); // Need to replace the sfx it's kinda trash
                                cashSfx.Play();
                                sfxPlayed = true;
                            }

                            Global.usedCoins.Add(coinId);
                            Global.ransomLeft -= CoinValues.ExtensionValues[goldType];

                            File.Delete(file);
                        }
                    }
                    catch { }

                }
                else if (extension == ".crucifix")
                {
                    try
                    {
                        Dictionary<string, string> crucifixData = GoldCoinManager.DecryptCoinFile(file);

                        if (crucifixData == null || !crucifixData.TryGetValue("RANSOM_COIN", out string coinId))
                            continue; // Corrupt/foreign file

                        if (!Global.usedCoins.Contains(coinId))
                        {
                            Global.usedCoins.Add(coinId);
                            File.Delete(file);

                            Global.crucifixUsed = true;
                            Global.ransomLeft = 0;
                        }
                    }
                    catch { }
                }
            }

            if (Global.ransomLeft <= 0)
            {
                Global.underRansom = false;

                if (Global.crucifixUsed)
                    new CrucifixWindow(this.Left, this.Top).Show();
                else
                    new ThankYou(this.Left, this.Top).Show();

                Close();
            }

        }
    }
}
