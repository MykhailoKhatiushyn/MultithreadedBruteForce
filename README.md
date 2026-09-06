## V## Version History
- **Version 1:** Implemented static salted SHA-256 hashing (`Hasher.cs`).
- **Version 2:** Implemented target random password generator (`PasswordGenerator.cs`) and independent hash validator (`PasswordValidator.cs`).
- **Version 3:** Implemented single-threaded brute-force engine (`BruteForceEngine.cs`) searching lengths 1 to 6 sequentially.