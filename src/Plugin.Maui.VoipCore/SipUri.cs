namespace Plugin.Maui.VoipCore;

/// <summary>
/// SIP URI helpers.
/// </summary>
public static class SipUri
{
    /// <summary>
    /// Normalizes a destination to a <c>sip:</c> or <c>sips:</c> URI.
    /// </summary>
    /// <exception cref="VoipCoreException">Thrown when <paramref name="destination"/> is empty.</exception>
    public static string Normalize(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new VoipCoreException(VoipCoreError.InvalidDestination, "A call destination is required.");
        }

        var value = destination.Trim();
        if (value.StartsWith("sip:", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("sips:", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"sip:{value}";
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="digits"/> is a non-empty DTMF sequence.
    /// </summary>
    public static bool IsValidDtmf(string? digits)
    {
        if (string.IsNullOrEmpty(digits))
        {
            return false;
        }

        foreach (var c in digits)
        {
            if (c is not ((>= '0' and <= '9') or '*' or '#' or 'A' or 'B' or 'C' or 'D' or 'a' or 'b' or 'c' or 'd'))
            {
                return false;
            }
        }

        return true;
    }
}
