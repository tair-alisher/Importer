namespace Importer.Types
{
    class Map
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Map(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
