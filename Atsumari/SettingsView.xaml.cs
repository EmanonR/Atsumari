using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
