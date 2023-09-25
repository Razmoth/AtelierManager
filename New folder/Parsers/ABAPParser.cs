namespace HOMEManager
{
    public static class ABAPParser
    {
        private const string Signature = "ppir";

        public static void Parse(byte[] data, string bundleName)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            var signature = reader.ReadStringToNull(4);
            if (signature != Signature)
                throw new Exception("Invalid Signature !!");

            var header = ABAPHeader.Parse(reader);

            var infos = new List<ABAPEntryInfo>();
            for (int i = 0; i < header.dataCount; i++)
            {
                var info = ABAPEntryInfo.Parse(reader);
                infos.Add(info);
            }

            foreach (var info in infos)
            {
                var entry = ABAPEntry.Parse(reader, header, info);
                Console.WriteLine($"Found {entry.Name} in {bundleName} !!");
                entry.Data = Decryptor.ABA.Decrypt(entry.Data);
                File.WriteAllBytes(Utils.GetFilePath(bundleName, entry.Name), entry.Data);
            }
        }
    }

    public record ABAPHeader
    {
        public uint rootHeadOffset;
        public uint nameHeadOffset;
        public uint dataHeadOffset;
        public uint rootBufferSize;
        public uint nameBufferSize;
        public int dataCount;

        public static ABAPHeader Parse(BinaryReader reader)
        {
            var header = new ABAPHeader()
            {
                rootHeadOffset = reader.ReadUInt32(),
                nameHeadOffset = reader.ReadUInt32(),
                dataHeadOffset = reader.ReadUInt32()
            };

            header.rootBufferSize = header.nameHeadOffset - header.rootHeadOffset;
            header.nameBufferSize = header.dataHeadOffset - header.nameHeadOffset;
            header.dataCount = (int)header.rootBufferSize >> 4;

            return header;
        }
    }

    public record ABAPEntryInfo
    {
        public int NameOffset;
        public int NameSize;
        public int DataOffset;
        public int DataSize;

        public static ABAPEntryInfo Parse(BinaryReader reader)
        {
            var info = new ABAPEntryInfo()
            {
                NameOffset = reader.ReadInt32(),
                NameSize = reader.ReadInt32(),
                DataOffset = reader.ReadInt32(),
                DataSize = reader.ReadInt32(),
            };
            return info;
        }
    }


    public record ABAPEntry
    {
        public string Name;
        public byte[] Data;

        public static ABAPEntry Parse(BinaryReader reader, ABAPHeader header, ABAPEntryInfo info)
        {
            var entry = new ABAPEntry();

            reader.BaseStream.Position = header.nameHeadOffset + info.NameOffset;
            entry.Name = reader.ReadStringToNull(info.NameSize);

            reader.BaseStream.Position = header.dataHeadOffset + info.DataOffset;
            entry.Data = reader.ReadBytes(info.DataSize);

            return entry;
        }
    }
}
