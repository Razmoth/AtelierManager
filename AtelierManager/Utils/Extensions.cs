using System.Text;

namespace AtelierManager
{
    public static class BinaryReaderExtension
    {
        public static string ReadStringToNull(this BinaryReader reader, int count = 32767)
        {
            var readBytes = 0;
            var sb = new StringBuilder();
            while (reader.BaseStream.Position < reader.BaseStream.Length && readBytes < count)
            {
                var c = reader.ReadChar();
                if (c == 0)
                    break;

                sb.Append(c);
                readBytes++;
            }
            return sb.ToString();
        }
    }
}
