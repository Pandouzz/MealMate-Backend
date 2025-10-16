using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

class PasswordHasher
{
    static Random rand = new Random();
    public static PwHashResult ComputerHash(string pw)
    {
        SHA256 sha = SHA256.Create();
        string salt = GenerateSalt();
        byte[] byteArray = Encoding.UTF8.GetBytes(pw + salt);
        byteArray = sha.ComputeHash(byteArray);
        string hexString = Convert.ToHexString(byteArray);
        return new PwHashResult { Hash = hexString, Salt = salt };
    }

    public static string GenerateSalt(int minLength = 12, int maxLength = 24)
    { 
        int len = rand.Next(minLength, maxLength + 1);
        byte[] saltBytes = new byte[len];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public class PwHashResult
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
        
    }
}