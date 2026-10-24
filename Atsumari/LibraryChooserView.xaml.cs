using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Atsumari
{
    public partial class LibraryChooserView : UserControl
    {
        List<Library> loadedLibraries = new();

        public event Action<Library>? libraryChosen;

        public LibraryChooserView()
        {
            InitializeComponent();
            LoadLibraries();
        }

        void LoadLibraries()
        {
            loadedLibraries = SaveManager.LoadAllLibraries();
            LibraryList.ItemsSource = loadedLibraries;
        }

        void Open_Click(object sender, RoutedEventArgs e) => OpenSelected();
        void Open_DoubleClick(object sender, MouseButtonEventArgs e) => OpenSelected();

        void OpenSelected()
        {
            if (LibraryList.SelectedItem is not Library chosenLibrary)
            {
                ChooserMessage.Text = "Pick a library first";
                return;
            }

            libraryChosen?.Invoke(chosenLibrary);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string newName = NewNameBox.Text.Trim();

            string? result = CheckName(newName);
            ChooserMessage.Text = result;

            if (result != null) return;

            SaveManager.SaveLibrary(new Library { libraryName = newName });
            LoadLibraries();
            NewNameBox.Text = "";
            ChooserMessage.Text = "";
        }

        string? CheckName(string name)
        {
            if (name.Length == 0) return "Name a library first.";

            foreach (Library library in loadedLibraries)
            {
                if (string.Equals(library.libraryName, name, StringComparison.OrdinalIgnoreCase)) 
                    return "Name already in use by another library.";
            }

            return null;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Library? chosen = (Library)LibraryList.SelectedItem;

            if (chosen == null)
            {
                ChooserMessage.Text = "Select a library first to remove it";
                return;
            }

            string libraryRemovedName = chosen.libraryName;

            var answer = MessageBox.Show($"Remove {libraryRemovedName}?", "Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;


            loadedLibraries.Remove(chosen);
            SaveManager.RemoveLibrary(chosen);

            ChooserMessage.Text = "Removed " + libraryRemovedName;

            LoadLibraries();
        }
    }
}
