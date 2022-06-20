using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TrackerModel
{
    public sealed class TrackerSecurity
    {
        public int LogRounds { private get; set; }

        /// <summary>
        /// Hashes the supplied user name and password.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string GetHashedPassword(string userName, string password)
        {
            int roundCount = 2 << LogRounds;
            string saltedPassword = userName + "|" + password;
            byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] result;
            SHA512 shaM = SHA512.Create();

            result = shaM.ComputeHash(saltedPasswordBytes);

            byte[] thisHash = shaM.ComputeHash(saltedPasswordBytes);
            for (int i = 0; i < roundCount; i++)
            {
                byte[] nextBytes = thisHash.Concat(saltedPasswordBytes).ToArray();
                thisHash = shaM.ComputeHash(nextBytes);
            }
            return Convert.ToBase64String(thisHash);
        }
    }
}
