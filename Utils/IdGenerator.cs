using System;

namespace inventio.Utils
{
    public class IdGenerator
    {
        public string GenerateId()
        {
            Random random = new();
            string randomPart = RandomString(32, random);
            string datePart = DateTime.Now.Ticks.ToString("x");
            return randomPart + datePart;
        }

        private static string RandomString(int length, Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }
            return new string(buffer);
        }
    }
}