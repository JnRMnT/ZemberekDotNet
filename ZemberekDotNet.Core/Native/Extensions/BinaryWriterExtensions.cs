using System.IO;

public static class BinaryWriterExtensions
{
    public static int WriteUTF(this BinaryWriter binaryWriter, string str)
    {
        int strlen = str.Length;
        int utflen = 0;
        int c, count = 0;
        int i;
        /* use charAt instead of copying String to char array */
        for (i = 0; i < strlen; i++)
        {
            c = str[i];
            if ((c >= 0x0001) && (c <= 0x007F))
            {
                utflen++;
            }
            else if (c > 0x07FF)
            {
                utflen += 3;
            }
            else
            {
                utflen += 2;
            }
        }

        if (utflen > 65535)
            throw new InvalidDataException(
                "encoded string too long: " + utflen + " bytes");

        byte[] bytearr = new byte[utflen + 2];
        bytearr[count++] = (byte)((utflen >> 8) & 0xFF);
        bytearr[count++] = (byte)((utflen >> 0) & 0xFF);

        for (i = 0; i < strlen; i++)
        {
            c = str[i];
            if (!((c >= 0x0001) && (c <= 0x007F))) break;
            bytearr[count++] = (byte)c.EnsureEndianness();
        }

        for (; i < strlen; i++)
        {
            c = str[i];
            if ((c >= 0x0001) && (c <= 0x007F))
            {
                bytearr[count++] = (byte)c;

            }
            else if (c > 0x07FF)
            {
                bytearr[count++] = (byte)((0xE0 | ((c >> 12) & 0x0F)));
                bytearr[count++] = (byte)((0x80 | ((c >> 6) & 0x3F)));
                bytearr[count++] = (byte)((0x80 | ((c >> 0) & 0x3F)));
            }
            else
            {
                bytearr[count++] = (byte)((0xC0 | ((c >> 6) & 0x1F)));
                bytearr[count++] = (byte)((0x80 | ((c >> 0) & 0x3F)));
            }
        }

        binaryWriter.Write(bytearr, 0, utflen + 2);
        return utflen + 2;
    }
}
