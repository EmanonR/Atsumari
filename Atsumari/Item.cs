using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Text.Json.Serialization;

namespace Atsumari
{
    public class Item(string filePath)
    {
        public string originalName { get; set; } = Path.GetFileNameWithoutExtension(filePath);
        public string displayName { get; set; } = Path.GetFileNameWithoutExtension(filePath);
        public string filePath { get; set; } = filePath;
        public List<string> tags { get; set; } = new();
        public List<string> collections { get; set; } = new();

        private ImageSource? icon;

        [JsonIgnore]
        public ImageSource? Icon
        {
            get
            {
                if (icon == null) icon = PictureHelper.GetPicture(filePath);
                return icon;
            }
        }
    }
}
