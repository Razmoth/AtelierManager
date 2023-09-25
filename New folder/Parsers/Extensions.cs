using System.Text;

namespace HOMEManager
{
    public static class BinaryReaderExtension
    {
        public static T[] ReadArray<T>(this BinaryReader reader, Func<T> action)
        {
            T[] array;
            var str = reader.ReadString();
            if (int.TryParse(str[1..], out var count) && count > 0)
            {
                array = new T[count];
                for (int i = 0; i < count; i++)
                {
                    array[i] = action();
                }
            }
            else array = Array.Empty<T>();
            return array;
        }

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
