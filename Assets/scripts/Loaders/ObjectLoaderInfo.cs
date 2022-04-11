using UnityEngine;

/// <summary>
/// Object loader info.
/// </summary>
/// Basically what gets written and read to and from lev.ark files.
/// Used to create instances of objectinteractions.
public class ObjectLoaderInfo : UWClass
{
    /// <summary>
    /// it's own index for look ups into the data to extract it's properties
    /// </summary>
    public short index;

    //Inventory Data. inventory data is not stored in a block of memory until write back so temp data is used here.
    public byte[] InventoryData;

    public bool IsInventory
    {//= false;
        get { return parentList == GameWorldController.instance.inventoryLoader; }
    }

    /// <summary>
    /// Check if the object should only have the base 4 bytes of static info.
    /// </summary>
    public bool IsStatic
    {
        get
        {
            if (IsInventory) { return true; }
            else { return index >= 256; }
        }
    }

    public TileMap map;

    /// <summary>
    /// Referernce to the raw data for this item.
    /// </summary>
    public byte[] DataBuffer
    {
        get
        {
            if (IsInventory)
            {
                if (InventoryData == null)
                {
                    InventoryData = new byte[8]; //8 bytes of static data.
                }
                //return a ref to the players inventory buffer
                return InventoryData;
                //SaveGame.InventoryData;
            }
            else
            {
                return map.lev_ark_block.Data;
            }
        }
    }

    byte GetAt(int index)
    {
        return DataBuffer[index];
    }

    int GetAt16(int index)
    {
        return (int)DataLoader.getValAtAddress(DataBuffer, index, 16);
    }

    int GetAt32(int index)
    {
        return (int)DataLoader.getValAtAddress(DataBuffer, index, 32);
    }

    void SetAt(int index, byte value)
    {
        DataBuffer[index] = value;
    }

    void SetAt16(int index, int value)
    {
        DataLoader.setValAtAddress(DataBuffer, index, 16, value);
    }

    void SetAt32(int index, int value)
    {
        DataLoader.setValAtAddress(DataBuffer, index, 32, value);
    }


