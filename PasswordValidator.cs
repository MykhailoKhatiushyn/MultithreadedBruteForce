using System;

namespace MultithreadedBruteForce
{
    public class PasswordValidator
    {
        public string TargetHash { get; private set; }

        public PasswordValidator(string targetHash)
        {
            TargetHash = targetHash ?? throw new ArgumentNullException(nameof(targetHash));
        }

        /// <summary>
        /// Validates if a candidate string produces a matching SHA256 salted hash.
        /// </summary>
        public bool Validate(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;

            string candidateHash = Hasher.HashPassword(candidate);
            return string.Equals(candidateHash, TargetHash, StringComparison.Ordinal);
        }
    }
}
