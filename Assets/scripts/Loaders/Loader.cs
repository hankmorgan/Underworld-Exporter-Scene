using System;
using System.IO;
using UnityEngine;

public class Loader : UWClass
{

    public static string BasePath = "C:\\GAMES\\UW1\\";
    public string filePath;//To the file relative to the root of the game folder
    public bool DataLoaded;



    /// <summary>
    /// Reads the file into the file buffer
    /// </summary>
    /// <returns><c>true</c>, if stream file was  read, <c>false</c> otherwise.</returns>
    /// <param name="Path">Path.</param>
    /// <param name="buffer">Buffer.</param>
    public static bool ReadStreamFile(String Path, out byte[] buffer)
    {
        //Path = Path.Replace("--", sep.ToString());
        if (!File.Exists(Path))
        {
            Debug.Log("DataLoader.ReadStreamFile : File not found : " + Path);
            buffer = null;
            return false;
        }
        else
        {
            buffer = System.IO.File.ReadAllBytes(Path);
            return (buffer != null);
        }
        //FileStream fs = File.OpenRead(Path);
        //if (fs.CanRead)
        //{
        //    buffer = new byte[fs.Length];
        //    for (int i = 0; i < fs.Length; i++)
        //    {
        //        buffer[i] = (byte)fs.ReadByte();
        //    }
        //    fs.Close();
        //    return true;
        //}
        //else
        //{
        //    fs.Close();
        //    buffer = new byte[0];
        //    return false;
        //}
    }


    public static long ConvertInt16(byte Byte1, byte Byte2)
    {
        // int b1 = (int)Byte1;
        //int b2 = (int)Byte2;
        return Byte2 << 8 | Byte1;
    }

    public static long ConvertInt24(byte Byte1, byte Byte2, byte Byte3)
    {
        return Byte3 << 16 | Byte2 << 8 | Byte1;
    }

    public static long ConvertInt32(byte Byte1, byte Byte2, byte Byte3, byte Byte4)
    {
        return Byte4 << 24 | Byte3 << 16 | Byte2 << 8 | Byte1;      //24 was 32
    }



    /// <summary>
    /// Gets the value at the specified address in the file buffer and performs any necessary -endian conversions
    /// </summary>
    /// <returns>The value at address.</returns>
    /// <param name="buffer">Buffer.</param>
    /// <param name="Address">Address.</param>
    /// <param name="size">Size of the data in bits</param>
    public static long getValAtAddress(byte[] buffer, long Address, int size)
    {//Gets contents of bytes the the specific integer address. int(8), int(16), int(32) per uw-formats.txt
        switch (size)
        {
            case 8:
                { return buffer[Address]; }
            case 16:
                { return ConvertInt16(buffer[Address], buffer[Address + 1]); }
            case 24:
                { return ConvertInt24(buffer[Address], buffer[Address + 1], buffer[Address + 2]); }
            case 32:
                { return ConvertInt32(buffer[Address], buffer[Address + 1], buffer[Address + 2], buffer[Address + 3]); }
            default:
                {
                    Debug.Log("Invalid data size in getValAtAddress");
                    return -1;
                }
        }
    }



}
