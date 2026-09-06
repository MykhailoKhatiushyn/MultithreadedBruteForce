## Version History
- **Version 1:** Implemented static salted SHA-256 hashing (`Hasher.cs`).
- **Version 2:** Implemented target random password generator (`PasswordGenerator.cs`) and independent hash validator (`PasswordValidator.cs`).
- **Version 3:** Implemented single-threaded brute-force engine (`BruteForceEngine.cs`) searching lengths 1 to 6 sequentially.
- **Version 4:** Implemented multi-threaded brute-force engine (`ParallelBruteForceEngine.cs`) constrained to (CPU Cores - 1) with instant thread cancellation.