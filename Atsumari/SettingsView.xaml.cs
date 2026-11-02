using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Atsumari
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void OpenDir_Click(object sender, RoutedEventArgs e)
        {
            string path = SaveManager.GetLibFolder();
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void DarkmodeToggle_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
