using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Text.Json.Serialization;

namespace Atsumari
{
    public class Item(string exePath)
    {
        public string Name { get; set; } = Path.GetFileNameWithoutExtension(exePath);
        public string ExePath { get; set; } = exePath;

        private ImageSource? icon;

        [JsonIgnore]
        public ImageSource? Icon
        {
            get
            {
                if (icon == null) icon = PictureHelper.GetPicture(exePath);
                return icon;
            }
        }
    }
}
