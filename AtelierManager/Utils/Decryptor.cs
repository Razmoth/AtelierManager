using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AtelierManager
{
    public static class Decryptor
    {
        public static byte[] Decrypt(byte[] bytes, string key)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            var signature = reader.ReadStringToNull(4);

            if (signature != "Aktk")
            {
                reader.BaseStream.Position = 0;
                return Array.Empty<byte>();
            }

            var version = reader.ReadInt16();
            if (version != 1)
            {
                throw new Exception("Invalid version !!");
            }

            var reserve = reader.ReadInt16();
            if (reserve != 0)
            {
                throw new Exception("Reserved should be 0, got {reserve} instead !!");
            }

            var isEncrypted = Convert.ToBoolean(reader.ReadInt32());
            var buffer = reader.ReadBytes(0x10);

            var relativeSize = reader.BaseStream.Length - reader.BaseStream.Position;
            var secretKey = string.Format(key, relativeSize);

            var data = reader.ReadBytes((int)relativeSize);
            if (isEncrypted)
            {
                var nonce = new byte[12];
                var context = new byte[0x200];

                var sha = SHA512.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
                var keyBytes = hash[..0x20];

                hash = sha.ComputeHash(hash);
                var nonceSeed = hash.ToArray();

                var counter = 0;
                var dataIndex = 0;

                var blockCount = data.Length / context.Length;
                var remainingCount = data.Length % context.Length;
                if (blockCount > 0)
                {
                    for (int i = 0; i < blockCount; i++)
                    {
                        GenerateKey(context, keyBytes, nonceSeed, nonce, counter++);
                        for (int j = 0; j < context.Length; j++)
                        {
                            data[dataIndex++] ^= context[j];
                        }
                    }
                }
                if (remainingCount > 0)
                {
                    GenerateKey(context, keyBytes, nonceSeed, nonce, counter++);
                    for (int i = 0; i < remainingCount; i++)
                    {
                        data[dataIndex++] ^= context[i];
                    }
                }
            }

            return data;
        }

        private static void GenerateKey(byte[] context, byte[] key, byte[] nonceSeedBytes, byte[] nonce, int counter)
        {

            var nonceSeedPart0High = BitConverter.ToUInt32(nonceSeedBytes, (counter % 0xD) | 0x30);
            var nonceSeedPart0Low = BitConverter.ToUInt32(nonceSeedBytes, counter / 0xD % 0xD);
            var nonceSeedPart1 = BitConverter.ToUInt32(nonceSeedBytes, counter / 0xA9 % 0xD | 0x10);
            var nonceSeedPart2 = BitConverter.ToUInt32(nonceSeedBytes, counter / 0x895 % 0xD | 0x20);

            var nonceSeed = BitOperations.RotateRight(nonceSeedPart0High, 0x1C * (int)((0x24924925 * (ulong)((counter / 0x152) & 0x3FFFFFFF)) >> 32) - 2 * (counter / 0xA9)) ^ BitOperations.RotateRight(nonceSeedPart0Low, -(3 * (counter / 0x93E) % 0x1B));

            var nonceInts = MemoryMarshal.Cast<byte, uint>(nonce);

            nonceInts[0] = nonceSeed;
            nonceInts[1] = nonceInts[0] ^ nonceSeedPart1;
            nonceInts[2] = nonceInts[1] ^ nonceSeedPart2;

            var chaCha20 = new ChaCha20(key, nonce, (uint)++counter);

            var rounds = new int[] { 12, 8, 8, 8, 4, 4, 4, 4 };
            for (int i = 0; i < rounds.Length; i++)
            {
                var iv = i == 0 ? new byte[0x40] : context.AsSpan((i - 1) * 0x40, 0x40).ToArray();
                chaCha20.DoRound(context.AsSpan(i * 0x40), iv, rounds[i]);
            }
        }

        public sealed class ChaCha20
        {
            private uint[] state;
            public ChaCha20(byte[] key, byte[] nonce, uint counter)
            {
                state = new uint[16];

                KeySetup(key);
                IvSetup(nonce, counter);
            }
            private void KeySetup(byte[] key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException("Key is null");
                }
                if (key.Length != 32)
                {
                    throw new ArgumentException(
                        $"Key length must be 32. Actual: {key.Length}"
                    );
                }

                // These are the same constants defined in the reference implementation.
                // http://cr.yp.to/streamciphers/timings/estreambench/submissions/salsa20/chacha8/ref/chacha.c
                byte[] constants = Encoding.ASCII.GetBytes("expand 32-byte k");

                state[4] = BinaryPrimitives.ReadUInt32LittleEndian(key);
                state[5] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(4));
                state[6] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(8));
                state[7] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(12));

                int keyIndex = key.Length - 16;

                state[8] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(keyIndex + 0));
                state[9] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(keyIndex + 4));
                state[10] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(keyIndex + 8));
                state[11] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(keyIndex + 12));

                state[0] = BinaryPrimitives.ReadUInt32LittleEndian(constants);
                state[1] = BinaryPrimitives.ReadUInt32LittleEndian(constants.AsSpan(4));
                state[2] = BinaryPrimitives.ReadUInt32LittleEndian(constants.AsSpan(8));
                state[3] = BinaryPrimitives.ReadUInt32LittleEndian(constants.AsSpan(12));
            }

            private void IvSetup(byte[] nonce, uint counter)
            {
                if (nonce == null)
                {
                    // There has already been some state set up. Clear it before exiting.
                    throw new ArgumentNullException("Nonce is null");
                }
                if (nonce.Length != 12)
                {
                    // There has already been some state set up. Clear it before exiting.
                    throw new ArgumentException(
                        $"Nonce length must be 12. Actual: {nonce.Length}"
                    );
                }

                state[12] = counter;
                state[13] = BinaryPrimitives.ReadUInt32LittleEndian(nonce);
                state[14] = BinaryPrimitives.ReadUInt32LittleEndian(nonce.AsSpan(4));
                state[15] = BinaryPrimitives.ReadUInt32LittleEndian(nonce.AsSpan(8));
            }

            public void DoRound(Span<byte> output, byte[] iv, int rounds)
            {
                uint[] x = new uint[16];    // Working buffer
                uint[] y = new uint[16];    // Working buffer
                byte[] tmp = new byte[64];  // Temporary buffer

                for (int i = 16; i-- > 0;)
                {
                    var value = this.state[i] ^ BinaryPrimitives.ReadUInt32LittleEndian(iv.AsSpan(i * 4));
                    x[i] = value;
                    y[i] = value;
                }

                for (int i = rounds; i > 0; i -= 2)
                {
                    QuarterRound(x, 0, 4, 8, 12);
                    QuarterRound(x, 1, 5, 9, 13);
                    QuarterRound(x, 2, 6, 10, 14);
                    QuarterRound(x, 3, 7, 11, 15);

                    QuarterRound(x, 0, 5, 10, 15);
                    QuarterRound(x, 1, 6, 11, 12);
                    QuarterRound(x, 2, 7, 8, 13);
                    QuarterRound(x, 3, 4, 9, 14);
                }

                for (int i = 16; i-- > 0;)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(tmp.AsSpan(i * 4), x[i] + y[i]);
                }

                state[12]++;
                if (state[12] <= 0)
                {
                    /* Stopping at 2^70 bytes per nonce is the user's responsibility */
                    state[13]++;
                }

                for (int i = 0x40; i-- > 0;)
                {
                    output[i] = tmp[i];
                }

                return;
            }

            public static void QuarterRound(uint[] x, uint a, uint b, uint c, uint d)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("Input buffer is null");
                }
                if (x.Length != 16)
                {
                    throw new ArgumentException();
                }

                x[a] += x[b]; x[d] = BitOperations.RotateLeft(x[d] ^ x[a], 16);
                x[c] += x[d]; x[b] = BitOperations.RotateLeft(x[b] ^ x[c], 12);
                x[a] += x[b]; x[d] = BitOperations.RotateLeft(x[d] ^ x[a], 8);
                x[c] += x[d]; x[b] = BitOperations.RotateLeft(x[b] ^ x[c], 7);
            }
        }
    }
}
