namespace Shop.Web.API.Helpers
{
    public static class PasswordHelper
    {
        // Work factor 11 = ~300ms on modern hardware. 
        // Increase to 12 in production if latency budget allows.
        private const int WorkFactor = 11;

        /// <summary>
        /// Hashes a plain-text password.
        /// Returns (hash, salt) — both stored in the Users table.
        /// </summary>
        public static (string hash, string salt) Hash(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
            string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return (hash, salt);
        }

        /// <summary>
        /// Verifies a plain-text password against a stored BCrypt hash.
        /// </summary>
        public static bool Verify(string password, string storedHash) =>
            BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
