using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultithreadedBruteForce
{
    public class ParallelBruteForceEngine
    {
        private readonly PasswordValidator _validator;
        private const int MaxLength = 6;

        public ParallelBruteForceEngine(PasswordValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        // Limit execution to maximum (CPU cores - 1) as required
        public int MaxDegreeOfParallelism => Math.Max(1, Environment.ProcessorCount - 1);

        /// <summary>
        /// Executes a parallel brute-force attack starting from length 1 up to 6 across multiple cores.
        /// </summary>
        public string? Execute(CancellationToken externalToken, Action<long>? onProgress = null)
        {
            using var internalCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            CancellationToken token = internalCts.Token;

            string charset = PasswordGenerator.CharacterSet;
            long totalAttempts = 0;
            string? foundPassword = null;

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = token
            };

            // Search sequentially by length from 1 to 6 without knowing length in advance
            for (int length = 1; length <= MaxLength; length++)
            {
                if (token.IsCancellationRequested || foundPassword != null)
                    break;

                try
                {
                    // Partition parallel workers across the first character of the search space
                    Parallel.For(0, charset.Length, options, (i, state) =>
                    {
                        if (token.IsCancellationRequested || foundPassword != null)
                        {
                            state.Stop();
                            return;
                        }

                        char[] buffer = new char[length];
                        buffer[0] = charset[i];

                        if (length == 1)
                        {
                            CheckCandidate(new string(buffer), internalCts, state, ref totalAttempts, ref foundPassword, onProgress);
                        }
                        else
                        {
                            GenerateCombinations(buffer, 1, charset, token, internalCts, state, ref totalAttempts, ref foundPassword, onProgress);
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // Threads stopped cleanly upon cancellation or password match
                }
            }

            return foundPassword;
        }

        private void GenerateCombinations(
            char[] buffer,
            int position,
            string charset,
            CancellationToken token,
            CancellationTokenSource internalCts,
            ParallelLoopState state,
            ref long totalAttempts,
            ref string? foundPassword,
            Action<long>? onProgress)
        {
            if (token.IsCancellationRequested || foundPassword != null)
            {
                state.Stop();
                return;
            }

            if (position == buffer.Length)
            {
                CheckCandidate(new string(buffer), internalCts, state, ref totalAttempts, ref foundPassword, onProgress);
                return;
            }

            for (int i = 0; i < charset.Length; i++)
            {
                if (token.IsCancellationRequested || foundPassword != null)
                {
                    state.Stop();
                    return;
                }

                buffer[position] = charset[i];
                GenerateCombinations(buffer, position + 1, charset, token, internalCts, state, ref totalAttempts, ref foundPassword, onProgress);
            }
        }

        private void CheckCandidate(
            string candidate,
            CancellationTokenSource internalCts,
            ParallelLoopState state,
            ref long totalAttempts,
            ref string? foundPassword,
            Action<long>? onProgress)
        {
            long attempts = Interlocked.Increment(ref totalAttempts);

            if (attempts % 100000 == 0)
            {
                onProgress?.Invoke(attempts);
            }

            if (_validator.Validate(candidate))
            {
                foundPassword = candidate;

                // Immediately stop all active worker threads across all cores
                internalCts.Cancel();
                state.Stop();
            }
        }
    }
}
