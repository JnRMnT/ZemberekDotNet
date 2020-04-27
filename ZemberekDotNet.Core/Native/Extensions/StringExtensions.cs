using System;
using System.Collections.Generic;
using System.Text;


public static class StringExtensions
{
    public static string ToStringOrEmpty(this string value)
    {
        if (value == null)
        {
            return string.Empty;
        }
        else
        {
            return value;
        }
    }

    public static bool IsEmpty(this string value)
    {
        return value.Length == 0;
    }
}

