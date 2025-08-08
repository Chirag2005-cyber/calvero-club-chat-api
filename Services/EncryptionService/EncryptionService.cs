using System.Security.Cryptography;
using System.Text;
using Api.Common;

namespace Api.Services.EncryptionService;

public class EncryptionService : IEncryptionService
{
    public string Encrypt(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            byte[] keyBytes = new byte[AppConstants.Encryption.AesKeySize];
            byte[] passwordBytes = Encoding.UTF8.GetBytes(key);
            Array.Copy(passwordBytes, keyBytes, Math.Min(passwordBytes.Length, keyBytes.Length));
            aes.Key = keyBytes;
            aes.GenerateIV();
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public string Decrypt(string cipherText, string key)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = new byte[AppConstants.Encryption.AesKeySize];
                byte[] passwordBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(passwordBytes, keyBytes, Math.Min(passwordBytes.Length, keyBytes.Length));
                aes.Key = keyBytes;
                byte[] iv = new byte[AppConstants.Encryption.AesIvSize];
                Array.Copy(cipherBytes, iv, AppConstants.Encryption.AesIvSize);
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipherBytes, AppConstants.Encryption.AesIvSize,
                    cipherBytes.Length - AppConstants.Encryption.AesIvSize))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    public string GenerateKey()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] keyBytes = new byte[AppConstants.Encryption.AesKeySize];
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    private byte[] GetJwtEncryptionKey(string jwtKey)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(jwtKey + "ENCRYPTION_SALT"));
        }
    }

    public string EncryptIdentity(string identity, string? jwtKey = null)
    {
        if (jwtKey == null) throw new ArgumentNullException(nameof(jwtKey));
        return EncryptWithKey(identity, GetJwtEncryptionKey(jwtKey));
    }

    public string DecryptIdentity(string encryptedIdentity, string? jwtKey = null)
    {
        if (jwtKey == null) throw new ArgumentNullException(nameof(jwtKey));
        return DecryptWithKey(encryptedIdentity, GetJwtEncryptionKey(jwtKey));
    }

    public string EncryptClaim(string value, string? jwtKey = null)
    {
        if (jwtKey == null) throw new ArgumentNullException(nameof(jwtKey));
        return EncryptWithKey(value, GetJwtEncryptionKey(jwtKey));
    }

    public string DecryptClaim(string encryptedValue, string? jwtKey = null)
    {
        if (jwtKey == null) throw new ArgumentNullException(nameof(jwtKey));
        return DecryptWithKey(encryptedValue, GetJwtEncryptionKey(jwtKey));
    }

    private string EncryptWithKey(string plainText, byte[] keyBytes)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.GenerateIV();
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string DecryptWithKey(string cipherText, byte[] keyBytes)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                byte[] iv = new byte[AppConstants.Encryption.AesIvSize];
                Array.Copy(cipherBytes, iv, AppConstants.Encryption.AesIvSize);
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipherBytes, AppConstants.Encryption.AesIvSize,
                    cipherBytes.Length - AppConstants.Encryption.AesIvSize))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}
