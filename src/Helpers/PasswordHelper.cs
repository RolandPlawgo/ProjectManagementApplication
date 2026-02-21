using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManagementApplication.Helpers
{
    public class PasswordHelper
    {
        private static readonly char[] _digits = "0123456789".ToCharArray();
        private static readonly char[] _lowercaseLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] _uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _nonAlphanumeric = "!@#$%^&*()-_=+[]{}|;:,.<>?".ToCharArray();

        public static string GeneratePassword(PasswordOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var requiredChars = new List<char>();
            var allChars = new List<char>();

            if (options.RequireDigit)
            {
                requiredChars.Add(GetRandom(_digits));
                allChars.AddRange(_digits);
            }
            if (options.RequireLowercase)
            {
                requiredChars.Add(GetRandom(_lowercaseLetters));
                allChars.AddRange(_lowercaseLetters);
            }
            if (options.RequireUppercase)
            {
                requiredChars.Add(GetRandom(_uppercaseLetters));
                allChars.AddRange(_uppercaseLetters);
            }
            if (options.RequireNonAlphanumeric)
            {
                requiredChars.Add(GetRandom(_nonAlphanumeric));
                allChars.AddRange(_nonAlphanumeric);
            }

            int remainingLength = options.RequiredLength - requiredChars.Count;
            if (remainingLength < 0)
                remainingLength = 0;

            for (int i = 0; i < remainingLength; i++)
            {
                requiredChars.Add(GetRandom(allChars.ToArray()));
            }

            return new string(Shuffle(requiredChars).ToArray());
        }

        private static char GetRandom(char[] pool)
        {
            var byteBuffer = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(byteBuffer);
            uint num = BitConverter.ToUInt32(byteBuffer, 0);
            return pool[num % pool.Length];
        }

        private static System.Collections.Generic.List<char> Shuffle(System.Collections.Generic.List<char> list)
        {
            using var rng = RandomNumberGenerator.Create();
            int n = list.Count;
            while (n > 1)
            {
                var box = new byte[4];
                rng.GetBytes(box);
                int k = (int)(BitConverter.ToUInt32(box, 0) % n--);
                var temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
            return list;
        }
    }
}
