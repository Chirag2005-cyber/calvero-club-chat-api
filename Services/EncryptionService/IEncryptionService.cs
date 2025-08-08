namespace Api.Services.EncryptionService;

public interface IEncryptionService
{
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherText, string key);
    string GenerateKey();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string EncryptIdentity(string identity, string? jwtKey = null);
    string DecryptIdentity(string encryptedIdentity, string? jwtKey = null);
    string EncryptClaim(string value, string? jwtKey = null);
    string DecryptClaim(string encryptedValue, string? jwtKey = null);
}
