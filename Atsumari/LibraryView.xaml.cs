using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Atsumari
{
    public partial class LibraryView : UserControl
    {
        string[] skip = { "unins", "crash" , "config", "install" , "setup"};

        private ICollectionView _itemsView = null!;
        private Library currentLibrary = null!;
        private bool isBusy;

        public LibraryView()
        {
            InitializeComponent();
        }

        public bool OpenLibrary(Library library)
        {
            if (isBusy) return false;

            currentLibrary = library;

            ItemsListBox.ItemsSource = library.itemsInLibrary;
            _itemsView = CollectionViewSource.GetDefaultView(library.itemsInLibrary);
            _itemsView.Filter = item => string.IsNullOrWhiteSpace(FilterBox.Text) || ((Item)item).displayName.Contains(FilterBox.Text, StringComparison.OrdinalIgnoreCase);
            return true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Executable (*.exe)|*.exe" };

            if (dialog.ShowDialog() == true)
            {
                bool exists = CheckForDuplicate(dialog.FileName);

                if (exists)
                {
                    MessageBox.Show("Item already added to library");
                    return;
                }

                AddItemsToList([new Item(dialog.FileName)]);
            }
        }

        private async void Add_Folder(object sender, RoutedEventArgs e)
        {
            if (currentLibrary == null) return;
            if (isBusy) return;

            var dialog = new OpenFolderDialog { Title = "Pick a folder" };

            if (dialog.ShowDialog() == true)
            {
                isBusy = true;
                ScanInfoBox.Text = $"Scanning...";

                string path = dialog.FolderName;

                try
                {
                    List<Item> newItems = await Task.Run(() => SearchPathForExes(path));

                    AddItemsToList(newItems);
                    ScanInfoBox.Text = $"Scan complete!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Scan failed:\n" + ex.Message);
                    ScanInfoBox.Text = $"Scanning failed!";
                }
                finally
                {
                    isBusy = false;
                }
            }


        }

        async void Window_Drop(object sender, DragEventArgs e)
        {
            if (currentLibrary == null) return;
            if (isBusy) return;

            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                isBusy = true;
                ScanInfoBox.Text = $"Scanning...";

                try
                {
                    List<Item> newItems = await Task.Run(() => SearchPathsForExes(paths.ToList()));

                    AddItemsToList(newItems);
                    ScanInfoBox.Text = $"Scan complete!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Scan failed:\n" + ex.Message);
                    ScanInfoBox.Text = $"Scanning failed!";
                }
                finally
                {
                    isBusy = false;
                }
            }
        }

        List<Item> SearchPathsForExes(List<string> paths)
        {
            List<Item> newItems = new();

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    // folder
                    newItems.AddRange(SearchPathForExes(path));
                }
                else if (File.Exists(path))
                {
                    // File

                    //Check if exe
                    if (!string.Equals(Path.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase)) continue;

                    newItems.Add(new Item(path));
                }
            }

            return newItems;
        }

        List<Item> SearchPathForExes(string path)
        {
            //Look for game exe
            List<Item> newItems = new();

            string? best = Directory.GetFiles(path, "*.exe")
                .Where(f => !skip.Any(w => Path.GetFileName(f).Contains(w, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            string[] subs = Directory.GetDirectories(path);

            if (best != null)
            {
                newItems.Add(new Item(best));

                //Return out early to skip further subfolders
                return newItems;
            }

            //Gather subfolders


            //Return if no subs
            if (subs.Length == 0) return new();

            //search in them
            foreach (string sub in subs)
            {
                newItems.AddRange(SearchPathForExes(sub));
            }

            //Finish
            return newItems;
        }

        void AddItemsToList(List<Item> newItems)
        {
            if (newItems.Count == 0) return;

            foreach (var item in newItems)
            {
                bool exists = CheckForDuplicate(item.filePath);

                if (exists) continue;

                currentLibrary.itemsInLibrary.Add(item);
            }

            RefreshList();

            //Save json
            SaveManager.SaveLibrary(currentLibrary);
        }

        private bool CheckForDuplicate(string checkPath)
        {
            return currentLibrary.itemsInLibrary.Any(i => String.Equals(i.filePath, checkPath, StringComparison.OrdinalIgnoreCase));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var chosen = ItemsListBox.SelectedItems.Cast<Item>().ToList();

            if (chosen.Count < 1)
            {
                MessageBox.Show("Select an item to remove it");
                return;
            }

            var answer = MessageBox.Show($"Remove {chosen.Count} items?", "Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;

            if (chosen.Count == 1)
            {
                ScanInfoBox.Text = $"Removed {chosen[0].displayName}.";
            }
            else
            {
                ScanInfoBox.Text = $"Removed {chosen.Count} items.";
            }

            foreach (var item in chosen)
            {
                currentLibrary.itemsInLibrary.Remove(item);
            }
            SaveManager.SaveLibrary(currentLibrary);
            RefreshList();
        }

        private void Remove_All(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show($"Remove all items?", "Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;

            int removeCount = currentLibrary.itemsInLibrary.Count;

            currentLibrary.itemsInLibrary.Clear();
            SaveManager.SaveLibrary(currentLibrary);
            RefreshList();

            ScanInfoBox.Text = $"Removed {removeCount} items.";
        }

        private void Launch_Click(object sender, RoutedEventArgs e) => LaunchExe();

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedItem == null) return;

            Item item = ItemsListBox.SelectedItem as Item;

            Process.Start("explorer.exe", $"/select,\"{item.filePath}\"");
        }

        private void ItemList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => LaunchExe();

        void LaunchExe()
        {
            if (ItemsListBox.SelectedItems.Count > 1)
            {
                MessageBox.Show("Select only one item to launch");
                return;
            }

            if (ItemsListBox.SelectedItem is not Item item)
            {
                MessageBox.Show("Select one first.");
                return;
            }

            if (string.IsNullOrEmpty(item.filePath))
            {
                MessageBox.Show("This item has no exe.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.filePath,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't start it:\n" + ex.Message);
            }
        }

        private void RefreshList()
        {
            _itemsView.Refresh();
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _itemsView.Refresh();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveManager.SaveLibrary(currentLibrary);
        }
    }
}
