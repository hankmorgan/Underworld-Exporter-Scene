﻿using System.Collections.Generic;
using UnityEngine;

public class ObjectLoader : DataLoader
{

    //System Shock object classes
    const int GUNS_WEAPONS = 0;
    const int AMMUNITION = 1;
    const int PROJECTILES = 2;
    const int GRENADE_EXPLOSIONS = 3;
    const int PATCHES = 4;
    const int HARDWARE = 5;
    const int SOFTWARE_LOGS = 6;
    const int FIXTURES = 7;
    const int GETTABLES_OTHER = 8;
    const int SWITCHES_PANELS = 9;
    const int DOORS_GRATINGS = 10;
    const int ANIMATED = 11;
    const int TRAPS_MARKERS = 12;
    const int CONTAINERS_CORPSES = 13;
    const int CRITTERS = 14;

    //System Shock object classes offsets. For this I will use their chunk offset value.
    const int GUNS_WEAPONS_OFFSET = 4010;
    const int AMMUNITION_OFFSET = 4011;
    const int PROJECTILES_OFFSET = 4012;
    const int GRENADE_EXPLOSIONS_OFFSET = 4013; //a guess
    const int PATCHES_OFFSET = 4014;
    const int HARDWARE_OFFSET = 4015;
    const int SOFTWARE_LOGS_OFFSET = 4016;
    const int FIXTURES_OFFSET = 4017;
    const int GETTABLES_OTHER_OFFSET = 4018;        //a guess
    const int SWITCHES_PANELS_OFFSET = 4019;
    const int DOORS_GRATINGS_OFFSET = 4020;
    const int ANIMATED_OFFSET = 4021;   //a guess?
    const int TRAPS_MARKERS_OFFSET = 4022;
    const int CONTAINERS_CORPSES_OFFSET = 4023;
    const int CRITTERS_OFFSET = 4024;
    const int SURVELLANCE_OFFSET = 4043;




    /*Some friendly array indices for shock objProperties array*/
    //Software
    const int SOFT_PROPERTY_VERSION = 0;
    const int SOFT_PROPERTY_LOG = 9;
    const int SOFT_PROPERTY_LEVEL = 2;
    //Buttons and panels

    const int BUTTON_PROPERTY_TRIGGER = 0;
    const int BUTTON_PROPERTY_PUZZLE = 1;
    const int BUTTON_PROPERTY_COMBO = 2;
    const int BUTTON_PROPERTY_TRIGGER_2 = 3;


    //Trigger props
    const int TRIG_PROPERTY_OBJECT = 0;
    const int TRIG_PROPERTY_TARGET_X = 1;
    const int TRIG_PROPERTY_TARGET_Y = 2;
    const int TRIG_PROPERTY_TARGET_Z = 3;
    const int TRIG_PROPERTY_FLAG = 4;
    const int TRIG_PROPERTY_VARIABLE = 5;
    const int TRIG_PROPERTY_VALUE = 6;
    const int TRIG_PROPERTY_OPERATION = 7;
    const int TRIG_PROPERTY_MESSAGE1 = 8;
    const int TRIG_PROPERTY_MESSAGE2 = 9;
    const int TRIG_PROPERTY_LIGHT_OP = 3;
    const int TRIG_PROPERTY_CONTROL_1 = 4;
    const int TRIG_PROPERTY_CONTROL_2 = 5;
    const int TRIG_PROPERTY_UPPERSHADE_1 = 6;
    const int TRIG_PROPERTY_LOWERSHADE_1 = 7;
    const int TRIG_PROPERTY_UPPERSHADE_2 = 8;
    const int TRIG_PROPERTY_LOWERSHADE_2 = 9;

    const int TRIG_PROPERTY_FLOOR = 5;
    const int TRIG_PROPERTY_CEILING = 6;
    const int TRIG_PROPERTY_SPEED = 7;
    const int TRIG_PROPERTY_TRIG_1 = 5;
    const int TRIG_PROPERTY_TRIG_2 = 6;
    const int TRIG_PROPERTY_EMAIL = 9;
    const int TRIG_PROPERTY_TYPE = 8;

    const int CONTAINER_CONTENTS_1 = 0;
    const int CONTAINER_CONTENTS_2 = 1;
    const int CONTAINER_CONTENTS_3 = 2;
    const int CONTAINER_CONTENTS_4 = 3;
    const int CONTAINER_WIDTH = 5;
    const int CONTAINER_HEIGHT = 6;
    const int CONTAINER_DEPTH = 7;
    const int CONTAINER_TOP = 8;
    const int CONTAINER_SIDE = 9;

    const int SCREEN_NO_OF_FRAMES = 0;
    const int SCREEN_LOOP_FLAG = 1;
    const int SCREEN_START = 2;
    const int SCREEN_SURVEILLANCE_TARGET = 3;

    const int WORDS_STRING_NO = 0;
    const int WORDS_FONT = 1;
    const int WORDS_SIZE = 2;
    const int WORDS_COLOUR = 3;

    const int BRIDGE_X_SIZE = 0;
    const int BRIDGE_Y_SIZE = 1;
    const int BRIDGE_HEIGHT = 2;
    const int BRIDGE_TOP_BOTTOM_TEXTURE = 3;
    const int BRIDGE_TOP_BOTTOM_TEXTURE_SOURCE = 4;
    const int BRIDGE_SIDE_TEXTURE = 5;
    const int BRIDGE_SIDE_TEXTURE_SOURCE = 6;

    /// <summary>
    /// Address in the file where the data is kept.
    /// </summary>
    //public long objectsAddress;


    /// <summary>
    /// The game objects currently in use.
    /// </summary>
    public ObjectLoaderInfo[] objInfo;

    /// <summary>
    /// Reference to the tilemap this object list is part of.
    /// </summary>
    TileMap map;

    /// <summary>
    /// The no of in use mobile objects in the object list.
    /// </summary>
    public short NoOfFreeMobile
    {
        get
        {
            return (short)getValAtAddress(map.lev_ark_block.Data, 0x7c02, 16);//7c02
        }
        set
        {
            map.lev_ark_block.Data[0x7c02] = (byte)(value & 0xFF);
            map.lev_ark_block.Data[0x7c03] = (byte)((value >> 8) & 0xFF);
        }
    }

    /// <summary>
    /// The no of in use static objects in the object list.
    /// </summary>
    public short NoOfFreeStatic
    {
        get
        {
            return (short)getValAtAddress(map.lev_ark_block.Data, 0x7c04, 16);//7c04
        }
        set
        {
            map.lev_ark_block.Data[0x7c04] = (byte)(value & 0xFF);
            map.lev_ark_block.Data[0x7c05] = (byte)((value >> 8) & 0xFF);
        }
    }

    ///// <summary>
    ///// Free lists are used to allocate available object slots.
    ///// </summary>
    //public short[] FreeMobileList = new short[254];
    //public short[] FreeStaticList = new short[768];

    struct xrefTable
    {
        public short tileX;// position
        public short tileY;// position
        public int next;
        public int MstIndex;// into master object table
        public int nextTile; //objects in next tile
        public short duplicate;
        public short duplicateAssigned;
        public short duplicateNextAssigned;
    };

    /// <summary>
    /// Readies the object list. Requires the tilemap to be read in first
    /// </summary>
    /// <param name="tileMap">Tile map.</param>
    /// <param name="lev_ark">Lev ark.</param>
    /// <param name="game">Game.</param>
    public void LoadObjectList(TileMap tileMap, UWBlock lev_ark)
    {
        map = tileMap;
        objInfo = new ObjectLoaderInfo[1024];
        BuildObjectListUW(objInfo, tileMap);

        setObjectTileXY(1, tileMap.Tiles, objInfo);

        setDoorBits(tileMap.Tiles, objInfo);
        setElevatorBits(tileMap.Tiles, objInfo);
        setTerrainChangeBits(tileMap.Tiles, objInfo);
        SetBullFrog(tileMap.Tiles, objInfo, tileMap.thisLevelNo);
        if (_RES == GAME_UW2)
        {
            setQbert(tileMap.Tiles, objInfo, tileMap.thisLevelNo);
            FindOffMapOscillatorTiles(tileMap.Tiles, objInfo, tileMap.thisLevelNo);
            SetColourCyclingTiles(tileMap.Tiles, objInfo, tileMap.thisLevelNo);
            SetFloorCollapseTiles(tileMap.Tiles, objInfo, tileMap.thisLevelNo);
        }
    }

    public void LoadObjectListShock(TileMap tileMap, byte[] lev_ark)
    {

        objInfo = new ObjectLoaderInfo[1600];
        BuildObjectListShock(tileMap.Tiles, objInfo, tileMap.texture_map, lev_ark, tileMap.thisLevelNo);

        setObjectTileXY(1, tileMap.Tiles, objInfo);

        //setDoorBits(tileMap.Tiles,objInfo);
        //setElevatorBits(tileMap.Tiles,objInfo);
        //setTerrainChangeBits(tileMap.Tiles,objInfo);
        //SetBullFrog(tileMap.Tiles,objInfo,tileMap.thisLevelNo);

    }


    //void BuildObjectListUW(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList,int[] texture_map, byte[] lev_ark, int LevelNo)
    bool BuildObjectListShock(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, short[] texture_map, byte[] archive_ark, short LevelNo)
    {

        short InUseFlag;
        int ObjectClass;
        int ObjectSubClass;
        int ObjectSubClassIndex;

        //int[] MasterAddressLookup=new int[1600];
        //long address_pointer=4;	

        //unsigned char *archive_ark;	//the full file.

        //The crossref table
        //The master table



        if (!LoadChunk(archive_ark, LevelNo * 100 + 4009, out Chunk xref_ark))
        {
            return false;
        }

        xrefTable[] xref;
        xref = new xrefTable[xref_ark.chunkUnpackedLength / 10];
        int xref_ptr = 0;
        short i;
        //int xRefLength = (xref_ark.chunkUnpackedLength/10);


        //Read in the xref table
        for (i = 0; i < xref_ark.chunkUnpackedLength / 10; i++)
        {
            xref[i].tileX = (short)getValAtAddress(xref_ark.data, xref_ptr + 0, 16);
            xref[i].tileY = (short)getValAtAddress(xref_ark.data, xref_ptr + 2, 16);
            xref[i].MstIndex = (int)getValAtAddress(xref_ark.data, xref_ptr + 4, 16);
            xref[i].next = (int)getValAtAddress(xref_ark.data, xref_ptr + 6, 16);
            xref[i].nextTile = (int)getValAtAddress(xref_ark.data, xref_ptr + 8, 16);
            if (xref[i].nextTile != i)
            {//object extends over many tiles
                xref[i].duplicate = 1;
                xref[i].duplicateAssigned = 0;  //when this changes to 1 in a later loop it is the particular instance to use.
            }
            xref_ptr += 10;
        }

        if (!LoadChunk(archive_ark, LevelNo * 100 + 4008, out Chunk mst_ark))
        {
            return false;
        }
        long mstaddress_pointer = 0;

        //now run through the master table and process the duplicates flag
        for (i = 0; i < mst_ark.chunkUnpackedLength / 27; i++)
        {
            xref_ptr = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 5, 16);
            if (xref[xref_ptr].duplicate == 1)
            {
                //picks the first duplicate found to show any others will be pushed aside.
                xref[xref_ptr].duplicateAssigned = 1;
            }
            mstaddress_pointer += 27;
        }

        for (i = 0; i < xref_ark.chunkUnpackedLength / 10; i++)
        {
            if ((xref[i].duplicate == 1) && (xref[i].duplicateAssigned != 1))
            {//These are xref entries that are considered extra. I just cut them out of their linked list.
                replaceLink(ref xref, (int)xref_ark.chunkUnpackedLength / 10, i, xref[i].next); //replace the links to the duplicate
                replaceMapLink(ref LevelInfo, i, xref[i].next);
            }
        }


        for (i = 0; i <= objList.GetUpperBound(0); i++)
        {
            //To stop later crashes in ascii dumps I set some inital values.
            objList[i] = new ObjectLoaderInfo(i, UWEBase.CurrentTileMap(), true)
            {
                index = (short)i,
                next = 0,
                item_id = 0,
                link = 0,
                owner = 0
            };
        }

        //now i can build based on my master list with no duplicates spoiling things.!	
        mstaddress_pointer = 0;
        for (i = 0; i < mst_ark.chunkUnpackedLength / 27; i++)
        {

            xref_ptr = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 5, 16);
            int MasterIndex = xref[xref_ptr].MstIndex;
            objList[MasterIndex].index = (short)MasterIndex;
            objList[MasterIndex].link = 0;
            //objList[MasterIndex].joint=0;
            objList[MasterIndex].heading = 0;
            //objList[MasterIndex].objectConversion = 0;
            objList[MasterIndex].invis = 0;
            objList[MasterIndex].xpos = 0;
            objList[MasterIndex].ypos = 0;
            objList[MasterIndex].zpos = 0;
            objList[MasterIndex].address = mstaddress_pointer;
            InUseFlag = (short)getValAtAddress(mst_ark.data, mstaddress_pointer, 8);
           // objList[MasterIndex].InUseFlag = InUseFlag;

            //objList[MasterIndex].levelno = (short)LevelNo;
            objList[MasterIndex].ObjectTileX = xref[xref_ptr].tileX;
            objList[MasterIndex].ObjectTileY = xref[xref_ptr].tileY;
            objList[MasterIndex].next = (short)xref[xref[xref_ptr].next].MstIndex;
            objList[MasterIndex].parentList = this;

