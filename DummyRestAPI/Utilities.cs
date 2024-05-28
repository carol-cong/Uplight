namespace DummyRestAPI;

public class Utilities
{
    public static string GenerateRandomtrings(int length)
    {
        Random rd = new Random();
        const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        char[] chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
        }

        return new string(chars);
    }
}
