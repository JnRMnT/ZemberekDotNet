using System;
using System.Collections.Generic;
using System.Text;


public static class StringExtensions
{

    public static bool IsEmpty(this string value)
    {
        return value.Length == 0;
    }
}

