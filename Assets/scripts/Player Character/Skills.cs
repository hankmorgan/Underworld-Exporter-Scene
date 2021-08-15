using UnityEngine;


public class Skills : UWClass
{


    /// <summary>
    /// Possible results for skill checks per vanilla game skill check function.
    /// </summary>
    public enum SkillRollResult
    {
        CriticalFailure = -1,
        Failure = 0,
        Success = 1,
        CriticalSuccess = 2
    };


    public const int SkillAttack = 1;
    public const int SkillDefense = 2;
    public const int SkillUnarmed = 3;
    public const int SkillSword = 4;
    public const int SkillAxe = 5;
    public const int SkillMace = 6;
    public const int SkillMissile = 7;
    public const int SkillMana = 8;
    public const int SkillLore = 9;
    public const int SkillCasting = 10;
    public const int SkillTraps = 11;
    public const int SkillSearch = 12;
    public const int SkillTrack = 13;
    public const int SkillSneak = 14;
    public const int SkillRepair = 15;
    public const int SkillCharm = 16;
    public const int SkillPicklock = 17;
    public const int SkillAcrobat = 18;
    public const int SkillAppraise = 19;
    public const int SkillSwimming = 20;

    //Character attributes
    public static int STR
    {
        get
        {
            int value = (int)SaveGame.GetAt(0x1F);
            GameWorldController.instance.objDat.critterStats[63].Strength = value;
            return value;
        }
        set
        {            
            GameWorldController.instance.objDat.critterStats[63].Strength = value;
            SaveGame.SetAt(0x1F, (byte)(value));
        }
    }
    public static int DEX
    {
        get
        {
            return (int)SaveGame.GetAt(0x20);
        }
        set
        {
            SaveGame.SetAt(0x20, (byte)(value));
        }
    }
    public static int INT
    {
        get
        {
            return (int)SaveGame.GetAt(0x21);
        }
        set
        {
            SaveGame.SetAt(0x21, (byte)(value));
        }
    }

    //Character skills
    public static int Attack
    {
        get
        {
            return (int)SaveGame.GetAt(0x22);
        }
        set
        {
            SaveGame.SetAt(0x22, (byte)(value));
        }
    }
    public static int Defense
    {
        get
        {
            return (int)SaveGame.GetAt(0x23);
        }
        set
        {
            SaveGame.SetAt(0x23, (byte)(value));
        }
    }
    public static int Unarmed
    {
        get
        {
            return (int)SaveGame.GetAt(0x24);
        }
        set
        {
            SaveGame.SetAt(0x24, (byte)(value));
        }
    }
    public static int Sword
    {
        get
        {
            return (int)SaveGame.GetAt(0x25);
        }
        set
        {
            SaveGame.SetAt(0x25, (byte)(value));
        }
    }
    public static int Axe
    {
        get
        {
            return (int)SaveGame.GetAt(0x26);
        }
        set
        {
            SaveGame.SetAt(0x26, (byte)(value));
        }
    }
    public static int Mace
    {
        get
        {
            return (int)SaveGame.GetAt(0x27);
        }
        set
        {
            SaveGame.SetAt(0x27, (byte)(value));
        }
    }
    public static int Missile
    {
        get
        {
            return (int)SaveGame.GetAt(0x28);
        }
        set
        {
            SaveGame.SetAt(0x28, (byte)(value));
        }
    }
    public static int ManaSkill
    {
        get
        {
            return (int)SaveGame.GetAt(0x29);
        }
        set
        {
            SaveGame.SetAt(0x29, (byte)(value));
        }
    }
    public static int Lore
    {
        get
        {
            return (int)SaveGame.GetAt(0x2A);
        }
        set
        {
            SaveGame.SetAt(0x2A, (byte)(value));
        }
    }
    public static int Casting
    {
        get
        {
            return (int)SaveGame.GetAt(0x2B);
        }
        set
        {
            SaveGame.SetAt(0x2B, (byte)(value));
        }
    }
    public static int Traps
    {
        get
        {
            return (int)SaveGame.GetAt(0x2C);
        }
        set
        {
            SaveGame.SetAt(0x2C, (byte)(value));
        }
    }
    public static int Search
    {
        get
        {
            return (int)SaveGame.GetAt(0x2D);
        }
        set
        {
            SaveGame.SetAt(0x2D, (byte)(value));
        }
    }
    public static int Track
    {
        get
        {
            return (int)SaveGame.GetAt(0x2E);
        }
        set
        {
            SaveGame.SetAt(0x2E, (byte)(value));
        }
    }
    public static int Sneak
    {
        get
        {
            return (int)SaveGame.GetAt(0x2F);
        }
        set
        {
            SaveGame.SetAt(0x2F, (byte)(value));
        }
    }
    public static int Repair
    {
        get
        {
            return (int)SaveGame.GetAt(0x30);
        }
        set
        {
            SaveGame.SetAt(0x30, (byte)(value));
        }
    }
    public static int Charm
    {
        get
        {
            return (int)SaveGame.GetAt(0x31);
        }
        set
        {
            SaveGame.SetAt(0x31, (byte)(value));
        }
    }
    public static int PickLock
    {
        get
        {
            return (int)SaveGame.GetAt(0x32);
        }
        set
        {
            SaveGame.SetAt(0x32, (byte)(value));
        }
    }
    public static int Acrobat
    {
        get
        {
            return (int)SaveGame.GetAt(0x33);
        }
        set
        {
            SaveGame.SetAt(0x33, (byte)(value));
        }
    }
    public static int Appraise
    {
        get
        {
            return (int)SaveGame.GetAt(0x34);
        }
        set
        {
            SaveGame.SetAt(0x34, (byte)(value));
        }
    }
    public static int Swimming
    {
        get
        {
            return (int)SaveGame.GetAt(0x35);
        }
        set
        {
            SaveGame.SetAt(0x35, (byte)(value));
        }
    }


