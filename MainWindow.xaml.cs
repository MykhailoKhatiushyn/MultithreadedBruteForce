using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MultithreadedBruteForce
{
    public partial class MainWindow : Window
    {
        private string? _currentTargetPassword;
        private string? _currentTargetHash;
        private CancellationTokenSource? _cts;
        private Stopwatch _stopwatch = new Stopwatch();
        private DispatcherTimer _timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += (s, e) => TxtElapsedTime.Text = $"Elapsed Time: {_stopwatch.Elapsed:mm\\:ss\\.ff}";
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            _currentTargetPassword = PasswordGenerator.GenerateRandomPassword();
            _currentTargetHash = Hasher.HashPassword(_currentTargetPassword);

            TxtPlainPassword.Text = $"Target Plain Password: {_currentTargetPassword} (Length: {_currentTargetPassword.Length})";
            TxtTargetHash.Text = $"Target SHA-256 Hash: {_currentTargetHash}";
            TxtResultPassword.Text = "Result: Target ready for attack.";
            TxtLogOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Generated target password '{_currentTargetPassword}' with hash '{_currentTargetHash}'\n");
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentTargetHash) || string.IsNullOrEmpty(_currentTargetPassword))
            {
                MessageBox.Show("Please generate a target password first!", "Warning", MessageBoxButtonOK, MessageBoxImage.Warning);
                return;
            }

            SetUiState(isRunning: true);
            _cts = new CancellationTokenSource();
            PasswordValidator validator = new PasswordValidator(_currentTargetHash);

            if (RbSingleThread.IsChecked == true)
            {
                await RunSingleThreadAttackAsync(validator, _cts.Token);
            }
            else if (RbMultiThread.IsChecked == true)
            {
                await RunMultiThreadAttackAsync(validator, _cts.Token);
            }
            else if (RbBenchmark.IsChecked == true)
            {
                await RunBenchmarkAsync(validator, _cts.Token);
            }

            SetUiState(isRunning: false);
        }

        private async Task RunSingleThreadAttackAsync(PasswordValidator validator, CancellationToken token)
        {
            TxtCoresUsed.Text = "Active Cores: 1 (Single-Thread)";
            ProgressBarAttack.IsIndeterminate = true;
            _stopwatch.Restart();
            _timer.Start();

            BruteForceEngine engine = new BruteForceEngine(validator);
            long attempts = 0;

            string? found = await Task.Run(() => engine.Execute(token, count =>
            {
                attempts = count;
                Dispatcher.Invoke(() => TxtTotalAttempts.Text = $"Total Attempts: {count:N0}");
            }), token);

            _stopwatch.Stop();
            _timer.Stop();
            ProgressBarAttack.IsIndeterminate = false;

            UpdateResultDisplay(found, attempts, _stopwatch.Elapsed);
        }

        private async Task RunMultiThreadAttackAsync(PasswordValidator validator, CancellationToken token)
        {
            ParallelBruteForceEngine engine = new ParallelBruteForceEngine(validator);
            TxtCoresUsed.Text = $"Active Cores: {engine.MaxDegreeOfParallelism} (Multi-Thread)";
            ProgressBarAttack.IsIndeterminate = true;
            _stopwatch.Restart();
            _timer.Start();

            long attempts = 0;

            string? found = await Task.Run(() => engine.Execute(token, count =>
            {
                attempts = count;
                Dispatcher.Invoke(() => TxtTotalAttempts.Text = $"Total Attempts: {count:N0}");
            }), token);

            _stopwatch.Stop();
            _timer.Stop();
            ProgressBarAttack.IsIndeterminate = false;

            UpdateResultDisplay(found, attempts, _stopwatch.Elapsed);
        }

        private async Task RunBenchmarkAsync(PasswordValidator validator, CancellationToken token)
        {
            TxtLogOutput.AppendText($"\n[{DateTime.Now:HH:mm:ss}] --- STARTING BENCHMARK COMPARISON ---\n");

            // Single Thread Pass
            TxtCoresUsed.Text = "Active Cores: 1 (Single-Thread)";
            ProgressBarAttack.IsIndeterminate = true;
            _stopwatch.Restart();
            _timer.Start();

            BruteForceEngine singleEngine = new BruteForceEngine(validator);
            long singleAttempts = 0;
            string? singleFound = await Task.Run(() => singleEngine.Execute(token, count =>
            {
                singleAttempts = count;
                Dispatcher.Invoke(() => TxtTotalAttempts.Text = $"Total Attempts: {count:N0}");
            }), token);

            _stopwatch.Stop();
            TimeSpan singleTime = _stopwatch.Elapsed;

            if (token.IsCancellationRequested)
            {
                _timer.Stop();
                ProgressBarAttack.IsIndeterminate = false;
                TxtResultPassword.Text = "Result: Attack canceled.";
                return;
            }

            // Multi Thread Pass
            ParallelBruteForceEngine multiEngine = new ParallelBruteForceEngine(validator);
            TxtCoresUsed.Text = $"Active Cores: {multiEngine.MaxDegreeOfParallelism} (Multi-Thread)";
            _stopwatch.Restart();

            long multiAttempts = 0;
            string? multiFound = await Task.Run(() => multiEngine.Execute(token, count =>
            {
                multiAttempts = count;
                Dispatcher.Invoke(() => TxtTotalAttempts.Text = $"Total Attempts: {count:N0}");
            }), token);

            _stopwatch.Stop();
            _timer.Stop();
            TimeSpan multiTime = _stopwatch.Elapsed;
            ProgressBarAttack.IsIndeterminate = false;

            BenchmarkResult result = new BenchmarkResult
            {
                TargetPassword = _currentTargetPassword!,
                TargetHash = _currentTargetHash!,
                SingleThreadTime = singleTime,
                MultiThreadTime = multiTime,
                TotalAttempts = Math.Max(singleAttempts, multiAttempts)
            };

            TxtResultPassword.Text = $"Result: Found '{multiFound}' successfully!";
            TxtLogOutput.AppendText(BenchmarkLogger.FormatReport(result) + "\n");
        }

        private void UpdateResultDisplay(string? foundPassword, long attempts, TimeSpan elapsed)
        {
            if (foundPassword != null)
            {
                TxtResultPassword.Text = $"Result: Found '{foundPassword}' in {elapsed.TotalSeconds:F2}s!";
                TxtLogOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] SUCCESS: Found password '{foundPassword}' after {attempts:N0} attempts in {elapsed.TotalMilliseconds:N0} ms.\n");
            }
            else
            {
                TxtResultPassword.Text = "Result: Canceled or not found.";
                TxtLogOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] Canceled or stopped after {attempts:N0} attempts.\n");
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            BtnStop.IsEnabled = false;
            TxtResultPassword.Text = "Result: Canceling threads...";
        }

        private void SetUiState(bool isRunning)
        {
            BtnStart.IsEnabled = !isRunning;
            BtnGenerate.IsEnabled = !isRunning;
            BtnStop.IsEnabled = isRunning;
            RbSingleThread.IsEnabled = !isRunning;
            RbMultiThread.IsEnabled = !isRunning;
            RbBenchmark.IsEnabled = !isRunning;
        }
    }
}