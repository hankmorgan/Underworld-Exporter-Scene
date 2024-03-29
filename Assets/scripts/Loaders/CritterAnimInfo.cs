﻿using UnityEngine;

public class CritterAnimInfo
{
    public const int NoOfAnims = 44;
    public string[,] animSequence;
    public int[,] animIndices;
    //public ArtLoader.RawImageData[,] animSprites;
    public Sprite[] animSprites;
    public string[] animName;

    public CritterAnimInfo()
    {
        animSequence = new string[NoOfAnims, 8];
        animIndices = new int[NoOfAnims, 8];
        switch (UWClass._RES)
        {
            case UWClass.GAME_UW2:
                animSprites = new Sprite[180];
                break;
            default:
                animSprites = new Sprite[128];
                break;
        }
        animName = new string[NoOfAnims];
    }
}
