using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompressionMode { none, LZF }

public static class Compressor  {

    public static byte[] Pack(byte[] data, CompressionMode mode)
    {
        if(mode==CompressionMode.LZF)
        {
            return CLZF2.Compress(data);
        }
        return data;
    }

    public static byte[] UnPack(byte[] data, CompressionMode mode)
    {
        if (mode == CompressionMode.LZF)
        {
            return CLZF2.Decompress(data);
        }
        return data;
    }
}
