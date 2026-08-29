using System;
using System.Text;

namespace WandEnhancer.Core.Security
{
    internal static class StringProtector
    {
        private static readonly byte[] XOR_KEY = new byte[]
        {
            0x4B, 0x77, 0x69, 0x77, 0x69, 0x50, 0x72, 0x6F,
            0x74, 0x65, 0x63, 0x74, 0x2D, 0x76, 0x32
        };

        public static string Decode(byte[] obfuscated, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = (byte)(obfuscated[i] ^ XOR_KEY[i % XOR_KEY.Length]);
            string decoded = Encoding.UTF8.GetString(result);
            Array.Clear(result, 0, result.Length);
            return decoded;
        }
    }
}