    //private string[] Skillnames = {"","ATTACK","DEFENSE","UNARMED","SWORD","AXE","MACE","MISSILE",
    //	"MANA","LORE","CASTING","TRAPS","SEARCH","TRACK","SNEAK","REPAIR",
    //	"CHARM","PICKLOCK","ACROBAT","APPRAISE","SWIMMING"};

    /// <summary>
    /// An implementation of the vanilla skill check function.
    /// </summary>
    /// <param name="skillValue"></param>
    /// <param name="targetValue"></param>
    /// <returns></returns>
    public static SkillRollResult SkillRoll(int skillValue, int targetValue)
    {
        int score = (skillValue - targetValue) + Random.Range(0, 30); //0 to 29;

        if (score < 0x1d)
        {
            if (score < 0x10)
            {
                if (score < 3)
                {
                    Debug.Log("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (CritFail)");
                    return SkillRollResult.CriticalFailure;//0xffff //critical failure
                }
                else
                {
                    Debug.Log("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (Fail)");
                    return SkillRollResult.Failure; //failure
                }
            }
            else
            {
                Debug.Log("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (Success)");
                return SkillRollResult.Success; //sucess
            }
        }
        else
        { //more than 29
            Debug.Log("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (CritSuccess)");
            return SkillRollResult.CriticalSuccess; //critical sucess
        }
    }



    public static bool TrySkill(int SkillToUse, int CheckValue)
    {//Prototype skill check code
     //Debug.Log ("Skill check Skill :" + Skillnames[SkillToUse] + " (" +GetSkill(SkillToUse) +") vs " + CheckValue);
        return (CheckValue < GetSkill(SkillToUse));
    }


    public static int GetSkill(int SkillNo)
    {//Gets the value for the requested skill

        switch (SkillNo)
        {
            case SkillAttack: return Attack;
            case SkillDefense: return Defense;
            case SkillUnarmed: return Unarmed;
            case SkillSword: return Sword;
            case SkillAxe: return Axe;
            case SkillMace: return Mace;
            case SkillMissile: return Missile;
            case SkillMana: return ManaSkill;
            case SkillLore: return Lore;
            case SkillCasting: return Casting;
            case SkillTraps: return Traps;
            case SkillSearch: return Search;
            case SkillTrack: return Track;
            case SkillSneak: return Sneak;
            case SkillRepair: return Repair;
            case SkillCharm: return Charm;
            case SkillPicklock: return PickLock;
            case SkillAcrobat: return Acrobat;
            case SkillAppraise: return Appraise;
            case SkillSwimming: return Swimming;
            default: return -1;
        }
    }

    public static void InitSkills()
    {
        STR = 0;
        INT = 0;
        DEX = 0;
        Attack = 0;
        Defense = 0;
        Unarmed = 0;
        Sword = 0;
        Axe = 0;
        Mace = 0;
        Missile = 0;
        ManaSkill = 0;
        Lore = 0;
        Casting = 0;
        Traps = 0;
        Search = 0;
        Track = 0;
        Sneak = 0;
        Repair = 0;
        Charm = 0;
        PickLock = 0;
        Acrobat = 0;
        Appraise = 0;
        Swimming = 0;
    }