    /// <summary>
    /// Indentifier of what the object is.
    /// </summary>
    public int item_id
    {//0-8
        get
        {
            int val = (int)GetAt16(PTR);
            return DataLoader.ExtractBits(val, 0, 0x1FF);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x1FF << 0); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x1FF) << 0) );
        }
    }

    /// <summary>
    /// Various Object Flags
    /// </summary>
    public short flags
    {//; //9-11
        get
        {
            int val = (int)GetAt16(PTR);
            return (short)DataLoader.ExtractBits(val, 9, 0x7);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x7 << 9); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x7) << 9));
        }
    }

    /// <summary>
    /// The enchantment flag for the object.
    /// </summary>
    public short enchantment
    {//;   //12  (short)(ExtractBits(Vals[0], 12, 1));
        get
        {
            int val = (int)GetAt16(PTR);
            return (short)DataLoader.ExtractBits(val, 12, 1);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x1 << 12); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x1) << 12));
        }
    }

    public short doordir
    {//;   //13 // (short)(ExtractBits(Vals[0], 13, 1))
        get
        {
            int val = (int)GetAt16(PTR);
            return (short)DataLoader.ExtractBits(val, 13, 1);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x1 << 13); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x1) << 13));
        }
    }

    public short invis     //14
    {//(short)(ExtractBits(Vals[0], 14, 1));
        get
        {
            int val = (int)GetAt16(PTR);
            return (short)DataLoader.ExtractBits(val, 14, 1);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x1 << 14); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x1) << 14));
        }
    }

    public short is_quant  //15
    {
        get
        {
            int val = (int)GetAt16(PTR);
            return (short)DataLoader.ExtractBits(val, 15, 1);
        }
        set
        {
            int existingValue = GetAt16(PTR);
            existingValue &= ~(0x1 << 15); //Mask out current val
            SetAt16(PTR, existingValue | ((value & 0x1) << 15));
        }
    }

    public int Obsolete_texture; // Note: some objects don't have flags and use the whole lower byte as a texture number
                        //(gravestone, picture, lever, switch, shelf, bridge, ..)
                        //This variable is uses for shorthand usage of this property
                        //Not used ??

    public short zpos
    {//; (short)(ExtractBits(Vals[1], 0, 0x7f)); 
        get
        {
            int val = (int)GetAt16(PTR + 2);
            return (short)DataLoader.ExtractBits(val, 0, 0x7f);
        }
        set
        {
            int existingValue = GetAt16(PTR+2);
            existingValue &= ~(0x7F << 0); //Mask out current val
            SetAt16(PTR+2, existingValue | ((value & 0x7F) << 0));
        }
    }

    public short heading
    {//;(short)(ExtractBits(Vals[1], 7, 0x7)); //bits 7-9
        get
        {
            int val = (int)GetAt16(PTR + 2);
            return (short)DataLoader.ExtractBits(val, 7, 0x7);
        }
        set
        {
            int existingValue = GetAt16(PTR+2);
            existingValue &= ~(0x7 << 7); //Mask out current val
            SetAt16(PTR+2, existingValue | ((value & 0x7) << 7));
        }
    }


    public short ypos
    {//(short)(ExtractBits(Vals[1], 10, 0x7));
        get
        {
            int val = (int)GetAt16(PTR + 2);
            return (short)DataLoader.ExtractBits(val, 10, 0x7);
        }
        set
        {
            if (index > 256)
            {
                if (value != ypos)
                {
                    Debug.Log("Changing ypos for static object " +  index);
                }
            }
            int existingValue = GetAt16(PTR+2);
            existingValue &= ~(0x7 << 10); //Mask out current val
            SetAt16(PTR+2, existingValue | ((value & 0x7) << 10));
        }
    }


    public short xpos
    {// (short)(ExtractBits(Vals[1], 13, 0x7));
        get
        {
            int val = (int)GetAt16(PTR + 2);
            return (short)DataLoader.ExtractBits(val, 13, 0x7);
        }
        set
        {
            if (index>256)
            {
                if(value!=xpos)
                {
                    Debug.Log("Changing xpos for static object  " + index);
                }                
            }
            int existingValue = GetAt16(PTR+2);
            existingValue &= ~(0x7 << 13); //Mask out current val
            SetAt16(PTR+2, existingValue | ((value & 0x7) << 13));
        }
    }

    public short quality
    { // (short)(ExtractBits(Vals[2], 0, 0x3f))
        get
        {
            int val = (int)GetAt16(PTR + 4);
            return (short)DataLoader.ExtractBits(val, 0, 0x3f);
        }
        set
        {
            int existingValue = GetAt16(PTR + 4);
            existingValue &= ~(0x3F << 0); //Mask out current val
            SetAt16(PTR + 4, existingValue | ((value & 0x3f) << 0));
        }
    }

    public short next
    {//(short)(ExtractBits(Vals[2], 6, 0x3ff));
        get
        {
            int val = (int)GetAt16(PTR + 4);
            return (short)DataLoader.ExtractBits(val, 6, 0x3ff);
        }
        set
        {
            int existingValue = GetAt16(PTR + 4);
            existingValue &= ~(0x3FF << 6); //Mask out current val
            SetAt16(PTR + 4, existingValue | ((value & 0x3FF) << 6));
        }
    }

    public short owner
    { // (short)(ExtractBits(Vals[2], 0, 0x3f))
        get
        {
            int val = (int)GetAt16(PTR + 6);
            return (short)DataLoader.ExtractBits(val, 0, 0x3f);
        }
        set
        {
            int existingValue = GetAt16(PTR + 6);
            existingValue &= ~(0x3F << 0); //Mask out current val
            SetAt16(PTR + 6, existingValue | ((value & 0x3f) << 0));
        }
    }

    public short link
    {//(short)(ExtractBits(Vals[2], 6, 0x3ff));
        get
        {
            int val = (int)GetAt16(PTR + 6);
            return (short)DataLoader.ExtractBits(val, 6, 0x3ff);
        }
        set
        {
            int existingValue = GetAt16(PTR + 6);
            existingValue &= ~(0x3FF << 6); //Mask out current val
            SetAt16(PTR + 6, existingValue | ((value & 0x3FF) << 6));
        }
    }

    //Mobile Properties. Only available on objects with indices <256

    public byte npc_hp
    {//
        get
        {
            if (IsStatic) { return 0; }
            return GetAt(PTR + 0x8);
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0x8, value);
            }
        }
    }

    public short ProjectileHeading
    {
        get
        {
            if (IsStatic) { return 0; }
            return (short)GetAt(PTR + 0x9);
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0x9, (byte)value);
            }
        }
    }

    //public short ProjectileHeadingMinor//defection to the right of the missile from the major heading.
    //{
    //    get
    //    {
    //        if (IsStatic) { return 0; }
    //        int val = (int)DataLoader.getValAtAddress(DataBuffer, PTR + 0x9, 8);
    //        return (short)(DataLoader.ExtractBits(val, 0, 0x1F));
    //    }
    //    set
    //    {//E0
    //        if (!IsStatic)
    //        {
    //            value &= 0x1F; //Keep value in range;
    //            int val = (byte)(ProjectileHeadingMajor << 5) | (value & 0x1F);
    //            DataBuffer[PTR + 0x9] = (byte)val;
    //        }
    //    }

    //}

    //public short ProjectileHeadingMajor //Cardinal direction 0 to 7 of the missile. North = 0 turning clockwise to North west = 7
    //{
    //    get
    //    {
    //        if (IsStatic) { return 0; }
    //        int val = (int)DataLoader.getValAtAddress(DataBuffer, PTR + 0x9, 8);
    //        return (short)(DataLoader.ExtractBits(val, 5, 0x7));
    //    }
    //    set
    //    {
    //        if (!IsStatic)
    //        {
    //            value &= 0x7; //Keep value in range;
    //            int val = (byte)((value & 0x7) << 5) | (ProjectileHeadingMinor & 0x1F);
    //            DataBuffer[PTR + 0x9] = (byte)val;
    //        }
    //    }
    //}

    public byte MobileUnk_0xA
    {
        get
        {
            if (IsStatic) { return 0; }
            return GetAt(PTR + 0xa);
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0xA, (byte)value);
            }
        }
    }

    public byte npc_goal
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xb);
            return (byte)(DataLoader.ExtractBits(val, 0, 0xF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xB);
                existingValue &= ~(0xF << 0); //Mask out current val
                SetAt16(PTR + 0xB, existingValue | ((value & 0xF) << 0));
            }
        }
    }

    public byte npc_gtarg
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xb);
            return (byte)(DataLoader.ExtractBits(val, 4, 0xFF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xB);
                existingValue &= ~(0xFF << 4); //Mask out current val
                SetAt16(PTR + 0xB, existingValue | ((value & 0xFF) << 4));
            }
        }
    }

    public byte AnimationFrame
    {//formerly MobileUnk_0xB_12_F
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xb);
            return (byte)(DataLoader.ExtractBits(val, 12, 0xF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xB);
                existingValue &= ~(0xF << 12); //Mask out current val
                SetAt16(PTR + 0xB, existingValue | ((value & 0xF) << 12));
            }
        }
    }

    public byte npc_level
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (byte)(DataLoader.ExtractBits(val, 0, 0xF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xD);
                existingValue &= ~(0xF << 0); //Mask out current val
                SetAt16(PTR + 0xD, existingValue | ((value & 0xF) << 0));
            }
        }
    }

    public short MobileUnk_0xD_4_FF
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (short)(DataLoader.ExtractBits(val, 4, 0xff));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xD);
                existingValue &= ~(0xFF << 4); //Mask out current val
                SetAt16(PTR + 0xD, existingValue | ((value & 0xFF) << 4));
            }
        }
    }

    public short NPC_PowerFlag
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (short)(DataLoader.ExtractBits(val, 0xA, 1));
        }
    }

    public short MobileUnk_0xD_12_1
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (short)(DataLoader.ExtractBits(val, 12, 1));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xD);
                existingValue &= ~(0x1 << 12); //Mask out current val
                SetAt16(PTR + 0xD, existingValue | ((value & 0x1) << 12));
            }
        }
    }

    public short npc_talkedto
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (short)(DataLoader.ExtractBits(val, 13, 1));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xD);
                existingValue &= ~(0x1 << 13); //Mask out current val
                SetAt16(PTR + 0xD, existingValue | ((value & 0x1) << 13));
            }
        }
    }

    public short npc_attitude
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xd);
            return (short)(DataLoader.ExtractBits(val, 14, 0x3));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xD);
                existingValue &= ~(0x3 << 14); //Mask out current val
                SetAt16(PTR + 0xD, existingValue | ((value & 0x3) << 14));
            }
        }
    }

    public short MobileUnk_0xF_0_3F
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xf);
            return (short)(DataLoader.ExtractBits(val, 0, 0x3F));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xF);
                existingValue &= ~(0x3F << 0); //Mask out current val
                SetAt16(PTR + 0xF, existingValue | ((value & 0x3F) << 0));
            }
        }
    }

    public short npc_height
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xf);
            return (short)(DataLoader.ExtractBits(val, 6, 0x3F));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xF);
                existingValue &= ~(0x3F << 6); //Mask out current val
                SetAt16(PTR + 0xF, existingValue | ((value & 0x3F) << 6));
            }
        }
    }

    public short MobileUnk_0xF_C_F
    {///Possibly used as a look up in to NPC charge modifiers?
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0xf);
            return (short)(DataLoader.ExtractBits(val, 0xC, 0xF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0xF);
                existingValue &= ~(0xF << 0xC); //Mask out current val
                SetAt16(PTR + 0xF, existingValue | ((value & 0xF) << 0xC));
            }
        }
    }

    public short MobileUnk_0x11
    {
        get
        {
            if (IsStatic) { return 0; }
            return (short)GetAt(PTR + 0x11);
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0x11, (byte)value);
            }
        }
    }

    public short ProjectileSourceID
    {
        get
        {
            if (IsStatic) { return 0; }
            return (short)GetAt(PTR + 0x12);
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0x12, (byte)value);
            }
        }
    }

    public short MobileUnk_0x13
    {
        get
        {
            if (IsStatic) { return 0; }
            return (short)(GetAt(PTR + 0x13));
        }
        set
        {
            if (!IsStatic)
            {
                SetAt(PTR + 0x13, (byte)value);
            }
        }
    }

    public short Projectile_Speed
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x14);
            return (short)(DataLoader.ExtractBits(val, 0, 0x7));
        }
        set
        {
            byte existingValue = (byte)GetAt16(PTR + 0x14);
            existingValue = (byte)(existingValue & ~(0x7 << 0)); //Mask out current val
            SetAt(PTR + 0x14,(byte)( existingValue | ((value & 0x7) << 0)));
        }
    }

    public short Projectile_Pitch
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x14);
            return (short)(DataLoader.ExtractBits(val, 3, 0x1F));
        }
        set
        {
            byte existingValue = (byte)GetAt16(PTR + 0x14);
            existingValue = (byte)(existingValue & ~(0x1F << 3)); //Mask out current val
            SetAt(PTR + 0x14, (byte)(existingValue | ((value & 0x1F) << 3)));
        }
    }

    public short npc_animation
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x15);
            return (short)(DataLoader.ExtractBits(val, 0, 0x7F));
        }
        set
        {
            byte existingValue = (byte)GetAt(PTR + 0x15);
            existingValue = (byte)(existingValue & ~(0x7F << 0)); //Mask out current val
            SetAt(PTR + 0xF, (byte)(existingValue | ((value & 0x7F) << 0x0)));
        }
    }

    //public short MobileUnk_0x15_4_1F
    //{
    //    get
    //    {
    //        if (IsStatic) { return 0; }
    //        int val = (int)Loader.getValAtAddress(DataBuffer, PTR + 0x15, 8);
    //        return (short)(DataLoader.ExtractBits(val, 4, 0x1F));
    //    }
    //    set
    //    {
    //        value &= 0x1F;//Keep in range
    //        int val = (value << 3) | (npc_animation & 0x7);
    //        DataBuffer[PTR + 0x15] = (byte)(val);
    //    }
    //}

    public short MobileUnk_0x16_0_F
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0x16);
            return (short)(DataLoader.ExtractBits(val, 0, 0xF));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0x16);
                existingValue &= ~(0xF << 0); //Mask out current val
                SetAt16(PTR + 0x16, (existingValue | ((value & 0xF) << 0x0)));
            }
        }
    }

    public short npc_yhome
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0x16);
            return (short)(DataLoader.ExtractBits(val, 4, 0x3F));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0x16);
                existingValue &= ~(0x3F << 4); //Mask out current val
                SetAt16(PTR + 0x16, (existingValue | ((value & 0x3F) << 0x4)));
            }
        }
    }

    public short npc_xhome
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt16(PTR + 0x16);
            return (short)(DataLoader.ExtractBits(val, 10, 0x3F));
        }
        set
        {
            if (!IsStatic)
            {
                int existingValue = GetAt16(PTR + 0x16);
                existingValue &= ~(0x3F << 10); //Mask out current val
                SetAt16(PTR + 0x16, (existingValue | ((value & 0x3F) << 10)));
            }
        }
    }
    public short npc_heading
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x18);
            return (short)(DataLoader.ExtractBits(val, 0, 0x1F));
        }
        set
        {
            byte existingValue = (byte)GetAt(PTR + 0x18);
            existingValue = (byte)(existingValue & ~(0x1F << 0)); //Mask out current val
            SetAt(PTR + 0x18, (byte)(existingValue | ((value & 0x1F) << 0x0)));
        }
    }

    public short MobileUnk_0x18_5_7
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x18);
            return (short)(DataLoader.ExtractBits(val, 5, 0x7));
        }
        set
        {
            byte existingValue = (byte)GetAt(PTR + 0x18);
            existingValue = (byte)(existingValue & ~(0x7 << 5)); //Mask out current val
            SetAt(PTR + 0x18, (byte)(existingValue | ((value & 0x7) << 0x5)));
        }
    }

    public short npc_hunger
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x19);
            return (short)(DataLoader.ExtractBits(val, 0, 0x3F));
        }
        set
        {
            byte existingValue = (byte)GetAt(PTR + 0x19);
            existingValue = (byte)(existingValue & ~(0x3F << 0)); //Mask out current val
            SetAt(PTR + 0x19, (byte)(existingValue | ((value & 0x3f) << 0x0)));
        }
    }

    public short MobileUnk_0x19_6_3
    {
        get
        {
            if (IsStatic) { return 0; }
            int val = (int)GetAt(PTR + 0x19);
            return (short)(DataLoader.ExtractBits(val, 6, 0x3));
        }
        set
        {
            byte existingValue = (byte)GetAt(PTR + 0x19);
            existingValue = (byte)(existingValue & ~(0x3 << 6)); //Mask out current val
            SetAt(PTR + 0x19, (byte)(existingValue | ((value & 0x3) << 6)));
        }
    }


    public short npc_whoami
    {
        get
        {
            if (IsStatic) { return 0; }
            return (short)GetAt(PTR + 0x1a);
        }
        set
        {
            SetAt(PTR + 0x1A, (byte)value);
        }
    }


    /// <summary>
    /// The X position of the projecile object in the world space (when not an NPC)
    /// </summary>
    /// Value is equal to the current (xhome<< 8) + (xpos <<5) +0xFh
    public int CoordinateX
    {
        get
        {
            return (short)GetAt16(PTR + 0xb);
        }
    }


    /// <summary>
    /// The Y position of the projecile object in the world space (when not an NPC)
    /// </summary>
    ///(yhome<<8) + (ypos<<5) +0xFh
    public int CoordinateY
    {
        get
        {
            return (short)GetAt16(PTR + 0xc);
        }
    }


    public int CoordinateZ
    {//(zpos<<3) + 0xFh is stored here
        get
        {
            return (short)GetAt16(PTR + 0xf);
        }
    }

    //Where are these values set???
    public short npc_health = 0;//Is this poisoning?
    public short npc_arms = 0;
    public short npc_power = 0;
    public short npc_name = 0;


    //My additions
    //public short InUseFlag//Based on values and no of values in the mobile and static free lists.
    //{
    //    get { return 1; }
    //    set { }
    //}


    public short levelno
    {//Use for unique naming of the object
        get
        {
            if (IsInventory)
            {
                return -1;
            }
            else
            {
                return map.thisLevelNo;
            }
        }
    }


    public short ObjectTileX = TileMap.ObjectStorageTile; //Position of the object on the tilemap
    public short ObjectTileY = TileMap.ObjectStorageTile;
    public long address;


    public ObjectInteraction instance;
    public ObjectLoader parentList;

    //SShock specific stuff
    public int ObjectClass;
    public int ObjectSubClass;
    public int ObjectSubClassIndex;

    public int Angle1;
    public int Angle2;
    public int Angle3;

    public int sprite;
    public int State;
    public int unk1;//Probably a texture index.

    public int[] shockProperties = new int[10]; //Shared properties memory
    public int[] conditions = new int[4];
    public int TriggerAction;//Needs to be split into a property.
    public int TriggerOnce;
     
    /// <summary>
    /// The GUID of this object instance. To guarantee unique object names.
    /// </summary>
    public System.Guid guid;


    /// <summary>
    /// Offset to object in file data
    /// </summary>
    public int PTR
    {
        get
        {
            if (IsInventory)
            {
                return 0;//Always
                //return (index-1) * 8;
            }
            else
            {
                if (index < 256)
                {//Mobile, 27 bytes per object
                    return 0x4000 + (index * 27);
                }
                else
                {//static, 8 bytes per object
                    return 0x5b00 + ((index - 256) * 8);
                }
            }
        }
    }

    /// <summary>
    /// Initialise the object class
    /// </summary>
    /// <param name="index"></param>
    /// <param name="map"></param>
    public ObjectLoaderInfo(short _index, TileMap _map, bool isWorldObject)
    {
        index = _index;
        map = _map;
        guid = System.Guid.NewGuid();
        if (isWorldObject)
        {
            parentList = UWEBase.CurrentObjectList();
        }
        else
        {
            parentList = GameWorldController.instance.inventoryLoader;
        }
    }

    //public ObjectLoaderInfo(int _index)
    //{//for shock to compile but possible crash
    //    index = _index;
    //    guid = System.Guid.NewGuid();
    //}

    /// <summary>
    /// Gets the type of the item from object masters. UWE object type codes.
    /// </summary>
    /// <returns>The item type.</returns>
    public int GetItemType()
    {
        return GameWorldController.instance.objectMaster.objProp[item_id].type;
    }

    /// <summary>
    /// resets all properties to zero to maintain compatability with UW2
    /// </summary>
    public static void CleanUp(ObjectLoaderInfo obj)
    {
        //TODO:test if this is needed for mobile objects as well.
        
        obj.item_id = 0;
        obj.flags = 0;
        obj.enchantment = 0;
        obj.doordir = 0;
        obj.invis = 0;
        obj.is_quant = 0;
        obj.zpos = 0;
        obj.xpos = 0;
        obj.ypos = 0;
        obj.heading = 0;
        obj.quality = 0;
        obj.next = 0;
        obj.owner = 0;
        obj.link = 0;
    }


    //public void Set()
    //{
    //    InUseFlag = 1;
    //}

    //public void Unset()
    //{
    //    InUseFlag = 0;
    //}

    //public bool isInUse()
    //{
    //    return (InUseFlag == 1);
    //}

    public string getDesc()
    {
        return GameWorldController.instance.objectMaster.objProp[item_id].desc;
    }

    public int useSpriteValue()
    {
        return GameWorldController.instance.objectMaster.objProp[item_id].useSprite;
    }

    public int StartFrameValue()
    {
        return GameWorldController.instance.objectMaster.objProp[item_id].startFrame;
    }
        public static ObjectLoaderInfo Clone(ObjectLoaderInfo toClone)
    {
        int startindex=2;
        if (toClone.index >= 256)
        {
            startindex = 256;
        }
        var newobj=ObjectLoader.newWorldObject(toClone.item_id, toClone.quality, toClone.owner, toClone.link, startindex);
        if (newobj!=null)
        {
            short origNext = newobj.next;
            for (int i=0; i<=7;i++)
            {
                newobj.SetAt(newobj.PTR + i, newobj.GetAt(toClone.PTR + i));
            }
            //Restore the original next value of the object.
            newobj.next = origNext;
            if (newobj.index<256)
            {
                for (int i = 8; i <= 0x1A; i++)
                {
                    newobj.SetAt(newobj.PTR + i, newobj.GetAt(toClone.PTR + i));
                }
            }  
        }
        return newobj;
    }
}