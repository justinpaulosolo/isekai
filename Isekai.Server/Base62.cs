using System.Text;

namespace Isekai.Server;

public static class Base62
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Encode(long value)
    {
        if (value == 0) return Alphabet[0].ToString();

        var sb = new StringBuilder();
        var n = value;

        while (n > 0)
        {
            sb.Insert(0, Alphabet[(int)(n % 62)]);
            n /= 62;
        }

        return sb.ToString();
    }

    public static long Decode(string code)
    {
        long result = 0;

        foreach (var c in code)
        {
            var digit = Alphabet.IndexOf(c);
            if (digit < 0)
                throw new ArgumentException($"Invalid Base62 character: '{c}'", nameof(code));

            result = result * 62 + digit;
        }

        return result;
    }
}