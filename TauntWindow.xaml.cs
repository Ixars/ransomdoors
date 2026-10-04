using System.IO;
using System.Windows;
using System.Windows.Media;

namespace rans0m
{
    public partial class TauntWindow : Window
    {
        private bool _closed = false;

        public TauntWindow()
        {
            InitializeComponent();
            Closed += (_, _) => _closed = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HideSystemMenu(this);

            Stream tauntSpawnStream = Global.GetResourceSteam("Sounds/tauntSpawn.wav");
            _ = Task.Run(() => SoundHandle.Create(tauntSpawnStream).Play());

            // Sets random window title, image and size
            Title = Global.tauntTitles[Global.rng.Next(Global.tauntTitles.Count)];
            Background = new ImageBrush(Global.tauntImages[Global.rng.Next(Global.tauntImages.Count)]);
            Width = Global.rng.Next(200, 400);
            Height = Global.rng.Next(200, 400);

            // Random pos
            Global.RandomPosWindow(this);

            // Glitch Idle Effect
            Global.GlitchIdle(this);

            // Closes after 4-10 seconds
            _ = CloseAfterDelayAsync();
        }

        private async Task CloseAfterDelayAsync()
        {
            await Task.Delay(Global.rng.Next(4000, 10 * 1000));
            // ResetRansom's cleanup sweep may have already closed this while the timer was counting down
            if (_closed) return;
            SoundHandle.Create(Global.GetResourceSteam("Sounds/tauntLeave.wav")).Play();
            Close();
        }
    }
}
