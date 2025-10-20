using System.Globalization;
using UnityEngine;

public static class TextUtils
{
    public static string FormatAsMoney(float value)
    {
        var culture = new CultureInfo("pt-BR");
        return string.Format(culture, "{0:C}", value);
    }
}
