﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
The basic character. Stats and interaction.
 */
public class UWCharacter : Character
{
    //Jump defaults
    public const float baseJumpHeight = 0.2f;
    public const float extraJumpHeight = 0.8f;
    public const float extraJumpHeightLeap = 1.6f;

    public string[] LayersForRay = new string[] { "Water", "MapMesh", "Lava", "Ice" };
    public Vector3 Rayposition;//= //transform.position;
    public Vector3 Raydirection = Vector3.down;
    public float Raydistance = 1.0f;
    int RayMask; //= LayerMask.GetMask(LayersForRay);

    public const int CharClassFighter = 0;
    public const int CharClassMage = 1;
    public const int CharClassBard = 2;
    public const int CharClassTinker = 3;
    public const int CharClassDruid = 4;
    public const int CharClassPaladin = 5;
    public const int CharClassRanger = 6;
    public const int CharClassShepard = 7;

    public static UWCharacter Instance;

    [Header("Player Position Status")]
    public int CurrentTerrain;
    public TerrainDatLoader.TerrainTypes terrainType;
    public bool Grounded;
    public bool onIce;
    public bool onIcePrev;
    public bool onLava;
    public bool onBridge;
    public Vector3 IceCurrentVelocity = Vector3.zero;

    [Header("Player Movement Status")]
    public SpellEffect[] ActiveSpell = new SpellEffect[3];      //What effects and enchantments (eg from items are equipped on the player)
    public SpellEffect[] PassiveSpell = new SpellEffect[10];
    private bool _isSwimming;
    public bool isSwimming
    {
        get
        {
            return _isSwimming;
        }
        set
        {
            if (!_isSwimming)
            {
                //Player is not already swimming          
                if (value == true)
                {
                    //Play splash effect
                    PlaySplashSound();
                }
            }
            _isSwimming = value;

        }

    }
    public int StealthLevel; //The level of stealth the character has.
    public int Resistance; //DR from spells.
    public bool Paralyzed
    {
        get { return ParalyzeTimer > 0; }
    }
    public int ParalyzeTimer
    {//0x306
        get
        {
            if(_RES==GAME_UW2)
            {
                return SaveGame.GetAt(0x306);
            }
           else
            {
                return 0;
            }
        }
        set
        {
            if (_RES == GAME_UW2)
            {
                SaveGame.SetAt(0x306, (byte)value);
            }
        }
    }
    public float currYVelocity;
    public float fallSpeed;
    public float braking;
    public float bounceMult = 1f;
    public Vector3 BounceMovement;
    public CameraBob cameraBob;
    // <summary>
    // Is the player fleeing from combat (recently attacked and no weapon drawn)
    // </summary>
    //public bool Fleeing;
    /// <summary>
    /// Weapon drawn music
    /// </summary>
    public bool WeaponDrawn;

    [Header("Player Magic Status")]
    public bool isFlying;
    public bool isLeaping;
    public bool isRoaming;
    public bool isFloating;
    public bool isSpeeding;
    public bool isWaterWalking;
    public bool isTelekinetic;
    public bool isTimeFrozen;
    public enum LuckState
    {
        Cursed,
        Neutral,
        Lucky
    };

    [SerializeField]
    LuckState _IsLucky = LuckState.Neutral;
    public LuckState isLucky
    {
        get
        {
            return _IsLucky;
        }
        set
        {
            {
                switch (_IsLucky)
                {
                    case LuckState.Cursed:
                        {
                            switch (value)
                            {
                                case LuckState.Cursed: break;
                                case LuckState.Neutral:
                                case LuckState.Lucky: _IsLucky = LuckState.Neutral; break;
                            }
                            break;
                        }
                    case LuckState.Neutral:
                        {
                            _IsLucky = value;
                            break;
                        }
                    case LuckState.Lucky:
                        {
                            switch (value)
                            {
                                case LuckState.Cursed: _IsLucky = LuckState.Neutral; break;
                                case LuckState.Neutral:
                                case LuckState.Lucky: _IsLucky = value; break;
                            }
                            break;
                        }
                }
            }
        }
    }
    public bool isBouncy;

    //Character Status
    public int FoodLevel //0-255 range.
    {
        get
        {
            return SaveGame.GetAt(0x3a);
        }
        set
        {
            if (value > 255)
            {
                value = 255;
            }
            if (value < 0)
            {
                value = 0;
            }
            SaveGame.SetAt(0x3a, (byte)value);
        }
    }
    public int Fatigue   //0-29 range
    {
        get
        {
            return SaveGame.GetAt(0x3b);
        }
        set
        {
            if (value > 255)
            {
                value = 255;
            }
            if (value < 0)
            {
                value = 0;
            }
            SaveGame.SetAt(0x3b, (byte)value);
        }
    }

    public int Intoxication //0-63 range
    {
        get
        {
            if (_RES==GAME_UW2)
            {
                //Debug.Log("Intoxication in UW2");
            }
            return (SaveGame.GetAt16(0x62) >> 4) & 0x3f;
        }
        set
        {
            if (_RES == GAME_UW2)
            {
               // Debug.Log("Intoxication in UW2");
            }
            int ExistingVal = SaveGame.GetAt16(0x62);
            ExistingVal &= 0xFC0F;
            ExistingVal |= ((value & 0x3f) << 4);
            SaveGame.SetAt16(0x62, ExistingVal);
        }
    }

    /// <summary>
    /// How badly poisoned is the player. The higher the value the longer poison ticks down for.
    /// </summary>
    public short play_poison
    {
        get
        {
            switch(_RES)
            {
                case GAME_UW2:
                    return (short)((SaveGame.GetAt(0x61)>>1) & 0xf);
                default:
                    return (short)((SaveGame.GetAt(0x60)>>2) & 0xf);
            }            
        }
        set
        {
            short currentVal = play_poison;
            if (value < 0) { value = 0; }
            if (currentVal == 0 && value != 0)
            {//Clears poisoning on the flask.
                UWHUD.instance.FlaskHealth.UpdatePoisonDisplay(true);
            }
            else if (currentVal != 0 && value == 0)
            {//Sets poisoning on the flask
                UWHUD.instance.FlaskHealth.UpdatePoisonDisplay(false);
            }
            switch(_RES)
            {
                case GAME_UW2:
                    {
                        int existingval = SaveGame.GetAt(0x61) & 0xE1;
                        existingval |= (value << 1);
                        SaveGame.SetAt(0x61, (byte)existingval);
                        break;
                    }                    
                default:
                    {
                        int existingval = SaveGame.GetAt(0x60) & 0xC3;
                        existingval |= (value << 2);
                        SaveGame.SetAt(0x61, (byte)existingval);
                        break;
                    }
            }
            UWHUD.instance.FlaskHealth.UpdateFlaskDisplay();
        }
    }

    public float lavaDamageTimer;//How long before applying lava damage
    private bool InventoryReady = false;
    /// <summary>
    /// Player character near death.
    /// </summary>
    public bool Injured
    {
        get
        {
            return Instance.CurVIT <= 10;
        }
    }
    /// <summary>
    /// PLayer character is dead
    /// </summary>
    public bool Death;

    [Header("Save game")]
    /// <summary>
    /// The xor encryption key for the save file
    /// </summary>
    public int XorKey = 0xD9;
    public bool decode = true;//decodes a save file
    public bool recode = true;//recodes a save file at indextochange with newvalue
    public bool recode_cheat = true;//recodes a save file into an all 30s character
    public int IndexToRecode = 0;
    public int ValueToRecode = 0;

    //Character related info
    public string CharName
    {
        get
        {
            var _charname = "";
            for (int i = 1; i < 14; i++)
            {
                var alpha = SaveGame.GetAt(i);
                if (alpha.ToString() != "\0")
                {
                    _charname += (char)alpha;
                }
            }
            return _charname;
        }
        set
        {
            var _chararray = value.ToCharArray();
            for (int i = 1; i < 14; i++)
            {
                if (i - 1 < value.Length)
                {
                    SaveGame.SetAt(i, (byte)_chararray[i - 1]);
                }
                else
                {
                    SaveGame.SetAt(i, (byte)0);
                }
            }
        }
    }

