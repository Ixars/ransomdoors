using System.Windows;

namespace rans0m
{
    public partial class CrucifixWindow : Window
    {
        private SoundHandle? _sound;
        private bool _closed;

        public CrucifixWindow(double left, double top)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
            Closed += (_, _) => _closed = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HideSystemMenu(this);

            Global.RansomPayed?.Invoke();

            _sound = SoundHandle.Create(Global.GetResourceSteam("Sounds/crucifix.wav"));
            _sound.Play();

            _ = CloseAfterFallbackDelayAsync();
        }

        private async Task CloseAfterFallbackDelayAsync()
        {
            await Task.Delay(15000);
            if (_closed) return;
            _sound?.Stop();
            Close();
        }

        private void Img_AnimationCompleted(object sender, RoutedEventArgs e)
        {
            if (_closed) return;
            _sound?.Stop();
            Close();
        }
    }
}
