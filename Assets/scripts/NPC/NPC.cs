﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC Properties and AI
/// </summary>
/// Controls AI status, animation, conversations and general properties.
public class NPC : MobileObject
{
    // public string debugname;
    public CharacterController CharController;

    /// <summary>
    /// Controller for NPC audio and ambient sounds
    /// </summary>
    public NPC_Audio npc_aud;

    public AudioSource audMovement; //AudioSource for walking & running based actions.
    public AudioSource audCombat; //Audiosource for combat actions performed by the npc
    public AudioSource audVoice; //Audiosource for spoken actions. Eg alerts and barks. Idle noises.
    public AudioSource audPhysical //AudioSource for impacts on the NPC. Eg if the player hits the npc the impact sound will come from here.
    {
        get
        {
            return objInt().aud;
        }
    }
    bool step;

    //attitude; 0:hostile, 1:upset, 2:mellow, 3:friendly
    public const int AI_ATTITUDE_HOSTILE = 0;
    public const int AI_ATTITUDE_UPSET = 1;
    public const int AI_ATTITUDE_MELLOW = 2;
    public const int AI_ATTITUDE_FRIENDLY = 3;

    public enum NPCCategory
    {
        ethereal = 0,
        humanoid = 0x1,
        flying = 0x2,
        swimming = 0x03,
        creeping = 0x4,
        crawling = 0x5,
        golem = 0x6,
        human = 0x51
    };

    public enum npc_goals
    {        
		npc_goal_stand_still_0 = 0,
        npc_goal_goto_1 = 1,
        npc_goal_wander_2 = 2,
		npc_goal_follow = 3,
		npc_goal_wander_4 = 4, //possibly this should be another standstill goal
		npc_goal_attack_5 = 5,
		npc_goal_attack_6 = 6,  //goal appears to be attack at a distance using ranged weapons
		npc_goal_stand_still_7 = 7, //same hehaviour as 0
        npc_goal_wander_8 = 8,
        npc_goal_attack_9 = 9, //goal appears to also be attack at a distance, possibly using magic attacks
        npc_goal_want_to_talk = 10,
        npc_goal_stand_still_11 = 11, //This goal is only seen in ethereal void creatures. 0xB
        npc_goal_stand_still_12 = 12,
		npc_goal_unk13 = 13,
        npc_goal_unk14 = 14,
        npc_goal_petrified = 15
    };

    public enum AttackStages
    {
        AttackPosition = 0,
        AttackAnimateRanged = 1,
        AttackAnimateMelee = 2,
        AttackExecute = 3,
        AttackWaitCycle = 4
    };

    enum AgentMasks
    {
        LAND = 3,
        WATER = 4,
        LAVA = 5,
        AIR = 6
    };


    //Animations are clasified by number
    public const int AI_RANGE_IDLE = 1;
    public const int AI_RANGE_MOVE = 10;

    public const int AI_ANIM_IDLE_FRONT = 1;
    public const int AI_ANIM_IDLE_FRONT_RIGHT = 2;
    public const int AI_ANIM_IDLE_RIGHT = 3;
    public const int AI_ANIM_IDLE_REAR_RIGHT = 4;
    public const int AI_ANIM_IDLE_REAR = 5;
    public const int AI_ANIM_IDLE_REAR_LEFT = 6;
    public const int AI_ANIM_IDLE_LEFT = 7;
    public const int AI_ANIM_IDLE_FRONT_LEFT = 8;

    public const int AI_ANIM_WALKING_FRONT = 10;
    public const int AI_ANIM_WALKING_FRONT_RIGHT = 20;
    public const int AI_ANIM_WALKING_RIGHT = 30;
    public const int AI_ANIM_WALKING_REAR_RIGHT = 40;
    public const int AI_ANIM_WALKING_REAR = 50;
    public const int AI_ANIM_WALKING_REAR_LEFT = 60;
    public const int AI_ANIM_WALKING_LEFT = 70;
    public const int AI_ANIM_WALKING_FRONT_LEFT = 80;

    public const int AI_ANIM_DEATH = 100;
    public const int AI_ANIM_ATTACK_BASH = 1000;
    public const int AI_ANIM_ATTACK_SLASH = 2000;
    public const int AI_ANIM_ATTACK_THRUST = 3000;
    public const int AI_ANIM_COMBAT_IDLE = 4000;
    public const int AI_ANIM_ATTACK_SECONDARY = 5000;

    private static readonly short[] CompassHeadings = { 0, -1, -2, -3, 4, 3, 2, 1, 0 };//What direction the npc is facing. To adjust it's animation

    // [Header("AI Target")]
    /// <summary>
    /// An object representing the npc_gtarg
    /// </summary>
    public GameObject gtarg
    {
        get
        {
            if (npc_gtarg < 5)
            {
                return UWCharacter.Instance.gameObject;
            }
            else
            {
                if (npc_gtarg < 256)
                {
                    if (CurrentObjectList().objInfo[npc_gtarg].instance != null)
                    {
                        return CurrentObjectList().objInfo[npc_gtarg].instance.gameObject;
                    }
                }
                return null;
            }
        }
    }

    [Header("Combat")]
    public AttackStages AttackState;
    public int CurrentAttack;//What attack the NPC is currently executing.

    ///Anim range defines which animation set to play.
    ///Multiple of 10 for dividing animations
    ///Angle index * Anim Range give AI_ANIM_X value to pick animation
    /// 
    [Header("Animation")]
    public int AnimRange = 1;
    public int NPC_IDi;
    public NPC_Animation newAnim;
    /// The animation index the NPC is facing
    private short facingIndex;
    /// Previous value for tracking changes
    private short PreviousFacing = -1;
    /// Previous value for tracking changes
    private int PreviousAnimRange = -1;
    /// The compass point (see heading array) the NPC is facing relative to the player
    private short CalcedFacing;
    ///Integer representation of the current facing of the character. To match with animation angles
    private short currentHeading;
    //Direction between the player and the NPC for calculating relative angle
    private Vector3 direction;  //vector between the player and the ai.
    /// The angle to the character from the player.

