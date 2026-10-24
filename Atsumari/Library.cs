using System.Text.Json.Serialization;

namespace Atsumari
{
    public class Library
    {
        [JsonIgnore]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public string libraryName { get; set; } = "";
        public List<Item> itemsInLibrary { get; set; } = new();
    }
}
