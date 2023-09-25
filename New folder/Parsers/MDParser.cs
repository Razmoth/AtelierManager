namespace HOMEManager
{
    public static class MDParser<T> where T : IParse<T>
    {
        public static T[] Read(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);
            reader.ReadInt32();
            var count = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();

            var data = new T[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = T.Parse(reader);
            }
            return data;
        }
    }

    public record JsonAssetBundleItem : IParse<JsonAssetBundleItem>
    {
        public string nm;
        public int sa;
        public int si;
        public int vr_ios;
        public int vr_dro;
        public string[] files;

        internal string Path => Utils.GetBundlePath(vr_dro, nm);
        internal string LocalPath => Utils.GetBundleLocalPath(nm);

        public static JsonAssetBundleItem Parse(BinaryReader reader)
        {
            var item = new JsonAssetBundleItem()
            {
                nm = reader.ReadString(),
                sa = reader.ReadInt32(),
                si = reader.ReadInt32(),
                vr_ios = reader.ReadInt32(),
                vr_dro = reader.ReadInt32(),
                files = reader.ReadArray(reader.ReadString),
            };
            return item;
        }
    }
}