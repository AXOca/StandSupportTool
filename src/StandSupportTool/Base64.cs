using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandSupportTool
{
    internal class Base64
    {
        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("The input string cannot be null or empty.", nameof(input));
            }

            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(inputBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during Base64 encoding.", ex);
            }
        }

        public static string Decode(string base64string)
        {
            if (string.IsNullOrEmpty(base64string))
            {
                throw new ArgumentException("The input string cannot be null or empty.", nameof(base64string));
            }

            try
            {
                byte[] base64Bytes = Convert.FromBase64String(base64string);
                return Encoding.UTF8.GetString(base64Bytes);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("The input is not a valid Base64 string.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during Base64 decoding.", ex);
            }
        }
    }
}
