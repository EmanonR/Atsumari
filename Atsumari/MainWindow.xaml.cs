using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Atsumari
{
    public partial class MainWindow : Window
    {

        private List<Item> items = new();
        string[] skip = { "unins", "crash" };

        Item selectedItem;

        public MainWindow()
        {
            InitializeComponent();

            items = SaveManager.LoadJson();
            RefreshList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Executable (*.exe)|*.exe" };

            if (dialog.ShowDialog() == true)
            {
                bool exists = items.Any(i => i.originalName == System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));

                if (exists)
                {
                    MessageBox.Show("Item already added to library");
                    return;
                }

                AddItemsToList([new Item(dialog.FileName)]);
            }
        }

        private void Add_Folder(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog { Title = "Pick a folder" };

            if (dialog.ShowDialog() == true)
            {
                AddItemsToList(SearchFolderForExes(dialog.FolderName));
            }
        }

        void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                List<Item> newItems = new();

                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // folder
                        newItems.AddRange(SearchFolderForExes(path));
                    }
                    else if (File.Exists(path))
                    {
                        // File

                        //Check if exe
                        MessageBox.Show(System.IO.Path.GetExtension(path));

                        newItems.Add(new Item(path));
                    }
                }

                AddItemsToList(newItems);
            }
        }

        List<Item> SearchFolderForExes(string folder)
        {
            //Look for game exe
            List<Item> newItems = new();

            string? best = Directory.GetFiles(folder, "*.exe")
                .Where(f => !skip.Any(w => f.Contains(w, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            if (best != null)
            {
                newItems.Add(new Item(best));

                //Return out early to skip further subfolders
                return newItems;
            }

            //Gather subfolders
            string[] subs = Directory.GetDirectories(folder);

            //Return if no subs
            if (subs.Length == 0) return new();

            //search in them
            foreach (string sub in subs)
            {
                newItems.AddRange(SearchFolderForExes(sub));
            }

            //Finish
            return newItems;
        }

        void AddItemsToList(List<Item> newItems)
        {
            if (newItems.Count == 0) return;

            foreach (var item in newItems)
            {
                items.Add(item);
            }

            RefreshList();

            //Save json
            SaveManager.SaveJson(items);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var chosen = ItemList.SelectedItems.Cast<Item>().ToList();

            if (chosen.Count < 1)
            {
                MessageBox.Show("Select an item to remove it");
                return;
            }

            var answer = MessageBox.Show($"Remove {chosen.Count} items?", "Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;


            foreach (var item in chosen)
            {
                items.Remove(item);
            }

            SaveManager.SaveJson(items);
            RefreshList();
        }

        private void Launch_Click(object sender, RoutedEventArgs e) => LaunchExe();

        private void ItemList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => LaunchExe(); 

        void LaunchExe()
        {
            if (ItemList.SelectedItems.Count > 1)
            {
                MessageBox.Show("Select only one item to launch");
                return;
            }

            if (ItemList.SelectedItem is not Item item)
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
            ItemList.ItemsSource = null;
            ItemList.ItemsSource = items;
        }

        private void ItemList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ItemList.SelectedItem is Item item)
            {
                selectedItem = item;

                selectedItemInput.Text = selectedItem.displayName;
                selectedItemIcon.Source = selectedItem.Icon;
            }
        }

        private void selectedItemInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (selectedItem == null) return;

            selectedItem.displayName = selectedItemInput.Text;
            SaveManager.SaveJson(items);
            RefreshList();
        }
    }
}