    [Header("Status")]
    ///flags the NPC as dead so we can kill them off in the next frame
    public bool NPC_DEAD;
    //Added by myself. This are set by spelleffects
    ///The NPC is poisoned
    public bool Poisoned;
    ///The NPC is parlyzed 
    public bool Paralyzed;
    ///Allows periodic updating of the NPC animation when frozen to support moving around them
    public short FrozenUpdate = 0;
    //Enemy types.
    ///Undead Enemy flag
    public bool isUndead
    {
        //TODO use SCALEDAMAGE to determine.
        get
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        switch (item_id)
                        {
                            case 72: //a_skeleton
                            case 85: //a_ghost
                            case 93: //a_spectre
                            case 105: //a_liche
                            case 106: //a_liche
                            case 107: //a_liche
                                return true;
                            default:
                                return false;
                        }
                    }
                default:
                    {
                        switch (item_id)
                        {
                            case 97: //a_ghost
                            case 99: //a_ghoul
                            case 100: //a_ghost
                            case 101: //a_ghost
                            case 105: //a_dark_ghoul
                            case 110: //a_ghoul	
                            case 113: //a_dire_ghost
                                return true;
                            default:
                                return false;
                        }
                    }
            }
        }
    }

    ///For storing spell effects applied to NPCs
    public SpellEffect[] NPCStatusEffects = new SpellEffect[3];

    /// <summary>
    /// Can the NPC fire off magic attacks.
    /// </summary>
    public bool MagicAttack
    {
        get
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        switch (item_id)
                        {
                            case 75: //an_imp
                            case 88: //a_brain_creature
                            case 96: //a_fire_elemental
                            case 104: //a_destroyer
                            case 105: //a_liche
                            case 111: //a_gazer
                            case 117: //a_human
                                return true;
                            default:
                                return false;
                        }
                    }
                default:
                    {
                        switch (item_id)
                        {
                            case 103: //a_mage
                            case 106: //a_mage
                            case 107: //a_mage
                            case 108: //a_mage
                            case 109: //a_mage
                            case 110: //a_ghoul
                            case 115: //a_mage
                            case 120: //A fire elemental
                            case 123: //tybal
                            case 75: //an_imp
                            case 81: //a_mongbat
                            case 102: //a_gazer
                            case 69: //a_acid_slug
                            case 122: //a_wisp
                                return true;
                            default:
                                return false;
                        }
                    }
            }
        }
    }

    /// <summary>
    /// Can the NPC fire off ranged attacks.
    /// </summary>
    public bool RangeAttack
    {
        get
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        switch (item_id)
                        {
                            case 73: //a_goblin
                            case 74: //a_goblin
                            case 82: //yeti
                            case 110: //fighter
                                return true;
                            default:
                                return false;
                        }
                    }
                default:
                    {
                        switch (item_id)
                        {
                            case 70: //a_goblin
                            case 71: //a_goblin
                            case 76: //a_goblin
                            case 77: //a_goblin
                            case 78: //a_goblin
                                return true;
                            default:
                                return false;
                        }
                    }
            }
        }
    }

    ///Transform position to launch projectiles from
    public GameObject NPC_Launcher;
    // public int Ammo = 0;//How many ranged attacks can this NPC execute. (ie how much ammo can it spawn)
    /// <summary>
    /// How long the npc waits before moving on.
    /// </summary>
    public float WaitTimer = 0f;
    /// <summary>
    /// How long it has taken to reach the destination.
    /// </summary>
    public float TravelTimer = 0f;
    [Header("NavMesh")]
    public NavMeshAgent Agent;
    float targetBaseOffset = 0f;
    float startBaseOffset = 0f;
    float floatTime = 0f;
    public float DistanceToGtarg;
    public bool ArrivedAtDestination;
    private short StartingHP;

    [Header("Positioning")]
    public int CurTileX = 0;
    public int CurTileY = 0;
    public int prevTileX = -1;
    public int prevTileY = -1;
    public int DestTileX;
    public int DestTileY;
    public Vector3 destinationVector;

    /// <summary>
    /// Initialise some basic info for the NPC ai.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        //if (npc_whoami != 0)
        //{
        //    debugname = StringController.instance.GetString(7, npc_whoami + 16);
        //}
        //else
        //{
        //    debugname = StringController.instance.GetSimpleObjectNameUW(item_id);
        //}

        NPC_IDi = item_id;
        StartingHP = npc_hp;
        newAnim = this.gameObject.AddComponent<NPC_Animation>();
        if (GameWorldController.instance.critsLoader[NPC_IDi - 64] == null)
        {
            GameWorldController.instance.critsLoader[NPC_IDi - 64] = new CritLoader(NPC_IDi - 64);
        }
        newAnim.critAnim = GameWorldController.instance.critsLoader[NPC_IDi - 64].critter.AnimInfo;
        newAnim.output = this.GetComponentInChildren<SpriteRenderer>();
        DestTileX = ObjectTileX;
        DestTileY = ObjectTileY;
        if (npc_goal == (short)npc_goals.npc_goal_petrified)
        {
            SpellEffectPetrified sep = this.gameObject.AddComponent<SpellEffectPetrified>();
            sep.counter = npc_gtarg;
            sep.Go();
        }

        //Start the NPC audio controller.
        audMovement = this.gameObject.AddComponent<AudioSource>();
        audMovement.maxDistance = 1;
        audMovement.spatialBlend = 1;
        audMovement.rolloffMode = AudioRolloffMode.Linear;
        audMovement.minDistance = 1;
        audMovement.maxDistance = 4;

        audCombat = this.gameObject.AddComponent<AudioSource>();
        audCombat.maxDistance = 1;
        audCombat.spatialBlend = 1;
        audCombat.rolloffMode = AudioRolloffMode.Linear;
        audCombat.minDistance = 1;
        audCombat.maxDistance = 4;

        audVoice = this.gameObject.AddComponent<AudioSource>();
        audVoice.maxDistance = 1;
        audVoice.spatialBlend = 1;
        audVoice.rolloffMode = AudioRolloffMode.Linear;
        audVoice.minDistance = 1;
        audVoice.maxDistance = 4;

        npc_aud = new NPC_Audio(this, audMovement, audCombat, audVoice, objInt().aud);
        StartCoroutine(playfootsteps());
        StartCoroutine(playIdleBarks());

    }

    void AI_INIT()
    {
        //if (ai == null) {
        if (!GameWorldController.NavMeshReady) { return; }
        if (Agent == null)
        {
            Vector3 pos = CurrentTileMap().getTileVector(CurTileX, CurTileY);
            //this.transform.position=pos;
            int mask = (int)AgentMask();
            mask = 1 << mask;
            NavMesh.SamplePosition(pos, out NavMeshHit hit, 0.2f, mask);// NavMesh.AllAreas
            if (hit.hit)
            {//Tests if the npc in question is on the nav mesh
                AddAgent(mask);
                //Agent.Warp(pos);
                Agent.Warp(objInt().startPos);
            }
        }
    }

    void AddAgent(int mask)
    {
        int agentId = GameWorldController.instance.NavMeshLand.agentTypeID;
        Agent = this.gameObject.AddComponent<NavMeshAgent>();
        Agent.autoTraverseOffMeshLink = false;
        Agent.speed = 2f * ((GameWorldController.instance.objDat.critterStats[item_id - 64].Speed / 12.0f));

        switch (_RES)
        {
            case GAME_UW2:
                {
                    switch (item_id)
                    {
                        case 65://cave bat
                        case 66://vampire bat
                        case 71://mongbat
                        case 75://imp
                        case 98://dire ghost
                        case 102://haunt
                        case 105://lich
                        case 106://lich
                        case 107://lich
                        case 111://gazer	
                            agentId = GameWorldController.instance.NavMeshAir.agentTypeID;
                            //agentId = GameWorldController.instance.NavMeshLand.agentTypeID;
                            break;
                        case 77://lurker
                        case 89://lurker
                            agentId = GameWorldController.instance.NavMeshWater.agentTypeID;
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                            //water
                            break;
                        case 96://Fire elemental				
                            agentId = GameWorldController.instance.NavMeshLava.agentTypeID;
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                            break;
                            //default:
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                    }
                    break;
                }
            default:
                {
                    switch (item_id)
                    {
                        case 66://bat
                        case 73://vampire bat
                        case 75://imp
                        case 81://mongbat
                                //case 100://ghost
                                //case 101://ghost
                        case 102://gazer
                        case 122://wisp
                        case 123://tybal
                            agentId = GameWorldController.instance.NavMeshAir.agentTypeID;
                            //agentId = GameWorldController.instance.NavMeshLand.agentTypeID;
                            //flyer
                            break;
                        case 87://lurker
                        case 116://deep lurker
                            agentId = GameWorldController.instance.NavMeshWater.agentTypeID;
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                            //water
                            break;
                        case 120:
                            //case 124://slasher
                            //fire elemental
                            agentId = GameWorldController.instance.NavMeshLava.agentTypeID;
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                            break;
                            //default:
                            //agentId = CurrentTileMap().getTileAgentID(CurTileX,CurTileY);
                            //break;
                    }
                    break;
                }
        }
        Agent.agentTypeID = agentId;
        Agent.areaMask = mask;
    }


    AgentMasks AgentMask()
    {
        AgentMasks agentMask = AgentMasks.LAND;//land
        switch (_RES)
        {
            case GAME_UW2:
                {
                    switch (item_id)
                    {
                        case 65://cave bat
                        case 66://vampire bat
                        case 71://mongbat
                        case 75://imp
                        case 98://dire ghost
                        case 102://haunt
                        case 105://lich
                        case 106://lich
                        case 107://lich
                        case 111://gazer	
                            return AgentMasks.AIR;
                        case 77://lurker
                        case 89://lurker//water
                            return AgentMasks.WATER;
                        case 96://Fire elemental				
                            return AgentMasks.LAVA;
                    }
                    break;
                }
            default:
                {
                    switch (item_id)
                    {
                        case 66://bat
                        case 73://vampire bat
                        case 75://imp
                        case 81://mongbat
                        case 100://ghost
                        case 101://ghost
                        case 102://gazer
                        case 122://wisp
                        case 123://tybal
                            return AgentMasks.AIR;
                        case 87://lurker
                        case 116://deep lurker
                            return AgentMasks.WATER;
                        case 120:
                            //case 124://slasher
                            //fire elemental
                            return AgentMasks.LAVA;
                    }
                    break;
                }
        }
        return agentMask;
    }



    /// <summary>
    /// Raises the death events for the player.
    /// </summary>
    /// Uses the conversation for special npcs like Tybal, the Golem and the Beholder.
    /// Dumps out their inventory.
    void OnDeath()
    {
        Debug.Log("Killing " + this.name);
        if (SpecialDeathCases())
        {
            return;
        }
        if (_RES == GAME_UW2)
        {//Improve players Win loss record in the arena
            if (Quest.FightingInArena)
            {
                for (int i = 0; i <= 4; i++)
                {
                    if (Quest.GetArenaOpponent(i) == objInt().ObjectIndex)
                    {//Update the players win-loss records and clear the fighter off the fight billing.
                        Quest.SetArenaOpponent(i,0);
                        
                        Quest.SetQuestVariable(129, Mathf.Min(255, Quest.GetQuestVariable(129) + 1));//Total no of kills
                        //Quest.x_clocks[14] = Mathf.Min(255, Quest.x_clocks[14] + 1);
                        Quest.IncrementXClock(14); //Also Total no of kills in the area.
                        //You have won the fight.
                        Quest.SetQuestVariable(24, 1);
                    }
                }
            }
        }
        if ((objInt().ObjectTileX <= 63) || (objInt().ObjectTileY <= 63))
        {//Only dump container if on map
            //objInt().InUseFlag = 0;
            objInt().npc_hp = 0;
            NPC_DEAD = true;//Tells the update to execute the NPC death animation
            PerformDeathAnim();
            //Dump npc inventory on the floor.
            Container cnt = this.GetComponent<Container>();
            if (cnt != null)
            {
                SetupNPCInventory();

                cnt.SpillContents();//Spill contents is still not 100% reliable so don't expect to get all the items you want.
            }
        }

        UWCharacter.Instance.AddXP(GameWorldController.instance.objDat.critterStats[item_id - 64].Exp);
        npc_aud.PlayDeathSound();

    }

    /// <summary>
    /// Setups the NPC inventory if they use a loot list.
    /// </summary>
    public void SetupNPCInventory()
    {
        if (_RES != GAME_UW2)
        {
            if (item_id == 64)
            {//Special case for rotworms.
                return;
            }
        }
        Container cnt = this.GetComponent<Container>();
        if (cnt != null)
        {
            if (cnt.CountItems() == 0)
            {
                if ((item_id >= 64) && (item_id <= 127))
                {
                    //Populate the container with a loot list
                    for (int i = 0; i <= GameWorldController.instance.objDat.critterStats[item_id - 64].Loot.GetUpperBound(0); i++)
                    {
                        if (GameWorldController.instance.objDat.critterStats[item_id - 64].Loot[i] != -1)
                        {
                            int itemid = GameWorldController.instance.objDat.critterStats[item_id - 64].Loot[i];
                            ObjectLoaderInfo newobjt = ObjectLoader.newWorldObject(itemid, (short)Random.Range(1, 41), 0, 0, 256);
                            if (newobjt != null)
                            {
                                if (itemid == 16)//Sling stone.
                                {
                                    newobjt.is_quant = 1;
                                    newobjt.link = (short)Random.Range(1, 10);
                                    newobjt.quality = 40;
                                }
                                else
                                {
                                    newobjt.is_quant = 0;
                                }
                                newobjt.instance = ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance._ObjectMarker, GameWorldController.instance.InventoryMarker.transform.position);
                                cnt.AddItemToContainer(newobjt.instance);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Some NPCs has special events when they die.
    /// </summary>
    /// <returns><c>true</c>, if no more processing is to be done., <c>false</c> otherwise.</returns>
    bool SpecialDeathCases()
    {
        switch (_RES)
        {
            case GAME_UW1:
                {
                    /*if (item_id==64)
                    {//Drop a rotworm corpse
                            ObjectLoaderInfo newobjt= ObjectLoader.newObject(217,0,0,0,256);
                            ObjectInteraction.CreateNewObject(CurrentTileMap(),newobjt, GameWorldController.instance.DynamicObjectMarker().gameObject,this.transform.position);
                            return false;	
                    }*/
                    switch (npc_whoami)
                    {
                        case 22: //The golem on level 6
                            {
                                if (ConversationVM.InConversation == false)
                                {
                                    NPC_DEAD = false;
                                    TalkTo();
                                }
                                return true;
                            }
                        case 110://The Gazer on level 2
                            {
                                //Quest.QuestVariablesOBSOLETE[4] = 1;
                                Quest.SetQuestVariable(4, 1);
                                return false;
                            }
                        case 142://Rodrick
                            {
                                //Quest.QuestVariablesOBSOLETE[11] = 1;
                                Quest.SetQuestVariable(11, 1);
                                return false;
                            }
                        case 231:   //Tybal
                            {
                                //Play the tybal death cutscene.
                                //Quest.isTybalDead=true;
                                Quest.GaramonDream = 7;//Advance to Tybal is dead range of dreams
                                Quest.DayGaramonDream = GameClock.Day;//Ensure dream triggers on next sleep
                                UWCharacter.Instance.PlayerMagic.CastEnchantment(this.gameObject, null, 226, Magic.SpellRule_TargetSelf, -1);
                                return false;
                            }
                    }


                    break;
                }

            case GAME_UW2:
                {
                    if (GameWorldController.instance.dungeon_level == 3)
                    {
                        if (item_id == 78)//Blood worms on level 3 of britannia. This is a quest for the friendly goblins
                        {
                           // Quest.QuestVariablesOBSOLETE[135]++;
                            Quest.SetQuestVariable(135, Quest.GetQuestVariable(135) + 1);
                        }
                    }
                    switch (npc_whoami)
                    {
                        case 32://Killing Praceor Loth 
                            //This variable is normally set by his final conversation but will also happen when you murder him.
                            //Also used to trigger earthquake effect.
                            {
                                //Quest.QuestVariablesOBSOLETE[7] = 1;
                                Quest.SetQuestVariable(7, 1);
                                ObjectInteraction trigObj = CurrentObjectList().objInfo[961].instance;
                                if (trigObj != null)
                                {
                                    if (trigObj.GetComponent<trigger_base>() != null)
                                    {
                                        trigObj.GetComponent<trigger_base>().Activate(null);//trigger the enter trigger that causes the earth quake.
                                    }
                                }
                                break;
                            }
                        case 47://Mors gothri in Kilhorn
                            if (Quest.GetQuestVariable(117) == 0)
                            {
                                Quest.SetQuestVariable(117, 1);
                                npc_hp = 50;//restore health.
                                npc_goal = (byte)npc_goals.npc_goal_stand_still_0;
                                npc_attitude = AI_ATTITUDE_UPSET;
                                TalkTo();
                                return true;
                            }

                            break;

                        case 58://Brain creatures in Kilhorn
                            Quest.SetQuestVariable(50, 1);
                            return false;
                        case 75: //Demon guard in Kilhorn.
                            if (NPC_IDi == 108)
                            {//Convert into a hordling
                                NPC_IDi = 94;
                                item_id = 94;
                                npc_hp = 92;
                                NPC_DEAD = false;
                                if (GameWorldController.instance.critsLoader[NPC_IDi - 64] == null)
                                {
                                    GameWorldController.instance.critsLoader[NPC_IDi - 64] = new CritLoader(NPC_IDi - 64);
                                }
                                newAnim.critAnim = GameWorldController.instance.critsLoader[NPC_IDi - 64].critter.AnimInfo;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        case 98://Zaria
                            //Quest.QuestVariablesOBSOLETE[25] = 1;
                            Quest.SetQuestVariable(25, 1);
                            return false;
                        case 99://Dorstag
                            //Quest.QuestVariablesOBSOLETE[121] = 1;
                            Quest.SetQuestVariable(121, 1);
                            return false;
                        case 145://The listener under the castle
                            //Quest.QuestVariablesOBSOLETE[11] = 1;
                            Quest.SetQuestVariable(11, 1);
                            //Quest.x_clocks[1]++;//Confirm this behaviour!
                            Quest.IncrementXClock(1);
                            return false;
                        case 152://Bliy Scup Ductosnore
                            {
                                // Quest.QuestVariablesOBSOLETE[122] = 1;
                                Quest.SetQuestVariable(122, 1);
                                //Fires off move trigger at 638 to delete the walls
                                ObjectInteraction obj = ObjectLoader.getObjectIntAt(638);
                                if (obj != null)
                                {
                                    if (obj.GetComponent<trigger_base>() != null)
                                    {
                                        obj.GetComponent<trigger_base>().Activate(UWCharacter.Instance.gameObject);
                                    }
                                }
                                obj = ObjectLoader.getObjectIntAt(649);
                                if (obj != null)
                                {
                                    if (obj.GetComponent<trigger_base>() != null)
                                    {
                                        obj.GetComponent<trigger_base>().Activate(UWCharacter.Instance.gameObject);
                                    }
                                }
                                return false;
                            }
                    }
                    break;
                }
        }
        return false;
    }


    /// <summary>
    /// Update the NPC state, AI and animations
    /// </summary>
    /// AI is only active when the player is close.
    public override void Update()
    {
        if (EditorMode == true) { return; }
        if (ConversationVM.InConversation) { return; }

        //TODO:Check if frozen npcs should behave like petrified (stone struck) ones.
        bool FreezeNpc = isNPCFrozen();
        newAnim.FreezeAnimFrame = FreezeNpc;
        if ((WaitTimer > 0) && (!FreezeNpc))
        {
            WaitTimer -= Time.deltaTime;
            if (WaitTimer < 0)
            {
                WaitTimer = 0;
            }
        }

        if (FreezeNpc)
        {
            if (Agent != null)
            {
                AgentStand();
            }
        }
        /*if (Frozen)
        {//NPC will not move until timer is complete.
                if (FrozenUpdate==0)
                {
                        newAnim.enabled=false;
                }
                else
                {
                        FrozenUpdate--;
                }
        }*/

        if (NPC_DEAD == true)
        {//Set the AI death state
         //Update the appearance of the NPC
            UpdateSprite();
            if ((WaitTimer <= 0) && (!FreezeNpc))
            {
                DumpRemains();
            }
            return;
        }

        CurTileX = (int)(transform.position.x / 1.2f);
        CurTileY = (int)(transform.position.z / 1.2f);


        UpdateNPCAWake();

        UpdateSpecialNPCBehaviours();

        //Update the appearance of the NPC
        UpdateSprite();

        //Update the Goal Target of the NPC
        UpdateGTarg();

        if ((npc_hp <= 0))
        {//Begin death handling.
            if ((objInt().ObjectTileX <= 63) || (objInt().ObjectTileY <= 63))
            {//Only kill on map npcs
                OnDeath();
            }
        }
        else
        {
            UpdateGoals();
        }

    }


    /// <summary>
    /// Hacks to control certain characters for special events.
    /// </summary>
    void UpdateSpecialNPCBehaviours()
    {
        if (_RES == GAME_UW2)
        {
            if (npc_whoami == 142) //Lord British
                if (GameWorldController.instance.dungeon_level == 0)
                {
                    if (Quest.GetQuestVariable(112,true) == 1)//Avatar has been fighting
                    {//Make sure I move to the correct location to talk to the avatar.
                        //THis is probably handled by a hack trap!
                        npc_xhome = 40;
                        npc_yhome = 38;
                    }
                }
        }
    }

    void UpdateNPCAWake()
    {
        //The AI is only active when the player is within a certain distance to the player camera.
        if (Vector3.Distance(this.transform.position, UWCharacter.Instance.CameraPos) <= 10)
        {
            if (objInt() != null)
            {
                if (TileMap.ValidTile(CurTileX, CurTileY))
                {
                    AI_INIT();
                }
                newAnim.enabled = true;
            }
        }
        else
        {
            newAnim.enabled = false;
            return;
        }
    }

    void NPCFollow()
    {//NPC is following the player or an object
        if (gtarg != null)
        {
            Vector3 ABf = this.transform.position - gtarg.transform.position;
            Vector3 Movepos = gtarg.transform.position + (0.9f * ABf.normalized);
            Agent.destination = Movepos;
            Agent.isStopped = false;
            //Set to idle										
            if (gtarg.name == "_Gronk")
            {
                //Help out the player dynamically
                if (UWCharacter.Instance.HelpMeMyFriends == true)
                {
                    UWCharacter.Instance.HelpMeMyFriends = false;
                    //If I'm not already busy with another NPC
                    //This will mean the NPC will turn on the player after combat?
                    if (UWCharacter.Instance.LastEnemyToHitMe != null)
                    {
                        //XG gtarg = UWCharacter.Instance.LastEnemyToHitMe;
                        npc_goal = (byte)npc_goals.npc_goal_attack_5;
                        npc_gtarg = (short)UWCharacter.Instance.LastEnemyToHitMe.GetComponent<ObjectInteraction>().ObjectIndex;
                        //gtargName = UWCharacter.Instance.LastEnemyToHitMe.name;
                    }
                }
                if ((_RES == GAME_UW1) && (GameWorldController.instance.dungeon_level == 8))
                {
                    //slasher of veils in the void needs to get rowdy. Otherwise he is passive when this level loas
                    if (item_id == 124)
                    {
                        npc_goal = (byte)npc_goals.npc_goal_attack_5;
                        npc_gtarg = 1;
                        //XG gtarg = UWCharacter.Instance.gameObject;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates the goals for NPCs that have no navmesh agent on them yet even though navmeshes are ready
    /// </summary>
    void UpdateGoalsForNonAgents()
    {
        switch ((npc_goals)npc_goal)
        {
            case npc_goals.npc_goal_want_to_talk:
                {
                    //I just want to talk to you
                    //ai.AI.WorkingMemory.SetItem<int> ("state", AI_STATE_STANDING);
                    AnimRange = AI_RANGE_IDLE;
                    if ((UWCharacter.Instance.isRoaming == false) && (ConversationVM.InConversation == false))
                    {
                        if (Vector3.Distance(this.transform.position, UWCharacter.Instance.CameraPos) <= 1.5)
                        {
                            TalkTo();
                        }
                    }
                    break;
                }
            default:
                return;
        }
    }

    void UpdateGoals()
    {
        if (Agent == null)
        {
            if (GameWorldController.NavMeshReady)
            {
                UpdateGoalsForNonAgents();
            }
            return;
        }
        if (!Agent.isOnNavMesh) { return; }
        if (isNPCFrozen()) { return; }
        //if (GameWorldController.instance.GenNavMeshNextFrame >0) {return;}//nav mesh update pending
        if (!GameWorldController.NavMeshReady) { return; }
        DistanceToGtarg = getDistanceToGtarg();

        //if ((npc_attitude == 0) && (Vector3.Distance (this.transform.position, UWCharacter.Instance.transform.position) <= UWCharacter.Instance.DetectionRange)) {
        if (//If the player comes close and I'm hostile. Make sure I go to combat mode.
                (npc_attitude == 0)
                &&
                (
                        (npc_goal != (short)npc_goals.npc_goal_attack_5)
                        &&
                        (npc_goal != (short)npc_goals.npc_goal_attack_9)
                )
                &&
                (DistanceToGtarg <= UWCharacter.Instance.DetectionRange)
                &&
                (npc_gtarg <= 3)
            )
        {
            npc_goal = (byte)npc_goals.npc_goal_attack_5;
            //Attack player
            npc_gtarg = 1;
        }
        if (targetBaseOffset != Agent.baseOffset)
        {//Raise or lower flying agents
            floatTime += Time.deltaTime;
            Agent.baseOffset = Mathf.Lerp(startBaseOffset, targetBaseOffset, floatTime);
        }

        switch ((npc_goals)npc_goal)
        {
            case npc_goals.npc_goal_want_to_talk:
                {
                    //I just want to talk to you
                    AnimRange = AI_RANGE_IDLE;
                    if ((UWCharacter.Instance.isRoaming == false) && (ConversationVM.InConversation == false))
                    {
                        if (Vector3.Distance(this.transform.position, UWCharacter.Instance.CameraPos) <= 1.5)
                        {
                            TalkTo();
                        }
                    }
                    break;
                }

            case npc_goals.npc_goal_goto_1: //Go to gtarg
            case npc_goals.npc_goal_stand_still_0:
            //Standing still
            case npc_goals.npc_goal_stand_still_7:
            case npc_goals.npc_goal_stand_still_11:
            case npc_goals.npc_goal_stand_still_12:
                {
                    DestTileX = npc_xhome; DestTileY = npc_yhome;
                    if ((CurTileX != npc_xhome) || (CurTileY != npc_yhome))
                    {
                        AnimRange = AI_RANGE_MOVE;
                        AgentGotoDestTileXY(ref DestTileX, ref DestTileY, ref CurTileX, ref CurTileY);
                    }
                    else
                    {
                        AnimRange = AI_RANGE_IDLE;
                        AgentStand();
                    }
                    break;
                }
            case npc_goals.npc_goal_wander_2://Wander randomly	
            case npc_goals.npc_goal_wander_4:
            case npc_goals.npc_goal_wander_8:
           // case npc_goals.npc_goal_flee://Morale failure. NPC flees in a random direction
                {
                    NPCWanderUpdate();
                    break;
                }
            case npc_goals.npc_goal_attack_5://Attack target	
            case npc_goals.npc_goal_attack_6: //attack at a range with magic?
            case npc_goals.npc_goal_attack_9:
                {
                    NPCCombatUpdate();
                    break;
                }
            case npc_goals.npc_goal_follow:
                {
                    //Follow target
                    NPCFollow();
                    break;
                }
            case npc_goals.npc_goal_petrified:
                {
                    AgentStand();
                    break;
                }
            default:
                {
                    Debug.Log("Unimplemented goal " + npc_goal);
                    break;
                }
        }

        if (
                (CurTileX != prevTileX)
                ||
                (CurTileY != prevTileY)
        )
        {
            switch ((npc_goals)npc_goal)
            {
                case npc_goals.npc_goal_want_to_talk:
                case npc_goals.npc_goal_stand_still_0:
                case npc_goals.npc_goal_stand_still_7:
                case npc_goals.npc_goal_stand_still_11:
                case npc_goals.npc_goal_stand_still_12:
                    if ((CurTileX != npc_xhome) && (CurTileY != npc_yhome))
                    {
                        NPCDoorUse();
                    }
                    break;
                default:
                    NPCDoorUse();
                    break;
            }
        }

        prevTileX = CurTileX;
        prevTileY = CurTileY;
    }

    /// <summary>
    /// NPC will attempt to open a door in their tile
    /// </summary>
    void NPCDoorUse()
    {
        //TODO:Make monster Types try and bash doors
        //bool Action=false; //true for open, false to bash.
        //Category 	Ethereal = 0x00 (Ethereal critters like ghosts, wisps, and shadow beasts), 
        //Humanoid = 0x01 (Humanlike non-thinking forms like lizardmen, trolls, ghouls, and mages),
        //Flying = 0x02 (Flying critters like bats and imps), 
        //Swimming = 0x03 (Swimming critters like lurkers), 
        //Creeping = 0x04 (Creeping critters like rats and spiders), 
        //Crawling = 0x05 (Crawling critters like slugs, worms, reapers (!), and fire elementals (!!)),
        //EarthGolem = 0x11 (Only used for the earth golem),
        //Human = 0x51 (Humanlike thinking forms like goblins, skeletons, mountainmen, fighters, outcasts, and stone and metal golems).
        switch ((NPCCategory)GameWorldController.instance.objDat.critterStats[item_id - 64].Category)
        {
            //case 0x01:
            //case 0x51:
            case NPCCategory.human:
            case NPCCategory.humanoid:
                break;//can open door
            default:
                return;//cannot open door (in the future will attempt to bash if hostile)
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (
                        ((x == 0) && ((y == -1) || (y == 1)))
                        ||
                        ((y == 0) && ((x == -1) || (x == 1)))
                        ||
                        ((x == 0) && (y == 0))
                )
                {
                    if (TileMap.ValidTile(CurTileX + x, CurTileY + y))
                    {
                        if (CurrentTileMap().Tiles[CurTileX + x, CurTileY + y].IsDoorForNPC)
                        {
                            GameObject door = DoorControl.findDoor(CurTileX + x, CurTileY + y);
                            if (door != null)
                            {
                                DoorControl dc = door.GetComponent<DoorControl>();
                                if (dc != null)
                                {
                                    if (dc.state() == false)
                                    {
                                        //door is closed
                                        if ((!dc.locked()) && (!dc.Spiked()))
                                        {
                                            //and can be opened.
                                            if (dc.DoorBusy == false)
                                            {
                                                dc.PlayerUse = false;
                                                dc.Activate(this.gameObject);
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

    }

    void NPCWanderUpdate()
    {
        bool AtDestination = ((DestTileX == CurTileX) && (DestTileY == CurTileY));
        TravelTimer += Time.deltaTime;
        if (TravelTimer >= 20f && AtDestination == false)
        {
            AtDestination = true;
        }
        if (!AtDestination)
        {
            //I need to move to this tile.
            AnimRange = AI_RANGE_MOVE;
            AgentGotoDestTileXY(ref DestTileX, ref DestTileY, ref CurTileX, ref CurTileY);
            ArrivedAtDestination = false;
        }
        else
        {       //I am at this tile. Stand idle for a random period of time
            AnimRange = AI_RANGE_IDLE;
            if (ArrivedAtDestination == false)
            {
                ArrivedAtDestination = true;
                if (WaitTimer == 0)
                {
                    WaitTimer = Random.Range(1f, 10f);
                }
            }
        }


        if (
                (WaitTimer <= 0) //I am not waiting around.
                &&
                (AtDestination)//I am at my destination already.
            )
        {
            if (AnimRange == AI_RANGE_IDLE)
            {//and I am idling.
                SetRandomDestination();
                ArrivedAtDestination = false;
            }
        }
    }
    /// <summary>
    /// Processes NPC Combat activitity
    /// </summary>
    void NPCCombatUpdate()
    {
        switch (AttackState)
        {
            case AttackStages.AttackPosition:
                if (gtarg != null)
                {
                    Vector3 AB = this.transform.position - gtarg.transform.position;
                    if (DistanceToGtarg > 1f)
                    {
                        if (DistanceToGtarg < 6f)
                        {//NPC is close to target.
                            if (MagicAttack || RangeAttack)
                            {
                                AgentStand();
                                transform.LookAt(gtarg.transform.position);
                                if (AgentCanAttack(NPC_Launcher.transform.position, gtarg.GetComponent<UWEBase>().GetImpactPoint(), gtarg, AB.magnitude))
                                {
                                    AnimRange = AI_ANIM_ATTACK_SECONDARY;
                                    AttackState = AttackStages.AttackAnimateRanged;
                                    WaitTimer = 0.8f;
                                }
                                else
                                {
                                    //Move to position
                                    AgentMoveToGtarg();
                                }
                            }
                            else
                            {
                                //Move to position
                                AgentMoveToGtarg();
                            }
                        }
                        else
                        {//Player is not close enough for an attack.
                            if (
                                    (DistanceToGtarg < UWCharacter.Instance.BaseEngagementRange + UWCharacter.Instance.DetectionRange)
                                    ||
                                    ((_RES == GAME_UW1) && (item_id == 124))//Slasher will never give up.
                                )
                            {//I'm close enough to still want to hunt you down.
                                AgentMoveToGtarg();
                            }
                            else
                            {
                                npc_goal = (byte)(npc_goals.npc_goal_wander_8);
                                AgentStand();
                            }
                        }
                    }
                    else
                    {
                        AgentStand();
                        transform.LookAt(gtarg.transform.position);
                        if (AgentCanAttack(NPC_Launcher.transform.position, gtarg.GetComponent<UWEBase>().GetImpactPoint(), gtarg, AB.magnitude))
                        {
                            SetRandomAttack();
                            AttackState = AttackStages.AttackAnimateMelee;
                            WaitTimer = 0.8f;
                        }
                        else
                        {
                            AgentMoveToGtarg();
                        }
                    }
                }
                else
                {
                    AgentStand();
                    AttackState = AttackStages.AttackWaitCycle;
                    WaitTimer = 0.8f;
                }
                break;
            case AttackStages.AttackAnimateMelee:
                if (WaitTimer <= 0.2f)
                {
                    ExecuteAttack();
                    AttackState = AttackStages.AttackExecute;
                    WaitTimer = 0.8f;
                }
                break;
            case AttackStages.AttackAnimateRanged:
                if (WaitTimer <= 0.2f)
                {
                    if (MagicAttack)
                    {
                        ExecuteMagicAttack();
                    }
                    else if (RangeAttack)
                    {
                        ExecuteRangedAttack();
                    }
                    AttackState = AttackStages.AttackExecute;
                    WaitTimer = 0.8f;
                }
                break;
            case AttackStages.AttackExecute:
                if (WaitTimer <= 0.2f)
                {
                    AttackState = AttackStages.AttackWaitCycle;
                    WaitTimer = 0.8f;
                }
                break;
            case AttackStages.AttackWaitCycle:
                AnimRange = AI_ANIM_COMBAT_IDLE;
                if (WaitTimer <= 0.2f)
                {
                    AttackState = AttackStages.AttackPosition;
                }
                break;
        }
    }

    void AgentMoveToGtarg()
    {
        //Move to position
        AgentMoveToPosition(gtarg.transform.position);
        AnimRange = AI_RANGE_MOVE;
    }

    void AgentGotoDestTileXY(ref int DestinationX, ref int DestinationY, ref int tileX, ref int tileY)
    {
        if (Agent.agentTypeID == GameWorldController.instance.NavMeshAir.agentTypeID)
        {
            //float tileHeight = (float)GameWorldController.instance.currentTileMap ().GetFloorHeight (tileX, tileY) * 0.15f;//Of current tile
            //float zpos = Random.Range (tileHeight, 4f);
            //AgentMoveToPosition( CurrentTileMap().getTileVector(DestTileX, DestTileY,zpos));	
            targetBaseOffset = 0.5f;//tileHeight + 0.2f ;//zpos - tileHeight;
            startBaseOffset = Agent.baseOffset;
            floatTime = 1f;
            //AgentMoveToPosition (GameWorldController.instance.currentTileMap ().getTileVector (DestTileX, DestTileY));
        }
        AgentMoveToPosition(CurrentTileMap().getTileVector(DestTileX, DestTileY));
    }

    void AgentStand()
    {
        if (Agent.isOnNavMesh)
        {
            destinationVector = this.transform.position;
            Agent.destination = this.transform.position;
            Agent.isStopped = true;
        }
    }

    void AgentMoveToPosition(Vector3 dest)
    {
        if (Agent.isOnNavMesh)
        {
            destinationVector = dest;
            Agent.destination = dest;
            Agent.isStopped = false;
        }
    }

    void UpdateGTarg()
    {
        //Update GTarg
        if (npc_gtarg <= 5)//PC
        {
            //gtarg = UWCharacter.Instance.transform.gameObject;
            ////////gtargName = "_Gronk";
            //////if (gtarg == null)
            //////{
            //////    gtarg = UWCharacter.Instance.transform.gameObject;
            //////}
            //////else
            //////{
            //////    if (gtarg.name != "_Gronk")
            //////    {
            //////        gtarg = UWCharacter.Instance.transform.gameObject;
            //////    }
            //////}
        }
        else
        {
            //////////Inbuilt NPC Gtargs not supported.
            ////////if (gtarg == null)
            ////////{
            ////////    if (gtargName != "")
            ////////    {
            ////////        gtarg = GameObject.Find(gtargName);
            ////////    }
            ////////}
            ////////else
            ////////{
            ////////    if (gtarg.name != gtargName)
            ////////    {
            ////////        gtarg = GameObject.Find(gtargName);
            ////////    }
            ////////}
            if (gtarg == null)
            {
                //I no longer have a goal target. Check what I was doing and revert to a different state.
                //Cases
                if ((npc_attitude > 0) && ((npc_goal == (short)npc_goals.npc_goal_attack_5) || (npc_goal == (short)npc_goals.npc_goal_attack_9)))
                {
                    //NPC Follower who has killed their target->Follow player.
                    npc_goal = (byte)npc_goals.npc_goal_follow;
                    npc_gtarg = 1;
                    //XG gtarg = UWCharacter.Instance.transform.gameObject;
                }
                if ((npc_attitude == 0) && ((npc_goal == (short)npc_goals.npc_goal_attack_5) || (npc_goal == (short)npc_goals.npc_goal_attack_9)))
                {
                    //NPC Enemy who has defeated their attacker->Focus on player.
                    npc_goal = (byte)npc_goals.npc_goal_attack_5;
                    npc_gtarg = 1;
                    //XG gtarg = UWCharacter.Instance.transform.gameObject;
                }
            }
        }
    }

    /// <summary>
    /// Tests if the NPCs line of sight to the target is not interupted by a door or a wall
    /// </summary>
    /// <returns><c>true</c>, if can attack was agented, <c>false</c> otherwise.</returns>
    /// <param name="src">Source.</param>
    /// <param name="target">Target.</param>
    /// <param name="targetGtarg">Target gtarg.</param>
    /// <param name="range">Range.</param>
    bool AgentCanAttack(Vector3 src, Vector3 target, GameObject targetGtarg, float range)
    {
        return !Physics.Linecast(src, target, GameWorldController.instance.MapMeshLayerMask | GameWorldController.instance.DoorLayerMask);
    }

    /// <summary>
    /// Applies the attack to the NPC
    /// </summary>
    /// <returns><c>true</c>, if attack was applyed, <c>false</c> otherwise.</returns>
    /// <param name="damage">Damage.</param>
    /// NPC becomes hostile on attack. 
    public override bool ApplyAttack(short damage)
    {
        if (!((_RES == GAME_UW1) && (item_id == 124)))
        {//Do not apply damage if attaching the slasher of veils.
            short NewHP = (short)(npc_hp - damage);
            if (NewHP < 0) { NewHP = 0; }
            npc_hp = (byte)NewHP;
            UWHUD.instance.MonsterEyes.SetTargetFrame(npc_hp, StartingHP);
        }
        return true;
    }

    /// <summary>
    /// Applies the attack from a known source
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="damage">Damage.</param>
    /// <param name="source">Source.</param>
    public override bool ApplyAttack(short damage, GameObject source)
    {
        if (source != null)
        {
            if (source.name == "_Gronk")
            {//PLayer attacked the npc
                if (npc_goal != (short)npc_goals.npc_goal_petrified)
                {
                    npc_attitude = 0;//Make the npc angry with the player.
                                     //Assumes the player has attacked
                    npc_gtarg = 1;
                    //XG gtarg = UWCharacter.Instance.gameObject;
                    //gtargName = gtarg.name;
                    npc_goal = (byte)npc_goals.npc_goal_attack_5;
                }
                if (npc_hp < 5)//Low health. 20% Chance for morale break
                {
                    if (Random.Range(0, 5) >= 4)
                    {
                        UWCharacter.Instance.PlayerMagic.CastEnchantment(source, this.gameObject, SpellEffect.UW1_Spell_Effect_CauseFear, Magic.SpellRule_TargetOther, -1);
                    }
                }

                //Alert nearby npcs that i have been attacked.
                //Will alert npcs of same item id or an allied type. (eg goblins & trolls)
                foreach (Collider Col in Physics.OverlapSphere(this.transform.position, 8.0f))
                {
                    if (Col.gameObject.GetComponent<NPC>() != null)
                    {
                        if (AreNPCSAllied(this, Col.gameObject.GetComponent<NPC>()))
                        {
                            if (Col.gameObject.GetComponent<NPC>().CanGetAngry())
                            {
                                Col.gameObject.GetComponent<NPC>().npc_attitude = 0;//Make the npc angry with the player.
                                Col.gameObject.GetComponent<NPC>().npc_gtarg = 1;
                                //Col.gameObject.GetComponent<NPC>().gtarg = UWCharacter.Instance.gameObject;
                                //Col.gameObject.GetComponent<NPC>().gtargName = gtarg.name;
                                Col.gameObject.GetComponent<NPC>().npc_goal = (byte)npc_goals.npc_goal_attack_5;
                            }
                        }
                    }
                }
            }
            //else
            {
                //		gtargName=source.name;
                //		//Makes the targeted entity attack the object that attacked it.
                //		npc_gtarg=999;
                //		gtarg=source;
                //		gtargName=source.name;
                //		npc_goal=(short)npc_goals.npc_goal_attack_5;;				
            }
        }

        ApplyAttack(damage);
        return true;
    }

    /// <summary>
    /// Determines whether this instance can get angry with the player
    /// </summary>
    /// <returns><c>true</c> if this instance can get angry; otherwise, <c>false</c>.</returns>
    bool CanGetAngry()
    {
        if (npc_goal == (short)npc_goals.npc_goal_petrified)
        { return false; }
        switch (_RES)
        {
            case GAME_UW1:
                if (npc_whoami == 27)//Garamon will be friendly always as long as you don't directly attack him
                {
                    return false;
                }
                break;
        }

        return true;
    }

    static bool AreNPCSAllied(NPC srcNPC, NPC dstNPC)
    {
        return (srcNPC.GetRace() == dstNPC.GetRace());
    }

    /// <summary>
    /// Begins a conversation if possible
    /// </summary>
    /// <returns><c>true</c>, if to was talked, <c>false</c> otherwise.</returns>
    public override bool TalkTo()
    {//Begin a conversation.				
        GameWorldController.instance.convVM.RunConversation(this);
        return true;
    }

    /// <summary>
    /// Looks at the NPC
    /// </summary>
    /// <returns>The <see cref="System.Boolean"/>.</returns>
    public override bool LookAt()
    {//TODO:For specific characters that don't follow the standard naming convention use their conversation for the lookat.
        string output = "";
        if (item_id != 123)//Tybal
        {
            output = StringController.instance.GetFormattedObjectNameUW(objInt(), NPCMoodDesc());
        }
        if ((npc_whoami >= 1) && (npc_whoami < 255))
        {
            if ((npc_whoami == 231) && (_RES == GAME_UW1))//Tybal
            {
                output = "You see Tybal";
            }
            else if (npc_whoami == 207)
            {//Warren spectre.
                output = "You see an " + NPCMoodDesc() + " " + StringController.instance.GetString(7, npc_whoami + 16);
            }
            else
            {
                if (npc_talkedto != 0)
                {
                    output = output + " named " + StringController.instance.GetString(7, npc_whoami + 16);
                }
            }

        }
        UWHUD.instance.MessageScroll.Add(output);
        //Debug.Log("My Target is " + gtarg.name + " because my npc_gtarg=" + npc_gtarg);
        return true;
    }

    /// <summary>
    /// Implements the study spell for NPCS
    /// </summary>
    public void StudyMonster()
    {
        //Get the power and undead status of the npc
        ///creature=0
        ///powerful creature=1
        ///undead creature=2
        ///powerful undead creature=3
        //Bit A at offset 0xD of the mobile data is how powerful the npc is.
        int PowerAndUndead = objInt().NPC_PowerFlag;
        if (ObjectInteraction.ScaleDamage(this.item_id, 1, 0x80) == 0)
        {//Presumably check if item is vulnerable to anti-undead damage??
            PowerAndUndead |= 2;
        }

        string Output = StringController.instance.GetString(1, 309 + PowerAndUndead);
        Output = Output + "a " + NPCMoodDesc() + " " + StringController.instance.GetObjectNounUW(objInt());

        UWHUD.instance.MessageScroll.Add(Output);

        Output = StringController.instance.GetString(1, 313) + npc_hp + "\n";

        UWHUD.instance.MessageScroll.Add(Output);


        if (GameWorldController.instance.objDat.critterStats[item_id - 64].Poison > 0)
        {
            Output = StringController.instance.GetString(1, 316);
            UWHUD.instance.MessageScroll.Add(Output);
        }


        Output = "";

        //List what spells it can cast.
        //None of these are implemeted yet to actually cast. 
        //Duplicate names are output here.
        //First 3 spells are in the critter data
        for (int k = 0; k < 3; k++)
        {
            if ((GameWorldController.instance.objDat.critterStats[item_id - 64].Spells[k]) > 0)
            {
                Output = Output + StringController.instance.GetString(6, 256 + (GameWorldController.instance.objDat.critterStats[item_id - 64].Spells[k] & 0x3F)) + " ";
            }
        }

        //then for a special case NPC (liche) there are 3 additional spells
        if (_RES == GAME_UW2)
        {
            if (item_id == 0x69)
            {
                //add fly, open! and flameproof
                Output += StringController.instance.GetString(6, 0x139);
                Output = Output + " " + StringController.instance.GetString(6, 0x123);
                Output = Output + " " + StringController.instance.GetString(6, 0x11c);
            }
        }

        if (Output.Length > 0)
        {
            Output = StringController.instance.GetString(1, 324) + Output;
            UWHUD.instance.MessageScroll.Add(Output);
        }


        //Print resistances
        Output = "";
        int[] Resistances = { 3, 4, 8, 0x10, 0x20, 0x40, 0x4b };
        //Magic, phys, fire, poison, cold, missiles

        for (int i = 0; i < 6; i++)
        {
            if (ObjectInteraction.ScaleDamage(item_id, 1, Resistances[i]) == 0)
            {
                if ((Resistances[i] <= 8) && (_RES == GAME_UW2) && (GameWorldController.instance.objDat.critterStats[item_id - 64].Race == 0x17))
                {
                    //do not apply the resistance to the liches. They should only have poison and rune of statis.
                }
                else
                {
                    Output = Output + StringController.instance.GetString(1, 0x146 + i) + " ";
                }
            }
        }

        if (_RES == GAME_UW2)
        {//IN addition all liches have immunity to statis runes. I wonder why :)
            if (GameWorldController.instance.objDat.critterStats[item_id - 64].Race == 0x17)
            {
                Output += StringController.instance.GetString(6, 0x125);
            }
        }

        if (Output.Length > 0)
        {
            Output = StringController.instance.GetString(1, 0x13D) + Output;
            UWHUD.instance.MessageScroll.Add(Output);
        }


    }



    /// <summary>
    /// Gives a mood string for NPCs
    /// </summary>
    /// <returns>The mood desc.</returns>
    private string NPCMoodDesc()
    {   //004€005€096€hostile
        //004€005€097€upset
        //004€005€098€mellow
        //004€005€099€friendly
        switch (npc_attitude)
        {
            case 0:
                return StringController.instance.GetString(5, 96);
            case 1:
                return StringController.instance.GetString(5, 97);
            case 2:
                return StringController.instance.GetString(5, 98);
            default:
                return StringController.instance.GetString(5, 99);

        }
    }


    /// <summary>
    ///Updates the appearance of the NPC
    /// </summary>
    /// Calculates the relative angle to the NPC
    void UpdateSprite()
    {
        //Get the relative vector between the player and the npc.
        direction = UWCharacter.Instance.gameObject.transform.position - this.gameObject.transform.position;
        //Convert the direction into an angle.
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        //Get the relative compass heading of the NPC.
        currentHeading = CompassHeadings[(int)Mathf.Round(((this.gameObject.transform.eulerAngles.y % 360) / 45f))];

        //Get an animation index number for the angle
        facingIndex = facing(angle);

        //Check the facing index and the animation range to see if they have changed since the last update.
        if ((PreviousFacing != facingIndex) && (AnimRange != PreviousAnimRange))
        {
            PreviousFacing = facingIndex;
            PreviousAnimRange = AnimRange;
        }
        //Offset the compass heading by the players relative heading.
        CalcedFacing = (short)(facingIndex + currentHeading);
        if (CalcedFacing >= 8)//Make sure it wrapps around correcly between 0 and 7 ->The compass headings.
        {
            CalcedFacing = (short)(CalcedFacing - 8);
        }
        if (CalcedFacing <= -8)
        {
            CalcedFacing = (short)(CalcedFacing + 8);
        }
        if (CalcedFacing < 0)
        {
            CalcedFacing = (short)(8 + CalcedFacing);
        }
        else if (CalcedFacing > 7)
        {
            CalcedFacing = (short)(8 - CalcedFacing);
        }

        //Calculate an animation index from the facing and the animation range.
        //0=front
        //1=frontright
        //7=frontleft

        //CalcFacingForDebug=CalcedFacing;
        if (
                ((AnimRange == AI_ANIM_ATTACK_BASH) && (!((CalcedFacing == 0) || (CalcedFacing == 1) || (CalcedFacing == 7))))
                ||
                ((AnimRange == AI_ANIM_ATTACK_SLASH) && (!((CalcedFacing == 0) || (CalcedFacing == 1) || (CalcedFacing == 7))))
                ||
                ((AnimRange == AI_ANIM_ATTACK_THRUST) && (!((CalcedFacing == 0) || (CalcedFacing == 1) || (CalcedFacing == 7))))
                ||
                ((AnimRange == AI_ANIM_COMBAT_IDLE) && (!((CalcedFacing == 0) || (CalcedFacing == 1) || (CalcedFacing == 7))))
        )
        {
            CalcedFacing = (short)((CalcedFacing + 1) * 1);//Use an idle animation if we can't see the attack
        }
        else
        {
            CalcedFacing = (short)((CalcedFacing + 1) * AnimRange);
        }


        //play the calculated animation.
        switch (CalcedFacing)
        {
            case AI_ANIM_IDLE_FRONT:
                {
                    playAnimation(14, false);
                    //playAnimation(NPC_ID +"_idle_front",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_FRONT_RIGHT:
                {
                    playAnimation(13, false);
                    //playAnimation(NPC_ID +"_idle_front_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_RIGHT:
                {
                    playAnimation(12, false);
                    //playAnimation(NPC_ID +"_idle_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_REAR_RIGHT:
                {
                    playAnimation(11, false);
                    //playAnimation(NPC_ID +"_idle_rear_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_REAR:
                {
                    playAnimation(10, false);
                    //playAnimation(NPC_ID +"_idle_rear",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_REAR_LEFT:
                {
                    playAnimation(17, false);
                    //playAnimation(NPC_ID + "_idle_rear_left",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_LEFT:
                {
                    playAnimation(16, false);
                    //	playAnimation(NPC_ID +"_idle_left",CalcedFacing);
                    break;
                }
            case AI_ANIM_IDLE_FRONT_LEFT:
                {
                    playAnimation(15, false);
                    //playAnimation(NPC_ID +"_idle_front_left",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_FRONT:
                {
                    playAnimation(22, false);
                    //playAnimation(NPC_ID +"_walking_front",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_FRONT_RIGHT:
                {
                    playAnimation(21, false);
                    //			playAnimation(NPC_ID + "_walking_front_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_RIGHT:
                {

                    playAnimation(20, false);
                    //playAnimation(NPC_ID + "_walking_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_REAR_RIGHT:
                {
                    playAnimation(19, false);
                    //playAnimation(NPC_ID +"_walking_rear_right",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_REAR:
                {
                    playAnimation(18, false);
                    //playAnimation(NPC_ID +"_walking_rear",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_REAR_LEFT:
                {
                    playAnimation(25, false);
                    //playAnimation(NPC_ID +"_walking_rear_left",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_LEFT:
                {
                    playAnimation(24, false);
                    //playAnimation(NPC_ID + "_walking_left",CalcedFacing);
                    break;
                }
            case AI_ANIM_WALKING_FRONT_LEFT:
                {
                    playAnimation(23, false);
                    //playAnimation(NPC_ID + "_walking_front_left",CalcedFacing);
                    break;
                }
            default://special non angled states
                {
                    switch (AnimRange)
                    {
                        case AI_ANIM_DEATH:
                            playAnimation(8, true); break;
                        //playAnimation (NPC_ID +"_death",AI_ANIM_DEATH);break;
                        case AI_ANIM_ATTACK_BASH:
                            //playAnimation (NPC_ID +"_attack_bash",AI_ANIM_ATTACK_BASH);break;
                            playAnimation(1, true); break;
                        case AI_ANIM_ATTACK_SLASH:
                            //playAnimation (NPC_ID +"_attack_slash",AI_ANIM_ATTACK_SLASH);break;
                            playAnimation(2, true); break;
                        case AI_ANIM_ATTACK_THRUST:
                            //playAnimation (NPC_ID +"_attack_thrust",AI_ANIM_ATTACK_THRUST);break;
                            playAnimation(3, true); break;
                        case AI_ANIM_COMBAT_IDLE:
                            //playAnimation (NPC_ID +"_combat_idle",AI_ANIM_COMBAT_IDLE);break;
                            playAnimation(0, true); break;
                        case AI_ANIM_ATTACK_SECONDARY:
                            //playAnimation (NPC_ID +"_attack_secondary",AI_ANIM_ATTACK_SECONDARY);break;
                            playAnimation(5, true); break;
                    }
                }
                break;
        }
    }


    /// <summary>
    /// Breaks down the angle in the the facing sector. Clockwise from 0)
    /// </summary>
    /// <param name="angle">Angle.</param>
    short facing(float angle)
    {
        if ((angle >= -22.5) && (angle <= 22.5))
        {
            return 0;//*AnimRange;//Facing forward
        }
        else
        {
            if ((angle > 22.5) && (angle <= 67.5))
            {//Facing forward left
                return 1;//*AnimRange;
            }
            else
            {
                if ((angle > 67.5) && (angle <= 112.5))
                {//facing (right)
                    return 2;//*AnimRange;
                }
                else
                {
                    if ((angle > 112.5) && (angle <= 157.5))
                    {//Facing away left
                        return 3;//*AnimRange;
                    }
                    else
                    {
                        if (((angle > 157.5) && (angle <= 180.0)) || ((angle >= -180) && (angle <= -157.5)))
                        {//Facing away
                            return 4;//*AnimRange;
                        }
                        else
                        {
                            if ((angle >= -157.5) && (angle < -112.5))
                            {//Facing away right
                                return 5;//*AnimRange;
                            }
                            else
                            {
                                if ((angle > -112.5) && (angle < -67.5))
                                {//Facing (left)
                                    return 6;//*AnimRange;
                                }
                                else
                                {
                                    if ((angle > -67.5) && (angle < -22.5))
                                    {//Facing forward right
                                        return 7;//*AnimRange;
                                    }
                                    else
                                    {
                                        return 0;//*AnimRange;//default
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    protected void playAnimation(int index, bool isConstantAnim)
    {
        newAnim.Play(index, isConstantAnim);
    }


    /// <summary>
    /// NPC tries to hit the player.
    /// </summary>
    public void ExecuteAttack()
    {
        if (ConversationVM.InConversation) { return; }
        if (UWCharacter.Instance.Death)//Don't attack if the player character is dead.
        { return; }
        if (gtarg == null)
        {
            return;
        }
        float weaponRange = 1.5f;
        Debug.Log(this.name + " is executing an attack on " + gtarg.name);

        Vector3 TargetingPoint;
        TargetingPoint = gtarg.GetComponent<UWEBase>().GetImpactPoint();//Aims for the objects impact point	                                                                        //}

        Ray ray = new Ray(NPC_Launcher.transform.position, TargetingPoint - NPC_Launcher.transform.position);

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, weaponRange))
        {
            if (hit.transform.Equals(this.transform))
            {
                Debug.Log("you've hit yourself ? " + hit.transform.name);
            }
            else
            {
                if (hit.transform.name == UWCharacter.Instance.name)
                {
                    UWCombat.NPC_Hits_PC(UWCharacter.Instance, this);
                }
                else
                {
                    //Has it hit another npc or object
                    if (hit.transform.GetComponent<NPC>() != null)
                    {
                        UWCombat.NPC_Hits_NPC(hit.transform.GetComponent<NPC>(), this);
                    }
                    else if (hit.transform.GetComponent<ObjectInteraction>() != null)
                    {
                        short attackDamage = (short)Random.Range(1, CurrentAttackDamage + 1);
                        hit.transform.GetComponent<ObjectInteraction>().Attack(attackDamage, this.gameObject);
                    }
                    else
                    {
                        Impact.SpawnHitImpact(Impact.ImpactDamage(), GetImpactPoint(), 46, 50);
                    }
                }
            }
        }
    }
    /// <summary>
    /// NPC casts a spell.
    /// </summary>
    public void ExecuteMagicAttack()
    {
        if (UWCharacter.Instance.Death)//Don't attack if the player character is dead.
        { return; }
        if (Vector3.Distance(this.transform.position, UWCharacter.Instance.CameraPos) > 8)
        {
            return;
        }
        UWCharacter.Instance.PlayerMagic.CastEnchantmentImmediate(NPC_Launcher, gtarg, SpellIndex(), Magic.SpellRule_TargetVector, Magic.SpellRule_Immediate);
    }

    /// <summary>
    /// Executes the ranged attack.
    /// </summary>
    public void ExecuteRangedAttack()
    {
        if (UWCharacter.Instance.Death)//Don't attack if the player character is dead.
        { return; }
        if (Vector3.Distance(this.transform.position, UWCharacter.Instance.CameraPos) > 8)
        {
            return;
        }
        Vector3 TargetingPoint;

        TargetingPoint = gtarg.GetComponent<UWEBase>().GetImpactPoint();//Aims for the objects impact point	
                                                                        //}
        Vector3 TargetingVector = (TargetingPoint - NPC_Launcher.transform.position).normalized;
        TargetingVector += new Vector3(0.0f, 0.3f, 0.0f);//Try and give the vector an arc
        Ray ray = new Ray(NPC_Launcher.transform.position, TargetingVector);
        RaycastHit hit = new RaycastHit();
        float dropRange = 0.5f;
        if (!Physics.Raycast(ray, out hit, dropRange))
        {///Checks No object interferes with the launch
            float force = 100f * Vector3.Distance(TargetingPoint, NPC_Launcher.transform.position);
            int projectiletype = RangedAttackProjectile();
            ObjectLoaderInfo newobjt = ObjectLoader.newWorldObject(projectiletype, 0, 0, 1, 256);
            if (newobjt != null)
            {
                newobjt.is_quant = 1;
                newobjt.ProjectileSourceID = ObjectIndex;
                GameObject launchedItem = ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, ray.GetPoint(dropRange - 0.1f)).gameObject;

                UnFreezeMovement(launchedItem);
                Vector3 ThrowDir = TargetingVector;
                ///Apply the force along the direction of the ray that the player has targetted along.
                launchedItem.GetComponent<Rigidbody>().AddForce(ThrowDir * force);
                GameObject myObjChild = new GameObject(launchedItem.name + "_damage");
                myObjChild.transform.position = launchedItem.transform.position;
                myObjChild.transform.parent = launchedItem.transform;
                ///Appends ProjectileDamage to the projectile to act as the damage delivery method.
                ProjectileDamage pd = myObjChild.AddComponent<ProjectileDamage>();
                pd.Source = this.gameObject;//This should be a property of the mobile object data
                pd.Damage = (short)GameWorldController.instance.objDat.rangedStats[projectiletype - 16].damage;//sling damage.
                pd.AttackCharge = 100f;
                pd.AttackScore = Dexterity;//Assuming there is no special ranged attack score?
                pd.ArmourDamage = EquipDamage;

            }
        }
    }

    /// <summary>
    /// Returns the type of projectile this npc will fire in a ranged attack
    /// </summary>
    /// <returns></returns>
    int RangedAttackProjectile()
    {
        switch (_RES)
        {
            case GAME_UW2:
                {
                    switch (item_id)
                    {//TODO:ID more values
                        case 110://iolo
                            return 21;
                        case 20://yeti snowball
                            return 28;
                        default:
                            return 16;
                    }
                }
            default://UW1 npcs only launch slingstones
                return 16;
        }

    }

    public override string ContextMenuDesc(int item_id)
    {
        if ((npc_talkedto != 0) && (npc_whoami != 0))
        {
            return StringController.instance.GetString(7, npc_whoami + 16);
        }
        else
        {
            return base.ContextMenuDesc(item_id);
        }
    }

    public override string ContextMenuUsedDesc()
    {
        TalkAvail = true;
        return base.ContextMenuUsedDesc();
    }

    public override string UseVerb()
    {
        return "talk";
    }

    public override Vector3 GetImpactPoint()
    {
        return NPC_Launcher.transform.position;
    }

    public override GameObject GetImpactGameObject()
    {
        return NPC_Launcher;
    }

    /// <summary>
    /// Returns the object that this NPC is targeting
    /// </summary>
    /// <returns>The gtarg.</returns>
    public GameObject getGtarg()
    {
        return gtarg;
    }


    public int Dexterity
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].Dexterity;
        }
    }

    public int Strength
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].Strength;
        }
    }

    public int Intelligence
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].Intelligence;
        }
    }


    public int CurrentAttackDamage
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].AttackDamage[CurrentAttack];
        }
    }

    public int CurrentAttackScore
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].AttackChanceToHit[CurrentAttack];
        }
    }

    public int Defence()
    {
        return GameWorldController.instance.objDat.critterStats[item_id - 64].Defence;
    }

    public int EquipDamage
    {
        get
        {
            return GameWorldController.instance.objDat.critterStats[item_id - 64].EquipDamage;
        }
    }

    public int GetRace()
    {
        return GameWorldController.instance.objDat.critterStats[item_id - 64].Race;
    }

    public int Room()
    {
        int tileX = (int)(transform.position.x / 1.2f);
        int tileY = (int)(transform.position.z / 1.2f);
        if (TileMap.ValidTile(tileX, tileY))
        {
            return CurrentTileMap().Tiles[tileX, tileY].roomRegion;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// What spell the NPC should use
    /// </summary>
    /// <returns>The index.</returns>
    public int SpellIndex()
    {
        switch (_RES)
        {
            case GAME_UW2:
                switch (item_id)
                {
                    case 88: //a_brain_creature
                        return SpellEffect.UW2_Spell_Effect_MindBlast;
                    case 96: //a_fire_elemental
                    case 104: //a_destroyer
                    case 105: //a_liche//
                        return SpellEffect.UW2_Spell_Effect_Fireball_alt01;
                    case 75: //an_imp
                    case 111: //a_gazer
                    case 117: //a_human
                    default:
                        return SpellEffect.UW2_Spell_Effect_MagicArrow_alt01;//Magic arrow 
                }
            default:
                switch (item_id)
                {
                    case 69://Acid slug
                        return SpellEffect.UW1_Spell_Effect_Acid_alt01;
                    case 120://Fire elemental
                        return SpellEffect.UW1_Spell_Effect_Fireball_alt01;
                    case 123://Tybal
                        return SpellEffect.UW1_Spell_Effect_SheetLightning_alt01;
                    default:
                        return SpellEffect.UW1_Spell_Effect_MagicArrow_alt01;//Magic arrow 
                }
        }

    }

    void SetRandomAttack()
    {
        int AttackProb = Random.Range(1, 100);
        //NPC npc = ai.Body.GetComponent<NPC>();
        int AccumulatedProb = 0;
        int AnimSelected;
        for (AnimSelected = 0; AnimSelected <= GameWorldController.instance.objDat.critterStats[NPC_IDi - 64].AttackProbability.GetUpperBound(0); AnimSelected++)
        {
            AccumulatedProb += GameWorldController.instance.objDat.critterStats[NPC_IDi - 64].AttackProbability[AnimSelected];
            if (AttackProb <= AccumulatedProb)
            {
                CurrentAttack = AnimSelected;
                break;
            }
        }

        switch (AnimSelected)
        {
            case 1:
                AnimRange = AI_ANIM_ATTACK_SLASH; break;
            case 2:
                AnimRange = AI_ANIM_ATTACK_THRUST; break;
            case 0:
            default:
                AnimRange = AI_ANIM_ATTACK_BASH; break;
        }


    }


    /// <summary>
    /// Sets a random locaton for the NPC to move to.
    /// </summary>
    void SetRandomDestination()
    {
        int distanceMultipler = 1;//For frightened NPCs
        TravelTimer = 0f;
        if (npc_goal == 6)
        {
            distanceMultipler = 6;//Runs further away from it's current location.
        }
        //Get a random spot.
        bool DestFound = false;
        Vector3 curPos = transform.position;

        int candidateDestTileX;
        int candidateDestTileY;
        for (int i = 0; i < 30; i++)
        {
            candidateDestTileX = npc_xhome + Random.Range(-2 * distanceMultipler, 3 * distanceMultipler);
            candidateDestTileY = npc_yhome + Random.Range(-2 * distanceMultipler, 3 * distanceMultipler);
            if (TileMap.ValidTile(DestTileX, DestTileY))
            {
                //&& (CurrentTileMap().GetTileType(candidateDestTileX, candidateDestTileY) != TileMap.TILE_SOLID)
                //if (CurrentTileMap().GetTileType(newTileX,newTileY) != TileMap.TILE_SOLID)
                if ((Room() == CurrentTileMap().GetRoom(candidateDestTileX, candidateDestTileY)) && (CurrentTileMap().GetTileType(candidateDestTileX, candidateDestTileY) != TileMap.TILE_SOLID))
                //if (CurrentTileMap().GetTileType(candidateDestTileX, candidateDestTileY) != TileMap.TILE_SOLID)
                {
                    DestTileX = candidateDestTileX;
                    DestTileY = candidateDestTileY;
                    DestFound = true;
                    break;
                }
            }
        }
        if (DestFound == false)
        {
            DestTileX = CurTileX;
            DestTileY = CurTileY;
        }

        //Move back home if too far away
        float xDist2 = Mathf.Pow(DestTileX - npc_xhome, 5f);
        float yDist2 = Mathf.Pow(DestTileY - npc_yhome, 5f);
        if (yDist2 + xDist2 >= 10)
        {
            DestTileX = npc_xhome;
            DestTileY = npc_yhome;
        }
    }

    /// <summary>
    /// Changes to the npcs death animation.
    /// </summary>
    void PerformDeathAnim()
    {
        AnimRange = AI_ANIM_DEATH;
        MusicController.LastAttackCounter = 0.0f;//Stops combat music unil the next time the player is hit
        MusicController.instance.PlaySpecialClip(MusicController.instance.VictoryTracks); //plays the fanfare
        WaitTimer = 0.8f;
    }


    void DumpRemains()
    {
        //Spawn a bloodstain at this spot.
        int bloodstain;
        //Nothing = 0x00, 
        //RotwormCorpse = 0x20, //A??????
        //Rubble = 0x40, 
        //WoodChips = 0x60, 
        //Bones = 0x80, 
        //GreenBloodPool = 0xA0, 
        //RedBloodPool = 0xC0, 
        //RedBloodPoolGiantSpider = 0xE0. 

        //switch (GameWorldController.instance.critterData.Remains[objInt.item_id-64])
        switch (GameWorldController.instance.objDat.critterStats[item_id - 64].Remains)
        {
            case 0x2://Rotworm corpse
                bloodstain = 217;
                break;
            case 0x4://Rubble
                bloodstain = 218;
                break;
            case 0x6://Woodchips
                bloodstain = 219;
                break;
            case 0x8://Bones
                bloodstain = 220;
                break;
            case 0xA://Greenblood
                bloodstain = 221;
                break;
            case 0xC://Redblood
                bloodstain = 222;
                break;
            case 0xE://RedBloodPoolGiantSpider
                bloodstain = 223;
                break;
            case 0://Nothing
                bloodstain = -1;
                break;
            default:
                Debug.Log(this.name + " should drop " + GameWorldController.instance.objDat.critterStats[item_id - 64].Remains);
                bloodstain = -1;
                break;
        }

        if (bloodstain != -1)
        {
            ObjectLoaderInfo newobjt = ObjectLoader.newWorldObject(bloodstain, 0, 0, 0, 256);
            ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, this.transform.position);
            //ObjectInteraction remains = ObjectInteraction.CreateNewObject(bloodstain);						
            //remains.gameObject.transform.parent=GameWorldController.instance.DynamicObjectMarker();
            //GameWorldController.MoveToWorld(remains.gameObject);
            //remains.transform.position=ai.Body.transform.position;
        }

        //Destroy(this.gameObject);
        ObjectInteraction.DestroyObjectFromUW(this.objInt());
    }

    public static int findNpcByWhoAmI(int whoami)
    {

        ObjectLoaderInfo[] objList = CurrentObjectList().objInfo;
        for (int i = 1; i < 256; i++)
        {
            if (objList[i].npc_whoami == whoami)
            {
                return i;
            }
        }
        return -1;
    }


    public static void SetNPCLocation(int index, int xhome, int yhome, NPC.npc_goals NewGoal)
    {
        if (index < 0) { return; }
        ObjectInteraction obj = ObjectLoader.getObjectIntAt(index);
        if (obj != null)
        {
            NPC npc = obj.GetComponent<NPC>();
            if (npc != null)
            {
                npc.npc_xhome = (short)xhome;
                npc.npc_yhome = (short)yhome;
                if ((short)NewGoal >= 0)
                {
                    npc.npc_goal = (byte)NewGoal;
                }
            }
        }
    }

    public static void SetNPCAttitudeGoal(int index, NPC.npc_goals NewGoal, short NewAttitude)
    {
        if (index < 0) { return; }
        ObjectInteraction obj = ObjectLoader.getObjectIntAt(index);
        if (obj != null)
        {
            NPC npc = obj.GetComponent<NPC>();
            if (npc != null)
            {
                if ((short)NewGoal >= 0)
                {
                    npc.npc_goal = (byte)NewGoal;
                }
                npc.npc_attitude = NewAttitude;
            }
        }
    }


    public short PoisonLevel()
    {
        return (short)GameWorldController.instance.objDat.critterStats[item_id - 64].Poison;
    }


    public float getDistanceToGtarg()
    {
        if (gtarg != null)
        {
            if (Agent != null)
            {
                if ((Agent.agentTypeID == GameWorldController.instance.NavMeshAir.agentTypeID))// || ((Agent.agentTypeID == GameWorldController.instance.NavMeshWater.agentTypeID)))
                {
                    return (this.transform.position - gtarg.transform.position).magnitude;
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(this.transform.position, gtarg.transform.position, Agent.areaMask, path))
                    {
                        //Taken from
                        //https://forum.unity.com/threads/getting-the-distance-in-nav-mesh.315846/
                        float lng = 0.0f;

                        if (path.status != NavMeshPathStatus.PathInvalid)
                        {
                            if (path.status == NavMeshPathStatus.PathPartial)
                            {//Add a penalty for incomplete routes
                                lng = 8f;
                            }

                            for (int i = 1; i < path.corners.Length; ++i)
                            {
                                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                            }
                        }

                        return lng;
                    }
                }

            }
            else
            {
                return (this.transform.position - gtarg.transform.position).magnitude;//if no ai etc 			
            }
        }
        return 1000f;//If no gtarg

    }

    /// <summary>
    /// Is the NPC frozen or subject to time freeze
    /// </summary>
    /// <returns><c>true</c>, if NPC frozen was ised, <c>false</c> otherwise.</returns>
    bool isNPCFrozen()
    {
        return (UWCharacter.Instance.isTimeFrozen || Paralyzed);
    }


    /// <summary>
    /// Tries to return the proper portrait for a UW2 NPC since the logic behind the allocation of char heads is unclear.
    /// </summary>
    /// <returns>The Portrait</returns>
    public Texture2D UW2NPCPortrait()
    {
        switch (npc_whoami)
        {
            case 1: //goblin guard
            case 2: //goblin
            case 3: //Borne
            case 4: //Garg
            case 5: //goblin armorer
            case 6: //Bishop
            case 7: //Marcus
            case 8: //Felix
            case 9: //goblin guard
            case 10: //goblin guard
            case 11: //Freemis
            case 12: //goblin guard
            case 13: //goblin guard
            case 14: //goblin guard
            case 15: //goblin guard
            case 16: //Milenus
            case 17: //Janar
            case 24: //Molloy
            case 25: //Helena
            case 26: //Trystero
            case 27: //Trystero
            case 28: //Reef
            case 29: //Lethe
            case 30: //Morphius
            case 31: //Lord Umbria
            case 32: //Praecor Loth
            case 34: //Silanus
            case 41: //Lobar
            case 42: //Kintara
            case 43: //Ogri
            case 44: //Mystell
            case 45: //Altara
            case 46: //Jerry the Rat
            case 47: //Mors Gotha
            case 48: //wisp
            case 49: //Relk
            case 50: //Lord Thibris
            case 51: //soldier
            case 52: //Chirl
            case 56: //Aron
            case 57: //Bishay
            case 72: //Mokpo
            case 73: //Beatrice
            case 74: //Sentinel 868
            case 75: //guard
            case 81: //Prinx
            case 82: //Mystell
            case 88: //Elster
            case 96: //Zoranthus
            case 97: //Zogith
            case 98: //Zaria
            case 99: //Dorstag
            case 100: //Krilner
            case 101: //Blog
            case 105: //Merzan
            case 128: //Fissif
            case 129: //guard
            case 130: //Nystul
            case 131: //Charles
            case 132: //Dupre
            case 133: //Geoffrey
            case 134: //Iolo
            case 135: //Julia
            case 136: //Miranda
            case 137: //Nanna
            case 138: //Nell
            case 139: //Nelson
            case 140: //Patterson
            case 141: //Lady Tory
            case 142: //Lord British
            case 143: //Feridwyn
            case 145: //The Listener
            case 146: //The Dripper
            case 149: //guard
            case 152: //Bliy Skup Ductosnore
            case 153: //Vorz Ductosnore
            case 156: //Historian
            case 168: //Syria
                {//Head is in charheads
                    GRLoader grCharHead = new GRLoader(GRLoader.CHARHEAD_GR);
                    return grCharHead.LoadImageAt((npc_whoami - 1));
                }
            default:
                {//Head is in genheads
                 //head in genhead.gr
                    int HeadToUse = item_id - 64;
                    if (HeadToUse > 59)
                    {
                        HeadToUse = 0;
                    }
                    GRLoader grGenHead = new GRLoader(GRLoader.GHED_GR);
                    return grGenHead.LoadImageAt(HeadToUse);
                }
        }
    }



    /// <summary>
    /// Play the walking sounds for the npc.
    /// </summary>
    /// <returns></returns>
    IEnumerator playfootsteps()
    {//WIP!!
        while (true)
        {
            yield return new WaitForSeconds(0.4f);
            if (Agent != null)
            {
                if (Agent.isOnNavMesh && Agent.velocity.magnitude != 0)
                {
                    if (newAnim.enabled)//AI is awake
                    {
                        if (!audMovement.isPlaying)
                        {
                            if (step)
                            {
                                npc_aud.PlayWalkingSound_1();
                                step = false;
                            }
                            else
                            {
                                npc_aud.PlayWalkingSound_2();
                                step = true;
                            }
                            //audMovement.Play();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Play the walking sounds for the npc.
    /// </summary>
    /// <returns></returns>
    IEnumerator playIdleBarks()
    {//WIP!!
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 20f));
            if (Agent != null)
            {
                if (newAnim.enabled)//AI is awake
                {
                    if (!audVoice.isPlaying)
                    {
                        npc_aud.PlayIdleSound();
                        //audVoice.Play();
                    }
                }
            }
        }
    }
}
