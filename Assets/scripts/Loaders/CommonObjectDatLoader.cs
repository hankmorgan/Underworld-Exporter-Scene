﻿using System.IO;

public class CommonObjectDatLoader : Loader
{


    /*
0000  Int8    height
0001  Int16   mass/stuff:
             bits 0-2: radius
             bit 3: 1 for npc's, 3d objects and misc. items (animated flg?)
             bits 4-15: mass in 0.1 stones
0003  Int8    flags
             0/2: object in range [336, 368[ seem to be 3d objects
             3: magic object (?)
             4: decal object (always 0 in uw1)
             5: is set when object can be picked up
             7: is set when object is a container
0004  Int16   monetary value
0006  Int8    bits 2..3: quality class (this value*6+quality gives index
                        into string block 5
0007  Int8    bit 7: if 1, item can have owner ("belongs to ...")
             bits 1..4: type? a=talisman, 9=magic, 3..5=ammunition
0008  Int8    scale value (?)
0009  Int8    unknown2
             bits 0-1: Can the object be identified, (matches heading bytes used on object to signify id level)
000A  Int8    bits 0-3: quality type 0-f
             bit 4: printable "look at" description when 1		  

*/
    public struct CommonObjectProperties
    {
        public int height;
        public int radius;
        public int animFlag;
        public int mass;

        public int Flag0;
        public int Flag1;
        public int Flag2;
        public int FlagMagicObject;
        public int FlagDecalObject;
        public int FlagCanBePickedUp;
        public int Flag6;
        public int FlagisContainer;

        public int Value;
        public int QualityClass;

        public int CanBelongTo;
        public int type;
        public int scaleValue;
        public int unk2;
        public int QualityType;
        public int LookAt;

    };

    public CommonObjectProperties[] properties;

    public CommonObjectDatLoader()
    {

        //int add_ptr;
        if (ReadStreamFile(Path.Combine(BasePath, "DATA", "COMOBJ.DAT"), out byte[] comobj_dat))
        {
            int len = (comobj_dat.GetUpperBound(0) - 2) / 11;
            properties = new CommonObjectProperties[len];
            int addressPtr = 2;//SKip first two bytes
            for (int i = 0; i <= properties.GetUpperBound(0); i++)
            {
                properties[i].height = (int)getValAtAddress(comobj_dat, addressPtr, 8);
                properties[i].radius = (int)getValAtAddress(comobj_dat, addressPtr + 1, 16) & 0x7;
                properties[i].animFlag = ((int)getValAtAddress(comobj_dat, addressPtr + 1, 16) >> 3) & 0x1;
                properties[i].mass = ((int)getValAtAddress(comobj_dat, addressPtr + 1, 16) >> 4);
                properties[i].Flag0 = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8)) & 0x01;//Flags0
                properties[i].Flag1 = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 1) & 0x01;//Flags1
                properties[i].Flag2 = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 2) & 0x01;//Flags2
                properties[i].FlagMagicObject = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 3) & 0x01;//Magic?
                properties[i].FlagDecalObject = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 4) & 0x01;//Decal
                properties[i].FlagCanBePickedUp = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 5) & 0x01;//Pickable
                properties[i].Flag6 = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 6) & 0x01;//Flags6
                properties[i].FlagisContainer = ((int)getValAtAddress(comobj_dat, addressPtr + 3, 8) >> 7) & 0x01;//Container

                properties[i].Value = (int)getValAtAddress(comobj_dat, addressPtr + 4, 16);
                properties[i].QualityClass = ((int)getValAtAddress(comobj_dat, addressPtr + 6, 8) >> 2) & 0x3;

                properties[i].CanBelongTo = ((int)getValAtAddress(comobj_dat, addressPtr + 7, 8) >> 7) & 0x1;

                properties[i].scaleValue = (int)getValAtAddress(comobj_dat, addressPtr + 8, 16);

                properties[i].QualityType = (int)getValAtAddress(comobj_dat, addressPtr + 10, 8) & 0xF;

                properties[i].LookAt = ((int)getValAtAddress(comobj_dat, addressPtr + 10, 8) >> 3) & 0x1;
                addressPtr += 11;
            }


        }
        else
        {
            return;
        }


        //Fix some bugs caused by incorrect data in object settings
        properties[302].FlagCanBePickedUp = 0;//fountains
        switch (_RES)
        {
            case GAME_UW2:
                properties[290].FlagCanBePickedUp = 0;//orbs
                break;
            default:
                properties[279].FlagCanBePickedUp = 0;//orbs
                properties[172].CanBelongTo = 0;//Gold plate
                break;
        }


    }

}