            ObjectClass = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 1, 8);
            objList[MasterIndex].ObjectClass = ObjectClass;
            ObjectSubClass = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 2, 8);
            objList[MasterIndex].ObjectSubClass = ObjectSubClass;
            int SubClassLink = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 3, 16);

            //Subclass per sspecs is  a link to the sub table. not the class it self. For that we need the object type.
            ObjectSubClassIndex = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 20, 8);

            objList[MasterIndex].ObjectSubClassIndex = ObjectSubClassIndex;
            int LookupIndex = getShockObjectIndex(ObjectClass, ObjectSubClass, ObjectSubClassIndex);//Into my object list not the sublist
            if (LookupIndex != -1)
            {

                objList[MasterIndex].item_id = LookupIndex;

                objList[MasterIndex].xpos = (short)getValAtAddress(mst_ark.data, mstaddress_pointer + 11, 8);
                objList[MasterIndex].ypos = (short)getValAtAddress(mst_ark.data, mstaddress_pointer + 13, 8);
                objList[MasterIndex].zpos = (short)getValAtAddress(mst_ark.data, mstaddress_pointer + 15, 8);

                objList[MasterIndex].Angle1 = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 16, 8);
                objList[MasterIndex].Angle2 = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 17, 8);
                objList[MasterIndex].Angle3 = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 18, 8);

                objList[MasterIndex].sprite = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 23, 8);
                objList[MasterIndex].State = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 23, 8);
                objList[MasterIndex].unk1 = (int)getValAtAddress(mst_ark.data, mstaddress_pointer + 24, 8);

                //fprintf(LOGFILE,"InUse = %d\n", objList[MasterIndex].InUseFlag);
                //fprintf(LOGFILE,"\tAIIndex = %d\n", getValAtAddress(mst_ark, mstaddress_pointer + 19, 8));
                //fprintf(LOGFILE,"\tHitPoints = %d\n", getValAtAddress(mst_ark, mstaddress_pointer + 21, 16));

                //fprintf(LOGFILE,"\tunk1 = %d\n",getValAtAddress(mst_ark,mstaddress_pointer+24,8));
                //fprintf(LOGFILE,"\tunk2 = %d\n",getValAtAddress(mst_ark,mstaddress_pointer+25,8));
                //fprintf(LOGFILE,"\tunk3 = %d\n",getValAtAddress(mst_ark,mstaddress_pointer+26,8));				
                //fprintf(LOGFILE,"\tXCoord= %d", getValAtAddress(mst_ark, mstaddress_pointer + 11, 8));
                //fprintf(LOGFILE,"\tXCoord high= %d\n",getValAtAddress(mst_ark,mstaddress_pointer+12,8)); same as tileX
                //fprintf(LOGFILE,"\tYCoord= %d", getValAtAddress(mst_ark, mstaddress_pointer + 13, 8));
                //fprintf(LOGFILE,"\tYCoord high= %d\n",getValAtAddress(mst_ark,mstaddress_pointer+14,8)); same as tileY
                //fprintf(LOGFILE,"\t(%d) ZCoord = %d\n", blockAddress + mstaddress_pointer + 15, getValAtAddress(mst_ark, mstaddress_pointer + 15, 8));
                //fprintf(LOGFILE,"\tAngles = (%d", getValAtAddress(mst_ark, mstaddress_pointer + 16, 8));
                //fprintf(LOGFILE,",%d", getValAtAddress(mst_ark, mstaddress_pointer + 17, 8));
                //fprintf(LOGFILE,"\,%d)\n", getValAtAddress(mst_ark, mstaddress_pointer + 18, 8));
                //fprintf(LOGFILE,"\tObjectType = %d\n", getValAtAddress(mst_ark, mstaddress_pointer + 20, 8));
                //fprintf(LOGFILE,"\tHitPoints = %d\n", getValAtAddress(mst_ark, mstaddress_pointer + 21, 16));
                //fprintf(LOGFILE,"\t(%d) State = %d", blockAddress + mstaddress_pointer+23, objList[MasterIndex].State);
                //fprintf(LOGFILE,"\t(%d,%d)\n", (objList[MasterIndex].State >> 4) & 0xF, objList[MasterIndex].State & 0xF);
                //fprintf(LOGFILE,"\tunk1 = %d", getValAtAddress(mst_ark, mstaddress_pointer + 24, 8));
                //fprintf(LOGFILE,"\tunk2 = %d", getValAtAddress(mst_ark, mstaddress_pointer + 25, 8));
                //fprintf(LOGFILE,"\tunk3 = %d\n", getValAtAddress(mst_ark, mstaddress_pointer + 26, 8));
                switch (ObjectClass)    //to get further properties specific to each class
                {
                    case GUNS_WEAPONS: break;
                    case AMMUNITION: break;
                    case PROJECTILES: break;
                    case GRENADE_EXPLOSIONS: break;
                    case PATCHES: break;
                    case HARDWARE: break;
                    case SOFTWARE_LOGS:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + SOFTWARE_LOGS_OFFSET, SOFTWARE_LOGS, SubClassLink, 9, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case FIXTURES:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + FIXTURES_OFFSET, FIXTURES, SubClassLink, 16, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case GETTABLES_OTHER:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + GETTABLES_OTHER_OFFSET, GETTABLES_OTHER, SubClassLink, 16, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case SWITCHES_PANELS:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + SWITCHES_PANELS_OFFSET, SWITCHES_PANELS, SubClassLink, 30, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case DOORS_GRATINGS:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + DOORS_GRATINGS_OFFSET, DOORS_GRATINGS, SubClassLink, 14, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case ANIMATED: break;
                    case TRAPS_MARKERS:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + TRAPS_MARKERS_OFFSET, TRAPS_MARKERS, SubClassLink, 28, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                    case CONTAINERS_CORPSES:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + CONTAINERS_CORPSES_OFFSET, CONTAINERS_CORPSES, SubClassLink, 21, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }

                    case CRITTERS:
                        {
                            lookUpSubClass(archive_ark, LevelInfo, LevelNo * 100 + CRITTERS_OFFSET, CRITTERS, SubClassLink, 46, xref, objList, texture_map, MasterIndex, LevelNo);
                            break;
                        }
                }
                UniqueObjectName(objList[MasterIndex]);
            }
            //else
            //{
            //   // objList[MasterIndex].InUseFlag = 0;
            //    //fprintf(LOGFILE,"\n\nInvalid item id!!\n");
            //}


            mstaddress_pointer += 27;
        }

        //turn obj indices into master indices
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                if (LevelInfo[x, y].indexObjectList != 0)
                { LevelInfo[x, y].indexObjectList = xref[LevelInfo[x, y].indexObjectList].MstIndex; }
            }
        }
        return true;
    }



    /// <summary>
    /// Builds the object list for UW
    /// </summary>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    /// <param name="texture_map">Texture map.</param>
    /// <param name="lev_ark">Lev ark.</param>
    /// <param name="LevelNo">Level no.</param>
    void BuildObjectListUW(ObjectLoaderInfo[] objList, TileMap map)
    {
        long address_pointer = 0;
        long objectsAddress = (64 * 64 * 4);
        for (short x = 0; x < 1024; x++)
        {   //read in master object list
            objList[x] = new ObjectLoaderInfo(x, map, true)
            {
                map = map,
                parentList = this,
                index = x,
                 address = map.lev_ark_block.Address + objectsAddress + address_pointer
            };

            if ((objList[x].item_id >= 464) && ((_RES == GAME_UW1) || (_RES == GAME_UWDEMO)))//Fixed for bugged out of range items
            {
                objList[x].item_id = 0;
            }
            HandleMovingDoors(objList, x);
            SetObjectTextureValue(objList, map.texture_map, x);
        }
    }


    /// <summary>
    /// Gets next available Free Mobile Object Slot
    /// </summary>
    /// <param name="AvailableSlot"></param>
    /// <returns>False if no slot is available</returns>
    public bool GetFreeMobileObject(out short AvailableSlot)
    {
        AvailableSlot = NoOfFreeMobile;
        if (AvailableSlot >= 2)
        {
            Debug.Log("Allocating Mobile Slot #" + AvailableSlot + " which is item index " + GetMobileAtSlot(AvailableSlot));
            NoOfFreeMobile--;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the item index at the specified slot
    /// </summary>
    /// <returns></returns>
    public short GetMobileAtSlot(short slot)
    {
        ////   7300    01fc   free list for mobile objects (objects 0002-00ff, 254 x 2 bytes)
        int offset = slot * 2;
        short val = (short)getValAtAddress(map.lev_ark_block.Data, 0x7300 + offset, 16);

        //return (int)getValAtAddress(map.lev_ark_block.Data, 0x74fc + offset, 16);
        return val;
    }


    /// <summary>
    /// Gets next available Free Static Object Slot
    /// </summary>
    /// <param name="AvailableSlot"></param>
    /// <returns>False if no slot is available</returns>
    public bool GetFreeStaticObject(out short AvailableSlot)
    {
        AvailableSlot = NoOfFreeStatic;
        if (AvailableSlot >= 2)
        {
            NoOfFreeStatic--;
            Debug.Log("Allocating Static Slot #" + AvailableSlot + " which is item index " + GetStaticAtSlot(AvailableSlot)) ;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Releases a free static object from the list.
    /// Shift all used objects up by one
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void ReleaseFreeStaticObject(int index)
    {
        int Slot = -1;
        //Find the slot it is currently in.
        for (int i = 0; i < 768; i++)
        {
            if (index == GetStaticAtSlot(i))
            {
                Slot = i;
                break;
            }
        }

        if (Slot > 0)
        {//Found

            ////Shift all values between start of used to object up by one in the list.
            //for (int i = Slot; i >= NoOfFreeStatic; i--)
            //{
            //    int ValueToShift = GetStaticAtSlot(i - 1);
            //    SetStaticAtSlot(i, ValueToShift);
            //}
            NoOfFreeStatic++;
            //Insert object at top of available list
            SetStaticAtSlot(NoOfFreeStatic, index);
            //Increment free object counter.
            
        }

        objInfo[index].next = 0;
    }


    /// <summary>
    /// Releases a free mobile object from the list.
    /// Shift all used objects up by one
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void ReleaseFreeMobileObject(int index)
    {
        short Slot = -1;
        //Find the slot it is currently in.
        for (short i = 0; i < 254; i++)
        {
            if (index == GetMobileAtSlot(i))
            {
                Slot = i;
                break;
            }
        }

        if (Slot > 0)
        {//Found
            NoOfFreeMobile++;
            //Insert object at top of available list
            SetMobileAtSlot(NoOfFreeMobile, index);

            ////Shift all values between start of used to object up by one in the list.
            //for (short i = Slot; i >= NoOfFreeMobile; i--)
            //{
            //    int ValueToShift = GetMobileAtSlot((short)(i - 1));
            //    SetMobileAtSlot(i, ValueToShift);
            //}

            ////Insert object at top of available list
            //SetMobileAtSlot(NoOfFreeMobile, index);
            ////Increment free object counter.
            //NoOfFreeStatic++;
        }
        objInfo[index].next = 0;
    }

    /// <summary>
    /// Gets the item index at the specified slot
    /// </summary>
    /// <returns></returns>
    public short GetStaticAtSlot(int slot)
    {
        ////74fc    0600   free list for static objects (objects 0100 - 03ff, 768 x 2 bytes)
        int offset = slot * 2;
        var val = (short)getValAtAddress(map.lev_ark_block.Data, 0x74fc + offset, 16);
        return val;
    }

    /// <summary>
    /// Sets the item index to store at the specified slot
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="itemIndex"></param>
    void SetStaticAtSlot(int slot, int itemIndex)
    {
        int offset = slot * 2;
        map.lev_ark_block.Data[0x74fc + offset] = (byte)(itemIndex & 0xFF);
        map.lev_ark_block.Data[0x74fc + offset + 1] = (byte)((itemIndex >> 8) & 0xFF);
    }


    /// <summary>
    /// Sets the item index to store at the specified slot
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="itemIndex"></param>
    void SetMobileAtSlot(int slot, int itemIndex)
    {
        int offset = slot * 2;
        map.lev_ark_block.Data[0x7300 + offset] = (byte)(itemIndex & 0xFF);
        map.lev_ark_block.Data[0x7300 + offset + 1] = (byte)((itemIndex >> 8) & 0xFF);
    }


    ///// <summary>
    ///// Builds the free object lists in order to identify which objects need to be created
    ///// </summary>
    ///// <param name="objList">Object list.</param>
    ///// <param name="lev_ark">Lev ark.</param>
    ///// <param name="address_pointer">Address pointer.</param>
    ///// <param name="objectsAddress">Objects address.</param>
    //void BuildFreeObjectLists(ObjectLoaderInfo[] objList, UWBlock lev_ark, ref long address_pointer, ref long objectsAddress)
    //{
    //    Debug.Log("BuildFreeObjectLists");
    //    return;
    //    //////////////// //NoOfFreeMobile = (int)getValAtAddress(lev_ark, 0x7c02, 16);
    //    ////////////////// NoOfFreeStatic = (int)getValAtAddress(lev_ark, 0x7c04, 16);
    //    //////////////// //	Debug.Log("This file has " + NoOfFreeMobile + " mobile object slots and " + NoOfFreeStatic + " static objects slots");
    //    //////////////// for (int i = 0; i <= objList.GetUpperBound(0); i++)
    //    //////////////// {
    //    ////////////////     if (i > 2)
    //    ////////////////     {
    //    ////////////////         objList[i].InUseFlag = 1;
    //    ////////////////         //Assume in use unless informed otherwise in the next loop.			
    //    ////////////////     }
    //    //////////////// }
    //    //////////////// objectsAddress = 0x7300;
    //    //////////////// //location of the mobile object free list
    //    //////////////// address_pointer = 0;
    //    //////////////// StreamWriter writer = new StreamWriter(Application.dataPath + "//..//_objInUse_At_Load_ark.txt", false);
    //    //////////////// string output = "Mobile List\n";
    //    //////////////// for (int i = 0; i <= NoOfFreeMobile; i++)
    //    //////////////// {
    //    ////////////////     int freed = (int)getValAtAddress(lev_ark, objectsAddress + address_pointer, 16);
    //    ////////////////     objList[freed].InUseFlag = 0;
    //    ////////////////     output = output + "Mobile Free:" + i + " = " + freed + "\n";
    //    ////////////////     address_pointer += 2;
    //    //////////////// }
    //    //////////////// output = output + "Count:" + NoOfFreeMobile + "\n";
    //    //////////////// for (int i = NoOfFreeMobile + 1; i < 254; i++)
    //    //////////////// {
    //    ////////////////     int freed = (int)getValAtAddress(lev_ark, objectsAddress + address_pointer, 16);
    //    ////////////////     output = output + "Mobile Junk:" + i + " = " + freed + "\n";
    //    ////////////////     address_pointer += 2;
    //    //////////////// }
    //    //////////////// output = output + "Static List\n";
    //    //////////////// objectsAddress = 0x74fc;
    //    //////////////// //location of the static object free list
    //    //////////////// address_pointer = 0;
    //    //////////////// for (int i = 0; i <= NoOfFreeStatic; i++)
    //    //////////////// {
    //    ////////////////     int freed = (int)getValAtAddress(lev_ark, objectsAddress + address_pointer, 16);
    //    ////////////////     objList[freed].InUseFlag = 0;
    //    ////////////////     output = output + "Static Free:" + i + " = " + freed + "\n";
    //    ////////////////     address_pointer += 2;
    //    //////////////// }
    //    //////////////// output = output + "Count (static):" + NoOfFreeStatic + "\n";
    //    //////////////// for (int i = NoOfFreeStatic + 1; i < 768; i++)
    //    //////////////// {
    //    ////////////////     int freed = (int)getValAtAddress(lev_ark, objectsAddress + address_pointer, 16);
    //    ////////////////     output = output + "Static Junk:" + i + " = " + freed + "\n";
    //    ////////////////     address_pointer += 2;
    //    //////////////// }
    //    //////////////// writer.Write(output);
    //    //////////////// writer.Close();
    //}

    static void HandleMovingDoors(ObjectLoaderInfo[] objList, int x)
    {
        if (objList[x].GetItemType() == ObjectInteraction.A_MOVING_DOOR)
        {
            //Moving doors have the following properties. The 320+owner is the door type that it is moving from.
            //To hack in support for moving doors that load from UW I am just going to convert the moving door to the final state
            //it should be in
            objList[x].item_id = 320 + objList[x].owner;
            switch (objList[x].item_id)
            {
                //closed doors
                case 320:
                case 321:
                case 322:
                case 323:
                case 324:
                case 325:
                case 327:
                    //secret door
                    objList[x].item_id += 8;
                    //objList[x].zpos-=24;
                    objList[x].flags = 5;
                    objList[x].enchantment = 1;
                    objList[x].owner = 0;
                    break;
                case 326:
                    //Portcullis
                    objList[x].item_id += 8;
                    //objList[x].zpos-=24;
                    objList[x].flags = 4;
                    objList[x].enchantment = 1;
                    objList[x].owner = 0;
                    break;
                //open doors
                case 328:
                case 329:
                case 330:
                case 331:
                case 332:
                case 333:
                case 335:
                //Open secret door
                case 334:
                    //open portcullis
                    objList[x].item_id -= 8;
                    //objList[x].zpos-=24;
                    objList[x].flags = 0;
                    objList[x].enchantment = 0;
                    objList[x].owner = 0;
                    break;
            }
        }
    }
    /// <summary>
    /// Sets the object texture value.
    /// </summary>
    /// <param name="objList">Object list.</param>
    /// <param name="texture_map">Texture map.</param>
    /// <param name="x">The x coordinate.</param>
    static void SetObjectTextureValue(ObjectLoaderInfo[] objList, short[] texture_map, int x)
    {
        if ((objList[x].GetItemType() == ObjectInteraction.TMAP_SOLID) || (objList[x].GetItemType() == ObjectInteraction.TMAP_CLIP))
        {
            objList[x].Obsolete_texture = texture_map[objList[x].owner]; //Sets the texture for tmap objects. 
        }

        //Some of this stuff should move to obj_base
        if (objList[x].GetItemType() == ObjectInteraction.BRIDGE)
        {
            if (objList[x].flags >= 2)
            {
                //267 + textureIndex;
                if (_RES == GAME_UW2)
                {
                    objList[x].Obsolete_texture = texture_map[objList[x].flags - 2];
                    //Sets the texture for bridge
                }
                else
                {
                    objList[x].Obsolete_texture = texture_map[objList[x].flags - 2 + 48];
                    //Sets the texture for bridge
                }
            }
            else
            {
                objList[x].Obsolete_texture = 267 + (objList[x].flags & 0x3F);
                //267 is an offset into my own textures config file.
            }
        }
        if (objList[x].GetItemType() == ObjectInteraction.BUTTON)
        {
            objList[x].Obsolete_texture = objList[x].flags;
        }
        if (objList[x].GetItemType() == ObjectInteraction.GRAVE)
        {
            objList[x].Obsolete_texture = objList[x].flags + 28;
        }
        if (objList[x].GetItemType() == ObjectInteraction.A_CREATE_OBJECT_TRAP)//Position the trap in the centre of the tile
        {
            //objList[x].x = 4;
            //objList[x].y = 4;
        }
        if (objList[x].GetItemType() == ObjectInteraction.A_CHANGE_TERRAIN_TRAP)
        {
            //bits 1-5 of the quality field is the floor texture.
            if (_RES == GAME_UW1)
            {
                int textureQuality = (objList[x].quality >> 1) & 0xf;
                if (textureQuality == 10)
                {
                    //Weird glitch texture
                    //textureQuality=-1;
                    //textureQuality = textureQuality - 10;
                    objList[x].Obsolete_texture = -1;
                }
                else
                    if (textureQuality > 10)
                {
                    //textureQuality=8;//Always seems to be this texture.
                    //textureQuality = -1;//use the texture already there?
                    objList[x].Obsolete_texture = -1;
                    //texture_map[(textureQuality)+48];//-1 to reuse the existing texture
                }
                else
                {
                    objList[x].Obsolete_texture = texture_map[(textureQuality) + 48];
                }
                if (objList[x].zpos > 96)
                {
                    //cap the zpos height at this
                    objList[x].zpos = 96;
                }
            }
        }
    }

    public static string UniqueObjectName(ObjectLoaderInfo currObj)
    {//returns a unique name for the object
     //"%s_%02d_%02d_%02d_%04d\0", GameWorldController.instance.objectMaster[currObj.item_id].desc, currObj.tileX, currObj.tileY, currObj.levelno, currObj.index);
        switch (currObj.GetItemType())
        {
            case ObjectInteraction.DOOR:
            case ObjectInteraction.HIDDENDOOR:
            case ObjectInteraction.PORTCULLIS:
                {
                    return "door_" + currObj.ObjectTileX.ToString("d3") + "_" + currObj.ObjectTileY.ToString("d3");
                }
            case ObjectInteraction.NPC_TYPE:
                {
                    string npcname = StringController.instance.GetString(7, currObj.npc_whoami + 16);
                    if ((currObj.npc_whoami != 0) && (npcname != ""))
                    {
                        return npcname + "_" + currObj.ObjectTileX.ToString("d2") + "_" + currObj.ObjectTileY.ToString("d2") + "_" + currObj.levelno.ToString("d2") + "_" + currObj.index.ToString("d4") + "_" + currObj.guid.ToString();
                    }
                    else
                    {
                        return currObj.getDesc() + "_" + currObj.ObjectTileX.ToString("d2") + "_" + currObj.ObjectTileY.ToString("d2") + "_" + currObj.levelno.ToString("d2") + "_" + currObj.index.ToString("d4") + "_" + currObj.guid.ToString();
                    }
                }
            default:
                {
                    return currObj.getDesc() + "_" + currObj.ObjectTileX.ToString("d2") + "_" + currObj.ObjectTileY.ToString("d2") + "_" + currObj.levelno.ToString("d2") + "_" + currObj.index.ToString("d4") + "_" + currObj.guid.ToString();
                }
        }
    }

    public static string UniqueObjectNameEditor(ObjectLoaderInfo currObj)
    {//returns a shorter unique name for the object editor
     //StringController.instance.GetSimpleObjectNameUW(objList[o].item_id)
     //	return GameWorldController.instance.objectMaster.desc[currObj.item_id] +"_"+currObj.index.ToString("d4");// + "_" + currObj.guid.ToString();
        return StringController.instance.GetSimpleObjectNameUW(currObj.item_id) + "_" + currObj.index.ToString("d4");
    }

    /// <summary>
    /// Sets the object tile X and Y values. Flags if the object exists in the map
    /// </summary>
    /// <param name="game">Game.</param>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    void setObjectTileXY(int game, TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList)
    {//Justs some useful info to know.
     //ObjectItem currObj;
        for (short x = 0; x < 64; x++)
        {
            for (short y = 0; y < 64; y++)
            {
                if (LevelInfo[x, y].indexObjectList != 0)
                {
                    long nextObj = LevelInfo[x, y].indexObjectList;
                    while (nextObj != 0)
                    {
                        objList[nextObj].ObjectTileX = x;
                        objList[nextObj].ObjectTileY = y;
                        //objList[nextObj].InUseFlag = 1;
                        /*Free	if ((isContainer(objList[nextObj])) || (GameWorldController.instance.objectMaster.type[objList[nextObj].item_id] == ObjectInteraction.NPC_TYPE))
                            {
                                    SetContainerInUse(game, LevelInfo,objList, objList[nextObj].index);
                            }
                            else if (GameWorldController.instance.objectMaster.type[objList[nextObj].item_id] == ObjectInteraction.A_CREATE_OBJECT_TRAP)
                            {
                                    if (objList[nextObj].InUseFlag == 0)
                                    {
                                            objList[nextObj].tileX = x;
                                            objList[nextObj].tileY = y;
                                            objList[nextObj].InUseFlag = 1;

                                    }
                                    if ( 
                                            (GameWorldController.instance.objectMaster.type[objList[objList[nextObj].link].item_id] == ObjectInteraction.NPC_TYPE)
                                            && 
                                            (objList[objList[nextObj].link].InUseFlag==0)
                                    )
                                    {
                                            SetContainerInUse(game, LevelInfo, objList, objList[objList[nextObj].link].index);
                                    }
                                    if (objList[nextObj].link!=0)
                                    {//Make sure the object to be spawned is linked
                                            objList[objList[nextObj].link].InUseFlag=1;
                                    }
                            }
                            else if (GameWorldController.instance.objectMaster.type[objList[nextObj].item_id] == ObjectInteraction.BRIDGE)
                            {
                                int TextureIndex=  (objList[nextObj].enchantment<<3) | objList[nextObj].flags & 0x3F;
                                if (TextureIndex<2)
                                {//Only flag the normal briges as such.
                                    LevelInfo[x,y].hasBridge=true;				
                                }

                            } FREE */

                        nextObj = objList[nextObj].next;
                    }
                }
            }
        }

        /*		for (int i = 0; i < 1024;i++)
				{//Make sure triggers, traps and special items are created.
						if (objList[i]!=null)
						{
								if (objList[i].InUseFlag == 0)
								{
										if ((isTrigger(objList[i]) )|| (isTrap(objList[i]) || (isAlwaysInUse(objList[i]))))
										{
												if (GameWorldController.instance.objectMaster.type[objList[i].item_id] != ObjectInteraction.A_SPELLTRAP)
												{//To fix bug with level 3 of uw1. Junk objects on level map are spell traps.
													objList[i].InUseFlag=1;		
													if (objList[i].link!=0)
													{
														if (GameWorldController.instance.objectMaster.type[objList[objList[i].link].item_id]== ObjectInteraction.A_SPELLTRAP)
														{
															objList[objList[i].link].InUseFlag=1;	
														}
													}

												}
												if (GameWorldController.instance.objectMaster.type[objList[i].item_id] == ObjectInteraction.A_CREATE_OBJECT_TRAP)
												{
														if (objList[i].link!=0)
														{
																objList[objList[i].link].InUseFlag = 1;			
																if (GameWorldController.instance.objectMaster.type[objList[objList[i].link].item_id]== ObjectInteraction.NPC_TYPE)
																{
																		SetContainerInUse(game, LevelInfo, objList, objList[objList[i].link].index);	
																}
														}
												}
										}
								}	
						}
				}*/

    }

    /// <summary>
    /// Is the object a container.
    /// </summary>
    /// <returns><c>true</c>, if container was ised, <c>false</c> otherwise.</returns>
    /// <param name="currobj">Currobj.</param>
    public static bool isContainer(ObjectLoaderInfo currobj)
    {
        return ((currobj.GetItemType() == ObjectInteraction.CONTAINER) || (currobj.GetItemType() == ObjectInteraction.CORPSE));
    }

    /// <summary>
    /// Tells if the objects that will never move
    /// </summary>
    /// <returns><c>true</c>, if static was ised, <c>false</c> otherwise.</returns>
    /// <param name="currobj">Currobj.</param>
    public static bool isStatic(ObjectLoaderInfo currobj)
    {
        if (isTrap(currobj) || isTrigger(currobj))
        {
            return true;
        }
        switch (currobj.GetItemType())
        {
            case ObjectInteraction.TMAP_CLIP:
            case ObjectInteraction.TMAP_SOLID:
            case ObjectInteraction.DOOR:
            case ObjectInteraction.HIDDENDOOR:
            case ObjectInteraction.PORTCULLIS:
            case ObjectInteraction.GRAVE:
            case ObjectInteraction.FOUNTAIN:
            case ObjectInteraction.BUTTON:
            case ObjectInteraction.PILLAR:
            case ObjectInteraction.BRIDGE:
            case ObjectInteraction.SIGN:
            case ObjectInteraction.LOCK:
                return true;
            default:
                return false;
        }
    }


    public static bool isAlwaysInUse(ObjectLoaderInfo currobj)
    {//Objects that will always be generated.
        switch (currobj.GetItemType())
        {
            case ObjectInteraction.SPELL:
            case ObjectInteraction.LOCK:
                return true;
            default:
                return false;
        }
    }

    public static bool isTrigger(ObjectLoaderInfo currobj)
    {//Tells if the object is a trigger that can set of a trap.
        switch (currobj.GetItemType())
        {
            case ObjectInteraction.A_MOVE_TRIGGER:
            case ObjectInteraction.A_PICK_UP_TRIGGER:
            case ObjectInteraction.A_USE_TRIGGER:
            case ObjectInteraction.A_LOOK_TRIGGER:
            case ObjectInteraction.A_STEP_ON_TRIGGER:
            case ObjectInteraction.AN_OPEN_TRIGGER:
            case ObjectInteraction.AN_UNLOCK_TRIGGER:
            case ObjectInteraction.A_TIMER_TRIGGER:
            case ObjectInteraction.A_SCHEDULED_TRIGGER:
            case ObjectInteraction.A_PRESSURE_TRIGGER:
            case ObjectInteraction.A_CLOSE_TRIGGER:
            case ObjectInteraction.AN_ENTER_TRIGGER:
            case ObjectInteraction.A_PROXIMITY_TRAP:
                {
                    return true;
                }
            default:
                {
                    return false;
                }
        }
    }


    public static bool isTrap(ObjectLoaderInfo currobj)
    {
        switch (currobj.GetItemType())
        {
            case ObjectInteraction.A_DAMAGE_TRAP:
            case ObjectInteraction.A_TELEPORT_TRAP:
            case ObjectInteraction.A_ARROW_TRAP:
            case ObjectInteraction.A_DO_TRAP:
            case ObjectInteraction.A_PIT_TRAP:
            case ObjectInteraction.A_CHANGE_TERRAIN_TRAP:
            case ObjectInteraction.A_SPELLTRAP:
            case ObjectInteraction.A_CREATE_OBJECT_TRAP:
            case ObjectInteraction.A_DOOR_TRAP:
            case ObjectInteraction.A_WARD_TRAP:
            case ObjectInteraction.A_TELL_TRAP:
            case ObjectInteraction.A_DELETE_OBJECT_TRAP:
            case ObjectInteraction.AN_INVENTORY_TRAP:
            case ObjectInteraction.A_SET_VARIABLE_TRAP:
            case ObjectInteraction.A_CHECK_VARIABLE_TRAP:
            case ObjectInteraction.A_COMBINATION_TRAP:
            case ObjectInteraction.A_TEXT_STRING_TRAP:
            case ObjectInteraction.AN_OSCILLATOR:
            case ObjectInteraction.A_CHANGE_TO_TRAP:
            case ObjectInteraction.A_CHANGE_FROM_TRAP:
            case ObjectInteraction.AN_EXPERIENCE_TRAP:
            case ObjectInteraction.A_JUMP_TRAP:
            case ObjectInteraction.A_SKILL_TRAP:
            case ObjectInteraction.A_NULL_TRAP:
            case ObjectInteraction.UNIMPLEMENTED_TRAP:
            case ObjectInteraction.A_PROXIMITY_TRAP:
            case ObjectInteraction.A_BRIDGE_TRAP:
                {
                    return true;
                }
            default:
                {
                    return false;
                }
        }
    }


    public static bool isMobile(ObjectLoaderInfo currobj)
    {
        return (currobj.index < 256);
    }

    //void SetContainerInUse(int game, TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int index)
    //{
    //		Debug.Log("SetContainerInUse. Is no longer in use..");
    //		return;
    //		//Take a container/npc and set inuseflag for it contents. 
    //		ObjectLoaderInfo currobj = objList[index];
    //		//currobj.InUseFlag == 1;
    //		objList[index].InUseFlag=1;
    //		if (currobj.link != 0)
    //		{//Object has contents.
    //				ObjectLoaderInfo tmpObj = objList[currobj.link];//Get the first item in the contents.
    //				objList[tmpObj.index].InUseFlag=1;
    //				if ((isContainer(tmpObj)) || (GameWorldController.instance.objectMaster.type[tmpObj.item_id] == ObjectInteraction.NPC_TYPE))
    //				{//If the first item is a container recursively set it's flag
    //						SetContainerInUse(game, LevelInfo, objList, tmpObj.index);
    //				}
    //				//Now loop through the linked list.
    //				if (tmpObj.next > 0)
    //				{
    //						while (tmpObj.next > 0)
    //						{
    //								tmpObj = objList[tmpObj.next];
    //								objList[tmpObj.index].InUseFlag = 1;
    //								//if the next object is a countainer loop through it.
    //								if ((isContainer(tmpObj)) || (GameWorldController.instance.objectMaster.type[tmpObj.item_id] == ObjectInteraction.NPC_TYPE))
    //								{//If the first item is a container recursively set it's flag
    //										SetContainerInUse(game,LevelInfo, objList, tmpObj.index);
    //								}
    //						}
    //				}
    //				tmpObj.InUseFlag = 1;
    //		}
    //}

    ///

    void setElevatorBits(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList)
    {//So I know the tile contains an elevator.
     //Note for shock this is set when I read in the object. I should probably do the same for UW.
        ObjectLoaderInfo currObj;
        for (short x = 0; x < 64; x++)
        {
            for (short y = 0; y < 64; y++)
            {
                if (LevelInfo[x, y].indexObjectList != 0)
                {
                    currObj = objList[LevelInfo[x, y].indexObjectList];
                    do
                    {
                        if (
                            ((currObj.GetItemType() == ObjectInteraction.A_DO_TRAP) && (currObj.quality == 3))
                            ||
                            ((currObj.GetItemType() == ObjectInteraction.AN_OSCILLATOR))
                            ||
                            ((currObj.GetItemType() == ObjectInteraction.A_PIT_TRAP))
                            )
                        {
                            //LevelInfo[x,y].hasElevator= true;
                            LevelInfo[x, y].TerrainChange = true;
                            //LevelInfo[x,y].ElevatorIndex = currObj.index;
                            currObj.ObjectTileX = x;
                            currObj.ObjectTileY = y;
                            break;
                        }
                        currObj = objList[currObj.next];
                    } while (currObj.index != 0);
                }
            }
        }
    }

    /// <summary>
    /// To i.d. that the tile terrains changes and I can later render both versions of the tile.
    /// </summary>
    /// <param name="LevelInfo"></param>
    /// <param name="objList"></param>
    /// Does not support off map terrain change traps.
    void setTerrainChangeBits(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList)
    {
        ObjectLoaderInfo currObj;
        for (short x = 0; x < 64; x++)
        {
            for (short y = 0; y < 64; y++)
            {
                if (LevelInfo[x, y].indexObjectList != 0)
                {
                    currObj = objList[LevelInfo[x, y].indexObjectList];
                    do
                    {
                        if (currObj.GetItemType() == ObjectInteraction.A_CHANGE_TERRAIN_TRAP)
                        {
                            LevelInfo[x, y].TerrainChange = true;
                            //LevelInfo[x,y].TerrainChangeIndex = currObj.index;
                            //Need to flag the range of tiles affected. X/y of the object gives the dimensions
                            for (int j = x; j <= x + currObj.xpos; j++)
                            {
                                for (int k = y; k <= y + currObj.ypos; k++)
                                {
                                    LevelInfo[j, k].TerrainChange = true;
                                    //Flag each of it's neighbours as well. May already be flagged.
                                    for (int q = -1; q <= 1; q++)
                                    {
                                        for (int r = -1; r <= 1; r++)
                                        {
                                            if (((j + r >= 0) && (j + r < 63)) && ((k + q >= 0) && (k + q < 63)))
                                            {
                                                LevelInfo[j + r, k + q].TerrainChange = true;
                                                LevelInfo[j + r, k + q].Render = true;
                                            }
                                        }
                                    }
                                }
                            }
                            currObj.ObjectTileX = x;
                            currObj.ObjectTileY = y;
                            //break;
                        }
                        currObj = objList[currObj.next];
                    } while (currObj.index != 0);
                }
            }

        }

    }


    void SetBullFrog(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int LevelNo)
    {
        //Special UW1 case for the bullfrog puzzle
        if ((LevelNo == 3) && (_RES == GAME_UW1))
        {
            for (int x = 48; x < 56; x++)
            {
                for (int y = 48; y < 56; y++)
                {
                    LevelInfo[x, y].TerrainChange = true;
                }
            }
        }
        else if ((_RES == GAME_UW2) && (LevelNo == 64))
        {//platform wave
            LevelInfo[10, 49].TerrainChange = true;
            LevelInfo[10, 50].TerrainChange = true;
            LevelInfo[10, 51].TerrainChange = true;
            LevelInfo[10, 45].TerrainChange = true;
            LevelInfo[10, 46].TerrainChange = true;
            LevelInfo[10, 47].TerrainChange = true;
        }

    }

    /// <summary>
    /// Sets the tiles that contain a floor collapse hack trap
    /// </summary>
    /// <param name="LevelInfo"></param>
    /// <param name="objList"></param>
    /// <param name="LevelNo"></param>
    void SetFloorCollapseTiles(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int LevelNo)
    {
        for (int i = 256; i <= objList.GetUpperBound(0); i++)
        {
            ObjectLoaderInfo currobj = objList[i];
            if ((currobj.link > 0) && (currobj.link <= objList.GetUpperBound(0)))
            {
                ObjectLoaderInfo linkobj = objList[currobj.link];
                if (isTrigger(currobj) && (linkobj.item_id == 387) && (linkobj.quality == 17))
                {
                    //Flag each tile in a 20 tile block as being a tile change tile
                    int triggerX = currobj.quality; int triggerY = currobj.owner;
                    for (int x = -10; x <= 10; x++)
                    {
                        for (int y = -10; y <= 10; y++)
                        {
                            if (TileMap.ValidTile(triggerX + x, triggerY + y))
                            {
                                if (LevelInfo[triggerX + x, triggerY + y].tileType == TileMap.TILE_OPEN)
                                {
                                    LevelInfo[triggerX + x, triggerY + y].TerrainChange = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Sets the qbert tiles that will change with the pyramid.
    /// </summary>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    /// <param name="LevelNo">Level no.</param>
    void setQbert(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int LevelNo)
    {
        if ((LevelNo == 68) && (_RES == GAME_UW2))
        {

            LevelInfo[45, 50].TerrainChange = true;
            LevelInfo[46, 50].TerrainChange = true;
            LevelInfo[47, 50].TerrainChange = true;
            LevelInfo[48, 50].TerrainChange = true;
            LevelInfo[49, 50].TerrainChange = true;

            LevelInfo[44, 51].TerrainChange = true;
            LevelInfo[45, 52].TerrainChange = true;
            LevelInfo[46, 53].TerrainChange = true;
            LevelInfo[47, 54].TerrainChange = true;
            LevelInfo[48, 55].TerrainChange = true;
            LevelInfo[49, 56].TerrainChange = true;

            LevelInfo[50, 51].TerrainChange = true;
            LevelInfo[50, 52].TerrainChange = true;
            LevelInfo[50, 53].TerrainChange = true;
            LevelInfo[50, 54].TerrainChange = true;
            LevelInfo[50, 55].TerrainChange = true;
        }
    }

    /// <summary>
    /// Finds the off map oscillator tiles.
    /// </summary>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    /// <param name="LevelNo">Level no.</param>
    /// Hack to fix guardians trap world
    void FindOffMapOscillatorTiles(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int LevelNo)
    {
        //Fix for UW2 map bug involving oscillator
        if (LevelNo == 71)
        {
            LevelInfo[34, 44].TerrainChange = true;
        }

        /*for (int i =0; i<=objList.GetUpperBound(0);i++)
        {
                if (isTrigger(objList[i]))
                {
                        if (objList[i].link!=0)
                        {
                                if(GameWorldController.instance.objectMaster.type[ objList[objList[i].link].item_id ] == ObjectInteraction.AN_OSCILLATOR)												
                                {
                                        if (TileMap.ValidTile(objList[i].quality, objList[i].owner))
                                        {
                                            LevelInfo[objList[i].quality, objList[i].owner].TerrainChange=true;
                                        }
                                }
                        }
                }
                if (isTrap(objList[i]))
                {
                        if (objList[i].link!=0)
                        {
                                if(GameWorldController.instance.objectMaster.type[ objList[objList[i].link].item_id ] == ObjectInteraction.AN_OSCILLATOR)												
                                {
                                        if (TileMap.ValidTile(objList[i].tileX, objList[i].tileY))
                                        {
                                                LevelInfo[objList[i].tileX, objList[i].tileY].TerrainChange=true;
                                        }
                                }
                        }
                }

        }*/
    }


    /// <summary>
    /// Marks the colour cycling tiles for terrain changing.
    /// </summary>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    /// <param name="LevelNo">Level no.</param>
    void SetColourCyclingTiles(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList, int LevelNo)
    {
        for (int i = 0; i <= objList.GetUpperBound(0); i++)
        {
            if (objList[i] != null)
            {
                if (objList[i].item_id == 387)
                {
                    if (objList[i].quality == 0xE)
                    {
                        for (int x = objList[i].ObjectTileX - 1; x <= objList[i].ObjectTileX + 5; x++)
                        {
                            for (int y = objList[i].ObjectTileY - 1; y <= objList[i].ObjectTileY + 5; y++)
                            {
                                if (TileMap.ValidTile(x, y))
                                {
                                    LevelInfo[x, y].TerrainChange = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the position in world space of the object at ObjectIndex in the current object list
    /// </summary>
    /// <param name="ObjectIndex"></param>
    /// <param name="WallAdjust"></param>
    /// <returns></returns>
    public static Vector3 CalcObjectXYZ(int ObjectIndex, short WallAdjust)
    {//?
        ObjectLoaderInfo[] objList = CurrentObjectList().objInfo;
        TileMap tileMap = CurrentTileMap();
        int x = objList[ObjectIndex].ObjectTileX;
        int y = objList[ObjectIndex].ObjectTileY;
        float ResolutionXY = 7.0f;  // A tile has a 7x7 grid for object positioning.
        float ResolutionZ = 128.0f; //UW has 127 posible z positions for an object in tile.
        if (_RES == GAME_SHOCK) { ResolutionXY = 256.0f; ResolutionZ = 256.0f; } //Shock has more "z" in it.

        float BrushX = 120f;
        float BrushY = 120f;
        float BrushZ = 15f;
        float objX = objList[ObjectIndex].xpos;
        float objY = objList[ObjectIndex].ypos;
        float offX = x * BrushX + objList[ObjectIndex].xpos * (BrushX / ResolutionXY);
        float offY = y * BrushY + objList[ObjectIndex].ypos * (BrushY / ResolutionXY);

        float zpos = objList[ObjectIndex].zpos;

        float ceil = tileMap.CEILING_HEIGHT;
        float offZ = zpos / ResolutionZ * ceil * BrushZ;
        if ((_RES != GAME_SHOCK) && (x < 64) && (y < 64))
        {//Adjust zpos by a fraction for objects on sloped tiles.
            switch (tileMap.Tiles[x, y].tileType)
            {
                case TileMap.TILE_SLOPE_N:
                    offZ += objY * (48.0f / BrushZ);
                    break;
                case TileMap.TILE_SLOPE_E:
                    offX += objX * (48.0f / BrushZ);
                    break;
                case TileMap.TILE_SLOPE_S:
                    offZ += (8.0f - objY) * (48.0f / BrushZ);
                    break;
                case TileMap.TILE_SLOPE_W:
                    offZ += (8.0f - objX) * (48.0f / BrushZ);
                    break;
            }
        }
        if ((x < 64) && (y < 64))
        {
            switch (objList[ObjectIndex].GetItemType())
            {
                case ObjectInteraction.TMAP_CLIP:
                case ObjectInteraction.TMAP_SOLID:
                    switch (objList[ObjectIndex].heading * 45)
                    {
                        case ObjectInteraction.HEADINGWEST:
                        case ObjectInteraction.HEADINGEAST:
                            offY = (y * BrushY) + 60f;//center in tile
                            if (objList[ObjectIndex].xpos == 0)
                            {
                                offX += 1.0f;//was 6.5
                            }
                            if (objList[ObjectIndex].xpos == 7)
                            {
                                offX -= 1.0f;//was 6.5
                            }
                            break;
                        case ObjectInteraction.HEADINGNORTH:
                        case ObjectInteraction.HEADINGSOUTH:
                            offX = (x * BrushX) + 60f;
                            if (objList[ObjectIndex].ypos == 0)
                            {
                                offY += 1.0f;//was 6.5
                            }
                            if (objList[ObjectIndex].ypos == 7)
                            {
                                offY -= 1.0f;//was 6.5
                            }
                            break;
                    }
                    break;

                case ObjectInteraction.DOOR:
                case ObjectInteraction.HIDDENDOOR:
                case ObjectInteraction.PORTCULLIS:
                    {
                        float DOORWIDTH = 80f;

                        //Doors will always go at the tile height.
                        int newZpos = tileMap.Tiles[x, y].floorHeight * 4;
                        offZ = ((newZpos / ResolutionZ) * (ceil)) * BrushZ;
                        int BridgeIndex = findObjectByTypeInTile(objList, objList[ObjectIndex].ObjectTileX, objList[ObjectIndex].ObjectTileY, ObjectInteraction.BRIDGE);
                        if (BridgeIndex != -1)
                        {//Adjust for possible bridges in this tile. If so the door goes at the bridge height
                            offZ = CalcObjectXYZ(BridgeIndex, 0).y * 100;
                        }


                        switch (objList[ObjectIndex].heading * 45)
                        {//Move the object position so it can located in the right position in the centre of the frame.
                            case ObjectInteraction.HEADINGWEST:
                                {
                                    offY = (objList[ObjectIndex].ObjectTileY * BrushY + DOORWIDTH + ((BrushY - DOORWIDTH) / 2f));
                                    //offY = (((float)(objList[ObjectIndex].tileY)*BrushY) + BrushY + ((BrushY - DOORWIDTH) / 2f)); 
                                    break;
                                }
                            case ObjectInteraction.HEADINGEAST:
                                {
                                    offY = (objList[ObjectIndex].ObjectTileY * BrushY + ((BrushY - DOORWIDTH) / 2f));
                                    //offY = ((float)objList[ObjectIndex].tileY*BrushY + ((BrushY - DOORWIDTH) / 2f)) ;
                                    break;
                                }
                            case ObjectInteraction.HEADINGNORTH:
                                {
                                    offX = (objList[ObjectIndex].ObjectTileX * BrushX + DOORWIDTH + ((BrushX - DOORWIDTH) / 2f));
                                    //offX = ((float)objList[ObjectIndex].tileX*BrushX + DOORWIDTH + ((BrushX - DOORWIDTH) / 2f));
                                    break;
                                }
                            case ObjectInteraction.HEADINGSOUTH:
                                {
                                    offX = (objList[ObjectIndex].ObjectTileX * BrushX + ((BrushX - DOORWIDTH) / 2f));
                                    //offX = ((float)objList[ObjectIndex].tileX*BrushX + ((BrushX - DOORWIDTH) / 2f)) ;
                                    break;
                                }
                        }
                        if (objList[ObjectIndex].xpos == 0) { offX += 2f; }
                        if (objList[ObjectIndex].xpos == 7) { offX -= 2f; }
                        if (objList[ObjectIndex].ypos == 0) { offY += 2f; }
                        if (objList[ObjectIndex].ypos == 7) { offY -= 2f; }
                        break;
                    }
                case ObjectInteraction.A_MOVE_TRIGGER:
                    {
                        if (tileMap.Tiles[x, y].tileType != TileMap.TILE_SOLID)
                        {
                            if (tileMap.Tiles[x, y].TerrainChange == false)
                            {
                                if (objList[ObjectIndex].zpos < tileMap.Tiles[x, y].floorHeight * 4)
                                {
                                    int newZpos = tileMap.Tiles[x, y].floorHeight * 4;
                                    offZ = ((newZpos / ResolutionZ) * (ceil)) * BrushZ;
                                }
                            }
                        }
                        break;
                    }
                case ObjectInteraction.BUTTON:
                case ObjectInteraction.SIGN:
                    {//TODO: make this based on heading so as to support angled walls									
                        if (objList[ObjectIndex].xpos == 0) { offX += 1.5f; }
                        if (objList[ObjectIndex].xpos == 7) { offX -= 1.5f; }
                        if (objList[ObjectIndex].ypos == 0) { offY += 1.5f; }
                        if (objList[ObjectIndex].ypos == 7) { offY -= 1.5f; }
                        if (zpos == 127)
                        {
                            offZ -= 25f;
                        }
                        break;
                    }



                default:
                    {
                        if (WallAdjust == 1)
                        {//Adjust the object x,y to avoid clipping into walls.
                            switch (_RES)
                            {
                                case GAME_SHOCK:
                                    if (objList[ObjectIndex].xpos == 0) { offX += 4f; }
                                    if (objList[ObjectIndex].xpos == 128) { offX -= 4f; }
                                    if (objList[ObjectIndex].ypos == 0) { offY += 4f; }
                                    if (objList[ObjectIndex].ypos == 128) { offY -= 3f; }
                                    break;
                                default:
                                    if (objList[ObjectIndex].xpos == 0) { offX += 4f; }
                                    if (objList[ObjectIndex].xpos == 7) { offX -= 4f; }
                                    if (objList[ObjectIndex].ypos == 0) { offY += 4f; }
                                    if (objList[ObjectIndex].ypos == 7) { offY -= 4f; }
                                    break;
                            }
                        }
                        break;
                    }

            }
        }




        return new Vector3(offX / 100.0f, offZ / 100.0f, offY / 100.0f);
    }







    /// <summary>
    /// Renders the object list stored on this class
    /// </summary>
    public static void RenderObjectList(ObjectLoader instance, TileMap tilemap, GameObject parent)
    {
        GameWorldController.LoadingObjects = true;
        //Clear out the children in the transform
        foreach (Transform child in parent.transform)
        {
            Object.Destroy(child.gameObject);
        }
        //Init the objects
        //instance.ObjectInteractions=new ObjectInteraction[1600];

        for (int i = 0; i <= instance.objInfo.GetUpperBound(0); i++)
        {
            if (instance.objInfo[i] != null)
            {
                //if ((instance.objInfo[i].InUseFlag == 1))   //|| (UWEBase.EditorMode)
                if (true)
                {
                    Vector3 position;
                    if (tilemap == null)
                    {
                        position = new Vector3(TileMap.ObjectStorageTile * 1.2f, 5f, TileMap.ObjectStorageTile * 1.2f);
                    }
                    else
                    {
                        position = CalcObjectXYZ(i, 1);
                    }

                    instance.objInfo[i].instance = ObjectInteraction.CreateNewObject(tilemap, instance.objInfo[i], instance.objInfo, parent, position);
                }
            }
        }

        LinkObjectListWands(instance);//Link wands to their spell objects
        LinkObjectListPotions(instance);

        GameWorldController.LoadingObjects = false;
    }


    /// <summary>
    /// Gets the object at index on the current level
    /// </summary>
    /// <returns>The <see cref="ObjectLoaderInfo"/>.</returns>
    /// <param name="index">Index.</param>
    public static ObjectLoaderInfo getObjectInfoAt(int index)
    {
        return CurrentObjectList().objInfo[index];
    }


    public static ObjectLoaderInfo getObjectInfoAt(int index, ObjectLoader objList)
    {
        //return CurrentObjectList().objInfo[index];
        return objList.objInfo[index];
    }


    /// <summary>
    /// Gets the object int at index.
    /// </summary>
    /// <returns>The <see cref="ObjectLoaderInfo"/>.</returns>
    /// <param name="index">Index.</param>
    public static ObjectInteraction getObjectIntAt(int index)
    {
        /*	if (CurrentObjectList().objInfo[index].instance==null)
            {
                    Debug.Log("Attempt to get null instance of object at " + index);
            }*/
        return CurrentObjectList().objInfo[index].instance;
    }

    /// <summary>
    /// Gets the object masters item type for the index.
    /// </summary>
    /// <param name="index">Index.</param>
    public static int GetItemTypeAt(int index)
    {
        return getObjectInfoAt(index).GetItemType();
    }

    /// <summary>
    /// Gets the item type at index and objList.
    /// </summary>
    /// <returns>The <see cref="System.Int32"/>.</returns>
    /// <param name="index">Index.</param>
    /// <param name="objList">Object list.</param>
    public static int GetItemTypeAt(int index, ObjectLoader objList)
    {
        return getObjectInfoAt(index, objList).GetItemType();
    }


    /// <summary>
    /// Gets the game object at index.
    /// </summary>
    /// <returns>The <see cref="UnityEngine.GameObject"/>.</returns>
    /// <param name="index">Index.</param>
    public static GameObject getGameObjectAt(int index)
    {
        if (CurrentObjectList().objInfo[index].instance != null)
        {
            return CurrentObjectList().objInfo[index].instance.gameObject;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Refreshes the data in the object list raw data to correctly store links(to containers/spells) and link container items.
    /// </summary>
    public static void RebuildObjectListUW(TileMap currTileMap, ObjectLoader currObjList)
    {

        Debug.Log("RebuildObjectListUW() This needs to be rewritten or made obsolete");

        //////////////////return;
        //////////////////if (currObjList == null) { return; }
        ////////////////////int[,] nexts = new int[64, 64]; //What was the last object found at this tile for next assignments.

        ////////////////////Update indices to match the array.
        //////////////////for (int i = 0; i <= currObjList.objInfo.GetUpperBound(0); i++)
        //////////////////{
        //////////////////    if ((_RES == GAME_UW2) && (i > 2))
        //////////////////    {
        //////////////////        ObjectLoaderInfo.CleanUp(currObjList.objInfo[i]);
        //////////////////    }
        //////////////////    //bool OnMap = currObjList.objInfo[i].ObjectTileX != TileMap.ObjectStorageTile;
        //////////////////    //if ((OnMap))
        //////////////////    //{//Only clear nexts if the object is onmap. Items destroyed will have their nexts cleared at that time.
        //////////////////    //    currObjList.objInfo[i].next = 0;
        //////////////////    //    if (currObjList.objInfo[i].instance != null)
        //////////////////    //    {
        //////////////////    //        currObjList.objInfo[i].instance.next = 0;
        //////////////////    //    }
        //////////////////    //}
        //////////////////}

        ////////////////////if (currTileMap != null)
        ////////////////////{
        ////////////////////    //Clear the tilemaps indexobjectlist
        ////////////////////    for (int x = 0; x <= TileMap.TileMapSizeX; x++)
        ////////////////////    {
        ////////////////////        for (int y = 0; y <= TileMap.TileMapSizeY; y++)
        ////////////////////        {
        ////////////////////            currTileMap.Tiles[x, y].indexObjectList = 0;
        ////////////////////        }
        ////////////////////    }
        ////////////////////}

        //////////////////foreach (Transform t in GameWorldController.instance.DynamicObjectMarker())
        //////////////////{
        //////////////////    if (t.gameObject.GetComponent<ObjectInteraction>() != null)
        //////////////////    {
        //////////////////        t.gameObject.GetComponent<ObjectInteraction>().OnSaveObjectEvent();
        //////////////////    }
        //////////////////}

        ////////////////////foreach (Transform t in GameWorldController.instance.DynamicObjectMarker())
        ////////////////////{
        ////////////////////    ObjectInteraction objInt = t.gameObject.GetComponent<ObjectInteraction>();
        ////////////////////    if (objInt != null)
        ////////////////////    {                
        ////////////////////        if (objInt.BaseObjectData == null)
        ////////////////////        {//Fill in missing object loader instances to avoid null errors
        ////////////////////            objInt.BaseObjectData = new ObjectLoaderInfo(objInt.BaseObjectData.index,GameWorldController.CurrentTileMap(),true);
        ////////////////////            objInt.BaseObjectData.InUseFlag = 0;
        ////////////////////            objInt.BaseObjectData.ObjectTileX = TileMap.ObjectStorageTile;
        ////////////////////            objInt.BaseObjectData.ObjectTileY = TileMap.ObjectStorageTile;
        ////////////////////        }
        ////////////////////        objInt.UpdatePosition(); //Update the coordinates and tile x and y of the object
        ////////////////////        if ((t.gameObject.GetComponent<Container>() != null))
        ////////////////////        {//Clear container link index. Will repoint later.
        ////////////////////            t.gameObject.GetComponent<ObjectInteraction>().link = 0;//TEST 

        ////////////////////        }
        ////////////////////    }
        ////////////////////}

        //////////////////////rebuild the linked list
        ////////////////////for (int i = 0; i <= currObjList.objInfo.GetUpperBound(0); i++)
        ////////////////////{
        ////////////////////    int x = currObjList.objInfo[i].ObjectTileX;
        ////////////////////    int y = currObjList.objInfo[i].ObjectTileY;
        ////////////////////    if (currObjList.objInfo[i].InUseFlag == 1)
        ////////////////////    {
        ////////////////////        if ((x != TileMap.ObjectStorageTile) && (y != TileMap.ObjectStorageTile))
        ////////////////////        {
        ////////////////////            if (nexts[x, y] == 0)
        ////////////////////            {//This object is the first in the chain at this tile
        ////////////////////                currTileMap.Tiles[x, y].indexObjectList = i;
        ////////////////////                nexts[x, y] = i;
        ////////////////////            }
        ////////////////////            else
        ////////////////////            {
        ////////////////////                currObjList.objInfo[nexts[x, y]].next = i;
        ////////////////////                currObjList.objInfo[nexts[x, y]].instance.next = i;
        ////////////////////                nexts[x, y] = i;
        ////////////////////            }
        ////////////////////        }
        ////////////////////    }
        ////////////////////}

        ////////////////////Rebuild container chains only!

        ////////////////////TODO:

        //////////////////////return;

        ////////////////////////Count the number of freeobjects in the mobile and static lists and update these lists as needed			
        //////////////////////currObjList.FreeMobileList = new int[254];
        //////////////////////currObjList.FreeStaticList = new int[768];
        //////////////////////int newFreeMobileObjectCount = 0;
        //////////////////////int newFreeStaticObjectCount = 0;
        //////////////////////for (int o = 2; o < 256; o++)
        //////////////////////{
        //////////////////////    if (currObjList.objInfo[o].InUseFlag == 0)
        //////////////////////    {//Store that the object slot is free in the array.			
        //////////////////////        currObjList.objInfo[o].instance = null;
        //////////////////////        currObjList.FreeMobileList[newFreeMobileObjectCount++] = o;
        //////////////////////    }
        //////////////////////}
        //////////////////////for (int o = 256; o <= currObjList.objInfo.GetUpperBound(0); o++)
        //////////////////////{
        //////////////////////    if (currObjList.objInfo[o].InUseFlag == 0)
        //////////////////////    {//Store that the object slot is free in the array.	
        //////////////////////        currObjList.objInfo[o].instance = null;
        //////////////////////        currObjList.FreeStaticList[newFreeStaticObjectCount++] = o;
        //////////////////////    }
        //////////////////////}

        //////////////////////for (int o = 2; o < currObjList.objInfo.GetUpperBound(0); o++)
        //////////////////////{
        //////////////////////    if (currObjList.objInfo[o].instance != null)
        //////////////////////    {
        //////////////////////        currObjList.CopyDataToList(currObjList.objInfo[o].instance, ref currObjList.objInfo[o]);
        //////////////////////    }
        //////////////////////}

        //////////////////////if (newFreeMobileObjectCount > 0)
        //////////////////////{
        //////////////////////    newFreeMobileObjectCount--;
        //////////////////////}
        //////////////////////if (newFreeStaticObjectCount > 0)
        //////////////////////{
        //////////////////////    newFreeStaticObjectCount--;
        //////////////////////}
        ////////////////////////Debug.Log(
        ////////////////////////		" Mobile was " + currObjList.NoOfFreeMobile + " now " + newFreeMobileObjectCount +
        ////////////////////////		" Static was " + currObjList.NoOfFreeStatic + " now " + newFreeStaticObjectCount 				
        ////////////////////////);
        //////////////////////currObjList.NoOfFreeMobile = newFreeMobileObjectCount;
        //////////////////////currObjList.NoOfFreeStatic = newFreeStaticObjectCount;



    }




    /// <summary>
    /// Creates the new object list with just the items in the object marker.
    /// </summary>
    /// <returns>The inventory object list.</returns>
    public static string[] UpdateInventoryObjectList(out int NoOfInventoryItems)
    {
        PlayerInventory pInv = UWCharacter.Instance.playerInventory;
        ObjectInteraction Prev = null;
        foreach (Transform child in GameWorldController.instance.InventoryMarker.transform)
        {
            if (child != null)
            {
                //Initialise the next value to zero.
                child.gameObject.GetComponent<ObjectInteraction>().next = 0;
            }
        }
        //Store the objects in an array to give them indices
        var InventoryObjectsList = new List<string>();
        //First add the objects on the paperdoll to the so that they are first.
        for (short s = 0; s <= 18; s++)
        {
            ObjectInteraction obj;
            if (s <= 10)//Paperdoll objects
            {
                obj = pInv.GetObjectIntAtSlot(s);
            }
            else
            {
                obj = pInv.playerContainer.GetItemAt((short)(s - 11));
            }
            if (obj != null)
            {
                InventoryObjectsList.Add(obj.name);
            }
        }
        foreach (Transform child in GameWorldController.instance.InventoryMarker.transform)
        {
            if (child != null)
            {
                if (!InventoryObjectsList.Contains(child.name))
                {
                    InventoryObjectsList.Add(child.name);
                }
            }
        }
        var InventoryObjects = InventoryObjectsList.ToArray();
        NoOfInventoryItems = InventoryObjects.Length;

        //int itemIndex=0;
        //Go through each slot

        for (int s = 0; s <= 18; s++)
        {
            ObjectInteraction obj;
            if (s <= 10)//Paperdoll objects
            {
                obj = pInv.GetObjectIntAtSlot(s);
            }
            else
            {
                obj = pInv.playerContainer.GetItemAt((short)(s - 11));
            }
            if (obj != null)
            {
                if (obj.GetComponent<Container>() != null)
                {
                    linkInventoryContainers(obj.GetComponent<Container>(), ref InventoryObjects);
                }
                if (Prev != null)
                {
                    Prev.GetComponent<ObjectInteraction>().next = (short)(System.Array.IndexOf(InventoryObjects, obj.name) + 1);
                }
                Prev = obj;
            }
        }
        return InventoryObjects;
    }


    /// <summary>
    /// Links the inventory list containers on the player. Recursive
    /// </summary>
    /// <param name="cn">Cn.</param>
    /// <param name="InventoryObjects">Inventory objects.</param>
    static void linkInventoryContainers(Container cn, ref string[] InventoryObjects)
    {
        bool cnLinked = false;
        cn.gameObject.GetComponent<ObjectInteraction>().link = 0;//Initially unlinked.
        int index = System.Array.IndexOf(InventoryObjects, cn.name) + 1;
        ObjectInteraction prev = null;

        //For each spot in the container
        for (short i = 0; i <= cn.MaxCapacity(); i++)
        {
            //Get the item name in the container
            ObjectInteraction item = cn.GetItemAt(i);
            if (item != null)
            {
                //Get the object
                //GameObject obj = GameObject.Find(itemname);
                //if (obj != null)
                //{
                index = System.Array.IndexOf(InventoryObjects, item.name) + 1;
                if (cnLinked == false)
                {//The container is first linked to this object							
                    cn.gameObject.GetComponent<ObjectInteraction>().link = (short)index;
                    cnLinked = true;
                    prev = item;
                }
                else
                {//The object needs to be a next of the previous object
                 //index= System.Array.IndexOf(InventoryObjects,obj.name);
                    prev.GetComponent<ObjectInteraction>().next = (short)index;
                    prev = item;
                }
                if (item.GetComponent<Container>() != null)
                {//if a container then link the items in that container.
                    linkInventoryContainers(item.GetComponent<Container>(), ref InventoryObjects);
                }
                // }
            }
        }
    }


    /// <summary>
    /// Re-Links the container contents when saving or leaving a level
    /// </summary>
    /// <param name="cn">Cn.</param>
    static void linkContainerContents(Container cn)
    {
        if (cn == null)
        {
            Debug.Log("Null container in LinkContainerContents");
            return;
        }
        int itemCounter = 0;
        ObjectInteraction cnObjInt = cn.gameObject.GetComponent<ObjectInteraction>();
        int PrevIndex = cnObjInt.ObjectIndex;
        if (cn.LockObject != 0)
        {
            ObjectInteraction lockObj = getObjectIntAt(cn.LockObject);
            if (lockObj != null)
            {
                if (lockObj.GetItemType() == ObjectInteraction.LOCK)
                {
                    itemCounter++;
                    cnObjInt.link = lockObj.ObjectIndex;
                    cnObjInt.link = lockObj.ObjectIndex;
                    PrevIndex = lockObj.ObjectIndex;
                }
            }
        }

        for (short i = 0; i < cn.GetCapacity(); i++)
        {
            ObjectInteraction itemObjInt = cn.GetItemAt(i);
            if (itemObjInt != null)
            {
                //ObjectInteraction itemObjInt = obj.GetComponent<ObjectInteraction>();
                if (itemCounter == 0)
                {//First item link to it from the container
                    cnObjInt.link = itemObjInt.ObjectIndex;
                    cnObjInt.link = itemObjInt.ObjectIndex;
                    PrevIndex = itemObjInt.ObjectIndex;
                }
                else
                {//the previous items next becomes this.
                    if (itemObjInt == null)
                    {
                        Debug.Log("null object on " + i + " for container " + cn.name);
                    }
                    getObjectIntAt(PrevIndex).next = itemObjInt.ObjectIndex;
                    getObjectIntAt(PrevIndex).next = itemObjInt.ObjectIndex;
                    itemObjInt.next = 0;//end for now.
                    itemObjInt.next = 0;
                    PrevIndex = itemObjInt.ObjectIndex;
                }
                itemCounter++;

                //If a container then link its contents as well
                //Is this still needed here????
                //TEST	if (obj.GetComponent<Container>()!=null)
                //TEST	{//Rebuild container chain
                //TESTif (obj!=cn.gameObject)
                //TEST{
                //TESTlinkContainerContents(obj.GetComponent<Container>());	
                //TEST	}						
                //TEST}
            }
        }
    }

    /// <summary>
    /// Gets the object index currently at the tile.
    /// </summary>
    /// <returns>The tile index next.</returns>
    /// <param name="tileX">Tile x.</param>
    /// <param name="tileY">Tile y.</param>
    public static int GetTileIndexNext(int tileX, int tileY)
    {
        return CurrentTileMap().Tiles[tileX, tileY].indexObjectList;
    }


    /// <summary>
    /// Assigns the object to the object list and returns the index it is at.
    /// </summary>
    /// <returns>The object to list.</returns>
    /// <param name="objInt">Object int.</param>
    public static int AssignObjectToStaticList(ref ObjectInteraction objInt)
    {
        //int startindex = 1;
        ////Check if objINt is an npc
        //if ((objInt.GetComponent<NPC>() == null))
        //{
        //    startindex = 256;//start of static list
        //                     //	startindex=979;
        //}
        //find a free slot in the list.
        if (CurrentObjectList().GetFreeStaticObject(out short index))
        //if (CurrentObjectList().getFreeSlot(startindex, out int index))
        {
            //Assign and return the reference
            //objInt.BaseObjectData = CurrentObjectList().objInfo[index];
            objInt.AssignBaseObjectData(CurrentObjectList().objInfo[index], index);
            //objInt.BaseObjectData.InUseFlag = 1;
           // objInt.BaseObjectData.index = (short)index;

            if ((objInt.GetComponent<Container>()) || (objInt.GetComponent<NPC>()))
            {//Put the container items back into the list as well
                Container cn = objInt.GetComponent<Container>();
                int itemCounter = 0;
                int prevLink = index;
                for (short i = 0; i <= cn.GetCapacity(); i++)
                {
                    ObjectInteraction objI = cn.GetItemAt(i);
                    if (objI != null)
                    {
                        
                        int newlink = AssignObjectToStaticList(ref objI);
                        if (itemCounter == 0)
                        {//First object											
                            objInt.link = (short)newlink;
                            prevLink = newlink;
                        }
                        else
                        {
                            CurrentObjectList().objInfo[prevLink].next = (short)newlink;
                            prevLink = newlink;

                        }
                        objI.next = 0;//init
                        itemCounter++;
                    }
                }
                if (itemCounter == 0)
                {
                    objInt.link = 0;//No contents
                }
            }
            //CurrentObjectList().CopyDataToList(objInt, ref objInt.BaseObjectData);
        }
        else
        {
            Debug.Log("Unable to assign object to list " + objInt.name);
        }
        return index;
    }

    ///// <summary>
    ///// Gets the free slot available to use starting from the specified index.
    ///// </summary>
    ///// <returns>The free slot.</returns>
    //public bool getFreeSlot(int startIndex, out int index)
    //{
       
    //    if (startIndex < 2)
    //    {
    //        startIndex = 2;
    //    }
    //    for (int i = startIndex; i <= objInfo.GetUpperBound(0); i++)
    //    {
    //        //if (objInfo[i].InUseFlag == 0)
    //       // {
    //            index = i;
    //            return true;
    //       // }
    //    }
    //    index = -1;
    //    return false;
    //}


    void CopyDataToList(ObjectInteraction objInt, ref ObjectLoaderInfo info)
    {
        Debug.Log("copy data to list");
        //////////return;
        //////////info.item_id = objInt.item_id;
        //////////info.flags = objInt.flags;  //9-12
        //////////info.enchantment = objInt.enchantment;  //12
        //////////info.doordir = objInt.doordir;  //13
        //////////info.invis = objInt.invis;      //14
        //////////info.is_quant = objInt.isquant; //15						
        //////////info.zpos = objInt.zpos;    //  0- 6   7   "zpos"      Object Z position (0-127)
        //////////info.heading = objInt.heading;  //        7- 9   3   "heading"   Heading (*45 deg)
        //////////info.xpos = objInt.xpos; //   10-12   3   "ypos"      Object Y position (0-7)
        //////////info.ypos = objInt.ypos; //  13-15   3   "xpos"      Object X position (0-7)
        //////////info.quality = objInt.quality;  //;     0- 5   6   "quality"   Quality
        //////////info.next = objInt.next; //    6-15   10  "next"      Index of next object in chain
        //////////info.owner = objInt.owner;  //Also special
        //////////info.link = objInt.link;    //also quantity	

        //////////info.ObjectTileX = objInt.ObjectTileX;
        //////////info.ObjectTileY = objInt.ObjectTileY;

        ////////////mobile object specific
        //////////if (info.index < 256)
        //////////{
        //////////    info.npc_hp = objInt.npc_hp;
        //////////    info.npc_goal = objInt.npc_goal;
        //////////    info.npc_gtarg = objInt.npc_gtarg;
        //////////    info.npc_level = (short)objInt.npc_level;
        //////////    info.npc_talkedto = objInt.npc_talkedto;
        //////////    info.npc_attitude = objInt.npc_attitude;
        //////////    info.npc_voidanim = objInt.npc_voidanim;
        //////////    info.npc_yhome = objInt.npc_yhome;
        //////////    info.npc_xhome = objInt.npc_xhome;
        //////////    info.npc_heading = objInt.npc_heading;
        //////////    info.npc_hunger = objInt.npc_hunger;
        //////////    info.npc_whoami = objInt.npc_whoami;
        //////////    info.npc_health = objInt.npc_health;
        //////////    info.npc_arms = objInt.npc_arms;
        //////////    info.npc_power = objInt.npc_power;
        //////////    info.npc_name = objInt.npc_name;
        //////////    info.npc_health = objInt.npc_height;
        //////////    info.Projectile_Pitch = objInt.Projectile_Pitch;
        //////////    info.Projectile_Speed = objInt.Projectile_Speed;
        //////////    //info.ProjectileHeadingMinor = objInt.ProjectileHeadingMinor;

        //////////    //info.ProjectileHeadingMajor = objInt.ProjectileHeadingMajor;
        //////////    info.MobileUnk_0xA = objInt.MobileUnk_0xA;
        //////////    info.MobileUnk_0xB_12_F = objInt.MobileUnk_0xB_12_F;
        //////////    info.MobileUnk_0xD_4_FF = objInt.MobileUnk_0xD_4_FF;
        //////////    info.MobileUnk_0xD_12_1 = objInt.MobileUnk_0xD_12_1;
        //////////    info.MobileUnk_0xF_0_3F = objInt.MobileUnk_0xF_0_3F;
        //////////    info.MobileUnk_0xF_C_F = objInt.MobileUnk_0xF_C_F;
        //////////    info.MobileUnk_0x11 = objInt.MobileUnk_0x11;
        //////////    info.ProjectileSourceID = objInt.ProjectileSourceID;
        //////////    info.MobileUnk_0x13 = objInt.MobileUnk_0x13;
        //////////    //info.Projectile_Sign = objInt.Projectile_Sign;
        //////////    info.MobileUnk_0x15_4_1F = objInt.MobileUnk_0x15_4_1F;
        //////////    info.MobileUnk_0x16_0_F = objInt.MobileUnk_0x16_0_F;
        //////////    info.MobileUnk_0x18_5_7 = objInt.MobileUnk_0x18_5_7;
        //////////    info.MobileUnk_0x19_6_3 = objInt.MobileUnk_0x19_6_3;
        //////////}


        ////////////Match the two instances
        //////////info.instance = objInt;
        //////////objInt.BaseObjectData = info;
    }


    /// <summary>
    /// Finds and returns the next available free slot for a new object and returns the objectloaderinfo for the found object with initial properties.
    /// </summary>
    /// <returns>The object.</returns>
    public static ObjectLoaderInfo newWorldObject(int item_id, short quality, short owner, short link, int startIndex)
    {
        short index = 0;
        if (startIndex >= 0)
        {
            bool SlotAvailable;
            if (startIndex >= 256)//Static
            {
                SlotAvailable = CurrentObjectList().GetFreeStaticObject(out index);
                index = CurrentObjectList().GetStaticAtSlot(index);
            }
            else
            {//mobile
                SlotAvailable = CurrentObjectList().GetFreeMobileObject(out index);
                index = CurrentObjectList().GetMobileAtSlot(index);
            }
           // Debug.Log("Allocating Slot " + index + " for newObject()");
            if (SlotAvailable)
            {
                CurrentObjectList().objInfo[index].guid = System.Guid.NewGuid();
                CurrentObjectList().objInfo[index].quality = quality;
                CurrentObjectList().objInfo[index].flags = 0;
                CurrentObjectList().objInfo[index].owner = owner;
                CurrentObjectList().objInfo[index].item_id = item_id;
                CurrentObjectList().objInfo[index].next = 0;
                CurrentObjectList().objInfo[index].link = link;
                CurrentObjectList().objInfo[index].zpos = 0;
                CurrentObjectList().objInfo[index].xpos = 0;
                CurrentObjectList().objInfo[index].ypos = 0;
                CurrentObjectList().objInfo[index].invis = 0;
                CurrentObjectList().objInfo[index].doordir = 0;
                CurrentObjectList().objInfo[index].is_quant = 0;
                CurrentObjectList().objInfo[index].enchantment = 0;
                CurrentObjectList().objInfo[index].ObjectTileX = TileMap.ObjectStorageTile;
                CurrentObjectList().objInfo[index].ObjectTileY = TileMap.ObjectStorageTile;
                CurrentObjectList().objInfo[index].index = index;
                return CurrentObjectList().objInfo[index];
            }
        }
        else
        {
            Debug.Log("New Object created. Shuld not happen!");
            ObjectLoaderInfo objI = new ObjectLoaderInfo(index, UWEBase.CurrentTileMap(), true)
            {
                //objI.guid = System.Guid.NewGuid();
                quality = (short)quality,
                flags = 0,
                owner = (short)owner,
                item_id = item_id,
                next = 0,
                link = link,
                zpos = 0,
                xpos = 0,
                ypos = 0,
                invis = 0,
                doordir = 0,
                is_quant = 0,
                enchantment = 0,
                ObjectTileX = TileMap.ObjectStorageTile,
                ObjectTileY = TileMap.ObjectStorageTile,
                //InUseFlag = 1,
                index = index
            };
            return objI;

        }

        return null;
    }




    void replaceLink(ref xrefTable[] xref, int tableSize, int indexToFind, int linkToReplace)
    {//Replaces a link in the Sytem Shock linked list when removing duplicate items that are in two tiles per the xref/master table reconciliaton.
        if ((indexToFind == 0) && (linkToReplace == 0))
        { return; }
        for (int i = 0; i < tableSize; i++)
        {
            if (xref[i].next == indexToFind)
            {
                xref[i].next = linkToReplace;
            }
        }
    }

    void replaceMapLink(ref TileInfo[,] levelInfo, int indexToFind, int linkToReplace)
    {//Replaces the specified object index (into tile) if it is a duplicate per the xref/master table reconciliaton.
        if ((indexToFind == 0) && (linkToReplace == 0))
        { return; }

        for (int x = 0; x <= TileMap.TileMapSizeX; x++)
        {
            for (int y = 0; y <= TileMap.TileMapSizeY; y++)
            {
                {
                    if (levelInfo[x, y].indexObjectList == indexToFind)
                    {
                        levelInfo[x, y].indexObjectList = linkToReplace;
                    }
                }
            }
        }
    }




    int getShockObjectIndex(int objClass, int objSubClass, int objSubClassIndex)
    {//Find the specified object in the array of shock objectmasters

        for (int i = 0; i <= GameWorldController.instance.objectMaster.objProp.GetUpperBound(0); i++)
        {

            if (
                (GameWorldController.instance.objectMaster.objProp[i].objClass == objClass)
                &&
                (GameWorldController.instance.objectMaster.objProp[i].objSubClass == objSubClass)
                &&
                (GameWorldController.instance.objectMaster.objProp[i].objSubClassIndex == objSubClassIndex)
            )
            {
                return i;
            }
        }
        return -1;  //not found
    }





    bool lookUpSubClass(byte[] archive_ark, TileInfo[,] LevelInfo, int BlockNo, int ClassType, int index, int RecordSize, xrefTable[] xRef, ObjectLoaderInfo[] objList, short[] texture_map, int objIndex, short levelNo)
    {
        //

        //unsigned char *sub_ark ;
        //long chunkUnpackedLength;
        //long chunkType;//compression type
        //long chunkPackedLength;

        //long chunkUnpackedLength =LoadShockChunk(filePath,BlockNo,sub_ark);

        //long blockAddress =getShockBlockAddress(BlockNo,archive_ark,&chunkPackedLength,&chunkUnpackedLength,&chunkType); 
        //if (blockAddress == -1) {return -1;}
        //sub_ark=new unsigned char[chunkUnpackedLength];
        //fprintf(LOGFILE,"\nLoading Chunk at %d\n",blockAddress);
        //LoadShockChunk(blockAddress,chunkType,archive_ark,sub_ark,chunkPackedLength,chunkUnpackedLength);
        if (!LoadChunk(archive_ark, BlockNo, out Chunk sub_ark))
        {
            return false;
        }

        int k = 0;
        int add_ptr = 0;

        while (k <= sub_ark.chunkUnpackedLength)
        {
            if (k == index)
            {
                switch (ClassType)
                {
                    case SOFTWARE_LOGS:
                        {
                            objList[objIndex].shockProperties[SOFT_PROPERTY_VERSION] = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 8);  //Software version
                            objList[objIndex].shockProperties[SOFT_PROPERTY_LOG] = (int)getValAtAddress(sub_ark.data, add_ptr + 7, 8) + 2488;   //Chunk containing log
                            objList[objIndex].shockProperties[SOFT_PROPERTY_LEVEL] = (int)getValAtAddress(sub_ark.data, add_ptr + 8, 8);    //Level No of Chunk
                                                                                                                                            //fprintf(LOGFILE,"\tSoftware Properties\n");
                                                                                                                                            //fprintf(LOGFILE,"\t\tVersion %d",objList[objIndex].shockProperties[SOFT_PROPERTY_VERSION]);
                                                                                                                                            //fprintf(LOGFILE,"\tLog Chunk %d",objList[objIndex].shockProperties[SOFT_PROPERTY_LOG]);
                                                                                                                                            //fprintf(LOGFILE,"\tLevel %d",objList[objIndex].shockProperties[SOFT_PROPERTY_LEVEL]);
                            return true;
                            //break;
                        }
                    case FIXTURES:
                        {
                            //fprintf(LOGFILE,"\tFixture Properties\n");
                            switch (objList[objIndex].ObjectSubClass)
                            {
                                case 0://regular fixtures
                                case 1:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    //fprintf(LOGFILE,"\n\tVal 0x6: %d", getValAtAddress(sub_ark.data, add_ptr + 6, 16));
                                    //fprintf(LOGFILE,"\n\tVal 0x8: %d", getValAtAddress(sub_ark.data, add_ptr + 8, 16));
                                    //fprintf(LOGFILE,"\n\tVal 0xA: %d", getValAtAddress(sub_ark.data, add_ptr + 0xA, 16));
                                    //fprintf(LOGFILE,"\n\tVal 0xC: %d", getValAtAddress(sub_ark.data, add_ptr + 0xC, 16));
                                    //fprintf(LOGFILE,"\n\tVal 0xE: %d", getValAtAddress(sub_ark.data, add_ptr + 0xE, 16));
                                    break;
                                case 2:
                                    switch (objList[objIndex].ObjectSubClassIndex)
                                    {
                                        case 0://SIGN
                                        case 1://ICON
                                        case 2://GRAFFITI
                                        case 4://painting
                                        case 5://poster
                                               //fprintf(LOGFILE,"\n\tImage to use is value in unk1. Offset from image 1350_0390.bmp or 1350_0403.bmp in objart.res or 0078_0000.bmp or 0079_0000 in objart3.res");
                                            break;
                                        case 3:
                                            {
                                                //based on SSHP interpretation
                                                int[] fontID = { 4, 7, 0, 10 };
                                                //float[] scale= { 1.0f, 0.75f, 0.5f, 0.25f };
                                                //fprintf(LOGFILE,"Words:");
                                                objList[objIndex].shockProperties[WORDS_STRING_NO] = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 16);
                                                //fprintf(LOGFILE,"\nSub chunk %d (from chunk 2152)", getValAtAddress(sub_ark.data, add_ptr + 6, 16));
                                                int FontNSize = (int)getValAtAddress(sub_ark.data, add_ptr + 8, 16);
                                                //fprintf(LOGFILE,"\nFont %d (+chunk 602)", fontID[FontNSize & 0x03]);
                                                objList[objIndex].shockProperties[WORDS_FONT] = fontID[FontNSize & 0x03] + 602;
                                                //fprintf(LOGFILE,"\nSize %d ", fontID[FontNSize>>4 & 0x03]);
                                                objList[objIndex].shockProperties[WORDS_SIZE] = fontID[FontNSize >> 4 & 0x03];
                                                //fprintf(LOGFILE,"\nColour %d ", getValAtAddress(sub_ark, add_ptr + 0xA, 16));
                                                objList[objIndex].shockProperties[WORDS_COLOUR] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xA, 16);
                                                //fprintf(LOGFILE,"\n\tVal 0x6: %d", getValAtAddress(sub_ark.data, add_ptr + 6, 16));
                                                //fprintf(LOGFILE,"\n\tVal 0x8: %d", getValAtAddress(sub_ark.data, add_ptr + 8, 16));
                                                //fprintf(LOGFILE,"\n\tVal 0xA: %d", getValAtAddress(sub_ark.data, add_ptr + 0xA, 16));
                                                //fprintf(LOGFILE,"\n\tVal 0xC: %d", getValAtAddress(sub_ark.data, add_ptr + 0xC, 16));
                                                //fprintf(LOGFILE,"\n\tVal 0xE: %d", getValAtAddress(sub_ark.data, add_ptr + 0xE, 16));
                                                break;
                                            }
                                        case 6:
                                        case 8:
                                        case 9:
                                            //fprintf(LOGFILE,"Screens:");
                                            objList[objIndex].shockProperties[SCREEN_NO_OF_FRAMES] = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 16);
                                            objList[objIndex].shockProperties[SCREEN_LOOP_FLAG] = (int)getValAtAddress(sub_ark.data, add_ptr + 8, 16);
                                            objList[objIndex].shockProperties[SCREEN_START] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                                            //fprintf(LOGFILE,"\nNo of Frames: %d", objList[objIndex].shockProperties[SCREEN_NO_OF_FRAMES]);
                                            //fprintf(LOGFILE,"\nLoop repeats: %d ", objList[objIndex].shockProperties[SCREEN_LOOP_FLAG]);
                                            //fprintf(LOGFILE,"\nStart Frame: %d (from chunk 321) = %d", objList[objIndex].shockProperties[SCREEN_START], 321 + objList[objIndex].shockProperties[SCREEN_START]);
                                            if ((objList[objIndex].shockProperties[SCREEN_START] >= 248) && (objList[objIndex].shockProperties[SCREEN_START] <= 255))
                                            {//Survellance
                                             //unsigned char *sur_ark;
                                                if (LoadChunk(archive_ark, levelNo * 100 + SURVELLANCE_OFFSET, out Chunk sur_ark))
                                                {

                                                    //fprintf(LOGFILE, "\n\tSurvellance Chunk at %d\n", blockAddress);
                                                    //LoadShockChunk(blockAddress, chunkType, archive_ark, sur_ark, chunkPackedLength, chunkUnpackedLength);
                                                    objList[objIndex].shockProperties[SCREEN_SURVEILLANCE_TARGET] = (int)getValAtAddress(sur_ark.data, (objList[objIndex].shockProperties[SCREEN_START] - 248) * 2, 16);
                                                    //fprintf(LOGFILE, "\tSurveillance item id: %d", objList[objIndex].shockProperties[SCREEN_SURVEILLANCE_TARGET]);

                                                }

                                            }
                                            break;
                                        default:
                                            //fprintf(LOGFILE,"\n\tVal 0x6: %d", getValAtAddress(sub_ark.data, add_ptr + 6, 16));
                                            //fprintf(LOGFILE,"\n\tVal 0x8: %d", getValAtAddress(sub_ark.data, add_ptr + 8, 16));
                                            //fprintf(LOGFILE,"\n\tVal 0xA: %d", getValAtAddress(sub_ark.data, add_ptr + 0xA, 16));
                                            //fprintf(LOGFILE,"\n\tVal 0xC: %d", getValAtAddress(sub_ark.data, add_ptr + 0xC, 16));
                                            //fprintf(LOGFILE,"\n\tVal 0xE: %d", getValAtAddress(sub_ark.data, add_ptr + 0xE, 16));
                                            break;
                                    }
                                    break;
                                case 7: //bridges etc
                                    int val = (int)getValAtAddress(sub_ark.data, add_ptr + 0x8, 8);
                                    objList[objIndex].shockProperties[BRIDGE_X_SIZE] = val & 0xF;
                                    objList[objIndex].shockProperties[BRIDGE_Y_SIZE] = (val >> 4) & 0xF;
                                    objList[objIndex].shockProperties[BRIDGE_HEIGHT] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x9, 8);
                                    val = (int)getValAtAddress(sub_ark.data, add_ptr + 0xA, 8);
                                    objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE_SOURCE] = (val >> 7) & 0x1;
                                    if (objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE_SOURCE] == 1)//Retrieve from texture map
                                    {
                                        objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE] = texture_map[val & 0x7F];
                                    }
                                    else
                                    {
                                        objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE] = val & 0x7F;
                                    }

                                    val = (int)getValAtAddress(sub_ark.data, add_ptr + 0xB, 8);
                                    objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE_SOURCE] = (val >> 7) & 0x1;
                                    if (objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE_SOURCE] == 1)//Retrieve from texture map
                                    {
                                        objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE] = texture_map[val & 0x7F];
                                    }
                                    else
                                    {
                                        objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE] = val & 0x7F;
                                    }
                                    //fprintf(LOGFILE, "\n\tBridge X Size : %d", objList[objIndex].shockProperties[BRIDGE_X_SIZE]);
                                    //fprintf(LOGFILE, "\n\tBridge Y Size : %d", objList[objIndex].shockProperties[BRIDGE_Y_SIZE]);
                                    //fprintf(LOGFILE, "\n\tBridge Height : %d", objList[objIndex].shockProperties[BRIDGE_HEIGHT]);
                                    //fprintf(LOGFILE, "\n\tBridge Top Texture : %d", objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE]);
                                    //fprintf(LOGFILE, "\n\tBridge Top Texture Source : %d", objList[objIndex].shockProperties[BRIDGE_TOP_BOTTOM_TEXTURE_SOURCE]);
                                    //fprintf(LOGFILE, "\n\tBridge Side Texture : %d", objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE]);
                                    //fprintf(LOGFILE, "\n\tBridge Side Texture Source : %d", objList[objIndex].shockProperties[BRIDGE_SIDE_TEXTURE_SOURCE]);

                                    /*fprintf(LOGFILE,"\n\tVal 0x6: %d", getValAtAddress(sub_ark, add_ptr + 6, 16));
    fprintf(LOGFILE,"\n\tVal 0x8: %d", getValAtAddress(sub_ark, add_ptr + 8, 16));
    fprintf(LOGFILE,"\n\tVal 0xA: %d", getValAtAddress(sub_ark, add_ptr + 0xA, 16));
    fprintf(LOGFILE,"(%d", getValAtAddress(sub_ark, add_ptr + 0xA, 8));
    fprintf(LOGFILE,",%d)", getValAtAddress(sub_ark, add_ptr + 0xB, 8));
    fprintf(LOGFILE,"\n\tVal 0xC: %d", getValAtAddress(sub_ark, add_ptr + 0xC, 16));
    fprintf(LOGFILE,"\n\tVal 0xE: %d", getValAtAddress(sub_ark, add_ptr + 0xE, 16));*/
                                    break;
                            }



                            return true;
                            //	break;
                        }
                    case GETTABLES_OTHER:
                        {
                            if (objList[objIndex].item_id == 191)   //Brief case contents
                            {
                                objList[objIndex].shockProperties[CONTAINER_CONTENTS_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x6, 16);
                                objList[objIndex].shockProperties[CONTAINER_CONTENTS_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x8, 16);
                                objList[objIndex].shockProperties[CONTAINER_CONTENTS_3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xA, 16);
                                objList[objIndex].shockProperties[CONTAINER_CONTENTS_4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                                //	fprintf(LOGFILE,"\tContents 1 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_1]);
                                //fprintf(LOGFILE,"\tContents 2 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_2]);
                                //fprintf(LOGFILE,"\tContents 3 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_3]);
                                //fprintf(LOGFILE,"\tContents 4 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_4]);
                            }

                            //for (int j = 0; j < RecordSize; j = j + 2)
                            //{
                            //fprintf(LOGFILE,"j=%d val(16) = %d\n", j, getValAtAddress(sub_ark, add_ptr+ j, 16));
                            //}

                            return true;
                            //break;
                        }
                    case SWITCHES_PANELS:
                        {
                            //fprintf(LOGFILE,"\n\tSwitch Properties\n");
                            //fprintf(LOGFILE,"\t\tSwitch Action?:%d",getValAtAddress(sub_ark.data,add_ptr+6,16));
                            //fprintf(LOGFILE,"\tVariable:%d",getValAtAddress(sub_ark.data,add_ptr+8,16));
                            //fprintf(LOGFILE,"\tFail Message:%d",getValAtAddress(sub_ark.data,add_ptr+10,16));

                            //TODO:Sort out the trigger action variable into a property
                            objList[objIndex].TriggerAction = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 16);
                            getShockButtons(LevelInfo, sub_ark, add_ptr, objList, objIndex);
                            return true;
                            //break;
                        }
                    case DOORS_GRATINGS:
                        {
                            //TODO: Sort out locking state 
                            //fprintf(LOGFILE,"\n\tDoor Properties\n");
                            //	int crossref = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 16);
                            //	if (crossref != 0)
                            //{
                            //		objList[objIndex].SHOCKLocked  = 1;	// = crossref;	
                            //	}
                            //else
                            //{
                            //			objList[objIndex].SHOCKLocked = 0;
                            //	}
                            //	objList[objIndex].link = getValAtAddress(sub_ark.data, add_ptr + 10, 8);	//Link to it's key. reusage from uw.

                            //fprintf(LOGFILE,"\t\tTrigger xref?:%d (%d)", xRef[crossref].MstIndex, crossref);
                            //	fprintf(LOGFILE,"\tMessage:%d", getValAtAddress(sub_ark.data, add_ptr + 8, 16));
                            //	fprintf(LOGFILE,"\tAccess Required:%d", getValAtAddress(sub_ark.data, add_ptr + 0xA, 8));
                            //	fprintf(LOGFILE, "\tOther val:%d\n", getValAtAddress(sub_ark.data, add_ptr + 0xB, 16));
                            return true;
                            //break;
                        }
                    case TRAPS_MARKERS: //and triggers too
                        {
                            //TODO: Fix conditions and trigger once values
                            objList[objIndex].conditions[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 8, 8);
                            objList[objIndex].conditions[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 9, 8);
                            objList[objIndex].conditions[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 10, 8);
                            objList[objIndex].conditions[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 11, 8);
                            objList[objIndex].TriggerOnce = (int)getValAtAddress(sub_ark.data, add_ptr + 7, 8);
                            //objList[objIndex].TriggerOnceGlobal = 0;
                            //fprintf(LOGFILE,"\tConditions: %d",objList[objIndex].conditions[0]);
                            //fprintf(LOGFILE,",%d",objList[objIndex].conditions[1]);
                            //fprintf(LOGFILE,",%d",objList[objIndex].conditions[2]);
                            //fprintf(LOGFILE,",%d\n",objList[objIndex].conditions[3]);
                            //fprintf(LOGFILE,"\tTrigger once: %d\n",objList[objIndex].TriggerOnce);
                            //objList[objIndex].address = blockAddress+add_ptr;
                            //fprintf(LOGFILE,"\tObject is at address %d\n", objList[objIndex].address);
                            if (objList[objIndex].GetItemType() == ObjectInteraction.SHOCK_TRIGGER_REPULSOR)
                            {
                                objList[objIndex].shockProperties[TRIG_PROPERTY_VALUE] = (int)getValAtAddress(sub_ark.data, add_ptr + 21, 8);
                                objList[objIndex].shockProperties[TRIG_PROPERTY_FLAG] = (int)getValAtAddress(sub_ark.data, add_ptr + 24, 8);
                                //fprintf(LOGFILE,"\tRepulsor Upwards Displacement is %d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_VALUE]);
                                if (objList[objIndex].shockProperties[TRIG_PROPERTY_FLAG] == 1)
                                {
                                    //fprintf(LOGFILE,"\tRepulsor is off (%d)\n", objList[objIndex].shockProperties[TRIG_PROPERTY_FLAG]);
                                }
                                else
                                {
                                    //fprintf(LOGFILE,"\tRepulsor is on (%d)\n", objList[objIndex].shockProperties[TRIG_PROPERTY_FLAG]);
                                }
                            }
                            else
                            {
                                getShockTriggerAction(LevelInfo, sub_ark, add_ptr, xRef, objList, objIndex);
                            }


                            return true;
                            //break;
                        }
                    case CONTAINERS_CORPSES:
                        //fprintf(LOGFILE,"\n\tContainer Properties\n");
                        objList[objIndex].shockProperties[CONTAINER_CONTENTS_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x6, 16);
                        objList[objIndex].shockProperties[CONTAINER_CONTENTS_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x8, 16);
                        objList[objIndex].shockProperties[CONTAINER_CONTENTS_3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xA, 16);
                        objList[objIndex].shockProperties[CONTAINER_CONTENTS_4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                        objList[objIndex].shockProperties[CONTAINER_WIDTH] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xE, 8);
                        objList[objIndex].shockProperties[CONTAINER_HEIGHT] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xF, 8);
                        objList[objIndex].shockProperties[CONTAINER_DEPTH] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 8);
                        objList[objIndex].shockProperties[CONTAINER_TOP] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x11, 8);
                        objList[objIndex].shockProperties[CONTAINER_SIDE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 8);
                        //fprintf(LOGFILE,"\tContents 1 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_1]);
                        //fprintf(LOGFILE,"\tContents 2 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_2]);
                        //fprintf(LOGFILE,"\tContents 3 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_3]);
                        //fprintf(LOGFILE,"\tContents 4 : %d\n", objList[objIndex].shockProperties[CONTAINER_CONTENTS_4]);
                        //fprintf(LOGFILE,"\tWidth : %d", objList[objIndex].shockProperties[CONTAINER_WIDTH]);
                        //fprintf(LOGFILE,"\tHeight : %d", objList[objIndex].shockProperties[CONTAINER_HEIGHT]);
                        //fprintf(LOGFILE,"\tDepth : %d\n", objList[objIndex].shockProperties[CONTAINER_DEPTH]);
                        //fprintf(LOGFILE,"\tTop : %d", objList[objIndex].shockProperties[CONTAINER_TOP]);
                        //fprintf(LOGFILE,"\tSide : %d", objList[objIndex].shockProperties[CONTAINER_SIDE]);
                        return true;
                    //break;

                    case CRITTERS:
                        //fprintf(LOGFILE,"\Critter Properties\n");
                        //for (int j = 0; j < RecordSize; j = j + 2)
                        //{
                        //fprintf(LOGFILE,"\tj=%d val(16) = %d\n", j, getValAtAddress(sub_ark.data, add_ptr + j, 16));
                        //}
                        return true;
                        //break;
                }
            }
            add_ptr += RecordSize;
            k++;
        }
        return false;
    }







    void getShockTriggerAction(TileInfo[,] LevelInfo, Chunk sub_ark, int add_ptr, xrefTable[] xRef, ObjectLoaderInfo[] objList, int objIndex)
    {
        //Look up what a trigger does in system shock. Different trigger types activate other triggers/ do things in different ways.
        //short PrintDebug = 1;// (objList[objIndex].item_id == 384);
        //fprintf(LOGFILE,"",UniqueObjectName(objList[objIndex]));	//Weirdness with garbage info getting printed out?
        int TriggerType = (int)getValAtAddress(sub_ark.data, add_ptr + 6, 8);
        objList[objIndex].TriggerAction = TriggerType;
        switch (TriggerType)
        {
            case ObjectInteraction.ACTION_DO_NOTHING:
                {//Default action.
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_DO_NOTHING or default for %s\n",UniqueObjectName(objList[objIndex]));
                            //DebugPrintTriggerVals(sub_ark, add_ptr,28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }	*/
                    break;
                }
            case ObjectInteraction.ACTION_TRANSPORT_LEVEL:
                {//Sends the player to the specified position in the level.

                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X] = (int)getValAtAddress(sub_ark.data, add_ptr + 12, 16);   //Target X of teleport
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y] = (int)getValAtAddress(sub_ark.data, add_ptr + 16, 16); //Target Y of teleport
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Z] = (int)getValAtAddress(sub_ark.data, add_ptr + 20, 16);   //Target Z of teleport

                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_TRANSPORT_LEVEL for %s\n",UniqueObjectName(objList[objIndex]));
                            //fprintf(LOGFILE,"\t\tDestination (%d,%d,%d)\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X], objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y],objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Z] );
                            //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }	*/
                    break;
                }
            case ObjectInteraction.ACTION_RESURRECTION:
                {//Brings the player back to life?
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_RESURRECTION for %s\n",UniqueObjectName(objList[objIndex]));
                            //DebugPrintTriggerVals(sub_ark, add_ptr,30);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }*/
                    objList[objIndex].shockProperties[TRIG_PROPERTY_VALUE] = (int)getValAtAddress(sub_ark.data, add_ptr + 16, 16);  //Target Health
                    break;
                }
            case ObjectInteraction.ACTION_CLONE:
                {//Need to do more research on what this does?
                 //	000C	int16	Object to transport.
                 //	000E	int16	Delete flag?
                 //	0010	int16	Tile destination X
                 //	0014	int16	Tile destination Y
                 //	0018	int16	Destination height?		
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_CLONE for %s\n",UniqueObjectName(objList[objIndex]));
                            //fprintf(LOGFILE,"\t\tObject to transport:%d\n",getValAtAddress(sub_ark,add_ptr+0xC,16));
                            //fprintf(LOGFILE,"\t\tDeleteFlag?:%d\n",getValAtAddress(sub_ark,add_ptr+0xE,16));
                            //fprintf(LOGFILE,"\t\tDestination tileX:%d\n",getValAtAddress(sub_ark,add_ptr+0x10,16));
                            //fprintf(LOGFILE,"\t\tDestination tileY:%d\n",getValAtAddress(sub_ark,add_ptr+0x14,16));
                            //fprintf(LOGFILE,"\t\tDestination height:%d\n",getValAtAddress(sub_ark,add_ptr+0x18,16));
                            //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	


                    }*/
                    objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);    //obj to transport
                    objList[objIndex].shockProperties[TRIG_PROPERTY_FLAG] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0E, 16);     //Delete flag
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16); //Target X
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16); //Target Y
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Z] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16); //Target z

                    break;
                }
            case ObjectInteraction.ACTION_SET_VARIABLE:
                {//Sets a game variable. I don't yet know what the various variables are. I suspect they may be in the exe so I'll have to just observe them in the wild?
                 //000C	int16	variable to set
                 //0010	int16	value
                 //0012	int16	?? action 00 set 01 add
                 //0014	int16	Optional message to receive
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_SET_VARIABLE for %s\n",UniqueObjectName(objList[objIndex]));
                            //fprintf(LOGFILE,"\t\tVariable to Set:%d\n",getValAtAddress(sub_ark,add_ptr+0xC,16));
                            //fprintf(LOGFILE,"\t\tValue:%d",getValAtAddress(sub_ark,add_ptr+0x10,16));
                            //fprintf(LOGFILE,"\t\taction?:%d (00 set 01 add)\n",getValAtAddress(sub_ark,add_ptr+0x12,16));
                            //fprintf(LOGFILE,"\t\tOptional Message:%d\n",getValAtAddress(sub_ark,add_ptr+0x14,16));
                            //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*					fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
        fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
        fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
        fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
        fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
        fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
        fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
        fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
                    //	}
                    objList[objIndex].shockProperties[TRIG_PROPERTY_VARIABLE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_VALUE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_OPERATION] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);

                    break;
                }
            case ObjectInteraction.ACTION_ACTIVATE:
                {//Turns on up to 4 other triggers.
                 //000C	int16	1st object to activate.
                 //000E	int16	Delay before activating object 1.
                 //0010	...	Up to 4 objects and delays stored here.	
                    objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                    objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xe, 16);
                    objList[objIndex].shockProperties[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                    objList[objIndex].shockProperties[4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);
                    objList[objIndex].shockProperties[5] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x16, 16);
                    objList[objIndex].shockProperties[6] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);
                    objList[objIndex].shockProperties[7] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x1A, 16);
                    /*if (PrintDebug==1)
                    {	
                            //fprintf(LOGFILE,"\tACTION_ACTIVATE for %s\n",UniqueObjectName(objList[objIndex]));

                            //fprintf(LOGFILE,"\t\t1st Object to activate raw :%d\t",objList[objIndex].shockProperties[0]);
                            //fprintf(LOGFILE,"1st Object Delay:%d\n",objList[objIndex].shockProperties[1]);

                            //fprintf(LOGFILE,"\t\t2nd Object to activate raw :%d\t",objList[objIndex].shockProperties[2]);		
                            //fprintf(LOGFILE,"2nd Object Delay:%d\n",objList[objIndex].shockProperties[3]);

                            //fprintf(LOGFILE,"\t\t3rd Object to activate raw :%d\t",objList[objIndex].shockProperties[4]);
                            //fprintf(LOGFILE,"3rd Object Delay:%d\n",objList[objIndex].shockProperties[5]);

                            //fprintf(LOGFILE,"\t\t4th Object to activate raw :%d\t",objList[objIndex].shockProperties[6]);		
                            //fprintf(LOGFILE,"4th Object Delay:%d\n",objList[objIndex].shockProperties[7]);	
                            //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
                    //}

                    break;
                }
            case ObjectInteraction.ACTION_LIGHTING:
                {//Controls lighting between the specified control points. The control points are usually null triggers but are sometimes regular objects as well.
                 //000C	int16	Control point 1
                 //000E	int16	Control point 2
                 //	...	?

                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 12, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 14, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_LIGHT_OP] = (int)getValAtAddress(sub_ark.data, add_ptr + 16, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 22, 8);//22,24,23,25
                    objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 23, 8);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 24, 8);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 25, 8);
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_LIGHTING for %s\n",UniqueObjectName(objList[objIndex]));
                            fprintf(LOGFILE,"\t\t(%d)Lighting Operation: %d\n", objList[objIndex].address+16, objList[objIndex].shockProperties[TRIG_PROPERTY_LIGHT_OP]);
                            fprintf(LOGFILE,"\t\tControl point 1:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1]);
                            fprintf(LOGFILE,"\t\tControl point 2:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2]);
                            fprintf(LOGFILE,"\t\t(%d)Not used? Upper Shade adjustment = %d\n", objList[objIndex].address +22, objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_1]);
                            fprintf(LOGFILE,"\t\t(%d)Not used? Lower Shade adjustment = %d\n", objList[objIndex].address +23 ,objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_1]);
                            fprintf(LOGFILE,"\t\t(%d)Upper Shade adjustment = %d\n", objList[objIndex].address +24 ,objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_2]);
                            fprintf(LOGFILE,"\t\t(%d)Lower Shade adjustment = %d\n", objList[objIndex].address +25 ,objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_2]);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }	*/
                    break;
                }
            case ObjectInteraction.ACTION_EFFECT:
                {//Preforms a special effect. One example is the power breaker sparking on research level.
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_EFFECT for %s\n",UniqueObjectName(objList[objIndex]));
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }		*/
                    break;
                }
            case ObjectInteraction.ACTION_MOVING_PLATFORM:
                {//Usually a lift or a blast door type setup. Both the floor and ceiling have parameters in this.

                    setElevatorProperties(LevelInfo, sub_ark, add_ptr, objList, objIndex);
                    //objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X] = getValAtAddress(sub_ark,add_ptr+0x0C,16);
                    //objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y] = getValAtAddress(sub_ark,add_ptr+0x10,16);
                    //objList[objIndex].shockProperties[TRIG_PROPERTY_FLOOR] = getValAtAddress(sub_ark,add_ptr+0x14,16);	//5
                    //objList[objIndex].shockProperties[TRIG_PROPERTY_CEILING] = getValAtAddress(sub_ark,add_ptr+0x16,16);	//6
                    //objList[objIndex].shockProperties[TRIG_PROPERTY_SPEED] = getValAtAddress(sub_ark,add_ptr+0x18,16);
                    //LevelInfo[objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X]][objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]].hasElevator =1;
                    //
                    //short ceilingFlag = (objList[objIndex].shockProperties[TRIG_PROPERTY_CEILING]<=SHOCK_CEILING_HEIGHT);
                    //short floorFlag = (objList[objIndex].shockProperties[TRIG_PROPERTY_FLOOR] <= SHOCK_CEILING_HEIGHT);
                    //short elevatorFlag = (ceilingFlag << 1) | (floorFlag);
                    //
                    //LevelInfo[objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X]][objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]].hasElevator &= elevatorFlag;

                    break;

                }
            case ObjectInteraction.ACTION_TIMER:
                {//Delays the trigger getting fired off. A good example is the aftermath of destroying the CPUs on hospital level.
                 //000C	int16	1st object to activate.?
                 //000E	int16	Delay before activating object 1.?

                    objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                    objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xe, 16);
                    objList[objIndex].shockProperties[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                    objList[objIndex].shockProperties[4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);
                    objList[objIndex].shockProperties[5] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x16, 16);
                    objList[objIndex].shockProperties[6] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);
                    objList[objIndex].shockProperties[7] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x1A, 16);
                    /*if (PrintDebug == 1)
                    {
                            fprintf(LOGFILE,"\tACTION_TIMER (i think) for %s\n", UniqueObjectName(objList[objIndex]));

                            fprintf(LOGFILE,"\t\t1st Object to activate raw :%d\t", objList[objIndex].shockProperties[0]);
                            fprintf(LOGFILE,"1st Object Delay:%d\n", objList[objIndex].shockProperties[1]);

                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                    }*/
                    break;
                }
            case ObjectInteraction.ACTION_CHOICE:
                {//A toggle?
                 //000C	int16	Trigger 1
                 //0010	int16	Trigger 2

                    objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_CHOICE for %s\n",UniqueObjectName(objList[objIndex]));
                            fprintf(LOGFILE,"\t\tTrigger1:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_1]);
                            fprintf(LOGFILE,"\t\tTrigger2:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_2]);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }*/
                    break;
                }
            case ObjectInteraction.ACTION_EMAIL:
                {//Sends the player an email. (Differs from a message in that a message just plays once and does not hit the data reader)
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_EMAIL for %s\n",UniqueObjectName(objList[objIndex]));
                            //	0F Player receives email
                            //000C	int16	Chunk no. of email (offset from 2441 0x0989)
                            //Note the subject line of an email may be used to chain a sequence of emails together (see sspecs)
                            fprintf(LOGFILE,"\t\tEmail chunk:", getValAtAddress(sub_ark,add_ptr+0x0C,16)+2441);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
   fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
   fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
   fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
   fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
   fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
   fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
   fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));*/
                    //}			
                    objList[objIndex].shockProperties[TRIG_PROPERTY_EMAIL] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16) + 2441;

                    break;

                }
            case ObjectInteraction.ACTION_RADAWAY:
                {//Radiation healing on the reactor?
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_RADAWAY for %s\n",UniqueObjectName(objList[objIndex]));
                            //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*				fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
    fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
    fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
    fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
    fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
    fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
    fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
    fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));*/
                    //}
                    break;
                }
            case ObjectInteraction.ACTION_CHANGE_STATE:
                {//Used a lot in switches. Needs more research. (changes the image?)
                    /*if (PrintDebug==1)
                    {
                            //fprintf(LOGFILE,"\tACTION_CHANGE_STATE for %s\n",UniqueObjectName(objList[objIndex]));
                            objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE] = getValAtAddress(sub_ark, add_ptr + 12, 16);
                            objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT] = getValAtAddress(sub_ark, add_ptr + 16, 16);
                            fprintf(LOGFILE, "\t\tObject to activate:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT]);
                            fprintf(LOGFILE, "\t\tNew type:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE]);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            //fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
                            //fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
                            //fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
                            //fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
                            //fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
                            //fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
                            //fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
                            //fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	
                    }*/
                    break;
                }
            case ObjectInteraction.ACTION_AWAKEN:
                {//Wakes up sleeping drones in between the two control points and sends them after you. (maybe)
                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                    /*if (PrintDebug == 1)
                    {
                            fprintf(LOGFILE,"\tACTION_AWAKEN for %s\n", UniqueObjectName(objList[objIndex]));
                            fprintf(LOGFILE,"\t\tControl point object1:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1]);
                            fprintf(LOGFILE,"\t\tControl point object2:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2]);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                    }*/
                    break;
                }
            case ObjectInteraction.ACTION_MESSAGE:
                {//A once off message. For example the computer voice when the cyborg conversion is activated.
                 //16 Trap message offset in Chunk 2151 
                 //000C	int16	"Success" message
                 //0010	int16	"Fail" message
                    objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_MESSAGE for %s\n",UniqueObjectName(objList[objIndex]));
                            fprintf(LOGFILE,"\t\tSuccess Message%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE1]);
                            fprintf(LOGFILE,"\t\tFail Message:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE2]);
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
                    //}			
                    break;
                }
            case ObjectInteraction.ACTION_SPAWN:
                {
                    //000C	int32	Class/subclass/type of object to spawn
                    //0010	int16	Control point 1 (object)
                    //0012	int16	Control point 2 (object)
                    //0014		??
                    //0018		??	

                    objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 32);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                    /*	if (PrintDebug==1)
                        {
                                fprintf(LOGFILE,"\tACTION_SPAWN for %s\n",UniqueObjectName(objList[objIndex]));
                                fprintf(LOGFILE,"\t\Class-sub-type to spawn:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE]);
                                fprintf(LOGFILE,"\t\tControl point object1:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1]);
                                fprintf(LOGFILE,"\t\tControl point object2:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2]);
                                fprintf(LOGFILE,"\t\t??:%d\n",getValAtAddress(sub_ark,add_ptr+0x14,16));		
                                fprintf(LOGFILE,"\t\t??:%d\n",getValAtAddress(sub_ark,add_ptr+0x18,16));	
                                DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                                /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
    fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
    fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
    fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
    fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
    fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
    fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
    fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));		*/
                    //	}
                    break;
                }
            case ObjectInteraction.ACTION_CHANGE_TYPE:
                {
                    //000C	int16	Object ID to change.
                    //0010	int8	New type.
                    //0012		??

                    objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
                    objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 8);
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tACTION_CHANGE_TYPE for %s\n",UniqueObjectName(objList[objIndex]));
                            fprintf(LOGFILE,"\t\Object to Change:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT]);
                            fprintf(LOGFILE,"\t\tNew Type (within subclass):%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE]);
                            //fprintf(LOGFILE,"\t\tChanges to %s\n", getObjectNameByClass(objList[objIndex].ObjectClass, objList[objIndex].ObjectSubClass, objList[objIndex].ObjectSubClassIndex);
                            fprintf(LOGFILE,"\t\t??:%d\n",getValAtAddress(sub_ark,add_ptr+0x12,16));	
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
                    //}			
                    break;
                }
            default:
                {
                    /*if (PrintDebug==1)
                    {
                            fprintf(LOGFILE,"\tUnknown triggeraction:%d for %s\n",TriggerType, UniqueObjectName(objList[objIndex]));
                            DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                            /*				fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
    fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
    fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
    fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
    fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
    fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
    fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
    fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
                    //}
                    break;
                }

        }

    }





    /******/




    void getShockButtons(TileInfo[,] LevelInfo, Chunk sub_ark, int add_ptr, ObjectLoaderInfo[] objList, int objIndex)
    {
        //I'm keeping this seperate from trigger action retrieval for the moment.

        //fprintf(LOGFILE,"\n\tVal_oc: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x0c,16));
        //fprintf(LOGFILE,"\tVal_oe: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x0E,16));
        //fprintf(LOGFILE,"\tVal_10: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x10,16));
        //fprintf(LOGFILE,"\tVal_12: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x12,16));
        //fprintf(LOGFILE,"\tVal_14: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x14,16));
        //fprintf(LOGFILE,"\tVal_16: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x16,16));
        //fprintf(LOGFILE,"\tVal_18: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x18,16));
        //fprintf(LOGFILE,"\tVal_1a: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x1A,16));	
        //
        //fprintf(LOGFILE,"\tSwitch Properties\n");
        if (objList[objIndex].ObjectSubClass == 0)
        {//regular buttons and switches
            switch (objList[objIndex].TriggerAction)    //Switches have action types as well.
            {
                case ObjectInteraction.ACTION_SET_VARIABLE:
                    {//Sets a game variable. I don't yet know what the various variables are. I suspect they may be in the exe so I'll have to just observe them in the wild?
                     //000C	int16	variable to set
                     //0010	int16	value
                     //0012	int16	?? action 00 set 01 add
                     //0014	int16	Optional message to receive
                     //fprintf(LOGFILE,"\tACTION_SET_VARIABLE for %s\n", UniqueObjectName(objList[objIndex]));
                     //fprintf(LOGFILE,"\t\tVariable to Set:%d\n", getValAtAddress(sub_ark, add_ptr + 0xC, 16));
                     //fprintf(LOGFILE,"\t\tValue:%d", getValAtAddress(sub_ark, add_ptr + 0x10, 16));
                     //fprintf(LOGFILE,"\t\taction?:%d (00 set 01 add)\n", getValAtAddress(sub_ark, add_ptr + 0x12, 16));
                     //fprintf(LOGFILE,"\t\tOptional Message:%d\n", getValAtAddress(sub_ark, add_ptr + 0x14, 16));
                     //DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_VARIABLE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_VALUE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_OPERATION] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_MESSAGE1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);
                        break;
                    }
                case ObjectInteraction.ACTION_ACTIVATE:
                    {   //Assume same behaviour as a trigger?
                        //fprintf(LOGFILE,"Switch:Action_Activate\n");
                        objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xC, 16);
                        objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0xe, 16);
                        objList[objIndex].shockProperties[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                        objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
                        objList[objIndex].shockProperties[4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);
                        objList[objIndex].shockProperties[5] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x16, 16);
                        objList[objIndex].shockProperties[6] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);
                        objList[objIndex].shockProperties[7] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x1A, 16);
                        break;
                    }
                case ObjectInteraction.ACTION_MOVING_PLATFORM:
                    {
                        //fprintf(LOGFILE,"Switch:Action_Moving_Platform\n");
                        setElevatorProperties(LevelInfo, sub_ark, add_ptr, objList, objIndex);
                        break;
                    }
                case ObjectInteraction.ACTION_CHOICE:
                    {
                        //fprintf(LOGFILE,"Switch:Action_Choice\n");
                        objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_TRIG_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
                        break;
                    }
                case ObjectInteraction.ACTION_LIGHTING:
                    {
                        //fprintf(LOGFILE,"Switch:Action_Lighting\n");
                        objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 12, 16);
                        if (objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] <= 3)
                        {   //seems to be a special case?
                            objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1] = objIndex;
                        }
                        objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 14, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 22, 8);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_1] = (int)getValAtAddress(sub_ark.data, add_ptr + 24, 8);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 23, 8);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 25, 8);
                        //fprintf(LOGFILE,"\t\tControl point 1:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_1]);
                        //fprintf(LOGFILE,"\t\tControl point 2:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_CONTROL_2]);
                        //fprintf(LOGFILE,"\t\t1st Time Upper Shade adjustment = %d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_1]);
                        //fprintf(LOGFILE,"\t\t1st Time Lower Shade adjustment = %d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_1]);
                        //fprintf(LOGFILE,"\t\t2nd Time Upper Shade adjustment = %d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_UPPERSHADE_2]);
                        //fprintf(LOGFILE,"\t\t2nd Time Lower Shade adjustment = %d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_LOWERSHADE_2]);
                        break;
                    }
                case ObjectInteraction.ACTION_CHANGE_TYPE:
                    {
                        //fprintf(LOGFILE,"Switch:Action_Change_Type\n");
                        objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 8);
                        //fprintf(LOGFILE, "\t\tObject to change:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT]);
                        //fprintf(LOGFILE, "\t\tNew type:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE]);
                        break;
                    }
                case ObjectInteraction.ACTION_CHANGE_STATE:
                    {
                        //fprintf(LOGFILE, "Switch:Action_Change_State\n");
                        objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE] = (int)getValAtAddress(sub_ark.data, add_ptr + 12, 16);
                        objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT] = (int)getValAtAddress(sub_ark.data, add_ptr + 16, 16);
                        //fprintf(LOGFILE, "\t\tObject to activate:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_OBJECT]);
                        //fprintf(LOGFILE, "\t\tNew type:%d\n", objList[objIndex].shockProperties[TRIG_PROPERTY_TYPE]);
                        //DebugPrintTriggerVals(sub_ark, add_ptr, 30);
                        break;
                    }
                default:
                    {
                        //fprintf(LOGFILE,"Switch:Default\n");
                        objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER] = (int)getValAtAddress(sub_ark.data, add_ptr + 12, 16);
                        objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER_2] = (int)getValAtAddress(sub_ark.data, add_ptr + 16, 16);
                        break;
                    }
            }

            //fprintf(LOGFILE,"\tDefault trigger target %d",objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER]);
            //fprintf(LOGFILE,"\n\tVal_oc: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x0c,16));
            //fprintf(LOGFILE,"\tVal_oe: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x0E,16));
            //fprintf(LOGFILE,"\tVal_10: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x10,16));
            //fprintf(LOGFILE,"\tVal_12: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x12,16));
            //fprintf(LOGFILE,"\tVal_14: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x14,16));
            //fprintf(LOGFILE,"\tVal_16: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x16,16));
            //fprintf(LOGFILE,"\tVal_18: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x18,16));
            //fprintf(LOGFILE,"\tVal_1a: %d\n" ,getValAtAddress(sub_ark,add_ptr+0x1A,16));	
            //DebugPrintTriggerVals(sub_ark, add_ptr, 30);

            return;
        }
        if ((objList[objIndex].ObjectSubClass == 2) && (objList[objIndex].ObjectSubClassIndex == 0))
        {//cyberspace terminal
         //000C  int16 X of target Cyberspace
         //0010  int16 Y of target Cyberspace
         //0014  int16 Z of target Cyberspace
         //0018  int16 Level (Cyberspace)
            objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);
            objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
            objList[objIndex].shockProperties[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);
            objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);
            //fprintf(LOGFILE,"\tCyberspace (%d,%d,%d @ %d)\n",objList[objIndex].shockProperties[0],objList[objIndex].shockProperties[1],objList[objIndex].shockProperties[2],objList[objIndex].shockProperties[3]);
            return;
        }

        if ((objList[objIndex].ObjectSubClass == 2) && (objList[objIndex].ObjectSubClassIndex >= 1))
        {//Fixup station/energy station
            objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);   //Amount of charge?/? always 255
            objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);  //Security level?? //reuse timer??
                                                                                                            //fprintf(LOGFILE,"\tEnergy Charge: %d %d\n",objList[objIndex].shockProperties[0] ,objList[objIndex].shockProperties[1] );
            return;
        }
        if ((objList[objIndex].ObjectSubClass == 3) && (objList[objIndex].ObjectSubClassIndex <= 3))
        {
            //puzzle panels. need to see them in the wild before I know what other stuff does
            objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);

            //if bit 28 is set (0x10000000) it is a block puzzle, else it is a wire puzzle.
            objList[objIndex].shockProperties[BUTTON_PROPERTY_PUZZLE] = ((int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 8) >> 28) & 0x01;
            if (objList[objIndex].shockProperties[BUTTON_PROPERTY_PUZZLE] == 1)
            {
                //	fprintf(LOGFILE,"\tPuzzle panel: Type is block\n");
            }
            else
            {
                //fprintf(LOGFILE,"\tPuzzle panel: Type is wire\n");
            }
            //fprintf(LOGFILE,"\tTrigger is %d",objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER]);
            return;
        }

        if ((objList[objIndex].ObjectSubClass == 3) && ((objList[objIndex].ObjectSubClassIndex == 4) || (objList[objIndex].ObjectSubClassIndex == 5) || (objList[objIndex].ObjectSubClassIndex == 6)))
        {//elevators

            //Elevators (9 3 5):
            //000C  int16 Map index of Panel of target Level1 (this means the panel no itself!)
            //000E  int16 Map index of Panel of target Level2
            //0012  int16 Map index of Panel of target Level3
            //0018  int16 Bitfield of accessible Levels (Actual)
            //001A  int16 Bitfield of accessible Levels (Shaft)
            //	    Levels with a 1 in the "shaft" field but not in the "Actual" field
            //	     give a "Shaft damage: Unable to go there" message.

            objList[objIndex].shockProperties[0] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);//elevator panel ids
            objList[objIndex].shockProperties[1] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0E, 16);
            objList[objIndex].shockProperties[2] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x12, 16);
            objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);//bitfields for access
            objList[objIndex].shockProperties[4] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x1A, 16);
            //fprintf(LOGFILE,"\tElevator to one of %d, %d or %d panels on other levels\n",objList[objIndex].shockProperties[0],objList[objIndex].shockProperties[2],objList[objIndex].shockProperties[2]);
            //fprintf(LOGFILE,"\tAccesable levels actual:%d shaft:%d\n",objList[objIndex].shockProperties[3],objList[objIndex].shockProperties[4]);

            return;
        }

        if ((objList[objIndex].ObjectSubClass == 3) && ((objList[objIndex].ObjectSubClassIndex == 7) || (objList[objIndex].ObjectSubClassIndex == 8)))
        {
            //Number Pads
            //000C	int16	Combination in BCD
            //000E  int16 Map Object to trigger
            //0018  int16 Map Object to Extra Trigger (?)
            int combo = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);
            int value =
                    (combo & 0x0F) * 1
                    + (combo >> 4 & 0x0F) * 10
                    + (combo >> 8 & 0x0F) * 100;
            objList[objIndex].shockProperties[BUTTON_PROPERTY_COMBO] = value;   // getValAtAddress(sub_ark,add_ptr+0x0c,16);

            objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0E, 16);
            objList[objIndex].shockProperties[3] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);  //extra trigger?
                                                                                                            //fprintf(LOGFILE,"\tNumber pad. Combo is %d, Triggers %d",objList[objIndex].shockProperties[BUTTON_PROPERTY_COMBO],objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER] );
            return;
        }

        //unknown object if all other tests fail. set the usual trigger value and keep an eye on this statement in debugging.
        objList[objIndex].shockProperties[BUTTON_PROPERTY_TRIGGER] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0c, 16);
        /*	shockProperties[0]  = getValAtAddress(sub_ark,add_ptr+0x0c,16);   
shockProperties[1]  = getValAtAddress(sub_ark,add_ptr+0x10,16);	
shockProperties[2]  = getValAtAddress(sub_ark,add_ptr+0x12,16);
shockProperties[3]  = getValAtAddress(sub_ark,add_ptr+0x14,16);
shockProperties[4]  = getValAtAddress(sub_ark,add_ptr+0x16,16);
shockProperties[5]  = getValAtAddress(sub_ark,add_ptr+0x18,16);
shockProperties[6]  = getValAtAddress(sub_ark,add_ptr+0x1A,16);
shockProperties[7]  = getValAtAddress(sub_ark,add_ptr+0x1B,16);
shockProperties[8]  = getValAtAddress(sub_ark,add_ptr+0x1C,16);	*/
        //fprintf(LOGFILE,"\tOther button type!");
        //DebugPrintTriggerVals(sub_ark, add_ptr, 30);

    }







    void setElevatorProperties(TileInfo[,] LevelInfo, Chunk sub_ark, int add_ptr, ObjectLoaderInfo[] objList, int objIndex)
    {

        //000C	int16	Tile x coord of platform
        //0010	int16	Tile y coord of platform
        //0014	int16	Target floor height
        //0016	int16	Target ceiling height
        //0018	int16	Speed

        objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x0C, 16);
        objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x10, 16);
        objList[objIndex].shockProperties[TRIG_PROPERTY_FLOOR] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x14, 16);    //5
        objList[objIndex].shockProperties[TRIG_PROPERTY_CEILING] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x16, 16);  //6
        objList[objIndex].shockProperties[TRIG_PROPERTY_SPEED] = (int)getValAtAddress(sub_ark.data, add_ptr + 0x18, 16);
        //LevelInfo[objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X]][objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]].hasElevator =1;

        //short ceilingFlag = (objList[objIndex].shockProperties[TRIG_PROPERTY_CEILING]<=SHOCK_CEILING_HEIGHT);
        //short floorFlag = (objList[objIndex].shockProperties[TRIG_PROPERTY_FLOOR] <= SHOCK_CEILING_HEIGHT);
        //short elevatorFlag = (ceilingFlag << 1) | (floorFlag);

        //LevelInfo[objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X]][objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]].hasElevator = elevatorFlag;
        //LevelInfo[objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X],objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]].hasElevator = elevatorFlag;

        /*if (PrintDebug==1)
        {
                fprintf(LOGFILE,"\tACTION_MOVING_PLATFORM action for %s\n", UniqueObjectName(objList[objIndex]));
                fprintf(LOGFILE,"\t\tTileX of Platform:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_X]);
                fprintf(LOGFILE,"\t\tTileY of Platform:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_TARGET_Y]);
                fprintf(LOGFILE,"\t\tTarget floor height:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_FLOOR]);
                fprintf(LOGFILE,"\t\tTarget ceiling height:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_CEILING]);
                fprintf(LOGFILE,"\t\tSpeed:%d\n",objList[objIndex].shockProperties[TRIG_PROPERTY_SPEED]);
                fprintf(LOGFILE,"\t\tMy elevator flag=%d\n",elevatorFlag);
                DebugPrintTriggerVals(sub_ark, add_ptr, 28);
                /*			fprintf(LOGFILE,"\t\tOther values 1:%d\n",getValAtAddress(sub_ark,add_ptr+12,16));
    fprintf(LOGFILE,"\t\tOther values 2:%d\n",getValAtAddress(sub_ark,add_ptr+14,16));
    fprintf(LOGFILE,"\t\tOther values 3:%d\n",getValAtAddress(sub_ark,add_ptr+16,16));
    fprintf(LOGFILE,"\t\tOther values 4:%d\n",getValAtAddress(sub_ark,add_ptr+18,16));
    fprintf(LOGFILE,"\t\tOther values 5:%d\n",getValAtAddress(sub_ark,add_ptr+20,16));
    fprintf(LOGFILE,"\t\tOther values 6:%d\n",getValAtAddress(sub_ark,add_ptr+22,16));		
    fprintf(LOGFILE,"\t\tOther values 7:%d\n",getValAtAddress(sub_ark,add_ptr+24,16));		
    fprintf(LOGFILE,"\t\tOther values 8:%d\n",getValAtAddress(sub_ark,add_ptr+26,16));	*/
        //	}	

    }


    /// <summary>
    /// Sets the door bits so I know if the tile contains a door..
    /// </summary>
    /// <param name="LevelInfo">Level info.</param>
    /// <param name="objList">Object list.</param>
    void setDoorBits(TileInfo[,] LevelInfo, ObjectLoaderInfo[] objList)
    {//
        ObjectLoaderInfo currObj;
        for (short x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                if (LevelInfo[x, y].indexObjectList != 0)
                {
                    currObj = objList[LevelInfo[x, y].indexObjectList];
                    do
                    {
                        if ((objList[currObj.index].GetItemType() == ObjectInteraction.DOOR)
                                || (objList[currObj.index].GetItemType() == ObjectInteraction.HIDDENDOOR)
                                || (objList[currObj.index].GetItemType() == ObjectInteraction.PORTCULLIS))
                        {
                            //if (currObj.Angle1 >0)
                            //	{
                            //	//This door is a flat grating. I don't support that yet!
                            //	break;
                            //	}
                            //else
                            //	{
                            LevelInfo[x, y].IsDoorForNPC = true;
                            //LevelInfo[x,y].DoorIndex = currObj.index;
                            //Put it's lock into use if it exists.
                            //I'm ignoring for the moment but it is here for compatability to vanilla.
                            //FREE	if (currObj.link!=0)
                            //FREE	{
                            //FREE	if (objList[currObj.link].InUseFlag==0)
                            //FREE	{
                            //FREE			objList[currObj.link].InUseFlag=1;	
                            //FREE			objList[currObj.link].tileX=TileMap.ObjectStorageTile;
                            //FREE			objList[currObj.link].tileY=TileMap.ObjectStorageTile;	
                            //FREE	}
                            //FREE
                            //FREE}
                            //	}
                            break;
                        }
                        //else
                        //{
                        //		if (GameWorldController.instance.objectMaster.type[objList[currObj.index].item_id] == ObjectInteraction.SHOCK_DOOR)
                        //		{
                        //LevelInfo[x,y].shockDoor = 1;
                        //LevelInfo[x,y].DoorIndex = currObj.index;
                        //		}
                        //}
                        currObj = objList[currObj.next];
                    } while (currObj.index != 0);
                }
            }

        }
    }


    /// <summary>
    /// Match wands to their spell links..
    /// </summary>
    /// <param name="objLoader">Object loader.</param>
    static public void LinkObjectListWands(ObjectLoader objLoader)
    {
        for (int o = 1; o <= objLoader.objInfo.GetUpperBound(0); o++)
        {
            if (objLoader.objInfo[o] != null)
            {
                if (objLoader.objInfo[o].instance != null)
                {
                    if (objLoader.objInfo[o].instance.GetComponent<Wand>() != null)
                    {
                        if (objLoader.objInfo[o].instance.enchantment == 1)
                        {
                            //Object contains it's own enchantment with infinite charges
                            //DO NOTHING	
                        }
                        else
                        {
                            int l = objLoader.objInfo[o].link;
                            if (l != 0)
                            {
                                if (l <= objLoader.objInfo.GetUpperBound(0))
                                {
                                    if (objLoader.objInfo[l].GetItemType() == ObjectInteraction.SPELL)
                                    {
                                        if (objLoader.objInfo[l].instance != null)
                                        {
                                            if (objLoader.objInfo[l].instance.GetComponent<a_spell>() != null)
                                            {
                                                objLoader.objInfo[o].instance.GetComponent<Wand>().linkedspell = objLoader.objInfo[l].instance.GetComponent<a_spell>();
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //Debug.Log("wand link for " + o + " is out of range Link=" + l);
                                //}
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Match wands to their spell links..
    /// </summary>
    /// <param name="objLoader">Object loader.</param>
    static public void LinkObjectListPotions(ObjectLoader objLoader)
    {
        for (int o = 1; o <= objLoader.objInfo.GetUpperBound(0); o++)
        {
            if (objLoader.objInfo[o] != null)
            {
                if (objLoader.objInfo[o].instance != null)
                {
                    if (objLoader.objInfo[o].instance.GetComponent<Potion>() != null)
                    {
                        if (objLoader.objInfo[o].instance.isquant == 1)
                        {
                            //Object contains it's own enchantment with infinite charges
                            //DO NOTHING	
                        }
                        else
                        {
                            int l = objLoader.objInfo[o].link;
                            if (l != 0)
                            {
                                if (l <= objLoader.objInfo.GetUpperBound(0))
                                {
                                    if ((objLoader.objInfo[l].GetItemType() == ObjectInteraction.SPELL) || (objLoader.objInfo[l].GetItemType() == ObjectInteraction.A_DAMAGE_TRAP))
                                    {
                                        if (objLoader.objInfo[l].instance != null)
                                        {
                                            objLoader.objInfo[o].instance.GetComponent<Potion>().linkedspell = objLoader.objInfo[l].instance.GetComponent<object_base>();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public static int findObjectByTypeInTile(ObjectLoaderInfo[] objList, short tileX, short tileY, int itemType)
    {
        for (int i = 0; i <= objList.GetUpperBound(0); i++)
        {
            //(objList[i].InUseFlag != 0)
            //&&

            if (

                    (objList[i].ObjectTileX == tileX)
                    &&
                    (objList[i].ObjectTileY == tileY)
                    &&
                    (objList[i].GetItemType() == itemType)
            )
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Clones an object an places it in the world list.
    /// </summary>
    /// <param name="toClone"></param>
    /// <returns></returns>
    public static ObjectInteraction Clone(ObjectInteraction toClone)
    {
        var newobjt = ObjectLoaderInfo.Clone(toClone.BaseObjectData);
        return ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, GameWorldController.instance.InventoryMarker.transform.position);

    }
}