    public static void AdvanceSkill(int SkillNo, int SkillPoints, int SkillPointCost)
    {//Increase a players skill by the specificed skill let
        if (UWCharacter.Instance.TrainingPoints >= SkillPointCost)
        {
            AdvanceSkill(SkillNo, SkillPoints);
        }
        else
        {//000~001~024~You are not ready to advance. \n
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_are_not_ready_to_advance_));
        }
    }


    public static void AdvanceSkill(int SkillNo, int SkillPoints)
    {//Increase a players skill by the specificed skill let
        switch (SkillNo)
        {
            case SkillAttack: Attack += SkillPoints; Attack = Mathf.Min(30, Attack); break;
            case SkillDefense: Defense += SkillPoints; Defense = Mathf.Min(30, Defense); break;
            case SkillUnarmed: Unarmed += SkillPoints; Unarmed = Mathf.Min(30, Unarmed); break;
            case SkillSword: Sword += SkillPoints; Sword = Mathf.Min(30, Sword); break;
            case SkillAxe: Axe += SkillPoints; Axe = Mathf.Min(30, Axe); break;
            case SkillMace: Mace += SkillPoints; Mace = Mathf.Min(30, Mace); break;
            case SkillMissile: Missile += SkillPoints; Missile = Mathf.Min(30, Missile); break;
            case SkillMana:
                ManaSkill += SkillPoints; ManaSkill = Mathf.Min(30, ManaSkill);
                UWCharacter.Instance.PlayerMagic.MaxMana = ManaSkill * 3;
                break;
            case SkillLore: Lore += SkillPoints; Lore = Mathf.Min(30, Lore); break;
            case SkillCasting: Casting += SkillPoints; Casting = Mathf.Min(30, Casting); break;
            case SkillTraps: Traps += SkillPoints; Traps = Mathf.Min(30, Traps); break;
            case SkillSearch: Search += SkillPoints; Search = Mathf.Min(30, Search); break;
            case SkillTrack: Track += SkillPoints; Track = Mathf.Min(30, Track); break;
            case SkillSneak: Sneak += SkillPoints; Sneak = Mathf.Min(30, Sneak); break;
            case SkillRepair: Repair += SkillPoints; Repair = Mathf.Min(30, Repair); break;
            case SkillCharm: Charm += SkillPoints; Charm = Mathf.Min(30, Charm); break;
            case SkillPicklock: PickLock += SkillPoints; PickLock = Mathf.Min(30, PickLock); break;
            case SkillAcrobat: Acrobat += SkillPoints; Acrobat = Mathf.Min(30, Acrobat); break;
            case SkillAppraise: Appraise += SkillPoints; Appraise = Mathf.Min(30, Appraise); break;
            case SkillSwimming: Swimming += SkillPoints; Swimming = Mathf.Min(30, Swimming); break;
        }
        StatsDisplay.UpdateNow = true;
    }

    public static void SetSkill(int SkillNo, int SkillPoints)
    {//Set a players skill by the specificed skill value
        switch (SkillNo)
        {
            case SkillAttack: Attack = SkillPoints; break;
            case SkillDefense: Defense = SkillPoints; break;
            case SkillUnarmed: Unarmed = SkillPoints; break;
            case SkillSword: Sword = SkillPoints; break;
            case SkillAxe: Axe = SkillPoints; break;
            case SkillMace: Mace = SkillPoints; break;
            case SkillMissile: Missile = SkillPoints; break;
            case SkillMana: ManaSkill = SkillPoints; break;
            case SkillLore: Lore = SkillPoints; break;
            case SkillCasting: Casting = SkillPoints; break;
            case SkillTraps: Traps = SkillPoints; break;
            case SkillSearch: Search = SkillPoints; break;
            case SkillTrack: Track = SkillPoints; break;
            case SkillSneak: Sneak = SkillPoints; break;
            case SkillRepair: Repair = SkillPoints; break;
            case SkillCharm: Charm = SkillPoints; break;
            case SkillPicklock: PickLock = SkillPoints; break;
            case SkillAcrobat: Acrobat = SkillPoints; break;
            case SkillAppraise: Appraise = SkillPoints; break;
            case SkillSwimming: Swimming = SkillPoints; break;
        }
        StatsDisplay.UpdateNow = true;
    }


    /// <summary>
    /// Gets the name of the skill from the strings.pak data 
    /// </summary>
    /// <param name="skillNo"></param>
    /// <returns></returns>
	public static string GetSkillName(int skillNo)
    {
        return StringController.instance.GetString(2, 31 + skillNo);
    }

    /// <summary>
    /// Tracks the monsters in the specified range around the player.
    /// If the skill check is failed then false information is generated for the player.
    /// </summary>
    /// <param name="origin">Gameobject at the centre of where to check from</param>
    /// <param name="Range">Range to include</param>
    /// <param name="SkillCheckPassed">If set to <c>true</c> skill check passed.</param>
    public static void TrackMonsters(GameObject origin, float Range, bool SkillCheckPassed)
    {
        if (SkillCheckPassed)
        {
            int[] NoOfMonsters = new int[8];
            for (int i = 0; i <= NoOfMonsters.GetUpperBound(0); i++)
            {
                NoOfMonsters[i] = 0;
            }
            bool NPCFound = false;
            foreach (Collider Col in Physics.OverlapSphere(origin.transform.position, Range))
            {
                if (Col.gameObject.GetComponent<NPC>() != null)
                {
                    NoOfMonsters[Compass.getCompassHeadingOffset(origin, Col.gameObject)]++;
                    NPCFound = true;
                }
            }
            if (NPCFound)
            {
                int MaxNoOfMonsters = 0;
                int MaxOffsetIndex = 0;

                for (int i = 0; i <= NoOfMonsters.GetUpperBound(0); i++)
                {
                    if (NoOfMonsters[i] > MaxNoOfMonsters)
                    {
                        MaxNoOfMonsters = NoOfMonsters[i];
                        MaxOffsetIndex = i;
                    }
                }
                string Heading = StringController.instance.GetString(1, StringController.str_to_the_north + MaxOffsetIndex);
                switch (MaxNoOfMonsters)
                {//TODO: What is a few?
                    case 0://Should not happen
                           //000~001~062~You detect no monster activity. \n	
                        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_no_monster_activity_) + Heading); break;
                    case 1://Single creature.
                           //000-001-059 //You detect a creature
                        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_a_creature_) + Heading); break;
                    case 2:
                    case 3:
                        //000-001-060 //You detect a few creatures
                        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_a_few_creatures_) + Heading); break;

                    default:
                        //000~001~061~You detect the activity of many creatures 
                        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_the_activity_of_many_creatures_) + Heading); break;

                }
            }
            else
            {
                //000~001~062~You detect no monster activity. \n	
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_no_monster_activity_));
            }
        }
        else
        {//Skill check has failed. Randomly tell the player something
            string Heading = StringController.instance.GetString(1, Random.Range(StringController.str_to_the_north, StringController.str_to_the_northwest + 1));
            switch (Random.Range(0, 5))
            {
                case 0:
                    //000~001~062~You detect no monster activity. \n	
                    UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_no_monster_activity_) + Heading); break;
                case 1://Single creature.
                       //000-001-059 //You detect a creature
                    UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_a_creature_) + Heading); break;
                case 2:
                case 3:
                    //000-001-060 //You detect a few creatures
                    UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_a_few_creatures_) + Heading); break;

                default:
                    //000~001~061~You detect the activity of many creatures 
                    UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_detect_the_activity_of_many_creatures_) + Heading); break;
            }
        }

    }


    /// <summary>
    /// Adds a bonus to skill increases based on a difference between a skill and its governing attributes.
    /// </summary>
    /// <param name="skillNo"></param>
    /// <returns></returns>
	public static int getSkillAttributeBonus(int skillNo)
    {
        int GoverningSkill = 0;
        int currentSkillScore = Skills.GetSkill(skillNo);

        switch (skillNo)
        {
            case SkillAttack:
            case SkillDefense:
            case SkillUnarmed:
            case SkillSword:
            case SkillAxe:
            case SkillMace:
            case SkillSwimming:
                GoverningSkill = Skills.STR;
                break;
            case SkillMissile:
            case SkillMana:
            case SkillLore:
            case SkillCasting:
            case SkillCharm:
            case SkillTrack:
                GoverningSkill = Skills.INT;
                break;

            case SkillTraps:
            case SkillSearch:
            case SkillSneak:
            case SkillRepair:
            case SkillPicklock:
            case SkillAcrobat:
            case SkillAppraise:
                GoverningSkill = Skills.DEX;
                break;
        }

        int SkillDiff = Mathf.Max(0, GoverningSkill - currentSkillScore);
        return SkillDiff / 5;
    }


    /// <summary>
    /// Rolls the dice on any skill check or combat swing.
    /// </summary>
    /// <returns>The roll.</returns>
    /// <param name="critter">Critter.</param>
    /// <param name="minRoll">Minimum roll.</param>
    /// <param name="MaxRoll">Max roll.</param>
    public static int DiceRoll(int minRoll, int MaxRoll)
    {

        switch (UWCharacter.Instance.isLucky)
        {
            case UWCharacter.LuckState.Cursed:
                {//Give a -5 penalty to all rolls.
                    int roll = Random.Range(minRoll, MaxRoll) - 5;
                    if (roll < 0) { roll = 0; }
                    roll = Mathf.Min(roll, MaxRoll);
                    return roll;
                }
            case UWCharacter.LuckState.Lucky:
                {//Give a +5 bonus to all rolls.
                    int roll = Random.Range(minRoll, MaxRoll);
                    roll = Mathf.Min(roll + 5, MaxRoll);
                    return roll;
                }
            case UWCharacter.LuckState.Neutral:
            default:
                {
                    return Random.Range(minRoll, MaxRoll);
                }
        }
    }

}
