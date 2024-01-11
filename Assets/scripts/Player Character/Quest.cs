using UnityEngine;
/// <summary>
/// Quest variables.
/// </summary>
/// 	 * per uw-specs.txt
/// 7.8.1 UW1 quest flags
/// 
/// flag   description
/// 
/// 0    Dr. Owl's assistant Murgo freed
/// 1    talked to Hagbard
/// 2    met Dr. Owl?
/// 3    permission to speak to king Ketchaval
/// 4    Goldthirst's quest to kill the gazer (1: gazer killed)
/// 5    Garamon, find talismans and throw into lava
/// 6    friend of Lizardman folk
/// 7    ?? (conv #24, Murgo)
/// 8    book from Bronus for Morlock
/// 9    "find Gurstang" quest
/// 10    where to find Zak, for Delanrey
/// 11    Rodrick killed
/// 
/// 32    status of "Knight of the Crux" quest
/// 0: no knight
/// 1: seek out Dorna Ironfist
/// 2: started quest to search the "writ of Lorne"
/// 3: found writ
/// 4: door to armoury opened
/// 
/// 36: No of talismans still to destroy.
/// 37: Garamon dream stages
/// 
/// UW2 Quest flags
/// 0: PT related - The troll is released
/// 1: PT related - Bishop is free?
/// 2: PT related - Borne wants you to kill bishop
/// 3: PT related - Bishop is dead
/// 
/// 6: Helena asks you to speak to praetor loth
/// 7: Loth is dead
/// 8:  Kintara tells you about Javra
/// 9:  Lobar tells you about the "virtues"
/// 10: PT related
/// 11: Listener under the castle is dead.
/// 12: used in Ice caverns to say the avatar can banish the guardians presence. Wand of altara?
/// 13: Mystral wants you to spy on altara
/// 14: Likely all lines of power have been cut.
/// 15: Altara tells you about the listener
/// 
/// 18: You learn that the Trikili can talk.
/// 19: You know Relk has found the black jewel (josie tells you)
/// 20: You've met Mokpo
/// 22: Blog is now your friend(?)
/// 23: You have used Blog to defeat dorstag
/// 24: You have won a duel in the arena
/// 25: You have defeated Zaria in the pits
/// 26: You know about the magic scepter (for the Zoranthus)
/// 27: You know about the sigil of binding/got the djinn bottle(by bringing the scepter to zorantus)
/// 28: Took Krilner as a slave
/// 29: You know Dorstag has the gem(?)
/// 30: Dorstag refused your challenge(?)
/// 32: Met a Talorid
/// 33: You have agreed to help the talorid (historian)
/// 34: Met or heard of Eloemosynathor
/// 35: The vortz are attacking!
/// 36: Quest to clarify question for Data Integrator
/// 37: talorus related (checked by futurian)
/// 38: talorus related *bliy scup is regenerated
/// 39: talorus related
/// 40: Dialogian has helped with data integrator
/// 43: Patterson has gone postal
/// 45: Janar has been met and befriended
/// 
/// 47: You have recieve the quest from the triliki
/// 48: You have dreamed about the void
/// 49: Bishop tells you about the gem.
/// 50: The keep is going to crash.
/// 51: You have visited the ice caves (britannia becomes icy)
/// 52: Have you cut the line of power in the ice caverns
/// 54: Checked by Mors Gotha? related to keep crashing
/// 55: Banner of Killorn returned (based on Scd.ark research)
/// 58: Set when meeting bishop. Bishop tells you about altara
/// 60: Possible prison tower variable. used in check trap on second highest level
/// 61: You've brought water to Josie
/// 
/// 63: Blog has given you the gem
/// 64: Is mors dead
/// 65: Pits related (checked by dorstag)
/// 68: You have given the answers to nystrul and the invasion (endgame) has begun.
/// 104: Set when you enter scintilus level 5 (set by variable trap)
/// 106: Meet mors gothi and got the book
/// 107: Set after freeing praetor loth and you/others now know about the horn.
/// 109: Set to 1 after first LB conversation. All castle occupants check this on first talk.
/// 110: Checked when talking to LB and Dupre. The guardians forces are attacking
/// 112: checked when talking to LB. You have been fighting with the others
/// 114: checked when talking to LB. The servants are restless
/// 115: checked when talking to LB. The servants are on strike
/// 116: The strike has been called off.
/// 117: Mors has been defeated in Kilorn
/// 118: The wisps tell you about the trilikisssss2
/// 119: Fizzit the thief surrenders
/// 120: checked by miranda?
/// 121: You have defeated Dorstag
/// 122: You have killed the bly scup ductosnore
/// 123: Relk is dead
/// 124 & 126 are referenced in teleport_trap
/// 128: 0-128 bit field of where the lines of power have been broken.
/// 129: How many enemies killed in the pits (also xclock 14)
/// 131: You are told that you are in the prison tower =1  
/// 	You are told you are in kilhorn keep =3
/// 	You are told you are in scintilus = 19
/// 	(this is probably a bit field.)
/// 132: Set to 2 during Kintara conversation
/// 133: How much Jospur owes you for fighting in the pits
/// 134: The password for the prison tower (random value)
/// 135: Checked by goblin in sewers  (no of worms killed on level. At more than 8 they give you fish)
/// 143: Set to 33 after first LB conversation. Set to 3 during endgame (is this what triggers the cutscenes?)
public class Quest : UWClass
{
    /// <summary>
    /// Returns the quest variable at the specified index
    /// </summary>
    /// <param name="questno"></param>
    /// <returns></returns>
    public static int GetQuestVariable(int questno, bool supressConsole = false)
    {
        int _returnvalue;
        switch (_RES)
        {
            case GAME_UW2:
                {
                    if (questno <= 127)
                    {//Quests are every 4 bytes. The first 4 bits are the four quests in that block of 4 bytes.
                        int offset = 0x66 + ((questno / 4) * 4);
                        int bit = questno % 4;
                        _returnvalue = (SaveGame.GetAt(offset) >> bit) & 0x1;
                    }
                    else
                    {
                        _returnvalue = SaveGame.GetAt(0xE7 + (questno - 128));
                    }
                    break;
                }
            default:
                {
                    if (questno <= 31)
                    {//read the quest from the bit field quests.
                        int offset = 0x66 + (questno / 8);
                        int bit = questno % 8;
                        _returnvalue = (SaveGame.GetAt(offset) >> bit) & 0x1;
                    }
                    else
                    {
                        _returnvalue = SaveGame.GetAt(0x6a + (questno - 32));
                    }
                    break;
                }
        }
        if (!supressConsole) { Debug.Log("Getting Quest #" + questno + " It's value is " + _returnvalue); }
        return _returnvalue;
    }
    /// <summary>
    /// Setting quest value
    /// </summary>
    /// <param name="questno"></param>
    /// <param name="value"></param>
    public static void SetQuestVariable(int questno, int value)
    {
        Debug.Log("Setting Quest #" + questno + " to " + value);
        switch (_RES)
        {
            case GAME_UW2:
                {
                    if (questno <= 127)
                    {//Quests are every 4 bytes. The first 4 bits are the four quests in that block of 4 bytes.
                        int offset = 0x66 + (questno / 4) * 4;
                        int bit = questno % 4;
                        byte existingValue = SaveGame.GetAt(offset);
                        byte mask = (byte)(1 << bit);
                        if (value >= 1)
                        {//set
                            existingValue |= mask;
                        }
                        else
                        {//unset
                            existingValue = (byte)(existingValue & (~mask));
                        }
                        SaveGame.SetAt(offset, existingValue);
                    }
                    else
                    {
                        SaveGame.SetAt(0xE7 + (questno - 128), (byte)value);
                    }
                    break;
                }
            default:
                {
                    if (questno <= 31)
                    {//read the quest from the bit field quests.
                        int offset = 0x66 + questno / 8;
                        int bit = questno % 8;

                        byte existingValue = SaveGame.GetAt(offset);
                        byte mask = (byte)(1 << bit);
                        if (value >= 1)
                        {//set
                            existingValue |= mask;
                        }
                        else
                        {//unset
                            existingValue = (byte)(existingValue & (~mask));
                        }
                        SaveGame.SetAt(offset, existingValue);
                    }
                    else
                    {
                        SaveGame.SetAt(0x6a + (questno - 32), (byte)value);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// The quest variable integers
    /// </summary>
    /// Typically these are for story advancement and conversations.
    ///public int[] QuestVariablesOBSOLETE = new int[256];

    /// <summary>
    /// The game Variables for the check/set variable traps
    /// </summary>
    /// Typically these are used for traps/triggers/switches.
    /// In UW1 64 x 1 byte variables.
    /// In UW2 128 x 2 byte variables.
    public static int GetVariable(int variableno)
    {
        if (_RES==GAME_UW2)
        {
            return SaveGame.GetAt16(0xFA + (variableno * 2));
        }
        else
        {
            return SaveGame.GetAt(0x71 + variableno);
        }
    }

    public static void SetVariable(int variableno, int newvalue)
    {
        if (_RES == GAME_UW2)
        {
            SaveGame.SetAt16(0xFA + (variableno * 2), (byte)newvalue);
        }
        else
        {
           SaveGame.SetAt(0x71 + variableno,(byte)newvalue);
        }
    }

    /// <summary>
    /// Additional variables in UW2. Possibly these are all bit fields hence the name Only known usage is the scintillus 5 switch puzzle
    /// </summary>
   // public static int[] BitVariables = new int[128];

    public static int GetBitVariable(int variableno)
    {
        return SaveGame.GetAt16(0x1fa + variableno);
    }

    public static void SetBitVariable(int variableno, int value)
    {
        SaveGame.SetAt16(0x1fa + variableno,value);
    }

    /// <summary>
    /// The x clocks tracks progress during the game and is used in firing events.
    /// </summary>
    /// The xclock is a range of 16 variables. When references by traps the index is -16 to get the below values.
    /// The X Clock is tied closely to SCD.ark and the scheduled events within that file.
    /// 1=Miranda conversations & related events in the castle
    ///     1 - Nystrul is curious about exploration.Set after entering lvl 1 from the route downwards. (set variable traps 17 may be related)
    ///     2 - After you visited another world.  (variable 17 is set to 16), dupre is tempted
    ///     3 - servants getting restless
    ///     4 - concerned about water, dupre is annoyed by patterson
    ///     5 - Dupre is bored / dupre is fetching water
    ///     7 - Miranda wants to talk to you pre tori murder
    ///     8 - tori is murdered
    ///     9 - Charles finds a key
    ///     11 - Go see Nelson
    ///     12 - Patterson kills Nelson
    ///     13 - Patterson is dead
    ///     14 - Gem is weak/Mors is in killorn(?)
    ///     15 - Nystrul wants to see you again re endgame
    ///     16 - Nystrul questions have been answered Mars Gotha comes
    /// 2=Nystrul conversations and no of blackrock gems treated
    /// 3=Djinn Capture
    ///     2 = balisk oil is in the mud
    ///     3 = bathed in oil
    ///     4 = baked in lava
    ///     5 = iron flesh cast (does not need to be still on when you break the bottle)
    ///     6 = djinn captured in body
    /// 14=Tracks no of enemies killed in pits. Does things like update graffiti.
    /// 15=Used in multiple convos. Possibly tells the game to process a change when updated?
    //public static int[] x_clocks = new int[16];
    public static int GetX_Clock(int x_clock)
    {
        Debug.Log("Getting XClock " + x_clock + " = " + SaveGame.GetAt(0x36E));
        return SaveGame.GetAt(0x36E);
    }

    public static void SetX_Clock(int x_clock, int value)
    {
        Debug.Log("Setting XClock " + x_clock + " to " + value);
        SaveGame.SetAt(0x36E + x_clock, (byte)value);
    }

    /// <summary>
    /// Increase the specified xclock by 1.
    /// </summary>
    /// <param name="x_clock"></param>
    public static void IncrementXClock(int x_clock)
    {
       if (GetX_Clock(x_clock)==255)
            {//max out
                return;
            }
       SetX_Clock(x_clock, SaveGame.GetAt(0x36E)+1);
    }

    /// <summary>
    /// Item ID for the sword of justice
    /// </summary>
    public const int TalismanSword = 10;
    /// <summary>
    /// Item ID for the shield of valor
    /// </summary>
    public const int TalismanShield = 55;
    /// <summary>
    /// Item id for the Taper of sacrifice
    /// </summary>
    public const int TalismanTaper = 147;
    public const int TalismanTaperLit = 151;
    /// <summary>
    /// Item Id for the cup of wonder.
    /// </summary>
    public const int TalismanCup = 174;
    /// <summary>
    /// Item ID for the book of honesty
    /// </summary>
    public const int TalismanBook = 310;
    /// <summary>
    /// Item Id for the wine of compassion.
    /// </summary>
    public const int TalismanWine = 191;
    /// <summary>
    /// Item ID for the ring of humility
    /// </summary>
    public const int TalismanRing = 54;
    /// <summary>
    /// Item ID for the standard of honour.
    /// </summary>
    public const int TalismanHonour = 287;

    /// <summary>
    /// The no of talismans to still be cast into abyss in order to complete the game.
    /// </summary>
    public static int TalismansRemaining
    {
        get { return GetQuestVariable(36); }
        set { SetQuestVariable(36, value); }
    }

    /// <summary>
    /// Tracks which garamon dream we are at.
    /// </summary>
    public static int GaramonDream//The next dream to play
    {
        get { return GetQuestVariable(37); }
        set { SetQuestVariable(37, value); }
    }

    /// <summary>
    /// Tracks which incense dream we are at
    /// </summary>
    public static int IncenseDream
    {
        get 
        { 
            return SaveGame.GetAt(0x60) & 0x3; 
        }
        set 
        {
            if (value >= 3) { value = 0; }//Wrap around on increase.
            byte existingValue = (byte)(SaveGame.GetAt(0x60) & 0xFC);
            existingValue = (byte)((value & 0x3) | existingValue);
            SaveGame.SetAt(0x60, existingValue);
        }
    }

    /// <summary>
    /// Tracks the last day that there was a garamon dream.
    /// </summary>
    public static int DayGaramonDream = -1;

    /// <summary>
    /// Is the orb on tybals level destroyed.
    /// </summary>
    /// Original save loading code had this flagged at bit 6??
    public static bool IsTybalsOrbDestroyed
    {
        get
        {
            return (int)((SaveGame.GetAt(0x61) >> 5) & 0x1) == 1;
        }
        set
        {
            byte existingValue = SaveGame.GetAt(0x61);
            byte mask = (1 << 5);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(0x61, existingValue);
        }
    }

    /// <summary>
    /// Has Garamon been buried. If so talismans can now be sacrificed.
    /// </summary>
    public static bool IsGaramonBuried
    {
        get 
        {
            return (int)((SaveGame.GetAt16(0x62) >> 10) & 0x3)==3;
        }
        set
        {
            int toSet = 0;
            if(value)
            {
                toSet = 3;
            }
            int ExistingVal = SaveGame.GetAt16(0x62);
            ExistingVal &= 0xF3FF;
            ExistingVal |= ((toSet & 0x3) << 4);
            SaveGame.SetAt16(0x62, ExistingVal);
        }
    }

    /// <summary>
    /// Is the cup of wonder found.
    /// </summary>
    public static bool IsCupOfWonderFound
    {
        get
        {
            return (int)((SaveGame.GetAt(0x61) >> 6) & 0x1) == 1;
        }
        set
        {
            byte existingValue = SaveGame.GetAt(0x61);
            byte mask = (1 << 6);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(0x61, existingValue);
        }
    }

    /// <summary>
    /// Is the player fighting in the arena on the pits of carnage level
    /// </summary>
    public static bool FightingInArena
    {
        get
        {
            return ((SaveGame.GetAt(0x64)>>2) & 0x1) == 1;
        }
        set
        {
            byte existingValue = SaveGame.GetAt(0x64);
            byte mask = (1<<2);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(0x64, existingValue);
        }
    }

    /// <summary>
    /// The arena opponents item ids
    /// </summary>
   // public static int[] ArenaOpponents = new int[5];
    
    
    public static int GetArenaOpponent(int index)
    {
        return SaveGame.GetAt(0x361 + index);
    }

    public static void SetArenaOpponent(int index, int characterindex)
    {
        SaveGame.SetAt(0x361 + index,(byte)characterindex);
    }
    /// Has the player eaten a dream plant.
    /// </summary>
    public static bool DreamPlantEaten
    {
        get
        {
            return (SaveGame.GetAt(0x64) & 0x1) == 1;
        }
        set
        {
            byte existingValue = SaveGame.GetAt(0x64);
            byte mask = (1);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(0x64, existingValue);
        }
    }

    /// <summary>
    /// Is the player in the dream world
    /// </summary>
    public static bool InDreamWorld
    {
        get
        {
            return ((SaveGame.GetAt(0x64)>>1) & 0x1) == 1;
        }
        set
        {
            byte existingValue = SaveGame.GetAt(0x64);
            byte mask = (1<<1);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(0x64, existingValue);
        }
    }

    //public static Quest instance;

    //void Awake()
    //{
    //    instance = this;
    //}

    //void Start()
    //{
    //    switch (_RES)
    //    {
    //        case GAME_UW2:
    //            QuestVariablesOBSOLETE = new int[147];
    //            break;
    //        default:
    //            QuestVariablesOBSOLETE = new int[36];
    //            break;
    //    }
    //}


    //    /// <summary>
    //    /// Gets the next incense dream
    //    /// </summary>
    //    /// <returns>The incense dream.</returns>
    //    public static int getIncenseDream()
    //    {
    //        if (IncenseDream >= 3)
    //        {//Loop around
    //            IncenseDream = 0;
    //        }
    //        return IncenseDream++;
    //    }
}