    public int Body//Which body/portrait this character has 
    {
        get
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            return (SaveGame.GetAt(offset) >> 2) & 0x7;
        }
        set
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            byte existingValue = SaveGame.GetAt(offset);
            existingValue = (byte)(existingValue & 0xE3); //mask out body.
            value = value << 2;
            existingValue = (byte)(existingValue | value);
            SaveGame.SetAt(offset, existingValue);
        }
    }

    public int CharClass
    {
        get
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            return (SaveGame.GetAt(offset) >> 5) & 0x7;
        }
        set
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            byte existingValue = SaveGame.GetAt(offset);
            existingValue = (byte)(existingValue & 0x1F); //mask out charclass
            value = value << 5;
            existingValue = (byte)(existingValue | value);
            SaveGame.SetAt(offset, existingValue);
        }
    }

    public int CharLevel
    {
        get { return SaveGame.GetAt(0x3E); }
        set { SaveGame.SetAt(0x3E, (byte)value); }
    }

    public int EXP
    {
        get
        {
            return (int)SaveGame.GetAt32(0x4F) / 10;
        }
        set
        {
            SaveGame.SetAt32(0x4F, value * 10);
        }
    }

    public int TrainingPoints
    {
        get { return SaveGame.GetAt(0x53); }
        set { SaveGame.SetAt(0x53, (byte)value); }
    }

    public bool isFemale
    {
        get
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            return ((int)(SaveGame.GetAt(offset) >> 1) & 0x1) == 0x1;
        }
        set
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            byte existingValue = SaveGame.GetAt(offset);
            byte mask = (1 << 1);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(offset, existingValue);
        }
    }

    public bool isLefty
    {
        get
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            return ((int)(SaveGame.GetAt(offset)) & 0x1) == 0x0;
        }
        set
        {
            int offset = 0x65;
            if (_RES == GAME_UW2) { offset = 0x66; }
            byte existingValue = SaveGame.GetAt(offset);
            byte mask = (1);
            if (value)
            {//set
                existingValue |= mask;
            }
            else
            {//unset
                existingValue = (byte)(existingValue & (~mask));
            }
            SaveGame.SetAt(offset, existingValue);
        }
    }

    //Player save game co-ordinates
    public int x_position
    {
        get { return (int)SaveGame.GetAt16(0x55); }
        set { SaveGame.SetAt16(0x55, value); }
    }

    public int y_position
    {
        get { return (int)SaveGame.GetAt16(0x57); }
        set { SaveGame.SetAt16(0x57, value); }
    }

    public int z_position
    {
        get { return (int)SaveGame.GetAt16(0x59); }
        set { SaveGame.SetAt16(0x59, value); }
    }
    public int heading
    {
        get { return (int)SaveGame.GetAt(0x5c); }
        set { SaveGame.SetAt(0x5c, (byte)value); }
    }

    /// <summary>
    /// X Co-ordinate for the player when they return from a plant induced dream in the void
    /// </summary>
    public int x_position_dream
    {//0x2fb
        get
        {
            return SaveGame.GetAt16(0x2fb);
        }
        set
        {
            SaveGame.SetAt16(0x2fb, value);
        }
    }

    /// <summary>
    /// Y Co-ordinate for the player when they return from a plant induced dream in the void
    /// </summary>
    public int y_position_dream
    {//0x2fd
        get
        {
            return SaveGame.GetAt16(0x2fd);
        }
        set
        {
            SaveGame.SetAt16(0x2fd, value);
        }
    }


    [Header("Speeds")]
    public float flySpeed;
    public float walkSpeed;
    public float speedMultiplier = 1.0f;
    public float swimSpeedMultiplier = 1.0f;//Is set to a fractional value when swimming.
    public float SwimTimer = 0.0f;  //How long has the player been swimming. Used to determine when to start applying damage.
    public float SwimDamageTimer; //For timing out drowning damage.


    [Header("Character Modules")]
    //Character skills
    //public Skills PlayerSkills;
    //Magic system
    public Magic PlayerMagic;
    //Inventory System
    public PlayerInventory playerInventory;
    //Combat System
    public UWCombat PlayerCombat;

    //public Feet playerFeet;

    /// <summary>
    /// In UW1 this is where the Silver tree has been planted
    /// </summary>
    /// If 0 it has not being planted.
    /// This value -1 gives the level where the player will resurrect to.
    public short ResurrectLevel
    {
        get
        {
            return (short)(SaveGame.GetAt(0x5F) >> 4);
        }
        set
        {
            int existingValue = SaveGame.GetAt(0x5F);
            existingValue = existingValue & 0x0F;//Preserve low nibble containing moonstone.
            existingValue = existingValue | ((value & 0xF) << 4); //Set high nibble to value
            SaveGame.SetAt(0x5F, (byte)existingValue);
        }
    }
    public Vector3 ResurrectPosition = Vector3.zero;//TODO change this to a search.
    public Vector3 MoonGatePosition = Vector3.zero;
    public short MoonGateLevel //= 2//Domain of the mountainmen
    {
        get
        {
            if (_RES == GAME_UW2)
            { Debug.Log("moonstone usage in UW2. Confirm this is correct"); }
            return (short)(SaveGame.GetAt(0x5F) & 0xF);
        }
        set
        {
            if (_RES == GAME_UW2)
            { Debug.Log("moonstone usage in UW2. Confirm this is correct"); }
            int existingValue = SaveGame.GetAt(0x5F);
            existingValue = existingValue & 0xF0;//Preserve high nibble containing silver tree.
            existingValue = existingValue | (value & 0xF); //Set high nibble to value
            SaveGame.SetAt(0x5F, (byte)existingValue);
        }
    }

    [Header("Teleportation")]
    public float teleportedTimer = 0f;
    public bool JustTeleported = false;
    /// <summary>
    /// The dream return position when you are dreaming in the void.
    /// </summary>
    public short DreamReturnTileX
    {
        get
        {
            Vector3 DreamReturn = new Vector3(x_position_dream / SaveGame.Ratio, 0f, y_position_dream / SaveGame.Ratio);
            return (short)(DreamReturn.x / 1.2f);
        }
        set
        {
            Vector3 dreamReturn = CurrentTileMap().getTileVector(value, DreamReturnTileY);
            x_position_dream = (int)(dreamReturn.x * SaveGame.Ratio);
        }
    }
    public short DreamReturnTileY
    {
        get
        {
            Vector3 DreamReturn = new Vector3(x_position_dream / SaveGame.Ratio, 0f, y_position_dream / SaveGame.Ratio);
            return (short)(DreamReturn.z / 1.2f);
        }
        set
        {
            Vector3 dreamReturn = CurrentTileMap().getTileVector(DreamReturnTileX, value);
            y_position_dream = (int)(dreamReturn.x * SaveGame.Ratio);
        }
    }
    /// <summary>
    /// The dream return level when you are dreaming in the void.
    /// </summary>
    /// Offset by -1 for actual level. 0 is no valid return level
    public short DreamReturnLevel
    {//0x301:
        get
        {
            return SaveGame.GetAt(0x301);
        }
        set
        {
            SaveGame.SetAt(0x301, (byte)value);
        }
    }
    public float DreamWorldTimer = 30f;//Not sure what values controls the time spent in dream world
    public Vector3 TeleportPosition;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        XAxis.enabled = false;
        YAxis.enabled = false;
        MouseLookEnabled = false;
        RayMask = LayerMask.GetMask(LayersForRay);
        StartCoroutine(playfootsteps());
    }

    public override void Begin()
    {
        base.Begin();
        if (_RES == GAME_SHOCK) { return; }
        InventoryReady = false;
        XAxis.enabled = false;
        YAxis.enabled = false;
        MouseLookEnabled = false;
        Cursor.SetCursor(UWHUD.instance.CursorIconBlank, Vector2.zero, CursorMode.ForceSoftware);
        InteractionMode = DefaultInteractionMode;

        //Tells other objects about this component;

        //RuneSlot.playerUW=this.GetComponent<UWCharacter>();
        //Magic.playerUW=this.GetComponent<UWCharacter>();
        //SpellProp.playerUW = this.gameObject.GetComponent<UWCharacter>();

        UWHUD.instance.InputControl.text = "";
        UWHUD.instance.MessageScroll.Clear();

        switch (Instance.Body)
        {
            case 0:
            case 2:
            case 3:
            case 4:
                GameWorldController.instance.weapongr = new WeaponsLoader(0); break;
            default:
                GameWorldController.instance.weapongr = new WeaponsLoader(1); break;
        }
    }

    void PlayerDeath()
    {//CHeck if the player has planted the seed and if so send them to that position.
     //mus.Death=true;
     //TODO:Turn of the player camera
     //UWCharacter.Instance.playerCam.cullingMask=31;
     //MusicController.instance.Death=true;
        Death = true;
        InteractionMode = InteractionModeUse;
        UWHUD.instance.window.UnSetFullScreen();


        UWHUD.instance.wpa.SetAnimation = -1;
        switch (_RES)
        {
            case GAME_UW2:
                DeathHandlingUW2(); break;
            default:
                DeathHandlingUW1(); break;
        }


        //Cancel the spell
        if (PlayerMagic.ReadiedSpell != "")
        {
            PlayerMagic.ReadiedSpell = "";
            UWHUD.instance.CursorIcon = UWHUD.instance.CursorIconDefault;
        }
    }

    /// <summary>
    /// Death handling rules for UW1
    /// </summary>
    private void DeathHandlingUW1()
    {
        if (UWHUD.instance.CutScenesSmall != null)
        {
            if (
                    (
                            ResurrectLevel != 0
                    )
                    &&
                    (
                            !
                            (
                                    (_RES == GAME_UW1)
                                    &&
                                    (GameWorldController.instance.dungeon_level == 8) //No resurrect in the ethereal void.
                            )
                    )
            )
            {
                UWHUD.instance.CutScenesSmall.anim.SetAnimation = "cs402.n01";//="Death_With_Sapling";
            }
            else
            {
                UWHUD.instance.CutScenesSmall.anim.SetAnimation = "cs403.n01";//Final death
            }
        }
    }
    /// <summary>
    /// Death handling rules for UW2
    /// </summary>
    private void DeathHandlingUW2()
    {
        //If you are killed by a "ally" npc in castle british you will resurrect in jail (quest 112)
        //      (if you have yet to talk to british at start of game you won't be let out)
        //If you are killed by an enemy npc in castle british you will die.
        //If you die while in an alternate dimension you will spawn in the gem chamber.

        switch (GameWorldController.instance.dungeon_level)
        {
            case 0://Britannia castle
                {
                    if (WasIKilledByAFriend())
                    {
                        //Quest.QuestVariablesOBSOLETE[112] = 1;
                        Quest.SetQuestVariable(112, 1);
                    }
                    if (Quest.GetQuestVariable(112) == 1)
                    {//You have been fighting your allies. You will awake in jail
                        UWHUD.instance.CutScenesSmall.anim.SetAnimation = "uw2resurrecttransition";
                        return;
                    }
                    else
                    {
                        UWHUD.instance.CutScenesSmall.anim.SetAnimation = "cs403.n01";//Final death
                        return;
                    }
                }
            case 1://Britannia sewer
            case 2:
            case 3:
            case 4:
                //You're gonna die down here
                UWHUD.instance.CutScenesSmall.anim.SetAnimation = "cs403.n01";//Final death
                return;
            default://resurrect in gem chamber
                UWHUD.instance.CutScenesSmall.anim.SetAnimation = "uw2resurrecttransition";
                return;
        }
    }

    bool WasIKilledByAFriend()
    {
        if (LastEnemyToHitMe != null)
        {
            if (LastEnemyToHitMe.GetComponent<NPC>() != null)
            {
                int whoami = LastEnemyToHitMe.GetComponent<NPC>().npc_whoami;
                switch (whoami)
                {
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.MaleGuard:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Nystrul:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Charles:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Dupre:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Geoffrey:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Iolo:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Julia:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Miranda:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Nanna:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Nell:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Nelson:

                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Tory:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.LordBritish:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Feridwyn:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.FemaleGuard:
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Syria:
                        return true;
                    case (int)a_hack_trap_castle_npcs.BritanniaNPCS.Patterson:
                        //TODO:Check what happens if patterson kills you before you unmask him as the traitor
                        return false;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Resurrects the player.
    /// </summary>
    public static void ResurrectPlayerUW1()
    {
        ResurrectCommon();

        if (GameWorldController.instance.dungeon_level != Instance.ResurrectLevel - 1)
        {
            if (_RES == GAME_UW1)
            {
                //Special case for the magic drain effect in UW1
                ResetTrueMana();
            }
            GameWorldController.instance.SwitchLevel((short)(Instance.ResurrectLevel - 1));
        }
        Instance.gameObject.transform.position = Instance.ResurrectPosition;

    }

    /// <summary>
    /// Common steps in resurrection.
    /// </summary>
    private static void ResurrectCommon()
    {
        Instance.Death = false;
        //UWCharacter.Instance.Fleeing = false;
        if (MusicController.instance != null)
        {
            MusicController.instance.Combat = false;
            MusicController.LastAttackCounter = 0.0f;
        }
        Instance.playerCam.cullingMask = HudAnimation.NormalCullingMask;
        Instance.isSwimming = false;
        Instance.play_poison = 0;
        Instance.CurVIT = Random.Range(Instance.MaxVIT / 2, Instance.MaxVIT);
    }

    public static void ResurrectPlayerUW2()
    {
        //If you are killed by a "ally" npc in castle british you will resurrect in jail (quest 112)
        //      (if you have yet to talk to british at start of game you won't be let out)
        //If you are killed by an enemy npc in castle british you will die.
        //If you die while in an alternate dimension you will spawn in the gem chamber.
        ResurrectCommon();
        switch (GameWorldController.instance.dungeon_level)
        {
            case 0://resurrect in jail
                float targetX = 42 * 1.2f + 0.6f;
                float targetY = 38 * 1.2f + 0.6f;
                float Height = CurrentTileMap().GetFloorHeight(42, 38) * 0.15f;
                Instance.transform.position = new Vector3(targetX, Height + 0.3f, targetY);
                a_hack_trap_castle_npcs.MakeEveryoneFriendly();
                //Move lord british
                ObjectInteraction obj = ObjectLoader.getObjectIntAt(NPC.findNpcByWhoAmI(142));
                if (obj != null)
                {
                    NPC npc = obj.GetComponent<NPC>();
                    if (npc != null)
                    {
                        if (npc.Agent == null)
                        {
                            npc.transform.position = CurrentTileMap().getTileVector(42, 35);
                        }
                        else
                        {
                            npc.Agent.Warp(CurrentTileMap().getTileVector(42, 35));
                        }
                    }
                }
                else
                {
                    Debug.Log("Lord British is missing. This should not happen.");
                }
                //008~009~004~You awaken in jail. \n
                //This message is supposed to be called by a scheduled trigger.
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(9, 4));
                break;
            default://resurrect in the gem chamber
                //000~001~361~You regain awareness in the cavern containing the pulsating blackrock gem. \n
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 361));
                GameWorldController.instance.SwitchLevel(4, 30, 39);//TODO:confirm exact co-ords
                break;
        }

    }

    public override float GetUseRange()
    {
        if (EditorMode)
        {
            return 100;
        }
        if (isTelekinetic == true)
        {
            return useRange * 8.0f;
        }
        else
        {
            if (playerInventory.ObjectInHand == null)
            {
                return useRange;
            }
            else
            {//Test if this is a pole. If so extend the use range by a small amount.
                if (playerInventory.ObjectInHand != null)
                {
                    switch (playerInventory.ObjectInHand.GetItemType())
                    {
                        case ObjectInteraction.POLE:
                            return useRange * 2;
                    }
                }
                return useRange;
            }
        }
    }

    public override float GetPickupRange()
    {
        if (isTelekinetic == true)
        {
            return pickupRange * 8.0f;
        }
        else
        {
            return pickupRange;
        }
    }

    void FlyingMode()
    {
        playerMotor.movement.maxFallSpeed = 0.0f;
        playerMotor.movement.maxForwardSpeed = flySpeed * speedMultiplier;
        playerMotor.movement.maxSidewaysSpeed = playerMotor.movement.maxForwardSpeed * 2 / 3;
        playerMotor.movement.maxBackwardsSpeed = playerMotor.movement.maxForwardSpeed / 3;

        if (((Input.GetKeyDown(GameWorldController.instance.config.FlyUp)) || (Input.GetKey(GameWorldController.instance.config.FlyUp))) && (WindowDetect.WaitingForInput == false))
        {
            //Fly up
            this.GetComponent<CharacterController>().Move(new Vector3(0, 0.2f * Time.deltaTime * speedMultiplier, 0));
        }
        else
        if (((Input.GetKeyDown(GameWorldController.instance.config.FlyDown)) || (Input.GetKey(GameWorldController.instance.config.FlyDown))) && (WindowDetect.WaitingForInput == false))
        {
            //Fly down
            this.GetComponent<CharacterController>().Move(new Vector3(0, -0.2f * Time.deltaTime * speedMultiplier, 0));
        }
    }

    /// <summary>
    /// Management of the character when in water.
    /// </summary>
    void SwimmingEffects()
    {
        if (InteractionMode == InteractionModeAttack)
        {
            InteractionMode = InteractionModeWalk;
        }

        swimSpeedMultiplier = Mathf.Max((float)(Skills.Swimming / 30.0f), 0.3f);//TODO:redo me
        SwimTimer += Time.deltaTime;
        //Not sure of what UW does here but for the moment 45seconds of damage gree swimming then 15s per skill point
        if (SwimTimer >= 05.0f + Skills.Swimming * 15.0f)
        {
            SwimDamageTimer += Time.deltaTime;
            if (SwimDamageTimer >= 10.0f)//Take Damage every 10 seconds.
            {
                ApplyDamage(1);
                SwimDamageTimer = 0.0f;
            }
        }
        else
        {
            SwimDamageTimer = 0.0f;
        }

        if (ObjectInteraction.PlaySoundEffects)
        {//Play splashing sounds.
            if (!footsteps.isPlaying)
            {
                //switch (Random.Range(1, 3))
                //{
                //    case 1:
                footsteps.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_SPLASH_1];
                //        break;
                //    case 2:
                //    default:
                //        aud.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_SPLASH_2];//Enter water
                //        break;
                //}
                footsteps.Play();
            }
        }
    }

    void PlaySplashSound()
    {
        if (ObjectInteraction.PlaySoundEffects)
        {
            if (!footsteps.isPlaying)
            {
                footsteps.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_SPLASH_2];
                footsteps.Play();
            }
        }
    }

    public override void Update()
    {
        if (GameWorldController.instance.AtMainMenu) { return; }
        if ((_RES == GAME_SHOCK) || (_RES == GAME_TNOVA))
        {
            if (isFlying)
            {
                flySpeed = 10f;
                FlyingMode();
            }
            return;
        }

        //Check if player is on ground.
        Grounded = IsGrounded();

        TerrainAndCurrentsUpdate();

        base.Update();

        FallDamageUpdate();

        if (_RES == GAME_UW2)
        {
            BounceUpdate();//For handling the bounce spell
        }

        if (EditorMode)
        {
            CurVIT = MaxVIT;
        }

        TeleportUpdate();//Control positioning after teleportation.

        InventoryUpdate();//Update inventory display

        if ((WindowDetect.WaitingForInput == true) && (Instrument.PlayingInstrument == false))//TODO:Make this cleaner!!
        {//TODO: This should be in window detect
            UWHUD.instance.InputControl.Select();
        }
        if ((CurVIT <= 0) && (Death == false))
        {
            PlayerDeath();
            return;
        }

        if (Death == true)
        {
            //Still processing death.
            return;
        }
        //if (_RES == GAME_UW2)
        //{
        //    ParalyzeUpdate();
        //}
        if (playerCam.enabled == true)
        {
            SwimUpdate();
        }
        //if (play_poison > 0)
        //{
        //    PoisonUpdate();
        //}

        playerMotor.enabled = ((!Paralyzed) && (!GameWorldController.instance.AtMainMenu) && (!ConversationVM.InConversation));

        if (_RES == GAME_UW2)
        {
            DreamWorldUpdate();
        }

        if ((isFlying) && (!Grounded))
        {//Flying spell
            FlyingMode();
        }
        else
        {
            if (isFloating)
            {
                playerMotor.movement.maxFallSpeed = 0.1f;//Default
            }
            else
            {
                playerMotor.movement.maxFallSpeed = 20.0f;//Default
                playerMotor.movement.maxForwardSpeed = walkSpeed * speedMultiplier * swimSpeedMultiplier;
                playerMotor.movement.maxSidewaysSpeed = playerMotor.movement.maxForwardSpeed * 2 / 3;
                playerMotor.movement.maxBackwardsSpeed = playerMotor.movement.maxForwardSpeed / 3;
            }
        }

        if (isLeaping)
        {//Jump spell						
            playerMotor.jumping.baseHeight = baseJumpHeight;
            playerMotor.jumping.extraHeight = extraJumpHeightLeap;
        }
        else
        {
            playerMotor.jumping.baseHeight = baseJumpHeight;
            playerMotor.jumping.extraHeight = extraJumpHeight;
        }

        if (isRoaming)//Magic eye spell
        {
            playerMotor.movement.maxFallSpeed = 0.0f;
        }
        if ((!isSpeeding) && (!onIce))
        {
            speedMultiplier = 1f;
        }

        //MusicController.instance.
        WeaponDrawn = (InteractionMode == InteractionModeAttack);

        if (PlayerMagic.ReadiedSpell != "")
        {//Player has a spell thats about to be cast. All other activity is ignored.	
            SpellMode();
            return;
        }

        if (UWHUD.instance.window.JustClicked == false)
        {
            if (Paralyzed == false)
            {
                PlayerCombat.PlayerCombatIdle();
            }
        }


        if (onLava == true)
        {
            OnLavaUpdate();
        }
        else
        {
            lavaDamageTimer = 0;
        }

        //Calculate how visible the player is.
        if (LightActive)//The player has a light and is therefore visible at max range.
        {
            DetectionRange = BaseDetectionRange;
        }
        else
        {//=MinRange+( (MaxRange-MinRange) * ((30-B4)/30))
            DetectionRange = MinDetectionRange + ((BaseDetectionRange - MinDetectionRange) * ((30.0f - (GetBaseStealthLevel() + StealthLevel)) / 30.0f));
        }
    }

    /// <summary>
    /// Updates the player if they are subject to ice, water currents and the like.
    /// </summary>
    private void TerrainAndCurrentsUpdate()
    {
        switch (terrainType)
        {//Check if the player is subject to a water current.
            case TerrainDatLoader.TerrainTypes.Lava:
            case TerrainDatLoader.TerrainTypes.Lavafall:
                {
                    onLava = true;
                    onIce = false;
                    isSwimming = false;
                    break;
                }
            case TerrainDatLoader.TerrainTypes.Ice_wall:
            case TerrainDatLoader.TerrainTypes.Ice_walls:
                {
                    if (onIcePrev == false)
                    {
                        IceCurrentVelocity = playerMotor.movement.velocity.normalized * 3f;
                    }
                    onIce = true;
                    onLava = false;
                    isSwimming = false;
                    break;
                }
            case TerrainDatLoader.TerrainTypes.WaterFlowEast:
                IceCurrentVelocity = new Vector3(.5f, 0f, 0f); isSwimming = IsGrounded(); onIce = false; onLava = false; break;
            case TerrainDatLoader.TerrainTypes.WaterFlowWest:
                IceCurrentVelocity = new Vector3(-.5f, 0f, 0f); isSwimming = IsGrounded(); onIce = false; onLava = false; break;
            case TerrainDatLoader.TerrainTypes.WaterFlowNorth:
                IceCurrentVelocity = new Vector3(0f, 0f, .5f); isSwimming = IsGrounded(); onIce = false; onLava = false; break;
            case TerrainDatLoader.TerrainTypes.WaterFlowSouth:
                IceCurrentVelocity = new Vector3(0f, 0f, -.5f); isSwimming = IsGrounded(); onIce = false; onLava = false; break;
            case TerrainDatLoader.TerrainTypes.Water:
            case TerrainDatLoader.TerrainTypes.Waterfall:
                isSwimming = IsGrounded(); IceCurrentVelocity = Vector3.zero; onIce = false; onLava = false; break;
            default:
                if (IceCurrentVelocity != Vector3.zero)
                {
                    braking += Time.deltaTime;
                    //Debug.Log("cancelling ice velocity");
                    IceCurrentVelocity = Vector3.Lerp(IceCurrentVelocity, Vector3.zero, braking);
                }
                else
                {
                    braking = 0f;
                }
                //IceCurrentVelocity= Vector3.zero; 
                isSwimming = false; onIce = false; onLava = false; break;
        }

        if ((isWaterWalking) || (Grounded == false) || (onBridge))
        {
            isSwimming = false;
            onIce = false;
            IceCurrentVelocity = Vector3.zero;
        }
        if ((Grounded == false) || (onBridge))
        {
            onLava = false;
        }

        if (IceCurrentVelocity != Vector3.zero)
        {
            if (onIce)
            {
                if (!aud.isPlaying)
                {
                    aud.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_ICE_SLIDE];
                    aud.Play();
                }
            }
            this.GetComponent<CharacterController>().Move(
                    new Vector3(
                            IceCurrentVelocity.x * Time.deltaTime * speedMultiplier,
                            IceCurrentVelocity.y * Time.deltaTime,
                            IceCurrentVelocity.z * Time.deltaTime * speedMultiplier
                            )
            );
        }
        onIcePrev = onIce;

        // Update camera bobbing
        if (cameraBob)
        {
            CameraBob.Mode bobMode = CameraBob.Mode.NONE;
            Vector3 velocity = playerMotor.movement.velocity;
            velocity.y = 0;
            float speed = velocity.magnitude;
            if (isSwimming)
            {
                bobMode = CameraBob.Mode.SWIM;
            }
            else if (isFloating || isFlying)
            {
                bobMode = CameraBob.Mode.FLY;
            }
            else if (Grounded && !Mathf.Approximately(speed, 0.0f))
            {
                Vector3 inputDir = playerMotor.inputMoveDirection;
                Vector3 charDir = transform.forward;
                inputDir.y = 0;
                charDir.y = 0;
                float angle = Vector3.Angle(inputDir, charDir);
                if (angle < 80.0f) bobMode = CameraBob.Mode.WALK_FORWARD;
                else if (angle < 100) bobMode = CameraBob.Mode.STRAFE;
                // else player is walking backwards - no bob
            }
            cameraBob.SetBobMode(bobMode);
            cameraBob.SetSpeed(speed);
        }
    }

    /// <summary>
    /// Update the player when on lava
    /// </summary>
    private void OnLavaUpdate()
    {
        if (!isFireProof())
        {
            lavaDamageTimer += Time.deltaTime;
            if (lavaDamageTimer >= 1.0f)//Take Damage every 1 second.
            {
                ApplyDamage(10);
                lavaDamageTimer = 0.0f;
            }
        }
        if (_RES == GAME_UW2)
        {//Stepped in Lava after covering in basilisk oil.
            if (Quest.GetX_Clock(3) == 3)
            {
                Quest.SetX_Clock(3, 4);
                //Quest.x_clocks[3] = 4;
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 334));
            }
        }
    }


    //Update the player when dreaming of the etheral void.
    private void DreamWorldUpdate()
    {
        if (Quest.InDreamWorld)
        {
            isFlying = true;
            DreamWorldTimer -= Time.deltaTime;
            if (DreamWorldTimer < 0)
            {
                DreamTravelFromVoid();
            }
        }
    }

    public void PoisonUpdate()
    {
        if (play_poison > 0)
        {
            CurVIT -= 3;
            play_poison--;
        }
    }

    /// <summary>
    /// Update some swiming related items.
    /// </summary>
    private void SwimUpdate()
    {
        if (isSwimming == true)
        {
            playerMotor.jumping.enabled = false;
            SwimmingEffects();
        }
        else
        {//0.9198418f
            playerMotor.jumping.enabled = ((!Paralyzed) && (!GameWorldController.instance.AtMainMenu) && (!ConversationVM.InConversation) && (!WindowDetect.InMap));
            swimSpeedMultiplier = 1.0f;
            SwimTimer = 0.0f;
        }
    }

    //private void ParalyzeUpdate()
    //{
    //    if (ParalyzeTimer > 0)
    //    {
    //        ParalyzeTimer -= Time.deltaTime;
    //    }
    //    if (ParalyzeTimer < 0)
    //    {
    //        ParalyzeTimer = 0;
    //    }
    //    Paralyzed = (ParalyzeTimer != 0);
    //}

    private void InventoryUpdate()
    {
        if ((PlayerInventory.Ready == true) && (InventoryReady == false))
        {
            if ((playerInventory != null))
            {
                if (playerInventory.currentContainer != null)
                {
                    playerInventory.Refresh();
                    InventoryReady = true;
                }
            }
        }
    }

    private void TeleportUpdate()
    {
        if ((JustTeleported))
        {
            teleportedTimer += Time.deltaTime;
            if (teleportedTimer >= 0.1f)
            {
                JustTeleported = false;
            }
            else
            {
                this.transform.position = new Vector3(TeleportPosition.x, this.transform.position.y, TeleportPosition.z);
            }
        }
    }

    private void BounceUpdate()
    {
        if (isBouncy)
        {
            bounceMult = 2;
        }
        else
        {
            bounceMult = 1;
        }
        if (BounceMovement.magnitude > 0)
        {
            playerController.Move(BounceMovement * Time.deltaTime);
            BounceMovement.y -= 20f * Time.deltaTime;
            if (BounceMovement.y < 0)
            {
                BounceMovement = Vector3.zero;
            }
        }
    }

    public void SpellMode()
    {//Casts a spell on right click.
        if (
                (Input.GetMouseButtonDown(1))
                && ((WindowDetect.CursorInMainWindow == true) || (MouseLookEnabled == true))
                && (UWHUD.instance.window.JustClicked == false)
                && ((PlayerCombat.AttackCharging == false) && (PlayerCombat.AttackExecuting == false))
        )
        {
            PlayerMagic.CastSpell(this.gameObject, PlayerMagic.ReadiedSpell, false);
            PlayerMagic.SpellCost = 0;
            UWHUD.instance.window.UWWindowWait(1.0f);
        }
    }

    /// <summary>
    /// Processes a pickup of quantity event
    /// </summary>
    /// <param name="quant">Quant.</param>
    public void OnSubmitPickup(short quant)
    {

        InputField inputctrl = UWHUD.instance.InputControl;

        Time.timeScale = 1.0f;
        inputctrl.gameObject.SetActive(false);
        WindowDetect.WaitingForInput = false;
        inputctrl.text = "";
        inputctrl.text = "";
        UWHUD.instance.MessageScroll.Clear();
        if (quant == 0)
        {//cancel
            QuantityObj = null;
        }
        if (QuantityObj != null)
        {
            if (quant >= QuantityObj.link)
            {
                Pickup(QuantityObj, playerInventory);
            }
            else
            {
                //split the obj.

                ObjectLoaderInfo newobjt = ObjectLoader.newWorldObject(QuantityObj.item_id, QuantityObj.quality, QuantityObj.owner, quant, 256);
                newobjt.is_quant = QuantityObj.isquant;
                newobjt.flags = QuantityObj.flags;
                newobjt.enchantment = QuantityObj.enchantment;
                newobjt.doordir = QuantityObj.doordir;
                newobjt.invis = QuantityObj.invis;
                ObjectInteraction Split = ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, QuantityObj.transform.position);
                //newobjt.InUseFlag = 1;
                QuantityObj.link -= quant;

                Pickup(Split, playerInventory);
                ObjectInteraction.Split(Split, QuantityObj);
                QuantityObj = null;//Clear out to avoid weirdness.
            }
        }
    }

    public void TalkMode()
    {//Talk to the object clicked on.
        Ray ray;
        if (MouseLookEnabled == true)
        {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            //ray= Camera.main.ViewportPointToRay(Input.mousePosition);
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, talkRange))
        {
            if (hit.transform.gameObject.GetComponent<ObjectInteraction>() != null)
            {
                hit.transform.gameObject.GetComponent<ObjectInteraction>().TalkTo();
            }
        }
        else
        {
            UWHUD.instance.MessageScroll.Add("Talking to yourself?");
        }
    }

    public override void LookMode()
    {//Look at the clicked item.
        Ray ray;
        if (MouseLookEnabled == true)
        {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, lookRange))
        {
            //Debug.Log ("Hit made" + hit.transform.name);
            ObjectInteraction objInt = hit.transform.GetComponent<ObjectInteraction>();
            if (objInt != null)
            {
                if (EditorMode)
                {//Select this object in the editor pane
                    IngameEditor.instance.ObjectSelect.value = objInt.ObjectIndex;
                }
                objInt.LookDescription();
                return;
            }
            else
            {
                int len = hit.transform.name.Length;
                if (len > 4) { len = 4; }
                switch (hit.transform.name.Substring(0, len).ToUpper())
                {
                    case "CEIL":
                        UWHUD.instance.MessageScroll.Add("You see the ceiling");
                        //GetMessageLog().text = "You see the ceiling";
                        break;
                    case "PILL":
                        //GetMessageLog().text = 
                        UWHUD.instance.MessageScroll.Add("You see a pillar");
                        break;
                    case "BRID":
                        //000~001~171~You see a bridge.
                        //GetMessageLog().text= 
                        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_see_a_bridge_));
                        break;
                    case "WALL":
                    case "TILE":
                    default:
                        //var p = hit.point;
                        //Debug.Log(p);
                        //int hitTileX = (short)(p.x / 1.2f);
                        //int hitTileY = (short)(p.z / 1.2f);
                        

                        if (hit.transform.GetComponent<PortcullisInteraction>() != null)
                        {
                            ObjectInteraction objPicked = hit.transform.GetComponent<PortcullisInteraction>().getParentObjectInteraction();
                            if (objPicked != null)
                            {
                                objPicked.LookDescription();
                            }
                            return;
                        }
                        //Taken from
                        //http://forum.unity3d.com/threads/get-material-from-raycast.53123/
                        Renderer rend = hit.collider.GetComponent<Renderer>();
                        if (rend == null)
                        {
                            return;
                        }
                        MeshCollider meshCollider = (MeshCollider)hit.collider;
                        int materialIndex = -1;
                        Mesh mesh = meshCollider.sharedMesh;
                        int triangleIdx = hit.triangleIndex;
                        int lookupIdx1 = mesh.triangles[triangleIdx * 3];
                        int lookupIdx2 = mesh.triangles[triangleIdx * 3 + 1];
                        int lookupIdx3 = mesh.triangles[triangleIdx * 3 + 2];
                        int submeshNr = mesh.subMeshCount;

                        for (int i = 0; i < submeshNr; i++)
                        {
                            int[] tr = mesh.GetTriangles(i);
                            for (int j = 0; j < tr.Length; j += 3)
                            {
                                if ((tr[j] == lookupIdx1) && (tr[j + 1] == lookupIdx2) && (tr[j + 2]) == lookupIdx3)
                                {
                                    materialIndex = i;
                                    break;
                                }
                            }
                            if (materialIndex != -1)
                            {
                                break;
                            }
                        }
                        if (materialIndex != -1)
                        {
                            if (rend.materials[materialIndex].name.Length >= 7)
                            {
                                if (int.TryParse(rend.materials[materialIndex].name.Substring(4, 3), out int textureIndex))//int.Parse(rend.materials[materialIndex].name.Substring(4,3));
                                {
                                    if ((textureIndex == 142) && (_RES != GAME_UW2))
                                    {//This is a window into the abyss.
                                        UWHUD.instance.CutScenesSmall.anim.SetAnimation = "VolcanoWindow_" + GameWorldController.instance.dungeon_level;
                                    }
                                    if (_RES==GAME_UW2)
                                    {
                                        Debug.Log(hit.normal);
                                        Vector3 normhit = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y));                                      
                                        if ((normhit == Vector3.up))
                                        {
                                            //this is a floor. Get the floor texture by subtracting from 510d
                                            textureIndex = 510 - textureIndex;
                                        }
                                        UWHUD.instance.MessageScroll.Add("You see " + StringController.instance.GetTextureName(textureIndex));
                                        
                                    }
                                    else
                                    {
                                        UWHUD.instance.MessageScroll.Add("You see " + StringController.instance.GetTextureName(textureIndex));
                                    }
                                }
                            }
                        }
                        break;

                }
            }
        }
    }

    public override void PickupMode(int ptrId)
    {
        //Picks up the clicked object in the view.
        PlayerInventory pInv = this.GetComponent<PlayerInventory>();
        if (pInv.ObjectInHand == null)//Player is not holding anything.
        {//Find the object within the pickup range.
            Ray ray;
            if (MouseLookEnabled == true)
            {
                ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, GetPickupRange()))
            {
                ObjectInteraction objPicked;
                objPicked = hit.transform.GetComponent<ObjectInteraction>();
                if (objPicked != null)//Only objects with ObjectInteraction can be picked.
                {
                    if (objPicked.CanBePickedUp == true)
                    {
                        //check for weight
                        if (objPicked.GetWeight() > playerInventory.getEncumberance())
                        {//000~001~095~That is too heavy for you to pick up.
                            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_that_is_too_heavy_for_you_to_pick_up_));
                            return;
                        }
                        if (ptrId == -2)
                        {
                            //right click check for quant.
                            //Pickup if either not a quantity or is a quantity of one.
                            if ((objPicked.isQuantityBln == false) || ((objPicked.isQuantityBln) && (objPicked.link == 1)) || (objPicked.isEnchanted))
                            {
                                objPicked = Pickup(objPicked, pInv);
                            }
                            else
                            {
                                //Debug.Log("attempting to pick up a quantity");

                                UWHUD.instance.MessageScroll.Set("Move how many?");
                                InputField inputctrl = UWHUD.instance.InputControl;
                                inputctrl.gameObject.SetActive(true);
                                inputctrl.gameObject.GetComponent<InputHandler>().target = this.gameObject;
                                inputctrl.gameObject.GetComponent<InputHandler>().currentInputMode = InputHandler.InputCharacterQty;

                                inputctrl.text = objPicked.GetQty().ToString();//"1";

                                inputctrl.Select();
                                QuantityObj = objPicked;
                                Time.timeScale = 0.0f;
                                WindowDetect.WaitingForInput = true;
                            }
                        }
                        else
                        {//Left click. Pick them all up.
                            objPicked = Pickup(objPicked, pInv);
                        }
                    }
                    else
                    {//000~001~096~You cannot pick that up.
                     //Object cannot be picked up. Try and use it instead
                        if (objPicked.isUsable)
                        {
                            UseMode();
                            UWHUD.instance.window.UWWindowWait(1.0f);
                        }
                        else
                        {
                            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_cannot_pick_that_up_));
                        }
                    }
                }
            }
        }
    }

    public void onLanding(float fallSpeed)
    {
        if (isSwimming == false)
        {
            float fallspeedAdjusted = fallSpeed - (Skills.GetSkill(Skills.SkillAcrobat) * 0.13f);
            //Do stuff with acrobat here. In the mean time a flat skill check.
            if (fallspeedAdjusted >= 5f)
            {
                //Debug.Log("Fallspeed = " + fallSpeed + " adjusted down to " + fallspeedAdjusted) ;
                ApplyDamage(Random.Range(1, 5));//TODO:As a function of the acrobat skill versus fall.
            }
            if (ObjectInteraction.PlaySoundEffects)
            {
                aud.clip = MusicController.instance.SoundEffects[0];
                aud.Play();
            }
        }
    }

    public void UpdateHungerAndFatigue()
    {//Called by the gameclock on the hour.
        Fatigue--;
        if (Fatigue < 0)
        {
            Fatigue = 0;
            //Do what everhappens when the player stays awake non-stop. 
        }
        FoodLevel--;
        if (FoodLevel < 0)
        {
            FoodLevel = 0;
        }
        if (FoodLevel < 10 && AutoEat)
        {
            AutoEatFood();//automatically eat some food
        }
        if (FoodLevel < 3)
        {
            ApplyDamage(1);//Starvation damage.
        }
    }

    /// <summary>
    /// Automatically eats food when hungry.
    /// </summary>
    void AutoEatFood()
    {
        ObjectInteraction foodtoeat = Instance.playerInventory.playerContainer.findItemOfCategory(ObjectInteraction.FOOD);
        if (foodtoeat != null)
        {
            foodtoeat.Use();
        }
    }

    public string GetFedStatus()
    {//Returns the string representing the players hunger.
        /*
   000~001~104~starving
   000~001~105~famished
   000~001~106~very hungry
   000~001~107~hungry
   000~001~108~peckish
   000~001~109~fed
   000~001~110~well fed
   000~001~111~full
   000~001~112~satiated
   */
        int FoodLevelString;
        if (FoodLevel < 0x1E)//starving
        {
            FoodLevelString = 0;
        }
        else if (FoodLevel < 0x3c)//Famished
        {
            FoodLevelString = 1;
        }
        else if (FoodLevel < 0x5a)//Very hungry
        {
            FoodLevelString = 2;
        }
        else if (FoodLevel < 0x78)//hungry
        {
            FoodLevelString = 3;
        }
        else if (FoodLevel < 0x96)//peckish
        {
            FoodLevelString = 4;
        }
        else if (FoodLevel < 0xB4)// fed
        {
            FoodLevelString = 5;
        }
        else if (FoodLevel < 0xD2)//well fed
        {
            FoodLevelString = 6;
        }
        else if (FoodLevel < 0xF0)//full
        {
            FoodLevelString = 7;
        }
        else //satiated
        {
            FoodLevelString = 8;
        }

        //return StringController.instance.GetString (1,104+((FoodLevel)/8));
        return StringController.instance.GetString(1, StringController.str_starving + FoodLevelString);

    }

    public string GetFatiqueStatus()
    {
        /*
000~001~113~fatigued
000~001~114~very tired
000~001~115~drowsy
000~001~116~awake
000~001~117~rested
000~001~118~wide awake	
*/
        return StringController.instance.GetString(1, StringController.str_fatigued + ((Fatigue) / 5));
    }

    public void RegenMana()
    {//Natural Regeneration of mana over time;
        PlayerMagic.CurMana += Random.Range(1, 6);
        if (PlayerMagic.CurMana > PlayerMagic.MaxMana)
        {
            PlayerMagic.CurMana = PlayerMagic.MaxMana;
        }
    }

    public void SetCharLevel(int level)
    {
        if (Instance.CharLevel < level)
        {
            //000~001~147~You have attained experience level
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, StringController.str_you_have_attained_experience_level_));
            TrainingPoints += 3;
            Instance.MaxVIT = 30 + ((Skills.STR * CharLevel) / 5);
            int defaultMaxMana = (Skills.INT * Skills.ManaSkill) >> 3;
            switch (_RES)
            {//TODO:max these properties?
                case GAME_UW1:
                    if ((GameWorldController.instance.dungeon_level == 6) && (!Quest.IsTybalsOrbDestroyed))
                    {
                        Instance.PlayerMagic.TrueMaxMana = defaultMaxMana;
                    }
                    else
                    {
                        Instance.PlayerMagic.MaxMana = defaultMaxMana;
                        Instance.PlayerMagic.CurMana = Instance.PlayerMagic.MaxMana;
                        Instance.PlayerMagic.TrueMaxMana = Instance.PlayerMagic.MaxMana;
                    }
                    break;
                default:
                    Instance.PlayerMagic.MaxMana = defaultMaxMana;
                    Instance.PlayerMagic.CurMana = Instance.PlayerMagic.MaxMana;
                    Instance.PlayerMagic.TrueMaxMana = Instance.PlayerMagic.MaxMana;
                    break;
            }


        }
        Instance.CharLevel = level;
    }

    /// <summary>
    /// Adds an XP reward to the character.
    /// </summary>
    /// <param name="xp">Xp.</param>
    public void AddXP(int xp)
    {//TODO:These are UW1 level thresholds        
        switch (_RES)
        {
            case GAME_UW2:
                {
                    int curskillthreashold = EXP % 150;
                    int newskillthreashold = (EXP + xp) % 150;
                    EXP += xp;
                    if (EXP <= 50)
                    {//1
                        SetCharLevel(1);
                    }
                    else if (EXP <= 100)
                    {//2
                        SetCharLevel(2);
                    }
                    else if (EXP <= 150)
                    {//3
                        SetCharLevel(3);
                    }
                    else if (EXP <= 200)
                    {//4
                        SetCharLevel(4);
                    }
                    else if (EXP <= 300)
                    {//5
                        SetCharLevel(5);
                    }
                    else if (EXP <= 400)
                    {//6
                        SetCharLevel(6);
                    }
                    else if (EXP <= 600)
                    {//7
                        SetCharLevel(7);
                    }
                    else if (EXP <= 800)
                    {//8
                        SetCharLevel(8);
                    }
                    else if (EXP <= 1200)
                    {//9
                        SetCharLevel(9);
                    }
                    else if (EXP <= 1600)
                    {//10
                        SetCharLevel(10);
                    }
                    else if (EXP <= 2400)
                    {//11
                        SetCharLevel(11);
                    }
                    else if (EXP <= 3200)
                    {//12
                        SetCharLevel(12);
                    }
                    else if (EXP <= 4800)
                    {//13
                        SetCharLevel(13);
                    }
                    else if (EXP <= 6400)
                    {//14
                        SetCharLevel(14);
                    }
                    else if (EXP < 65535)
                    {
                        SetCharLevel(15);
                        if (newskillthreashold > curskillthreashold)
                        {//Additional skill point every 150 exp per mitch aigner
                            TrainingPoints++;
                        }
                    }
                    else
                    {
                        EXP = 65535;
                    }
                    break;
                }
            default:
                {
                    EXP += xp;
                    if (EXP <= 600)
                    {//1
                        SetCharLevel(1);
                    }
                    else if (EXP <= 1200)
                    {//2
                        SetCharLevel(2);
                    }
                    else if (EXP <= 1800)
                    {//3
                        SetCharLevel(3);
                    }
                    else if (EXP <= 2400)
                    {//4
                        SetCharLevel(4);
                    }
                    else if (EXP <= 3000)
                    {//5
                        SetCharLevel(5);
                    }
                    else if (EXP <= 3600)
                    {//6
                        SetCharLevel(6);
                    }
                    else if (EXP <= 4200)
                    {//7
                        SetCharLevel(7);
                    }
                    else if (EXP <= 4800)
                    {//8
                        SetCharLevel(8);
                    }
                    else if (EXP <= 5400)
                    {//9
                        SetCharLevel(9);
                    }
                    else if (EXP <= 6000)
                    {//10
                        SetCharLevel(10);
                    }
                    else if (EXP <= 6600)
                    {//11
                        SetCharLevel(11);
                    }
                    else if (EXP <= 7200)
                    {//12
                        SetCharLevel(12);
                    }
                    else if (EXP <= 7800)
                    {//13
                        SetCharLevel(13);
                    }
                    else if (EXP <= 8400)
                    {//14
                        SetCharLevel(14);
                    }
                    else if (EXP <= 9000)
                    {//15
                        SetCharLevel(15);
                    }
                    else if (EXP <= 9600)
                    {
                        SetCharLevel(16);
                    }
                    else
                    {
                        EXP = 9600;//Cap XP in UW1
                        SetCharLevel(16);
                    }
                    break;
                }
        }
    }

    public int GetBaseStealthLevel()
    {
        return Skills.GetSkill(Skills.SkillSneak);
    }

    public void Sleep()
    {
        switch (_RES)
        {
            case GAME_UW2:
                SleepUW2(); break;
            default:
                SleepUW1(); break;
        }
    }


    /// <summary>
    /// The player goes to sleep in UW2
    /// </summary>
    public void SleepUW2()
    {
        if (!CheckForMonsters())
        {

            if (Quest.DreamPlantEaten)
            {
                DreamTravelToVoid();
            }
            else
            {
                if (Instance.FoodLevel >= 3)
                {
                    if (IsGaramonTime())
                    {//PLay a garamon dream
                        UWHUD.instance.MessageScroll.Add("You dream of the guardian");
                    }
                    else
                    {//Regular sleep with a fade to black
                        StartCoroutine(SleepDelay());
                    }
                }
            }
            SleepRegen();
        }
        else
        {
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 14));
        }
    }

    /// <summary>
    /// The player goes to sleep in UW1
    /// </summary>
    public void SleepUW1()
    {
        //Rules to implement for sleeping
        //Only sleep if there are no hostile monsters nearby.
        //If a normal sleep do a fade to black in the small cutscene window.
        //If hungry then it is an uneasy sleep.
        //There is a small chance of a monster spawning.
        //If a dream/vision use the full screen.
        //Do a incense dream before a garamon dream. Turn the incense into ashes afterwards
        //Do one Garamon dream per game day.
        //When tybal is dead. Jump to the bury the bones dream
        //Restore an amount of health or mana after a sleep.
        //Track the state of the garamon/cup of wonder dreams in Quest.

        /*
      000~001~014~There are hostile creatures near!
      000~001~015~You make camp.
      000~001~016~You go to sleep.
      000~001~017~You are starving.
      000~001~018~You feel rested.
      000~001~019~Your sleep is uneasy.
      000~001~020~You can't go to sleep here!
      000~001~021~Your sleep is interrupted!
    */

        if (!CheckForMonsters())
        {
            ObjectInteraction incense = Instance.playerInventory.findObjInteractionByID(277);
            if (incense != null)
            {
                IncenseDream(incense);
            }
            else
            {
                if (Instance.FoodLevel >= 3)
                {
                    if (IsGaramonTime())
                    {//PLay a garamon dream
                        PlayGaramonDream(Quest.GaramonDream++);
                    }
                    else
                    {//Regular sleep with a fade to black
                        StartCoroutine(SleepDelay());
                    }
                }
            }
            SleepRegen();
        }
        else
        {
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 14));
        }
    }

    /// <summary>
    /// Play the dream that happens when you sleep after sniffing incense.
    /// </summary>
    /// <param name="incense"></param>
    void IncenseDream(ObjectInteraction incense)
    {
        UWHUD.instance.EnableDisableControl(UWHUD.instance.CutsceneFullPanel.gameObject, true);
        incense.consumeObject();
        Cutscene_Incense d = UWHUD.instance.gameObject.AddComponent<Cutscene_Incense>();
        UWHUD.instance.CutScenesFull.cs = d;
        UWHUD.instance.CutScenesFull.Begin();
    }

    /// <summary>
    /// Special dream after taking a dream plant that teleports you to the dreamworld.
    /// </summary>
    void DreamTravelToVoid()
    {
        //Record the players position.	
        Quest.DreamPlantEaten = false;
        DreamReturnTileX = TileMap.visitTileX;
        DreamReturnTileY = TileMap.visitTileY;
        DreamReturnLevel = (short)(GameWorldController.instance.dungeon_level+1);
        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 24));
        GameWorldController.instance.SwitchLevel(68, 32, 27);//TODO:implement other destinations.
        Quest.InDreamWorld = true;
        DreamWorldTimer = 30f;
        //Quest.QuestVariablesOBSOLETE[48] = 1;
        Quest.SetQuestVariable(48, 1);
    }

    /// <summary>
    /// Returns you from the void when your dream is finished.
    /// </summary>
    void DreamTravelFromVoid()
    {
        Quest.InDreamWorld = false;
        isFlying = false;
        GameWorldController.instance.SwitchLevel((short)(DreamReturnLevel-1), DreamReturnTileX, DreamReturnTileY);
        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 25));
    }


    /// <summary>
    /// Regenerates the characters stats when sleeping and advances game world
    /// </summary>
    void SleepRegen()
    {
        for (int i = Instance.Fatigue; i < 29; i += 3)//Sleep restores at a rate of 3 points per hour
        {
            if (Instance.FoodLevel >= 3)
            {
                GameClock.Advance();
                //Move time forward.
            }
            else
            {
                //Too hungry to sleep.
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 17));
                UWHUD.instance.EnableDisableControl(UWHUD.instance.CutsceneFullPanel, false);
                Instance.Fatigue += i;
                return;
                // true;
            }
        }
        Instance.Fatigue = 29;
        //Fully rested
        if (Instance.CurVIT < Instance.MaxVIT)
        {
            //Random regen of an amount of health
            Instance.CurVIT += Random.Range(1, Instance.MaxVIT - Instance.CurVIT + 1);
        }
        if (Instance.PlayerMagic.CurMana < Instance.PlayerMagic.MaxMana)
        {
            //Random regen of an amount of mana
            Instance.PlayerMagic.CurMana += Random.Range(1, Instance.PlayerMagic.MaxMana - Instance.PlayerMagic.CurMana + 1);
        }
    }

    /// <summary>
    /// Determines if hostile monsters are in the area of the player.
    /// </summary>
    /// <returns></returns>
    private bool CheckForMonsters()
    {//Finds monsters in the area.
        foreach (Collider Col in Physics.OverlapSphere(this.transform.position, 10.0f))
        {
            if (Col.gameObject.GetComponent<NPC>() != null)
            {
                NPC npc = Col.gameObject.GetComponent<NPC>();
                if (npc.npc_attitude == NPC.AI_ATTITUDE_HOSTILE)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// CHecks if we are due for a dream from garamon.
    /// </summary>
    /// Each time you get a garamon dreamer the appointment is advanced + 1 days. 
    /// <returns></returns>
    private bool IsGaramonTime()
    {
        if (Quest.GaramonDream == 6)
        {
            return true;//All done.
        }
        if (Quest.GaramonDream == 7)
        {
            return true;//Tybal is dead. Time to play a special dream to refflect that.
        }

        if (GameClock.game_days >= Quest.DayGaramonDream)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Play the next Garamon Dream.
    /// </summary>
    /// <param name="dreamIndex"></param>
    void PlayGaramonDream(int dreamIndex)
    {
        int DaysToWait = 0;
        switch (dreamIndex)
        {
            case 0:
                Cutscene_Dream_1 d1 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_1>();
                UWHUD.instance.CutScenesFull.cs = d1;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 1:
                Cutscene_Dream_2 d2 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_2>();
                UWHUD.instance.CutScenesFull.cs = d2;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 2:
                Cutscene_Dream_3 d3 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_3>();
                UWHUD.instance.CutScenesFull.cs = d3;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 3:
                Cutscene_Dream_4 d4 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_4>();
                UWHUD.instance.CutScenesFull.cs = d4;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 4:
                Cutscene_Dream_5 d5 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_5>();
                UWHUD.instance.CutScenesFull.cs = d5;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 5:
                Cutscene_Dream_6 d6 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_6>();
                UWHUD.instance.CutScenesFull.cs = d6;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 6:
                Cutscene_Dream_7 d7 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_7>();
                UWHUD.instance.CutScenesFull.cs = d7;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 7: // Tybal is dead.
                Cutscene_Dream_7 d8 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_7>();
                UWHUD.instance.CutScenesFull.cs = d8;
                UWHUD.instance.CutScenesFull.Begin();
                Quest.GaramonDream = 3;//Move back in the sequence
                DaysToWait = 1;
                break;
            case 8:
                Cutscene_Dream_9 d9 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_9>();
                UWHUD.instance.CutScenesFull.cs = d9;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
            case 9:
                Cutscene_Dream_10 d10 = UWHUD.instance.gameObject.AddComponent<Cutscene_Dream_10>();
                UWHUD.instance.CutScenesFull.cs = d10;
                UWHUD.instance.CutScenesFull.Begin();
                DaysToWait = 1;
                break;
        }

        Quest.DayGaramonDream = GameClock.game_days + DaysToWait;
    }

    /// <summary>
    /// Restores health and mana when the character wakes up
    /// </summary>
    /// Rise and Shine Sunshine
    /// <param name="sunshine"></param>
    static void RestoreHealthMana(UWCharacter sunshine)
    {
        sunshine.CurVIT += Random.Range(1, 40);
        if (sunshine.CurVIT > sunshine.MaxVIT)
        {
            sunshine.CurVIT = sunshine.MaxVIT;
        }

        sunshine.PlayerMagic.CurMana += Random.Range(1, 40);
        if (sunshine.PlayerMagic.CurMana > sunshine.PlayerMagic.MaxMana)
        {
            sunshine.PlayerMagic.CurMana = sunshine.PlayerMagic.MaxMana;
        }
    }

    /// <summary>
    /// Wakes up the player and tells them how they slept.
    /// </summary>
    /// <param name="sunshine"></param>
    public static void WakeUp(UWCharacter sunshine)
    {//Todo: Test the quality of the sleep and check for monster interuption.
        RestoreHealthMana(sunshine);
        UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 18));
    }

    /// <summary>
    /// Special screen effect for sleep.
    /// </summary>
    /// <returns></returns>
    IEnumerator SleepDelay()
    {
        UWHUD.instance.EnableDisableControl(UWHUD.instance.CutsceneFullPanel.gameObject, true);
        UWHUD.instance.CutScenesFull.SetAnimationFile = "FadeToBlackSleep";
        yield return new WaitForSeconds(3f);
        UWHUD.instance.CutScenesFull.SetAnimationFile = "Anim_Base";
        UWHUD.instance.EnableDisableControl(UWHUD.instance.CutsceneFullPanel.gameObject, false);
    }

    /// <summary>
    /// Resets the true mana to handle the magic drain effect in tybals layer of UW1
    /// </summary>
    public static void ResetTrueMana()
    {
        if (Instance.PlayerMagic.MaxMana < Instance.PlayerMagic.TrueMaxMana)
        {
            Instance.PlayerMagic.MaxMana = Instance.PlayerMagic.TrueMaxMana;
            if (Instance.PlayerMagic.CurMana == 0)
            {
                Instance.PlayerMagic.CurMana = Instance.PlayerMagic.MaxMana / 4;
            }
        }
    }

    /// <summary>
    /// Checks if the player is poison resistant
    /// </summary>
    /// <returns><c>true</c>, if poison resistant, <c>false</c> otherwise.</returns>
    public bool isPoisonResistant()
    {
        return (this.gameObject.GetComponent<SpellEffectImmunityPoison>() != null);
    }

    /// <summary>
    /// Is the player flameproof
    /// </summary>
    /// <returns><c>true</c>, if fire proof was ised, <c>false</c> otherwise.</returns>
    public bool isFireProof()
    {
        return (this.gameObject.GetComponent<SpellEffectFlameproof>() != null);
    }

    /// <summary>
    ///  Is the player magic resistant.
    /// </summary>
    /// <returns><c>true</c>, if magic resistant was ised, <c>false</c> otherwise.</returns>
    public bool isMagicResistant()
    {
        return (this.gameObject.GetComponent<SpellEffectMagicResistant>() != null);
    }

    /// <summary>
    /// Gets the spell reduction of damage for magic spell types
    /// </summary>
    /// <returns>The spell resistance.</returns>
    /// <param name="sp">Sp.</param>
    public int getSpellResistance(SpellProp sp)
    {
        int ratio = 1;
        switch ((sp.damagetype))
        {
            case SpellProp.DamageTypes.fire:
                {
                    if (isFireProof())
                    {
                        ratio++;
                    }
                    if (isMagicResistant())
                    {
                        ratio++;
                    }
                    break;
                }
            case SpellProp.DamageTypes.poison:
                {
                    if (isPoisonResistant())
                    {
                        ratio++;
                    }
                    if (isMagicResistant())
                    {
                        ratio++;
                    }
                    break;
                }
            case SpellProp.DamageTypes.physcial:
                {
                    if (isMagicResistant())
                    {
                        ratio++;
                    }
                    if (Instance.Resistance > 0)
                    {
                        ratio++;
                    }
                    break;
                }
            //case SpellProp.DamageTypes.acid:
            //case SpellProp.DamageTypes.magic:W				
            //case SpellProp.DamageTypes.electric:
            //case SpellProp.DamageTypes.aid:
            //case SpellProp.DamageTypes.psychic:
            //case SpellProp.DamageTypes.holy:
            //case SpellProp.DamageTypes.light:
            //case SpellProp.DamageTypes.protection:
            //case SpellProp.DamageTypes.mobility:
            default:
                if (isMagicResistant())
                {
                    ratio++;
                }
                break;
        }
        return ratio;
    }

    /// <summary>
    /// Raises the controller collider hit event.
    /// </summary>
    /// <param name="hit">Hit.</param>
    /// Bounces the player off the wall if they are sliding on ice
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (onIce)
        {//"Water","MapMesh","Lava","Ice"
            if (
                    (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                    ||
                    (hit.collider.gameObject.layer == LayerMask.NameToLayer("MapMesh"))
                    ||
                    (hit.collider.gameObject.layer == LayerMask.NameToLayer("Lava"))
                    ||
                    (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ice"))
            )
            {
                if (hit.normal.y < 1f)//Probably a wall
                {
                    Vector3 reflected = Vector3.Reflect(Instance.IceCurrentVelocity, hit.normal);
                    IceCurrentVelocity = new Vector3(reflected.x, 0f, reflected.z);
                }
            }
        }
        else
        {
            if (isBouncy)
            {
                if (hit.normal.y == 1f)
                {
                    if (BounceMovement == Vector3.zero)
                    {
                        if (hit.controller.velocity.y < -0.3f)
                        {
                            BounceMovement = new Vector3(hit.controller.velocity.x, -hit.controller.velocity.y, hit.controller.velocity.z) * bounceMult; // Vector3.Reflect(hit.controller.velocity, hit.normal) * bounceMult;	
                        }
                    }
                }
                if (hit.collider.name == "Tile_00_00")
                {
                    BounceMovement = Vector3.zero;
                }
            }

        }
    }

    /// <summary>
    /// Determines whether this instance is grounded.
    /// </summary>
    /// <returns><c>true</c> if this instance is grounded; otherwise, <c>false</c>.</returns>
    bool IsGrounded()
    {
        onBridge = false;
        Rayposition = transform.position;

        Physics.Raycast(Rayposition, Raydirection, out RaycastHit hit, Raydistance, RayMask);

        if (hit.collider != null)
        {
            if (hit.collider.name.Contains("bridge"))
            {
                onBridge = true;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks the players fall speed to see if they have fallen from a height
    /// </summary>
    void FallDamageUpdate()
    {
        if (Grounded == false)
        {
            if (playerMotor.movement.velocity.y < currYVelocity)
            {
                fallSpeed = Mathf.Max(-Instance.playerMotor.movement.velocity.y, fallSpeed);
            }
            else
            {
                fallSpeed = 0.0f;
            }
        }
        else
        {
            if (fallSpeed > 0.0f)
            {
                //Check fall damage.
                onLanding(fallSpeed);

                fallSpeed = 0.0f;
            }
        }
        currYVelocity = playerMotor.movement.velocity.y;
    }

    /// <summary>
    /// Plays sound effects for foot steps.
    /// </summary>
    /// <returns></returns>
    ///     public const int SOUND_EFFECT_FOOT_1 = 1;
    ///     public const int SOUND_EFFECT_FOOT_2 = 2;
    ///     public const int SOUND_EFFECT_FOOT_GRAVELLY = 47;
    ///     public const int SOUND_EFFECT_FOOT_ICE = 48;

    IEnumerator playfootsteps()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);
            if (Grounded && playerMotor.movement.velocity.magnitude != 0)
            {
                if (!footsteps.isPlaying)
                {
                    if (step)
                    {
                        footsteps.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_FOOT_1];
                        step = false;
                    }
                    else
                    {
                        footsteps.clip = MusicController.instance.SoundEffects[MusicController.SOUND_EFFECT_FOOT_2];
                        step = true;
                    }
                    footsteps.Play();
                }
            }
        }
    }
}
