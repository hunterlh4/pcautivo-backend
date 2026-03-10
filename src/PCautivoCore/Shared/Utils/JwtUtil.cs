using PCautivoCore.Shared.Settings;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace PCautivoCore.Shared.Utils;

public interface IJwtUtil
{
    string GenerateToken(string subject);
    string GenerateToken(string subject, object payload);
    bool ValidateToken(string token);
    bool ValidateToken(string token, out object? payload);
    int GetExpiresIn();
}

public class JwtUtil : IJwtUtil
{
    private readonly JwtSetting _jwtSetting;

    public JwtUtil(IConfiguration configuration)
    {
        _jwtSetting = configuration.GetSection("Jwt").Get<JwtSetting>() ?? throw new Exception("Failed to get JwtSetting");
    }

    private byte[] GetSecrets()
    {
        string secretKey = _jwtSetting.SecretKey;

        return Encoding.UTF8.GetBytes(secretKey);
    }

    public string GenerateToken(string subject)
    {
        try
        {
            int expiresIn = _jwtSetting.ExpiresIn;
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(_jwtSetting.ExpiresIn);

            byte[] secrets = GetSecrets();

            var algorithm = new HMACSHA256Algorithm();

            string token = JwtBuilder.Create()
                .WithAlgorithm(algorithm)
                .WithSecret(secrets)
                .Subject(subject)
                .ExpirationTime(expiresAt.ToUnixTimeSeconds())
                .Issuer(_jwtSetting.Issuer)
                .Audience(_jwtSetting.Audience)
                .Encode();

            return token;
        }
        catch (Exception)
        {
            throw new Exception("Error generating token");
        }
    }

    public string GenerateToken(string subject, object payload)
    {
        try
        {
            int expiresIn = _jwtSetting.ExpiresIn;
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(_jwtSetting.ExpiresIn);

            byte[] secrets = GetSecrets();

            var algorithm = new HMACSHA256Algorithm();

            string token = JwtBuilder.Create()
                .WithAlgorithm(algorithm)
                .WithSecret(secrets)
                .Subject(subject)
                .ExpirationTime(expiresAt.ToUnixTimeSeconds())
                .Issuer(_jwtSetting.Issuer)
                .Audience(_jwtSetting.Audience)
                .AddClaims((IDictionary<string, object>)payload)
                .Encode();

            return token;
        }
        catch (Exception)
        {
            throw new Exception("Error generating token");
        }
    }

    public bool ValidateToken(string token)
    {
        try
        {
            byte[] secrets = GetSecrets();

            var algorithm = new HMACSHA256Algorithm();

            var payload = JwtBuilder.Create()
                 .WithAlgorithm(algorithm)
                 .WithSecret(secrets)
                 .MustVerifySignature()
                 .Decode<IDictionary<string, object>>(token);

            return payload != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool ValidateToken(string token, out object? payload)
    {
        try
        {
            byte[] secrets = GetSecrets();

            var algorithm = new HMACSHA256Algorithm();

            payload = JwtBuilder.Create()
                 .WithAlgorithm(algorithm)
                 .WithSecret(secrets)
                 .MustVerifySignature()
                 .Decode<IDictionary<string, object>>(token);

            return payload != null;
        }
        catch (Exception)
        {
            payload = null;

            return false;
        }
    }

    public int GetExpiresIn()
    {
        return _jwtSetting.ExpiresIn;
    }
}