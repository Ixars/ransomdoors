using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace rans0m
{
    public partial class ThankYou : Window
    {
        double previousLeft;
        double previousTop;
        public ThankYou(double left, double top)
        {
            InitializeComponent();
            Global.HideSystemMenu(this);

            previousLeft = left;
            previousTop = top;
        }

        private async void PlayAnimation()
        {
            Global.RansomPayed();

            Brush ogBackground = grid.Background;
            grid.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.img_thx.Opacity = 0;
            this.img_oksign.Opacity = 0;

            this.Left = previousLeft;
            this.Top = previousTop;

            var (screenW, screenH) = Global.OverlaySizeDips();

            double totalWidth = Width * 1.3;
            double totalHeight = Height * 1.3;
            double centerLeft = (screenW / 2) - totalWidth / 2;
            double centerTop = (screenH / 2) - totalHeight / 2;

            await Task.Delay(200);
            this.Left = Global.Lerp(this.Left, centerLeft, 0.5);
            this.Top = Global.Lerp(this.Top, centerTop, 0.5);
            this.Width = Global.Lerp(this.Width, totalWidth, 0.5);
            this.Height = Global.Lerp(this.Height, totalHeight, 0.5);

            await Task.Delay(100);
            this.Left = Global.Lerp(this.Left, centerLeft, 1);
            this.Top = Global.Lerp(this.Top, centerTop, 1);
            this.Width = Global.Lerp(this.Width, totalWidth, 1);
            this.Height = Global.Lerp(this.Height, totalHeight, 1);

            await Task.Delay(200);
            grid.Background = ogBackground;
            this.img_ransom.Opacity = 0;
            this.img_oksign.Opacity = 1;

            SoundHandle thankYouSfx = SoundHandle.Create(Global.GetResourceSteam("Sounds/thankyou.wav"));
            thankYouSfx.Play();

            Global.CenterWindow(this);

            DoubleAnimation growAnimation = new DoubleAnimation
            {
                From = 0.1,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            img_thx.Opacity = 0;

            scaleOkSign.BeginAnimation(ScaleTransform.ScaleXProperty, growAnimation);
            scaleOkSign.BeginAnimation(ScaleTransform.ScaleYProperty, growAnimation);

            DoubleAnimation growAnimationThx = new DoubleAnimation
            {
                From = 0.1,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };

            _=Task.Delay(400).ContinueWith(_ => {
                Dispatcher.Invoke(() =>
                {
                    img_thx.Opacity = 100;

                    scaleTxtThx.BeginAnimation(ScaleTransform.ScaleXProperty, growAnimationThx);
                    scaleTxtThx.BeginAnimation(ScaleTransform.ScaleYProperty, growAnimationThx);
                });

                Task.Delay(4000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => Close());
                });
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HideSystemMenu(this);
            PlayAnimation();
        }
    }
}