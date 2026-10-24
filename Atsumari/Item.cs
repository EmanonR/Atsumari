using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Atsumari
{
    public class Item(string filePath) : INotifyPropertyChanged
{
        public string originalName { get; set; } = Path.GetFileNameWithoutExtension(filePath);
        private string _displayName = Path.GetFileNameWithoutExtension(filePath);
        public string filePath { get; set; } = filePath;
        public List<string> tags { get; set; } = new();
        public List<string> collections { get; set; } = new();

        private ImageSource? icon;
        private bool iconRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string displayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName == value) return;

                _displayName = value;
                OnPropertyChanged(nameof(displayName));
            }
        }

        [JsonIgnore]
        public ImageSource? Icon
        {
            get
            {
                if (!iconRequested)
                {
                    iconRequested = true;
                    LoadIcon();
                }

                return icon;
            }
        }

        private async void LoadIcon()
        {
            string path = filePath;
            ImageSource? loaded = await Task.Run(() => PictureHelper.GetPicture(path));

            icon = loaded;
            OnPropertyChanged(nameof(Icon));
        }
    }
}
