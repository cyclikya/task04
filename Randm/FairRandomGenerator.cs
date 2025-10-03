using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

namespace Randm
{
    public class FairRandomRecord
    {
        public int MortyValue { get; set; }
        public int RickValue { get; set; }
        public int Final { get; set; }
        public int Modulo { get; set; }
        public byte[] Key { get; set; } = null!;
        public byte[] Hmac { get; set; } = null!;

        public string KeyHex => Convert.ToHexStringLower(Key);
        public string HmacHex => Convert.ToHexStringLower(Hmac);
    }

    public class FairRandomGenerator
    {
        private readonly GameCore _host;
        private readonly List<FairRandomRecord> _records = new();

        public FairRandomGenerator(GameCore host)
        {
            _host = host;
        }

        public int Generate(int n, string purpose)
        {
            if (n <= 0)
                throw new ArgumentException("n must be > 0", nameof(n));

            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            int mortyValue = RandomNumberGenerator.GetInt32(n);

            var mortyBytes = BitConverter.GetBytes(mortyValue);
            byte[] hmac = ComputeHmacSha3(key, mortyBytes);

            Console.WriteLine($"Morty: (fair rng for {purpose}) HMAC={Convert.ToHexStringLower(hmac)}");

            int rick = ReadIntFromConsole($"Rick: enter your number [0,{n}) for randomness:", 0, n - 1);

            int final = (mortyValue + rick) % n;

            var rec = new FairRandomRecord
            {
                Key = key,
                MortyValue = mortyValue,
                RickValue = rick,
                Final = final,
                Modulo = n,
                Hmac = hmac
            };
            _records.Add(rec);

            return final;
        }

        public FairRandomRecord[] ConsumeAndGetRecentRecords()
        {
            var a = _records.ToArray();
            _records.Clear();
            return a;
        }

        private static byte[] ComputeHmacSha3(byte[] key, byte[] message)
        {
            var hmac = new HMac(new Sha3Digest(512));
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(message, 0, message.Length);
            byte[] outmac = new byte[hmac.GetMacSize()];
            hmac.DoFinal(outmac, 0);
            return outmac;
        }

        private int ReadIntFromConsole(string prompt, int min, int max)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                Console.Write("> ");
                var s = Console.ReadLine();
                if (!int.TryParse(s, out int v))
                {
                    Console.WriteLine("Please enter an integer.");
                    continue;
                }
                if (v < min || v > max)
                {
                    Console.WriteLine($"Value must be between {min} and {max}.");
                    continue;
                }
                return v;
            }
        }
    }
}
