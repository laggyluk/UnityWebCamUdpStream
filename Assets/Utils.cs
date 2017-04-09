using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.IO;
using System.Reflection;
using System.Linq;

public static class ListExtra
{
    public static void Resize<T>(this List<T> list, int sz, T c = default(T))
    {
        int cur = list.Count;
        if (sz < cur)
            list.RemoveRange(sz, cur - sz);
        else if (sz > cur)
            list.AddRange(Enumerable.Repeat(c, sz - cur));
    }

    public static void AddMany<T>(this List<T> list, params T[] elements)
    {
        list.AddRange(elements);
    }

    private static System.Random rng = new System.Random();

    public static T RandomElement<T>(this IList<T> list)
    {
        return list[rng.Next(list.Count)];
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[rng.Next(array.Length)];
    }

}

public static class Utils
{    
    static char[] _alphaChars = new char[62];
    static System.Random random = new System.Random();

    public static string appPath; //exe folder
    public static string dataPath;//streaming assets folder

    static Utils()
    {
        _alphaChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        //find paths
        string s = "";
#if UNITY_EDITOR || !UNITY_STANDALONE //nie dziala na mobile
        s = UnityEngine.Application.dataPath;
#endif
#if !(UNITY_WP8 || UNITY_WP_8_1)
        if (s == "")
        {
            if (Assembly.GetEntryAssembly() != null)
            {
                s = Assembly.GetEntryAssembly().Location;
            }
            else
                if (Environment.GetCommandLineArgs()[0] != null)
            {
                s = Environment.GetCommandLineArgs()[0];
            }
        }
#endif
        appPath = Path.GetDirectoryName(s) + "/";
        dataPath = appPath + "Assets/StreamingAssets/";
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1
        dataPath = UnityEngine.Application.persistentDataPath + "/";
        //System.String.PrimaryLanguageOverride = "aa";

#endif
#if UNITY_STANDALONE
        dataPath = UnityEngine.Application.streamingAssetsPath + "/";
#endif
        if (s == "") throw new Exception("Utils: path to resources not found, figure out some other method!");
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static Color RandomColor()
    {
        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
    }

    public static string RandomString(int size)
    {
        StringBuilder builder = new StringBuilder(size);
        char ch;
        for (int i = 0; i < size; i++)
        {
            int r = random.Next(0, _alphaChars.Length);
            ch = _alphaChars[r]; //watch out for index out of bounds
            builder.Append(ch);
        }

        return builder.ToString();
    }

    //returns first part of string, to the separator and removes that from rest of string
    public static string EatString(ref string s, char separator = ';')
    {
        int i = s.IndexOf(separator);
        string result = "";
        if (i > -1)
        {
            result = s.Substring(0, i);
            if (s.Length > i + 1)
                s = s.Substring(i + 1, s.Length - i - 1);
            else s = "";
        }
        else
        {
            result = s;//separator not found;
            //s = "";
        }
        return result;
    }

    static string timestampFormat = "MM-dd HH:mm:ss";
    public static void Log(string text, params object[] args)
    {
        Debug.Log(string.Format("{0}>log: {1}", DateTime.Now.ToString(timestampFormat), string.Format(text, args)));
    }

    public static int getTouchCount()
    {
        int fingerCount = 0;
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                fingerCount++;
        }
        return fingerCount;
    }

    public static bool isInRectangle(float centerX, float centerY, float radius, float x, float y)
    {
        return x >= centerX - radius && x <= centerX + radius &&
            y >= centerY - radius && y <= centerY + radius;
    }

    //test if coordinate (x, y) is within a radius from coordinate (center_x, center_y)
    public static bool isPointInCircle(float centerX, float centerY,
        float radius, float x, float y)
    {
        if (isInRectangle(centerX, centerY, radius, x, y))
        {
            double dx = centerX - x;
            double dy = centerY - y;
            dx *= dx;
            dy *= dy;
            double distanceSquared = dx + dy;
            double radiusSquared = radius * radius;
            return distanceSquared <= radiusSquared;
        }
        return false;
    }

}

