using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Atsumari
{
    public static class PictureHelper
    {

        public static ImageSource? GetPicture(string path)
        {
            try
            {
                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (icon == null) return null;

                var image = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                image.Freeze();
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
