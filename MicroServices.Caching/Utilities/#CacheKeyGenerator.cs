using System;
using System.Text;
using System.Security.Cryptography;

namespace MicroServices.Caching.Utilities
{
    /// <summary>
    /// Provides robust cache key generation with consistent formatting, sanitization, and hash options
    /// </summary>
    public static class CacheKeyGenerator
    {
        private const int MAX_KEY_LENGTH = 250; // Safe limit for most cache systems

        /// <summary>
        /// Generates a standardized cache key with prefix and identifier
        /// </summary>
        /// <typeparam name="T">Type of the identifier</typeparam>
        /// <param name="prefix">Domain prefix (e.g., "user", "product")</param>
        /// <param name="id">Unique identifier</param>
        /// <param name="useHash">Whether to hash long keys for consistent length</param>
        /// <returns>Formatted cache key</returns>
        public static string GenerateKey<T>(string prefix, T id, bool useHash = false)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be empty", nameof(prefix));

            var baseKey = $"{SanitizePrefix(prefix)}_{id}";

            return useHash || baseKey.Length > MAX_KEY_LENGTH
                ? GenerateHashedKey(baseKey)
                : baseKey;
        }

        /// <summary>
        /// Generates a composite key from multiple parts
        /// </summary>
        /// <param name="parts">Key components</param>
        /// <returns>Composite key joined with standardized separator</returns>
        public static string GenerateCompositeKey(params object[] parts)
        {
            if (parts == null || parts.Length == 0)
                throw new ArgumentException("At least one key part is required");

            var builder = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null)
                    throw new ArgumentNullException($"Key part at index {i} is null");

                builder.Append(parts[i].ToString());

                if (i < parts.Length - 1)
                    builder.Append("|");
            }

            var compositeKey = builder.ToString();
            return compositeKey.Length > MAX_KEY_LENGTH
                ? GenerateHashedKey(compositeKey)
                : compositeKey;
        }

        /// <summary>
        /// Generates a versioned key for cache busting
        /// </summary>
        public static string GenerateVersionedKey(string baseKey, string version)
        {
            return $"{SanitizePrefix(baseKey)}_v{version}";
        }

        private static string SanitizePrefix(string prefix)
        {
            return prefix.Trim()
                       .ToLowerInvariant()
                       .Replace(" ", "_")
                       .Replace(":", "-");
        }

        private static string GenerateHashedKey(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return "h_" + BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Generates a region-specific key for distributed caching
        /// </summary>
        public static string GenerateRegionalKey(string baseKey, string region)
        {
            return $"{region}:{SanitizePrefix(baseKey)}";
        }
    }
}