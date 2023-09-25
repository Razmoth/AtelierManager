using Newtonsoft.Json;

namespace HOMEManager
{
    public class ABRecord
    {
        [JsonProperty("projectName")]
        public string ProjectName { get; set; }
        [JsonProperty("assetBundleName")]
        public string AssetBundleName { get; set; }
        [JsonProperty("cryptoAlgorithm")]
        public CryptoAlgorithm CryptoAlgorithm { get; set; }
        [JsonProperty("offsetInBytes")]
        public int OffsetInBytes { get; set; }
        [JsonProperty("hash")]
        public RecordedHash Hash { get; set; }
        [JsonProperty("lastWriteTime")]
        public long LastWriteTime { get; set; }
        [JsonProperty("isStreamingSceneAssetBundle")]
        public bool IsStreamingSceneAssetBundle { get; set; }
        [JsonProperty("allDependencies")]
        public string[] AllDependencies { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }

        public string Url => Utils.GetMitakePath(ProjectName, AssetBundleName);
        public string Path => Utils.GetMitakeLocalPath(ProjectName, AssetBundleName);
        public string Name => System.IO.Path.GetFileNameWithoutExtension(AssetBundleName);

        public void Unpack(byte[] data)
        {
            byte[] bytes;

            switch (CryptoAlgorithm)
            {  
                case CryptoAlgorithm.Xor:
                    var xorMS = new MemoryStream(data, OffsetInBytes, data.Length - OffsetInBytes);
                    var xorReader = new BinaryReader(xorMS);

                    bytes = xorReader.ReadBytes((int)(xorReader.BaseStream.Length - xorReader.BaseStream.Position));
                    Decryptor.XOR.Decrypt(bytes, Hash.GetKey128());
                    break;
                case CryptoAlgorithm.AES:
                    var aesMS = new MemoryStream(data, OffsetInBytes, data.Length - OffsetInBytes);
                    var aesReader = new BinaryReader(aesMS);

                    var ivCount = aesReader.ReadInt32();
                    var iv = aesReader.ReadBytes(ivCount);

                    aesReader.ReadInt32();
                    var encrypted = aesReader.ReadBytes((int)(aesReader.BaseStream.Length - aesReader.BaseStream.Position));

                    bytes = Decryptor.AES.Decrypt(encrypted, Hash.GetKey256(), iv);
                    break;
                default:
                    bytes = data.AsSpan(OffsetInBytes, data.Length - OffsetInBytes).ToArray();
                    break;
            }

            File.WriteAllBytes(Path, bytes);
        }
    }
}
