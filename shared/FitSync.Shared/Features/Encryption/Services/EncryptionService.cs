namespace FitSync.Shared.Features.Encryption.Services;

using System.Security.Cryptography;
using System.Text;
using FitSync.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EncryptionService(
    ILogger<EncryptionService> logger,
    IOptions<DataProtectionOptions> options
) : IEncryptionService
{
    private readonly ILogger<EncryptionService> logger = logger;
    private readonly byte[] key = SHA256.HashData(
        Encoding.UTF8.GetBytes(options.Value.DataProtectionKey)
    );

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return plaintext;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = this.key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();

            // Write IV to the beginning of the stream
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plaintext);
            }

            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to encrypt data");
            throw;
        }
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
        {
            return ciphertext;
        }

        try
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);

            using var aes = Aes.Create();
            aes.Key = this.key;

            // Extract IV from the beginning of the ciphertext
            var iv = new byte[aes.IV.Length];
            Array.Copy(ciphertextBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(
                ciphertextBytes,
                iv.Length,
                ciphertextBytes.Length - iv.Length
            );
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to decrypt data");
            throw;
        }
    }
}
