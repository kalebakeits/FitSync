namespace FitSync.Shared.Features.Encryption.Extensions;

using System.Text.Json;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Services;

public static class EncryptionExtensions
{
    public const string encryptionPrefix = "ENC:";

    public static void SetAuthData<T>(
        this Integration integration,
        T data,
        IEncryptionService encryptionService
    )
    {
        string json = JsonSerializer.Serialize(data);
        integration.AuthData = encryptionService.Encrypt(json);
    }

    public static T GetAuthData<T>(
        this Integration integration,
        IEncryptionService encryptionService
    )
    {
        string json = encryptionService.Decrypt(integration.AuthData);
        return JsonSerializer.Deserialize<T>(json)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize auth data for integration {integration.Id}."
            );
    }

    public static User Encrypt(this User user, IEncryptionService encryptionService)
    {
        if (!string.IsNullOrEmpty(user.Email) && !user.Email.StartsWith(encryptionPrefix))
        {
            user.Email = encryptionPrefix + encryptionService.Encrypt(user.Email);
        }
        return user;
    }

    public static string Decrypt(this User user, IEncryptionService encryptionService)
    {
        if (!string.IsNullOrEmpty(user.Email) && user.Email.StartsWith(encryptionPrefix))
        {
            return encryptionService.Decrypt(user.Email[4..]);
        }
        return user.Email;
    }
}
