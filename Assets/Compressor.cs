using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompressionMode { none, LZF }

public static class Compressor  {

    public static byte[] Pack(byte[] data, CompressionMode mode)
    {
        if(mode==CompressionMode.LZF)
        {
            byte[] compressed = CLZF2.Compress(data);
            return compressed;
        }
        return data;
    }

    public static byte[] UnPack(byte[] data, CompressionMode mode)
    {
        if (mode == CompressionMode.LZF)
        {
            byte[] decompressed = CLZF2.Decompress(data);
            return decompressed;
        }
        return data;
    }
}
