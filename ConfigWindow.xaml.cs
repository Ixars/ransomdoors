using System.Windows;

namespace rans0m
{
    public partial class ConfigWindow : Window
    {
        private bool _loaded = false;
        public ConfigWindow() { InitializeComponent(); }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cb_spawnAutomatically.IsChecked = Config.SpawnAutomatically;
            cb_a90.IsChecked = Config.a90Mode;
            tb_minSpawnDelay.Text = Config.MinSpawnDelay.ToString();
            tb_maxSpawnDelay.Text = Config.MaxSpawnDelay.ToString();
            tb_duration.Text = Config.InfectionDuration.ToString();

            cb_crashOnDeath.IsChecked = Config.CrashOnDeath;
            cb_cmdOnDeath.IsChecked = Config.ExecCMDOnDeath;
            tb_cmd.Text = Config.CMDOnDeath;

            rb_userFolderMode.IsChecked = !Config.UseDrawerMode;
            rb_drawerMode.IsChecked = Config.UseDrawerMode;
            tb_ransomAmount.Text = Config.RansomAmount.ToString();

            tb_cmd.IsEnabled = cb_cmdOnDeath.IsChecked ?? false;
            tb_minSpawnDelay.IsEnabled = cb_spawnAutomatically.IsChecked ?? false;
            tb_maxSpawnDelay.IsEnabled = cb_spawnAutomatically.IsChecked ?? false;
            tb_crucChance.Text = Config.CrucChance.ToString();
            tb_honeyChance.Text = Config.HoneypotChance.ToString();

            _loaded = true;
        }

        private void FieldUpdated()
        {
            if (!_loaded) return;

            tb_cmd.IsEnabled = cb_cmdOnDeath.IsChecked ?? false;
            tb_minSpawnDelay.IsEnabled = cb_spawnAutomatically.IsChecked ?? false;
            tb_maxSpawnDelay.IsEnabled = cb_spawnAutomatically.IsChecked ?? false;

            Config.SpawnAutomatically = cb_spawnAutomatically.IsChecked ?? false;
            Config.a90Mode = cb_a90.IsChecked ?? false;
            Config.MinSpawnDelay = ParseClamped(tb_minSpawnDelay.Text, Config.MinSpawnDelay, 0, 86400);
            Config.MaxSpawnDelay = ParseClamped(tb_maxSpawnDelay.Text, Config.MaxSpawnDelay, 0, 86400);
            Config.InfectionDuration = ParseClamped(tb_duration.Text, Config.InfectionDuration, 5, 86400);

            Config.CrashOnDeath = cb_crashOnDeath.IsChecked ?? false;
            Config.ExecCMDOnDeath = cb_cmdOnDeath.IsChecked ?? false;
            Config.CMDOnDeath = tb_cmd.Text;

            Config.UseDrawerMode = rb_drawerMode.IsChecked ?? false;
            Config.RansomAmount = ParseClamped(tb_ransomAmount.Text, Config.RansomAmount, 1, 1000000);
            Config.CrucChance = ParseClamped(tb_crucChance.Text, Config.CrucChance, 0, 100);
            Config.HoneypotChance = ParseClamped(tb_honeyChance.Text, Config.HoneypotChance, 0, 100);

            Config.SaveConfig();
        }
        private void FieldUpdated(object sender, RoutedEventArgs e) => FieldUpdated();
        private void FieldUpdated(object sender, System.Windows.Controls.TextChangedEventArgs e) => FieldUpdated();

        private static int ParseClamped(string text, int fallback, int min, int max)
        {
            int value = Int32.TryParse(text, out int parsed) ? parsed : fallback;
            return Math.Clamp(value, min, max);
        }

        private void btn_spawn_Click(object sender, RoutedEventArgs e)
        {
            _=Global.overlayWindow.SpawnRansom();
            // Cuts the current spawn delay
            Global.overlayWindow?.RetriggerSpawnLoop();
        }

    }
}
