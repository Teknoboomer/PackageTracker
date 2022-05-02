using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TrackerModel
{
    public sealed class TrackerSecurity
    {
        public int LogRounds { private get; set; }

        public string GetHashedPassword(string userName, string password)
        {
            string hashedPassword = HashPassword(password, userName);

            return hashedPassword;
        }

        private string HashPassword(string clearPassword, string salt)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();

            int roundCount = 2 << LogRounds;
            string saltedPassword = salt + "|" + clearPassword;
            byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] thisHash = sha256.ComputeHash(saltedPasswordBytes);
            for (int i = 0; i < roundCount; i++)
            {
                byte[] nextBytes = thisHash.Concat(saltedPasswordBytes).ToArray();
                thisHash = sha256.ComputeHash(nextBytes);
            }
            return Convert.ToBase64String(thisHash);
        }
    }
}
