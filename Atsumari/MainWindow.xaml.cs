using System.Windows;

namespace Atsumari
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ChooserScreen.libraryChosen += OnLibraryChosen;
            ChooserScreen.libraryDeleted += OnLibraryDeleted;
        }

        void OnLibraryChosen(Library library)
        {
            bool r = LibraryScreen.OpenLibrary(library);
            if (r)
            {
                LibraryTab.IsEnabled = true;
                MainTabs.SelectedItem = LibraryTab;
            }
            else
            {
                MessageBox.Show("A scan is still running.\nCant open library during scan");
            }
        }

        void OnLibraryDeleted(Library library)
        {
            if (LibraryScreen.openLibraryId != library.ID) return;

            LibraryScreen.CloseLibrary();
            LibraryTab.IsEnabled = false;
        }
    }
}