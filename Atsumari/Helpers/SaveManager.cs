using System.IO;
using System.Text.Json;

namespace Atsumari
{
    public static class SaveManager
    {
        public static string GetLibFolder()
        {
            string folder = Path.Combine(GetAppFolder(), "Libraries");
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string GetAppFolder()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Atsumari");
            Directory.CreateDirectory(folder);

            return folder;
        }

        public static Library LoadLibrary(string filePath)
        {
            string text = File.ReadAllText(filePath);
            Library library = JsonSerializer.Deserialize<Library>(text) ?? new Library();
            library.ID = Path.GetFileNameWithoutExtension(filePath);
            return library;
        }

        public static void SaveLibrary(Library library)
        {
            string text = JsonSerializer.Serialize(library);
            string file = Path.Combine(GetLibFolder(), library.ID + ".json");
            File.WriteAllText(file, text);
        }

        public static void RemoveLibrary(Library library)
        {
            string path = Path.Combine(GetLibFolder(), library.ID + ".json");
            File.Delete(path);
        }

        public static List<Library> LoadAllLibraries()
        {
            List<Library> librariesFound = new();

            foreach (string file in Directory.GetFiles(GetLibFolder(), "*.json"))
            {
                try
                {
                    librariesFound.Add(LoadLibrary(file));
                }
                catch (JsonException) { }
                catch (IOException) { } 
            }

            return librariesFound;
        }
    }
}
