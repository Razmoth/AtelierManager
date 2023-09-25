using Newtonsoft.Json;
using System.Buffers.Binary;

namespace HOMEManager
{
    public class ABDownloadManifest
    {
        [JsonProperty("_projectName")]
        public string ProjectName { get; set; }
        [JsonProperty("_records")]
        public ABRecord[] Records { get; set; }
        [JsonProperty("_assetBundleNamesWithVariant")]
        public string[] AssetBundleNamesWithVariant { get; set; }

        public static ABDownloadManifest Parse(byte[] data)
        {
            ABDownloadManifest manifest = new ABDownloadManifest();

            var deflatedSize = BinaryPrimitives.ReadUInt32LittleEndian(data);
            var json = Utils.DeflateJson(data[4..], deflatedSize);
            JsonConvert.PopulateObject(json, manifest);

            return manifest;
        }
    }
}
