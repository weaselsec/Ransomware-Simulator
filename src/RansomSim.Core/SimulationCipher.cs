using System.Security.Cryptography;
using System.Text;

namespace RansomSim.Core;

/// <summary>
/// AES-256-GCM helpers used by the simulator.
///
/// Layout of a .locked file:
///   [12-byte nonce][16-byte tag][ciphertext]
///
/// The ciphertext is byte-for-byte the same length as the plaintext, which is enough
/// realism for a lab while keeping restore trivial.
/// </summary>
public static class SimulationCipher
{
    public const int NonceSize = 12;
    public const int TagSize = 16;
    public const int KeySize = 32;
    public const string LockedExtension = ".locked";

    /// <summary>Creates a random 32-byte key, or derives one from a passphrase (PBKDF2-SHA256).</summary>
    public static byte[] CreateKey(string? passphrase, string salt)
    {
        if (string.IsNullOrEmpty(passphrase))
            return RandomNumberGenerator.GetBytes(KeySize);

        using var kdf = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes(salt), 150_000, HashAlgorithmName.SHA256);
        return kdf.GetBytes(KeySize);
    }

    /// <summary>Encrypts one file into <paramref name="destinationPath"/>. The source file is only read.</summary>
    public static long EncryptFile(string sourcePath, string destinationPath, byte[] key)
    {
        var plaintext = File.ReadAllBytes(sourcePath); // read-only access to the original
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag, AssociateData(sourcePath));
        }

        var temp = destinationPath + ".tmp";
        using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fs.Write(nonce, 0, nonce.Length);
            fs.Write(tag, 0, tag.Length);
            fs.Write(ciphertext, 0, ciphertext.Length);
            fs.Flush(true);
        }

        // Atomic-ish publish: the .locked file never exists in a half-written state.
        File.Move(temp, destinationPath, overwrite: true);
        return plaintext.Length;
    }

    /// <summary>Decrypts a .locked file back to <paramref name="destinationPath"/>.</summary>
    public static long DecryptFile(string lockedPath, string destinationPath, byte[] key)
    {
        var blob = File.ReadAllBytes(lockedPath);
        if (blob.Length < NonceSize + TagSize)
            throw new SimulationException($"'{lockedPath}' is too short to be a simulation payload.");

        var nonce = blob.AsSpan(0, NonceSize).ToArray();
        var tag = blob.AsSpan(NonceSize, TagSize).ToArray();
        var ciphertext = blob.AsSpan(NonceSize + TagSize).ToArray();
        var plaintext = new byte[ciphertext.Length];

        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext, AssociateData(OriginalPathFor(lockedPath)));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        var temp = destinationPath + ".restoring";
        File.WriteAllBytes(temp, plaintext);
        File.Move(temp, destinationPath, overwrite: true);
        return plaintext.Length;
    }

    /// <summary>Binds ciphertext to its original path so a swapped file fails authentication.</summary>
    private static byte[] AssociateData(string originalPathOrLockedPath)
    {
        var normalized = originalPathOrLockedPath.Replace('\\', '/').ToLowerInvariant();
        if (normalized.EndsWith(LockedExtension, StringComparison.Ordinal))
            normalized = normalized[..^LockedExtension.Length];
        return Encoding.UTF8.GetBytes(normalized);
    }

    /// <summary>Reconstructs the original path recorded in a .locked file name.</summary>
    private static string OriginalPathFor(string lockedPath) => lockedPath;

    /// <summary>Returns the .locked path for an original file.</summary>
    public static string LockedPathFor(string originalPath) => originalPath + LockedExtension;

    public static bool IsLockedFile(string path) =>
        path.EndsWith(LockedExtension, StringComparison.OrdinalIgnoreCase);
}
