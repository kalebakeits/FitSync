namespace FitSync.Shared.Features.Encryption.Extensions;

using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Services;

public static class EncryptionExtensions
{
    public const string encryptionPrefix = "ENC:";

    public static UserCredential Encrypt(
        this UserCredential credential,
        IEncryptionService encryptionService
    )
    {
        if (
            !string.IsNullOrEmpty(credential.Username)
            && !credential.Username.StartsWith(encryptionPrefix)
        )
        {
            credential.Username = encryptionPrefix + encryptionService.Encrypt(credential.Username);
        }
        if (
            !string.IsNullOrEmpty(credential.Password)
            && !credential.Password.StartsWith(encryptionPrefix)
        )
        {
            credential.Password = encryptionPrefix + encryptionService.Encrypt(credential.Password);
        }
        return credential;
    }

    public static (string Username, string Password) Decrypt(
        this UserCredential credential,
        IEncryptionService encryptionService
    )
    {
        string username = credential.Username;
        string password = credential.Password;

        if (!string.IsNullOrEmpty(username) && username.StartsWith(encryptionPrefix))
        {
            username = encryptionService.Decrypt(username[4..]);
        }
        if (!string.IsNullOrEmpty(password) && password.StartsWith(encryptionPrefix))
        {
            password = encryptionService.Decrypt(password[4..]);
        }
        return (username, password);
    }

    // User extensions
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
