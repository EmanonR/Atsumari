using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MediaBrowser
{
    public static class SaveManager
    {

        public static List<Item> LoadJson()
        {
            string json = GetFile();

            if (File.Exists(json))
            {
                string file = File.ReadAllText(json);
                List<Item>? back = JsonSerializer.Deserialize<List<Item>>(file) ?? new();
                return back;
            }
            else
            {
                return new();
            }
        }

        public static void SaveJson(List<Item> items)
        {
            string text = JsonSerializer.Serialize(items);

            string json = GetFile();

            File.WriteAllText(json, text);
        }

        public static string GetFile()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaBrowser");
            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "Items.json");
        }
    }
}
