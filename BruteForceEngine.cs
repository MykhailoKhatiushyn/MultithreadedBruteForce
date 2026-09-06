using System;
using System.Threading;

namespace MultithreadedBruteForce
{
    public class BruteForceEngine
    {
        private readonly PasswordValidator _validator;
        private const int MaxLength = 6;

        public BruteForceEngine(PasswordValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Executes a single-threaded brute force attack sequentially from length 1 up to 6.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel execution instantly when requested.</param>
        /// <param name="onProgress">Callback providing total attempted combination count.</param>
        /// <returns>Found password string, or null if canceled/not found.</returns>
        public string? Execute(CancellationToken cancellationToken, Action<long>? onProgress = null)
        {
            string charset = PasswordGenerator.CharacterSet;
            long totalAttempts = 0;

            // Begin searching sequentially from length 1 up to MaxLength (6)
            for (int length = 1; length <= MaxLength; length++)
            {
                char[] currentCandidate = new char[length];
                if (GenerateCombinations(currentCandidate, 0, charset, cancellationToken, ref totalAttempts, onProgress, out string? foundPassword))
                {
                    return foundPassword;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            return null;
        }

        private bool GenerateCombinations(
            char[] current,
            int position,
            string charset,
            CancellationToken cancellationToken,
            ref long totalAttempts,
            Action<long>? onProgress,
            out string? foundPassword)
        {
            foundPassword = null;

            if (cancellationToken.IsCancellationRequested)
                return false;

            // When a complete string candidate of current length is assembled
            if (position == current.Length)
            {
                totalAttempts++;

                // Report metrics back to UI periodically every 100,000 attempts
                if (totalAttempts % 100000 == 0)
                {
                    onProgress?.Invoke(totalAttempts);
                }

                string candidate = new string(current);
                if (_validator.Validate(candidate))
                {
                    foundPassword = candidate;
                    return true;
                }
                return false;
            }

            // Loop through character set positions
            for (int i = 0; i < charset.Length; i++)
            {
                current[position] = charset[i];
                if (GenerateCombinations(current, position + 1, charset, cancellationToken, ref totalAttempts, onProgress, out foundPassword))
                {
                    return true;
                }

                if (cancellationToken.IsCancellationRequested)
                    return false;
            }

            return false;
        }
    }
}