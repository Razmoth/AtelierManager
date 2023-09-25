using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace HOMEManager
{
    public struct RecordedHash
    {
        [JsonProperty("u0")]
        public uint U0 { get; set; }
        [JsonProperty("u1")]
        public uint U1 { get; set; }
        [JsonProperty("u2")]
        public uint U2 { get; set; }
        [JsonProperty("u3")]
        public uint U3 { get; set; }

        public RecordedHash(uint u0, uint u1, uint u2, uint u3)
        {
            U0 = u0;
            U1 = u1;
            U2 = u2;
            U3 = u3;
        }

        public byte[] GetKey128()
        {
            var key = new uint[] {
                U0,
                U1,
                U2,
                U3
            };
            return MemoryMarshal.AsBytes<uint>(key).ToArray();
        }
        public byte[] GetKey256()
        {
            var key = new uint[] {
                U1 ^ U0,
                U2 ^ U1,
                U3 ^ U2,
                U3 ^ U0,
                U2 ^ U0,
                U3 ^ U1,
                U0  << 0x10 | U1 >> 0x10,
                U2  << 0x10 | U3 >> 0x10
            };
            return MemoryMarshal.AsBytes<uint>(key).ToArray();
        }
    }
}
