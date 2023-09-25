using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AtelierManager
{
    public static class CatalogParser
    {
        public static List<BundleInfo> Parse(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            var catalog = JObject.Parse(str);

            var bundlesInfos = new List<BundleInfo>();
            try
            {
                if (catalog.TryGetValue("_fileCatalog", out var fileCatalogToken))
                {
                    var fileCatalog = fileCatalogToken as JObject;
                    if (fileCatalog.TryGetValue("_bundles", out var bundlesToken))
                    {
                        var bundles = bundlesToken as JArray;
                        bundlesInfos.AddRange(bundles.ToArray().Select(x => x.ToObject<BundleInfo>()));
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to parse catalog !!");
            }
            return bundlesInfos;
        }
    }

    public record BundleInfo
    {
        [JsonProperty("_relativePath")]
        public string RelativePath { get; set; }
        [JsonProperty("_bundleName")]
        public string BundleName { get; set; }
        [JsonProperty("_hash")]
        public string Hash { get; set; }
        [JsonProperty("_crc")]
        public uint Crc { get; set; }
        [JsonProperty("_fileSize")]
        public uint FileSize { get; set; }
        [JsonProperty("_fileMd5")]
        public string FileMd5 { get; set; }
        [JsonProperty("_compression")]
        public int Compression { get; set; }
        [JsonProperty("_userData")]
        public string UserData { get; set; }

        public string Key => $"{BundleName}-{{0}}-{Hash}-{Crc}";
    }
}
