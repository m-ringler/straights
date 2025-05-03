// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Text;

/// <summary>
/// Encodes binary string in Base64url format.
/// </summary>
public static class Base64UrlEncoder
{
    private const string BASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public static string EncodeBase64Url(string binary)
    {
        var result = new StringBuilder();

        for (int i = 0; i < binary.Length; i += 6)
        {
            string bits = binary.Substring(i, Math.Min(6, binary.Length - i));

            while (bits.Length < 6)
            {
                bits += "0";
            }

            int index = Convert.ToInt32(bits, 2);
            _ = result.Append(BASE[index]);
        }

        return result.ToString();
    }

    public static string DecodeBase64Url(string base64Url)
    {
        var result = new StringBuilder();

        for (int i = 0; i < base64Url.Length; i++)
        {
            int index = BASE.IndexOf(base64Url[i]);
            string bits = Convert.ToString(index, 2).PadLeft(6, '0');
            _ = result.Append(bits);
        }

        return result.ToString();
    }
}
