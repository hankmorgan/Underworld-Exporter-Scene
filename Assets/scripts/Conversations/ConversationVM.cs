﻿using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Implementation of the conversation virtual machine
/// based on UWAdventures hacking tools.
/// </summary>
public class ConversationVM : UWEBase
{

    //TODO:Make sure pickups support containers

    /// Is the user entering a quantity
    public static bool EnteringQty;

    public static bool InConversation;
    public static int CurrentConversation;

    ///The NPC is talking
    public const int NPC_SAY = 0;
    ///The PC is talking
    public const int PC_SAY = 1;
    ///Printed text is displayed
    public const int PRINT_SAY = 2;

    //The op codes for the vm.
    const short cnv_NOP = 0;
    const short cnv_OPADD = 1;
    const short cnv_OPMUL = 2;
    const short cnv_OPSUB = 3;
    const short cnv_OPDIV = 4;
    const short cnv_OPMOD = 5;
    const short cnv_OPOR = 6;
    const short cnv_OPAND = 7;
    const short cnv_OPNOT = 8;
    const short cnv_TSTGT = 9;
    const short cnv_TSTGE = 10;
    const short cnv_TSTLT = 11;
    const short cnv_TSTLE = 12;
    const short cnv_TSTEQ = 13;
    const short cnv_TSTNE = 14;
    const short cnv_JMP = 15;
    const short cnv_BEQ = 16;
    const short cnv_BNE = 17;
    const short cnv_BRA = 18;
    const short cnv_CALL = 19;
    const short cnv_CALLI = 20;
    const short cnv_RET = 21;
    const short cnv_PUSHI = 22;
    const short cnv_PUSHI_EFF = 23;
    const short cnv_POP = 24;
    const short cnv_SWAP = 25;
    const short cnv_PUSHBP = 26;
    const short cnv_POPBP = 27;
    const short cnv_SPTOBP = 28;
    const short cnv_BPTOSP = 29;
    const short cnv_ADDSP = 30;
    const short cnv_FETCHM = 31;
    const short cnv_STO = 32;
    const short cnv_OFFSET = 33;
    const short cnv_START = 34;
    const short cnv_SAVE_REG = 35;
    const short cnv_PUSH_REG = 36;
    const short cnv_STRCMP = 37;
    const short cnv_EXIT_OP = 38;
    const short cnv_SAY_OP = 39;
    const short cnv_RESPOND_OP = 40;
    const short cnv_OPNEG = 41;


    const int import_function = 0x111;
    const int import_variable = 0x10f;

    const int return_void = 0;
    const int return_int = 0x129;
    const int return_string = 0x12b;

    //const int TradeAreaOffset=0;//This hack is a bad hack...

   // private static string[] ObjectMasterList;

    //The input and output controls
    //private static Text Output;
    private static Text PlayerInput;

    private int currConv = 0;//The conversation that is being ran.

    public int MaxAnswer;
    private int NPCTalkedToIndex = 0;

    /// <summary>
    /// The player will teleport at the end of the conversation
    /// </summary>
    private bool Teleport = false;
    private int TeleportLevel = -1;
    private int TeleportTileX = -1;
    private int TeleportTileY = -1;
    private bool SettingUpFight = false;

    public static bool VMLoaded = false;

    /// <summary>
    /// Imported function and memory data from the conv.ark file
    /// </summary>
    struct ImportedFunctions
    {
        //0000   Int16   length of function name
        //0002   n*char  name of function
        public string functionName;
        //n+02   Int16   ID (imported func.) / memory address (variable)
        public int ID_or_Address;
        //	n+04   Int16   unknown, always seems to be 1
        public int import_type;//n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
        public int return_type; //n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)
    };

    /// <summary>
    /// Conversation header defines the size and layout of the converstation.
    /// </summary>
    struct cnvHeader
    {
        //0000   Int16   unknown, always seems to be 0x0828, or 28 08
        //0002   Int16   unknown, always 0x0000
        public int CodeSize;  //0004   Int16   code size in number of instructions (16-bit words)
                              ////0006   Int16   unknown, always 0x0000
                              //0008   Int16   unknown, always 0x0000
        public int StringBlock;//		000A   Int16   game strings block to use for conversation strings
        public int NoOfMemorySlots;//	000C   Int16   number of memory slots reserved for variables (*)
        public int NoOfImportedGlobals;//000E   Int16   number of imported globals (functions + variables)
                                       //0010           start of imported functions list	
        public ImportedFunctions[] functions;
        public short[] instuctions;
    };


    cnvHeader[] conv;

    /// <summary>
    /// The memory stack for the VM.
    /// </summary>
    CnvStack stack;

    /// The player answer.
    public static int PlayerAnswer;
    ///The text the player has typed in response to a question
    public static string PlayerTypedAnswer;
    ///Conversation waiting mode. Player has to enter a menu option
    public static bool WaitingForInput;
    ///Conversation waiting mode. Player has to press any key to continue
    public static bool WaitingForMore;
    ///Conversation waiting mode. Player has to type an answer.
    public static bool WaitingForTyping;
    ///The array that maps menu options in a bablf_menu to their answer numbers
    public static int[] bablf_array = new int[10];
    ///Tells if we are using a bablf_menu
    public static bool usingBablF;
    /// The answer from the bablf_menu
    public static int bablf_ans = 0;

    public static ConversationVM instance;

    private void Awake()
    {
        instance = this;
    }


    public void InitConvVM()
    {

        switch (_RES)
        {
            case GAME_UW2:
                LoadCnvArkUW2(Path.Combine(Loader.BasePath, "DATA", "CNV.ARK")); break;
            default:
                LoadCnvArk(Path.Combine(Loader.BasePath, "DATA", "CNV.ARK")); break;
        }
        VMLoaded = true;
    }


    void Update()
    {
        if (WaitingForTyping)
        {
            UWHUD.instance.InputControl.Select();//Keep focus on input control	
        }
    }

    /// <summary>
    /// Loads the cnv ark file and parses it to initialise the conversation headers and imported functions
    /// </summary>
    /// <param name="cnv_ark_path">Cnv ark path.</param>
    public void LoadCnvArk(string cnv_ark_path)
    {
        if (Loader.ReadStreamFile(cnv_ark_path, out byte[] cnv_ark))
        {
            int NoOfConversations = (int)Loader.getValAtAddress(cnv_ark, 0, 16);
            conv = new cnvHeader[NoOfConversations];
            for (int i = 0; i < NoOfConversations; i++)
            {
                int add_ptr = (int)Loader.getValAtAddress(cnv_ark, 2 + i * 4, 32);
                if (add_ptr != 0)
                {
                    /*
0000   Int16   unknown, always seems to be 0x0828, or 28 08
0002   Int16   unknown, always 0x0000
0004   Int16   code size in number of instructions (16-bit words)
0006   Int16   unknown, always 0x0000
0008   Int16   unknown, always 0x0000
000A   Int16   game strings block to use for conversation strings
000C   Int16   number of memory slots reserved for variables (*)
000E   Int16   number of imported globals (functions + variables)
0010           start of imported functions list
*/
                    conv[i].CodeSize = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0x4, 16);
                    conv[i].StringBlock = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xA, 16);
                    conv[i].NoOfMemorySlots = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xC, 16);
                    conv[i].NoOfImportedGlobals = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xE, 16);
                    conv[i].functions = new ImportedFunctions[conv[i].NoOfImportedGlobals];
                    int funcptr = add_ptr + 0x10;
                    for (int f = 0; f < conv[i].NoOfImportedGlobals; f++)
                    {

                        /*0000   Int16   length of function name
0002   n*char  name of function
n+02   Int16   ID (imported func.) / memory address (variable)
n+04   Int16   unknown, always seems to be 1
n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)*/
                        int len = (int)Loader.getValAtAddress(cnv_ark, funcptr, 16);
                        for (int j = 0; j < len; j++)
                        {
                            conv[i].functions[f].functionName += (char)Loader.getValAtAddress(cnv_ark, funcptr + 2 + j, 8);
                        }
                        conv[i].functions[f].ID_or_Address = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 2, 16);
                        conv[i].functions[f].import_type = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 6, 16);
                        conv[i].functions[f].return_type = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 8, 16);
                        funcptr += len + 10;
                    }
                    conv[i].instuctions = new short[conv[i].CodeSize];
                    int counter = 0;
                    for (int c = 0; c < conv[i].CodeSize * 2; c += 2)
                    {
                        conv[i].instuctions[counter++] = (short)Loader.getValAtAddress(cnv_ark, funcptr + c, 16);
                    }
                }
            }
        }
    }




    /// <summary>
    /// Loads the cnv ark file and parses it to initialise the conversation headers and imported functions
    /// </summary>
    /// <param name="cnv_ark_path">Cnv ark path.</param>
    public void LoadCnvArkUW2(string cnv_ark_path)
    {
        int address_pointer = 2;
        if (!Loader.ReadStreamFile(cnv_ark_path, out byte[] tmp_ark))
        {
            Debug.Log("unable to load uw2 conv ark");
            return;
        }

        int NoOfConversations = (int)Loader.getValAtAddress(tmp_ark, 0, 32);

        conv = new cnvHeader[NoOfConversations];

        for (int i = 0; i < NoOfConversations; i++)
        {
            int compressionFlag = (int)Loader.getValAtAddress(tmp_ark, address_pointer + (NoOfConversations * 4), 32);
            int isCompressed = (compressionFlag >> 1) & 0x01;
            int add_ptr = (int)Loader.getValAtAddress(tmp_ark, address_pointer, 32);
            if (add_ptr != 0)
            {
                if (isCompressed == 1)
                {
                    int datalen = 0;
                    byte[] cnv_ark = DataLoader.unpackUW2(tmp_ark, add_ptr, ref datalen);
                    add_ptr = 0;
                    /*
   0000   Int16   unknown, always seems to be 0x0828, or 28 08
   0002   Int16   unknown, always 0x0000
   0004   Int16   code size in number of instructions (16-bit words)
   0006   Int16   unknown, always 0x0000
   0008   Int16   unknown, always 0x0000
   000A   Int16   game strings block to use for conversation strings
   000C   Int16   number of memory slots reserved for variables (*)
   000E   Int16   number of imported globals (functions + variables)
   0010           start of imported functions list
    */
                    conv[i].CodeSize = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0x4, 16);
                    conv[i].StringBlock = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xA, 16);
                    conv[i].NoOfMemorySlots = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xC, 16);
                    conv[i].NoOfImportedGlobals = (int)Loader.getValAtAddress(cnv_ark, add_ptr + 0xE, 16);
                    conv[i].functions = new ImportedFunctions[conv[i].NoOfImportedGlobals];
                    long funcptr = add_ptr + 0x10;
                    for (int f = 0; f < conv[i].NoOfImportedGlobals; f++)
                    {

                        /*0000   Int16   length of function name
        0002   n*char  name of function
        n+02   Int16   ID (imported func.) / memory address (variable)
        n+04   Int16   unknown, always seems to be 1
        n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
        n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)
        */
                        int len = (int)Loader.getValAtAddress(cnv_ark, funcptr, 16);
                        for (int j = 0; j < len; j++)
                        {
                            conv[i].functions[f].functionName += (char)Loader.getValAtAddress(cnv_ark, funcptr + 2 + j, 8);
                        }
                        conv[i].functions[f].ID_or_Address = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 2, 16);
                        conv[i].functions[f].import_type = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 6, 16);
                        conv[i].functions[f].return_type = (int)Loader.getValAtAddress(cnv_ark, funcptr + len + 8, 16);
                        funcptr += len + 10;
                    }
                    conv[i].instuctions = new short[conv[i].CodeSize];
                    int counter = 0;
                    for (int c = 0; c < conv[i].CodeSize * 2; c += 2)
                    {
                        conv[i].instuctions[counter++] = (short)Loader.getValAtAddress(cnv_ark, funcptr + c, 16);
                    }


                }
                else
                {
                    Debug.Log("uncompressed flag in cnv.ark");
                }
            }
            address_pointer += 4;
        }

    }

    /// <summary>
    /// Displaies the instruction set.
    /// </summary>
    public void DisplayInstructionSet()
    {
        string result = "String Block = " + conv[currConv].StringBlock + "\n";
        result += "Code Size = " + conv[currConv].CodeSize + "\n";

        //Display the properties of the conversation
        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
        {
            if (conv[currConv].functions[i].import_type == import_function)
            {
                result += "Function : " + conv[currConv].functions[i].ID_or_Address + " " + conv[currConv].functions[i].functionName + "\n";
            }
            else
            {
                result += "Variable : " + conv[currConv].functions[i].ID_or_Address + " " + conv[currConv].functions[i].functionName + "\n";
            }
        }





        for (int z = 0; z < conv[currConv].CodeSize; z++)
        {
            switch (conv[currConv].instuctions[z])
            {
                case cnv_NOP: result += z + ":" + "NOP\n"; break;
                case cnv_OPADD: result += z + ":" + "OPADD\n"; break;
                case cnv_OPMUL: result += z + ":" + "OPMUL\n"; break;
                case cnv_OPSUB: result += z + ":" + "OPSUB\n"; break;
                case cnv_OPDIV: result += z + ":" + "OPDIV\n"; break;
                case cnv_OPMOD: result += z + ":" + "OPMOD\n"; break;
                case cnv_OPOR: result += z + ":" + "OPOR\n"; break;
                case cnv_OPAND: result += z + ":" + "OPAND\n"; break;
                case cnv_OPNOT: result += z + ":" + "OPNOT\n"; break;
                case cnv_TSTGT: result += z + ":" + "TSTGT\n"; break;
                case cnv_TSTGE: result += z + ":" + "TSTGE\n"; break;
                case cnv_TSTLT: result += z + ":" + "TSTLT\n"; break;
                case cnv_TSTLE: result += z + ":" + "TSTLE\n"; break;
                case cnv_TSTEQ: result += z + ":" + "TSTEQ\n"; break;
                case cnv_TSTNE: result += z + ":" + "TSTNE\n"; break;
                case cnv_JMP: result += z + ":" + "JMP "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_BEQ:
                    {//conv[currConv].instuctions[stack.instrp+1];	
                        result += z + ":" + "BEQ ";
                        z++;
                        result += " " + conv[currConv].instuctions[z] + " // ";
                        result += " to " + (conv[currConv].instuctions[z] + z);
                        result += "\n"; break;
                    }

                case cnv_BNE: result += z + ":" + "BNE "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_BRA: result += z + ":" + "BRA "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_CALL: result += z + ":" + "CALL "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_CALLI:
                    {
                        result += z + ":" + "CALLI "; z++;
                        //result += z + ":" + " "+ conv[currConv].instuctions[z] + " // ";
                        result += " " + conv[currConv].instuctions[z] + " // ";
                        int arg1 = conv[currConv].instuctions[z];
                        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
                        {
                            if ((conv[currConv].functions[i].ID_or_Address == arg1) && (conv[currConv].functions[i].import_type == import_function))
                            {
                                result += conv[currConv].functions[i].functionName + "\n";
                                break;
                            }
                        }
                        break;
                    }

                case cnv_RET: result += z + ":" + "RET\n"; break;
                case cnv_PUSHI: result += z + ":" + "PUSHI "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_PUSHI_EFF: result += z + ":" + "PUSHI_EFF "; z++; result += " " + conv[currConv].instuctions[z] + "\n"; break;
                case cnv_POP: result += z + ":" + "POP\n"; break;
                case cnv_SWAP: result += z + ":" + "SWAP\n"; break;
                case cnv_PUSHBP: result += z + ":" + "PUSHBP\n"; break;
                case cnv_POPBP: result += z + ":" + "POPBP\n"; break;
                case cnv_SPTOBP: result += z + ":" + "SPTOBP\n"; break;
                case cnv_BPTOSP: result += z + ":" + "BPTOSP\n"; break;
                case cnv_ADDSP: result += z + ":" + "ADDSP\n"; break;
                case cnv_FETCHM: result += z + ":" + "FETCHM\n"; break;
                case cnv_STO: result += z + ":" + "STO\n"; break;
                case cnv_OFFSET: result += z + ":" + "OFFSET\n"; break;
                case cnv_START: result += z + ":" + "START\n"; break;
                case cnv_SAVE_REG: result += z + ":" + "SAVE_REG\n"; break;
                case cnv_PUSH_REG: result += z + ":" + "PUSH_REG\n"; break;
                case cnv_STRCMP: result += z + ":" + "STRCMP\n"; break;
                case cnv_EXIT_OP: result += z + ":" + "EXIT_OP\n"; break;
                case cnv_SAY_OP:
                    {
                        result += z + ":" + "SAY_OP  //";
                        int stringToSay = conv[currConv].instuctions[z - 1];
                        string output = StringController.instance.GetString(conv[currConv].StringBlock, stringToSay);
                        result += output + "\n";
                        break;
                    }
                case cnv_RESPOND_OP: result += z + ":" + "RESPOND_OP\n"; break;
                case cnv_OPNEG: result += z + ":" + "OPNEG\n"; break;
            }
        }
        //}
        //Write the result to file

        TextWriter tw = new StreamWriter(Path.Combine(Loader.BasePath, "conversation_debug.txt"));
        tw.Write(result);
        tw.Close();
    }




    /// <summary>
    /// Runs the conversation.
    /// </summary>
    public void RunConversation(NPC npc)
    {
        string npcname = "";
        if (!VMLoaded)
        {
            InitConvVM();
            if (_RES == GAME_UW2)
            {
                UWHUD.instance.UW2ConversationBG.texture = GameWorldController.instance.bytloader.LoadImageAt(2, false);
            }
        }

        if (npc.npc_whoami == 0)
        {
            currConv = 256 + (npc.item_id - 64);
            npcname = StringController.instance.GetSimpleObjectNameUW(npc.objInt());
        }
        else
        {
            currConv = npc.npc_whoami;
            if (_RES == GAME_UW2)
            {
                currConv++;
            }
        }

        if (npc.npc_whoami > 255)
        {//Generic conversation.
            npcname = StringController.instance.GetSimpleObjectNameUW(npc.objInt());
        }

        if ((conv[currConv].CodeSize == 0) || (npc.npc_whoami == 255))
        {//006~007~001~You get no response.
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(7, 1));
            return;
        }

        if (//Don't talk to hostile NPCS in UW2
                (_RES == GAME_UW2)
                &&
                (
                        (npc.npc_goal == (short)NPC.npc_goals.npc_goal_attack_5)
                ||
                        (npc.npc_goal == (short)NPC.npc_goals.npc_goal_attack_9)
                )
        )
        {
            UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(7, 1));
            return;
        }


        if (npcname == "")
        {
            UWHUD.instance.NPCName.text = StringController.instance.GetString(7, npc.npc_whoami + 16);
        }
        else
        {
            UWHUD.instance.NPCName.text = npcname;
        }

        UWHUD.instance.PCName.text = UWCharacter.Instance.CharName; ;



        Character.InteractionMode = Character.InteractionModeInConversation;//Set converation mode.
        CurrentConversation = npc.npc_whoami;//To make obsolete
        InConversation = true;

        UWHUD.instance.RefreshPanels(UWHUD.HUD_MODE_CONV);
        UWHUD.instance.Conversation_tl.Clear();
        UWHUD.instance.MessageScroll.Clear();
        PlayerInput = UWHUD.instance.MessageScroll.NewUIOUt;

        UWHUD.instance.RefreshPanels(UWHUD.HUD_MODE_CONV);

        ///Clear the trade slots for the npcs
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            UWHUD.instance.npcTrade[i++].clear();
        }

        ///Sets up the portraits for the player and the NPC
        RawImage portrait = UWHUD.instance.ConversationPortraits[0];
        RawImage npcPortrait = UWHUD.instance.ConversationPortraits[1];
        GRLoader grPCHead = new GRLoader(GRLoader.HEADS_GR);
        if (UWCharacter.Instance.isFemale)
        {
            //portrait.texture=Resources.Load <Texture2D> (_RES +"/HUD/PlayerHeads/heads_"+ (UWCharacter.Instance.Body+5).ToString("0000"));//TODO:playerbody
            portrait.texture = grPCHead.LoadImageAt(UWCharacter.Instance.Body + 5);
        }
        else
        {
            //portrait.texture=Resources.Load <Texture2D> (_RES +"/HUD/PlayerHeads/heads_"+ (UWCharacter.Instance.Body).ToString("0000"));//TODO:playerbody		
            portrait.texture = grPCHead.LoadImageAt(UWCharacter.Instance.Body);
        }


        switch (_RES)
        {
            case GAME_UW2:
                {
                    //GRLoader grCharHead = new GRLoader(GRLoader.CHARHEAD_GR);
                    npcPortrait.texture = npc.UW2NPCPortrait(); //grCharHead.LoadImageAt((npc.npc_whoami-1));
                    npc.SetupNPCInventory();
                    //if ((npc.npc_whoami != 0) && (npc.npc_whoami <= 28))
                    //{
                    //    GRLoader grCharHead = new GRLoader(GRLoader.CHARHEAD_GR);
                    //    npcPortrait.texture = grCharHead.LoadImageAt((npc.npc_whoami - 1));
                    //}
                    //else
                    //{
                    //    //head in genhead.gr
                    //    int HeadToUse = npc.item_id - 64;
                    //    if (HeadToUse > 59)
                    //    {
                    //        HeadToUse = 0;
                    //    }
                    //    GRLoader grGenHead = new GRLoader(GRLoader.GHED_GR);
                    //    npcPortrait.texture = grGenHead.LoadImageAt(HeadToUse);
                    //}
                    break;
                }
            default:
                {
                    if ((npc.npc_whoami != 0) && (npc.npc_whoami <= 28))
                    {
                        GRLoader grCharHead = new GRLoader(GRLoader.CHARHEAD_GR);
                        npcPortrait.texture = grCharHead.LoadImageAt((npc.npc_whoami - 1));
                    }
                    else
                    {
                        //head in genhead.gr
                        int HeadToUse = npc.item_id - 64;
                        if (HeadToUse > 59)
                        {
                            HeadToUse = 0;
                        }
                        GRLoader grGenHead = new GRLoader(GRLoader.GENHEAD_GR);
                        npcPortrait.texture = grGenHead.LoadImageAt(HeadToUse);
                    }
                    break;
                }
        }


        UWHUD.instance.MessageScroll.Clear();
        ClearConversationOptions();

        /*End UI Setup*/

        ///Cancels player movement
        UWCharacter.Instance.playerMotor.enabled = false;

        ///Sets the music to the conversation theme
        if (MusicController.instance != null)
        {
            MusicController.instance.InMap = true;
        }
        //lastObjectTraded=null;

        //BuildObjectList();

        DisplayInstructionSet();
        ///Slows the world down so no other npc will attack or interupt the conversation
        Time.timeScale = 0.00f;
        Teleport = false;
        SettingUpFight = false;
        StopAllCoroutines();
        UWHUD.instance.EnableDisableControl(UWHUD.instance.FreeLookCursor.gameObject, false);
        StartCoroutine(RunConversationVM(npc));
    }








    /// <summary>
    /// Main looping function for a conversation
    /// </summary>
    /// <returns>The conversation V.</returns>
    private IEnumerator RunConversationVM(NPC npc)
    {
        //basep = 0;
        //stack.result_register = 1;//Set a default value

        bool finished = false;
        stack = new CnvStack(4096);
        stack.set_stackp(200);//Skip over imported memory for the moment
        stack.basep = 0;
        WaitingForInput = false;
        WaitingForTyping = false;
        WaitingForMore = false;

        //Import the variables
        ImportVariableMemory(npc);

        // execute one instruction
        //switch(code[stack.instrp])
        while ((finished == false))
        {
            switch (conv[currConv].instuctions[stack.instrp])
            {
                case cnv_NOP:
                    {
                        break;
                    }


                case cnv_OPADD:
                    {
                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    }


                case cnv_OPMUL:
                    {
                        stack.Push(stack.Pop() * stack.Pop());
                        break;
                    }


                case cnv_OPSUB:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        stack.Push(arg2 - arg1);
                        break;
                    }


                case cnv_OPDIV:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        //if (arg1==0)
                        //	throw ua_ex_div_by_zero;
                        stack.Push(arg2 / arg1);
                        break;
                    }


                case cnv_OPMOD:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        //if (arg1==0)
                        //	throw ua_ex_div_by_zero;
                        stack.Push(arg2 % arg1);
                        break;
                    }


                case cnv_OPOR:
                    {
                        stack.Push(stack.Pop() | stack.Pop());
                    }
                    break;

                case cnv_OPAND:
                    {
                        stack.Push(stack.Pop() & stack.Pop());
                    }
                    break;

                case cnv_OPNOT:
                    {
                        if (stack.Pop() == 0)
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        //stack.Push(!stack.Pop());
                        break;
                    }


                case cnv_TSTGT:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        if (arg2 > arg1)
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        //stack.Push(arg2 > arg1);
                        break;
                    }


                case cnv_TSTGE:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        if (arg2 >= arg1)
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }

                        //stack.Push(arg2 >= arg1);
                        break;
                    }


                case cnv_TSTLT:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        if (arg2 < arg1)
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        //stack.Push(arg2 < arg1);
                        break;
                    }


                case cnv_TSTLE:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        if (arg2 <= arg1)
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        //stack.Push(arg2 <= arg1);
                        break;
                    }


                case cnv_TSTEQ:
                    {
                        if (stack.Pop() == stack.Pop())
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        break;
                    }


                case cnv_TSTNE:
                    {
                        //int val1 = stack.Pop();
                        //int val2 = stack.Pop();
                        if (stack.Pop() != stack.Pop())
                        //if ((val1) != (val2))
                        {
                            stack.Push(1);
                        }
                        else
                        {
                            stack.Push(0);
                        }
                        //stack.Push(arg2 != arg1);
                        break;
                    }

                case cnv_JMP:
                    {//Debug.Log("instr = " +stack.instrp + " JMP to " +  conv[currConv].instuctions[stack.instrp+1]);
                        stack.instrp = conv[currConv].instuctions[stack.instrp + 1] - 1;
                        break;
                    }

                case cnv_BEQ:
                    {
                        //int origInstrp= stack.instrp;
                        if (stack.Pop() == 0)
                        {
                            stack.instrp += conv[currConv].instuctions[stack.instrp + 1];
                        }

                        else
                        {
                            stack.instrp++;
                        }
                        //Debug.Log("BEQ to " + stack.instrp + " at " + origInstrp);
                        break;
                    }

                case cnv_BNE:
                    {
                        //int origInstrp= stack.instrp;
                        if (stack.Pop() != 0)
                        {
                            stack.instrp += conv[currConv].instuctions[stack.instrp + 1];
                        }
                        else
                        {
                            stack.instrp++;
                        }
                        //Debug.Log("BNE to " + stack.instrp + " at " + origInstrp);
                        break;
                    }

                case cnv_BRA:
                    {
                        //int origInstrp= stack.instrp;
                        stack.instrp += conv[currConv].instuctions[stack.instrp + 1];
                        //Debug.Log("BRA to " + stack.instrp + " at " + origInstrp);
                        /*int offset = conv[currConv].instuctions[stack.instrp+1];
        if (offset >0)
        {							
            stack.instrp += offset;	
        }
        else
        {		
            stack.instrp += offset;
        }*/
                        break;
                    }

                case cnv_CALL: // local function
                    {
                        //int origInstrp= stack.instrp;
                        // stack value points to next instruction after call
                        //Debug.Log("inst=" + stack.instrp + "stack ptr" + stack.stackptr + " new inst=" + (conv[currConv].instuctions[stack.instrp+1]-1));
                        stack.Push(stack.instrp + 1);
                        stack.instrp = conv[currConv].instuctions[stack.instrp + 1] - 1;
                        stack.call_level++;
                        //Debug.Log("CALL to " + stack.instrp + " at " + origInstrp);
                        break;
                    }

                case cnv_CALLI: // imported function
                    {
                        int arg1 = conv[currConv].instuctions[++stack.instrp];
                        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
                        {
                            if ((conv[currConv].functions[i].ID_or_Address == arg1) && (conv[currConv].functions[i].import_type == import_function))
                            {
                                Debug.Log("Calling function  " + arg1 + " which is currently : " + conv[currConv].functions[i].functionName );
                                yield return StartCoroutine(run_imported_function(conv[currConv].functions[i], npc));
                                break;
                            }
                        }
                        break;
                    }


                case cnv_RET:
                    {
                        if (--stack.call_level < 0)
                        {
                            // conversation ended
                            finished = true;
                        }
                        else
                        {
                            //Debug.Log("instr = " +stack.instrp + " returning to " + arg1);
                            stack.instrp = stack.Pop();
                        }
                        break;
                    }


                case cnv_PUSHI:
                    {
                        //Debug.Log("Instruction:" + stack.instrp +" Pushing Immediate value :" +conv[currConv].instuctions[stack.instrp+1] + " => " + stack.stackptr);
                        stack.Push(conv[currConv].instuctions[++stack.instrp]);
                        break;
                    }


                case cnv_PUSHI_EFF:
                    {
                        int offset = conv[currConv].instuctions[stack.instrp + 1];
                        if (offset >= 0)
                        {
                            stack.Push(stack.basep + offset);

                        }
                        else
                        {
                            offset--; //to skip over base ptr;
                            stack.Push(stack.basep + offset);
                        }
                        stack.instrp++;
                        break;
                    }


                case cnv_POP:
                    {
                        stack.Pop();
                        break;
                    }


                case cnv_SWAP:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        stack.Push(arg1);
                        stack.Push(arg2);
                        break;
                    }

                case cnv_PUSHBP:
                    {
                        //Debug.Log("Instruction:" + stack.instrp +" Pushing Base Ptr :" + stack.basep + " => " + stack.stackptr);							
                        stack.Push(stack.basep);
                        break;
                    }


                case cnv_POPBP:
                    {
                        int arg1 = stack.Pop();
                        stack.basep = arg1;
                        break;
                    }

                case cnv_SPTOBP:
                    {
                        stack.basep = stack.stackptr;
                        break;
                    }


                case cnv_BPTOSP:
                    {
                        stack.set_stackp(stack.basep);
                        break;
                    }


                case cnv_ADDSP:
                    {
                        int arg1 = stack.Pop();
                        /// fill reserved stack space with dummy values
                        for (int i = 0; i <= arg1; i++)
                            stack.Push(0);

                        //stack.Set_stackp(stack.stackptr+arg1);//This will probably cause problems down the line....
                        //stack.Set_stackp(stack.stackptr+arg1);
                        break;
                    }


                case cnv_FETCHM:
                    {
                        //Debug.Log("Instruction:" + stack.instrp +" Fetching address :" + stack.TopValue + " => " + stack.at(stack.TopValue));
                        //stack.at(arg1);
                        stack.Push(stack.at(stack.Pop()));
                        break;
                    }


                case cnv_STO:
                    {
                        int value = stack.Pop();
                        //int value = stack.at(stack.stackptr-1);
                        int index = stack.Pop();
                        //int index = stack.at(stack.stackptr-2);
                        if (index < conv[currConv].NoOfImportedGlobals)
                        {
                            PrintImportedVariable(index, value);
                        }
                        stack.Set(index, value);

                        break;
                    }


                case cnv_OFFSET:
                    {
                        int arg1 = stack.Pop();
                        int arg2 = stack.Pop();
                        //Debug.Log("Offset " +arg1 + " & " + arg2  + "= " + (arg1+arg2-1));
                        arg1 += arg2 - 1;
                        //Debug.Log("Instruction:" + stack.instrp +" Offset pushed : " + arg1 + " => " + stack.stackptr);

                        stack.Push(arg1);
                        break;
                    }


                case cnv_START:
                    {
                        // do nothing
                        break;
                    }

                case cnv_SAVE_REG:
                    {
                        stack.result_register = stack.Pop();
                        break;
                    }


                case cnv_PUSH_REG:
                    {
                        stack.Push(stack.result_register);
                        break;
                    }


                case cnv_EXIT_OP:
                    {
                        // finish processing (we still might be in some sub function)
                        finished = true;
                        break;
                    }


                case cnv_SAY_OP:
                    {
                        int arg1 = stack.Pop();
                        yield return StartCoroutine(say_op(arg1));
                        break;
                    }

                case cnv_RESPOND_OP:
                    {// do nothing
                        Debug.Log("Respond_Op");
                        break;
                    }

                case cnv_OPNEG:
                    {
                        stack.Push(-stack.Pop());
                        break;
                    }


                default: // unknown opcode
                         //throw ua_ex_unk_opcode;
                    break;
            }

            // process next instruction
            ++stack.instrp;
            if (stack.instrp > conv[currConv].instuctions.GetUpperBound(0))
            {
                finished = true;
            }
        }
        yield return new WaitForSecondsRealtime(2f);
        yield return StartCoroutine(EndConversation(npc));
    }

    /// <summary>
    /// Ends the conversation.
    /// </summary>
    public IEnumerator EndConversation(NPC npc)
    {

        UWHUD.instance.EnableDisableControl(UWHUD.instance.FreeLookCursor.gameObject, true);

        //Copy back private variables to the globals file.

        for (int c = 0; c <= GameWorldController.instance.bGlobals.GetUpperBound(0); c++)
        {
            if (CurrentConversation == GameWorldController.instance.bGlobals[c].ConversationNo)
            {
                GameWorldController.instance.bGlobals[c].Globals[NPCTalkedToIndex] = 1;
                for (int x = 0; x <= GameWorldController.instance.bGlobals[c].Globals.GetUpperBound(0); x++)
                {
                    //Copy Private variables
                    GameWorldController.instance.bGlobals[c].Globals[x] = stack.at(x);
                }
                break;
            }
        }
        int maxAddress = 0;//where does the imported memory end
                           //Copy back npc related variables that need to update. Eg attitude. Talked to etc.
        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
        {
            if (conv[currConv].functions[i].import_type == import_variable)
            {
                int address = conv[currConv].functions[i].ID_or_Address;
                if (address > maxAddress)
                {
                    maxAddress = address;
                }
                switch (conv[currConv].functions[i].functionName.ToLower())
                {
                    case "npc_talkedto":
                        npc.npc_talkedto = 1; break;
                    //npc.npc_talkedto = stack.at(address);break;
                    case "npc_gtarg":
                        npc.npc_gtarg = (short)stack.at(address); break;
                    case "npc_attitude":
                        npc.npc_attitude = (short)stack.at(address); break;
                    case "npc_goal":
                        npc.npc_goal = (byte)stack.at(address); break;
                    case "npc_power":
                        npc.npc_power = (short)stack.at(address); break;
                    case "npc_arms":
                        npc.npc_arms = (short)stack.at(address); break;
                    case "npc_hp":
                        npc.npc_hp = (byte)stack.at(address); break;
                    case "npc_health":
                        npc.npc_health = (short)stack.at(address); break;
                    case "npc_hunger":
                        npc.npc_hunger = (short)stack.at(address); break;
                    case "npc_whoami":
                        npc.npc_whoami = (short)stack.at(address); break;
                    case "npc_yhome":
                        npc.npc_yhome = (short)stack.at(address); break;
                    case "npc_xhome":
                        npc.npc_xhome = (short)stack.at(address); break;
                }

            }
        }
        if (_RES == GAME_UW2)
        {//Hack to fix quest flag settings for garg
            if (currConv == 5)
            {
                Debug.Log("GARG CONVERSATION HACK");
                //Quest.QuestVariablesOBSOLETE[0] = stack.at(31);
                Quest.SetQuestVariable(0, stack.at(31));
            }
        }

        ClearConversationOptions();

        ///Give movement back to the player			
        UWCharacter.Instance.playerMotor.enabled = true;

        Container cn = UWCharacter.Instance.playerInventory.currentContainer;

        ///Return any items in the trade area to their owner
        for (int i = 0; i <= UWHUD.instance.playerTrade.GetUpperBound(0); i++)
        {
            TradeSlot SlotToClear = UWHUD.instance.playerTrade[i];
            if (SlotToClear.objectInSlot != null)
            {///Moves the object to the players container or to the ground
                if (Container.GetFreeSlot(cn) != -1)//Is there space in the container.
                {                    
                    var moved=GameWorldController.MoveToInventory(SlotToClear.objectInSlot);
                    cn.AddItemToContainer(moved);
                    moved.transform.parent = GameWorldController.instance.InventoryMarker.transform;                    
                    UWCharacter.Instance.GetComponent<PlayerInventory>().Refresh();
                }
                else
                {
                    //Move to the players positon where they can pick it up.
                    ObjectInteraction.SetPosition(SlotToClear.objectInSlot, UWCharacter.Instance.transform.position);
                 }
            }
            SlotToClear.clear();//clear any graphics still there.
        }

        ///Return any items in the trade area to their npc owner.
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            UWHUD.instance.npcTrade[i].clear();
        }


        ///Puts the time scales back to normal
        Time.timeScale = 1.0f;
        yield return StartCoroutine(WaitForMore());
        yield return new WaitForSeconds(0.5f);
        InConversation = false;
        //npc.npc_talkedto=1;
        UWHUD.instance.Conversation_tl.Clear();
        UWHUD.instance.MessageScroll.Clear();

        Character.InteractionMode = Character.InteractionModeTalk;
        if (MusicController.instance != null)
        {
            MusicController.instance.InMap = false;
        }
        if (CurrentObjectInHand != null)
        {
            Character.InteractionMode = Character.InteractionModePickup;
        }
        StopAllCoroutines();

        ///Resets the UI
        UWHUD.instance.RefreshPanels(UWHUD.HUD_MODE_INVENTORY);

        //Process scheduled events
        if ((_RES == GAME_UW2) && (EditorMode == false))
        {

            if (GameWorldController.instance.events != null)
            {
                GameWorldController.instance.events.ProcessEvents();
            }
        }
        if (SettingUpFight == false)
        {
            Quest.FightingInArena = false;//Clear the arena flag
        }
        if (Teleport)
        {
            if (TeleportLevel == GameWorldController.instance.dungeon_level)
            {//stay on this level
                float targetX = TeleportTileX * 1.2f + 0.6f;
                float targetY = TeleportTileY * 1.2f + 0.6f;
                float Height = CurrentTileMap().GetFloorHeight(TeleportTileX, TeleportTileY) * 0.15f;
                UWCharacter.Instance.transform.position = new Vector3(targetX, Height + 0.1f, targetY);
                UWCharacter.Instance.TeleportPosition = UWCharacter.Instance.transform.position;
            }
            else
            {
                UWCharacter.Instance.JustTeleported = true;
                UWCharacter.Instance.teleportedTimer = 0f;
                UWCharacter.Instance.playerMotor.movement.velocity = Vector3.zero;
                GameWorldController.instance.SwitchLevel((short)TeleportLevel, (short)TeleportTileX, (short)TeleportTileY);
            }
        }
    }


    void ImportVariableMemory(NPC npc)
    {
        //Copy the stored values from glob first
        for (int c = 0; c <= GameWorldController.instance.bGlobals.GetUpperBound(0); c++)
        {
            if (npc.npc_whoami == GameWorldController.instance.bGlobals[c].ConversationNo)
            {
                //cnv.privateVariables = new int[GameWorldController.instance.bGlobals[c].Globals.GetUpperBound(0)+1];
                for (int x = 0; x <= GameWorldController.instance.bGlobals[c].Globals.GetUpperBound(0); x++)
                {
                    //Copy Private variables
                    //cnv.privateVariables[x]	= GameWorldController.instance.bGlobals[c].Globals[x];	
                    stack.Set(x, GameWorldController.instance.bGlobals[c].Globals[x]);
                }
                break;
            }
        }


        //Add in the imported variables.
        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
        {
            if (conv[currConv].functions[i].import_type == import_variable)
            {
                int address = conv[currConv].functions[i].ID_or_Address;
                switch (conv[currConv].functions[i].functionName.ToLower())
                {
                    case "game_mins":
                        stack.Set(address, GameClock.Minute); break;
                    case "game_days":
                        stack.Set(address, GameClock.Day); break;
                    case "game_time"://What should be set here? Seconds?
                         stack.Set(address, GameClock.Second); break;
                    case "riddlecounter":
                        stack.Set(address, 0); break;
                    case "dungeon_level":
                        stack.Set(address, GameWorldController.instance.dungeon_level + 1); break;
                    case "npc_name":
                        stack.Set(address, StringController.instance.AddString(conv[currConv].StringBlock, StringController.instance.GetString(7, npc.npc_whoami + 16))); break;
                    case "npc_level":
                        stack.Set(address, npc.npc_level); break;
                    case "npc_talkedto":
                        NPCTalkedToIndex = address;
                        stack.Set(address, npc.npc_talkedto); break;
                    case "npc_gtarg":
                        stack.Set(address, npc.npc_gtarg); break;
                    case "npc_attitude":
                        stack.Set(address, npc.npc_attitude); break;
                    case "npc_goal":
                        stack.Set(address, npc.npc_goal); break;
                    case "npc_power":
                        stack.Set(address, npc.npc_power); break;
                    case "npc_arms":
                        stack.Set(address, npc.npc_arms); break;
                    case "npc_hp":
                        stack.Set(address, npc.npc_hp); break;
                    case "npc_health":
                        stack.Set(address, npc.npc_health); break;
                    case "npc_hunger":
                        stack.Set(address, npc.npc_hunger); break;
                    case "npc_whoami":
                        stack.Set(address, npc.npc_whoami); break;
                    case "npc_yhome":
                        stack.Set(address, npc.npc_yhome); break;
                    case "npc_xhome":
                        stack.Set(address, npc.npc_xhome); break;
                    case "play_sex":
                        {
                            if (UWCharacter.Instance.isFemale)
                            {
                                stack.Set(address, 1);
                            }
                            else
                            {
                                stack.Set(address, 0);
                            }
                            break;
                        }

                    //case "play_drawn":
                    case "play_poison":
                        stack.Set(address, UWCharacter.Instance.play_poison); break;
                    case "play_name":
                        stack.Set(address, StringController.instance.AddString(conv[currConv].StringBlock, UWCharacter.Instance.CharName)); break;
                    //case "new_player_exp":
                    case "play_level":
                        stack.Set(address, UWCharacter.Instance.CharLevel); break;
                    case "play_mana":
                        stack.Set(address, UWCharacter.Instance.PlayerMagic.CurMana); break;
                    case "play_hp":
                        stack.Set(address, UWCharacter.Instance.CurVIT); break;
                    //case "play_power":

                    //case "play_arms":
                    //	case "play_health":
                    case "play_hunger":
                        stack.Set(address, UWCharacter.Instance.FoodLevel); break;
                    default:
                        //Debug.Log("unimplemented memory import " + conv[currConv].functions[i].functionName);
                        break;

                }
            }
        }
    }


    IEnumerator say_op(int arg1)
    {
        yield return StartCoroutine(say_op(arg1, NPC_SAY));
        yield return 0;
    }


    IEnumerator say_op(int arg1, int PrintType)
    {
        yield return StartCoroutine(say_op(StringController.instance.GetString(conv[currConv].StringBlock, arg1), PrintType));
        yield return 0;
    }


    IEnumerator say_op(string text, int PrintType)
    {
        //Debug.Log("Saying :" + text);
        yield return new WaitForSecondsRealtime(0.2f);
        if (text.Trim() == "")
        {
            yield return 0;
        }
        if (text.Contains("@"))
        {
            text = TextSubstitute(text);
        }
        text=text.Replace('\0'.ToString(), "");
        string[] Lines = text.Split(new string[] { "\n" }, System.StringSplitOptions.None);

        for (int s = 0; s <= Lines.GetUpperBound(0); s++)
        {//Lines
            if (Lines[s].Trim() != "")
            {
                string[] Paragraphs = Lines[s].Split(new string[] { "\\m" }, System.StringSplitOptions.None);

                for (int i = 0; i <= Paragraphs.GetUpperBound(0); i++)
                {
                    string Markup;
                    switch (PrintType)
                    {
                        case PC_SAY:
                            Markup = "<color=red>"; break;//[FF0000]
                        case PRINT_SAY:
                            Markup = "<color=purple>"; break;//[000000]
                        default:
                            Markup = "<color=black>"; break;//[00FF00]
                    }
                    UWHUD.instance.Conversation_tl.Add(Paragraphs[i], Markup); //\n	
                    if (i < Paragraphs.GetUpperBound(0))
                    {
                        //UWHUD.instance.Conversation_tl.Add("<color=white>MORE</color>");
                        UWHUD.instance.Conversation_tl.Add("MORE", "<color=white>");
                        yield return StartCoroutine(WaitForMore());
                    }
                }
            }
        }


        yield return new WaitForSecondsRealtime(0.2f);

        yield return 0;
    }


    string TextSubstitute(string input)
    {
        //X: source of variable to substitute, one of these: GSP
        //G: game global variable
        //S: stack variable
        //P: pointer variable
        //Y: type of variable, one of these: SI
        //S: value is a string number into current string block
        //I: value is an integer value
        //<num>: decimal value
        //<extension>: format: C<number>: use array index <number>

        string RegExForFindingReplacements = "([@][GSP][SI])([0-9]*)([S][I])?([0-9]*)?([C][0-9]*)?";
        //"(@)([GSP])([SI])([0-9])*([S][I][0-9]*)*([C][0-9])*";
        //string RegExForFindingReplacementsTypes = "(@)([GSP])([SI])";

        MatchCollection matches = Regex.Matches(input, RegExForFindingReplacements);
        for (int sm = 0; sm < matches.Count; sm++)
        {
            string ReplacementString = matches[sm].Value;
            if (matches[sm].Success)
            {
                string ReplacementType = "";
                int ReplacementValue = 0;
                int OffsetValue = 0;
                string FoundString = "";

                for (int sg = 0; sg < matches[sm].Groups.Count; sg++)
                {
                    if (matches[sm].Groups[sg].Success)
                    {
                        switch (sg)
                        {
                            case 1: //Replacement Type
                                ReplacementType = matches[sm].Groups[sg].Value; break;
                            case 2: //Replacement value
                                {
                                    if (int.TryParse(matches[sm].Groups[sg].Value, out int val))
                                    {
                                        ReplacementValue = val;
                                    }
                                    else
                                    {
                                        ReplacementValue = 0;
                                    }
                                    break;
                                }
                            case 3: //Offset Type (should only be SI?)
                                string OffsetType = matches[sm].Groups[sg].Value;
                                break;
                            case 4: //Offset value
                                {
                                    if (int.TryParse(matches[sm].Groups[sg].Value, out int val))
                                    {
                                        OffsetValue = val;
                                    }
                                    else
                                    {
                                        OffsetValue = 0;
                                    }
                                    break;
                                }
                            case 5: //formatting specifier (unimplemented
                                string formatting = matches[sm].Groups[sg].Value;
                                break;
                        }
                    }

                    //Debug.Log("group " + matches[sm].Groups[sg].Success + " " + matches[sm].Groups[sg].Value);
                }



                //Now replace
                switch (ReplacementType)
                {
                    case "@GS": //Global string.
                        {
                            FoundString = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(ReplacementValue));
                            break;
                        }
                    case "@GI": //Global integer
                        {
                            Debug.Log("@GI String replacement (" + ReplacementValue + ")");//Sometimes this works with val+1 other times val!!
                            FoundString = stack.at(ReplacementValue).ToString();
                            break;
                        }
                    case "@SS": //Stack string
                        {
                            if (OffsetValue != 0)
                            {
                                int actualptr = stack.at(stack.basep + OffsetValue);
                                FoundString = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(stack.basep + actualptr));
                            }
                            else
                            {
                                FoundString = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(stack.basep + ReplacementValue));
                            }

                            break;
                        }
                    case "@SI": //Stack integer
                        {//TODO: this +1 behaves inconsistently. UW1 or UW2 difference???
                            if (_RES == GAME_UW2)
                            {
                                FoundString = stack.at(stack.basep + ReplacementValue).ToString();
                            }
                            else
                            {
                                FoundString = stack.at(stack.basep + ReplacementValue + 1).ToString();//Skip over 1 for basepointer	
                            }

                            break;
                        }

                    case "@PS": //Pointer string
                        {
                            FoundString = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(stack.at(stack.basep + ReplacementValue)));
                            break;
                        }
                    case "@PI": //Pointer integer
                        {
                            if (ReplacementValue < 0)
                            {
                                FoundString = stack.at(stack.at(stack.basep + ReplacementValue - 1)).ToString();//-1 for params
                            }
                            else
                            {
                                FoundString = stack.at(stack.at(stack.basep + ReplacementValue)).ToString();
                            }
                            break;
                        }
                }
                if (FoundString != "")
                {
                    input = input.Replace(ReplacementString, FoundString);
                }
            }
        }


        return input;
    }

    /*	string TextSubstitute(string input)
		{
				//X: source of variable to substitute, one of these: GSP
				//G: game global variable
				//S: stack variable
				//P: pointer variable
				//Y: type of variable, one of these: SI
				//S: value is a string number into current string block
				//I: value is an integer value
				//<num>: decimal value
				//<extension>: format: C<number>: use array index <number>
				string regexForStringReplacement="@[GSP][SI]([-+]?[0-9]*\\.?[0-9]+)";
				string regexForStringReplacementType="@[GSP][SI]";
				string regexForStringReplacementValue="([-+]?[0-9]*\\.?[0-9]+)";
				MatchCollection matches=Regex.Matches(input, regexForStringReplacement);
				for (int s = 0; s<matches.Count; s++)
				{
						if (matches[s].Success)
						{
								string ReplacementString=matches[s].Groups[0].Value;
								Match ReplacementValue =  Regex.Match(ReplacementString, regexForStringReplacementValue);
								if (ReplacementValue.Success)
								{
										int value = int.Parse(ReplacementValue.Groups[0].Value);
										//Try and find what type of replacement this is
										Match ReplacementType = Regex.Match(ReplacementString, regexForStringReplacementType);
										string FoundString="";
										if (ReplacementType.Success)
										{
												switch (ReplacementType.Groups[0].Value)
												{
												case "@GS": //Global string.
														{
																FoundString= StringController.instance.GetString(conv[currConv].StringBlock,stack.at(value));
																break;
														}
												case "@GI": //Global integer
														{
																Debug.Log("@GI String replacement (" + value + ")");//Sometimes this works with val+1 other times val!!
																//Shak works with val + 1
																/*if (value < conv[currConv].NoOfMemorySlots - conv[currConv].NoOfImportedGlobals)
																{
																		FoundString= stack.at(value+1).ToString();		
																}
																else
																{*/
    /*	here																		FoundString= stack.at(value).ToString();
                                                            //}

                                                        //FoundString= stack.at(value).ToString();
                                                            break;	
                                                    }
                                            case "@SS": //Stack string
                                                    {//TODO: Miranda's conversation in UW2 where she tells you about the lines of power that are cut is of format @SS1SI10
                                                            FoundString= StringController.instance.GetString(conv[currConv].StringBlock,stack.at(stack.basep+value));
                                                            break;	
                                                    }
                                            case "@SI": //Stack integer
                                                    {//TODO: this +1 behaves inconsistently. UW1 or UW2 difference???
                                                            if (_RES==GAME_UW2)
                                                            {
                                                                    FoundString= stack.at(stack.basep+value).ToString();	
                                                            }
                                                            else
                                                            {
                                                                    FoundString= stack.at(stack.basep+value+1).ToString();//Skip over 1 for basepointer	
                                                            }

                                                            break;	
                                                    }

                                            case "@PS": //Pointer string
                                                    {
                                                            FoundString= StringController.instance.GetString(conv[currConv].StringBlock,stack.at(stack.at(stack.basep+value)));
                                                            break;	
                                                    }
                                            case "@PI": //Pointer integer
                                                    {
                                                            if (value <0)
                                                            {
                                                                    FoundString= stack.at(stack.at(stack.basep+value-1)).ToString();//-1 for params
                                                            }
                                                            else
                                                            {
                                                                    FoundString= stack.at(stack.at(stack.basep+value)).ToString();	
                                                            }

                                                            break;	
                                                    }
                                            }
                                            if (FoundString!="")
                                            {
                                                    //Debug.Log("To Replace "+ ReplacementString + " Type=" + ReplacementType.Groups[0].Value + " value=" + value + " replacing with " + FoundString);
                                                    input=input.Replace(ReplacementString,FoundString);
                                            }	
                                    }	
                            }
                    }		
            }
            return input.Replace("C2","");
    }*/


    IEnumerator run_imported_function(ImportedFunctions func, NPC npc)
    {
        //if (func.functionName!="babl_menu")
        //{
        //	Debug.Log("Calling " + func.functionName + " at " + stack.instrp);		
        //}
        switch (func.functionName.ToLower())
        {
            case "babl_menu":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value

                    yield return StartCoroutine(babl_menu(args[0]));
                    break;
                }

            case "babl_fmenu":
                {///FMENU appears bugged. Causes stackptr issues further down the line (see conversation 88  and door opening)
                    //stack.Pop();
                    int start = stack.at(stack.stackptr - 2);//Not sure if this is correct for all conversations but lets try it anyway!
                    int flagstart = stack.at(stack.stackptr - 3);
                    yield return StartCoroutine(babl_fmenu(start, flagstart));
                    break;
                }

            case "babl_ask":
                {
                    yield return StartCoroutine(babl_ask());
                    break;
                }

            case "get_quest":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value


                    //int[] args = argsArray();
                    //stack.Pop();
                    //int index= stack.at(stack.Pop());
                    //int index= stack.at( stack.at( stack.stackptr-2 ) );
                    stack.result_register = get_quest(stack.at(args[0]));
                    break;
                }

            case "set_quest":
                {
                    //int[] args = argsArray();
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value

                    //int val = stack.Pop();
                    //int index= stack.at(stack.Pop()); //stack.at( stack.at( stack.stackptr-5 ) );
                    //stack.at( stack.at( stack.stackptr-4 ) ) ;
                    set_quest(stack.at(args[0]), stack.at(args[1]));//Or the other way around.
                    break;
                }

            case "print":
                {//TODO:review use of argsarray
                 //int[] args = argsArray();								
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    yield return StartCoroutine(say_op(stack.at(args[0]), PRINT_SAY));
                    break;
                }

            case "x_skills":
                {
                    //int val1 = stack.Pop();
                    //int val2 = stack.Pop();
                    //int val3 = stack.Pop();
                    //int val4 = stack.Pop();
                    int[] args = new int[4];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    args[3] = stack.at(stack.stackptr - 5);//ptr to value
                    stack.result_register = x_skills(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]), stack.at(args[3]));//Or the other way around.
                    break;
                }

            case "set_likes_dislikes":
                {
                    //stack.Pop();
                    //int index1= stack.Pop();
                    //int index2= stack.Pop();
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value

                    set_likes_dislikes(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "sex":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    stack.result_register = sex(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "random":
                {
                    //	int[] args = argsArray();
                    //stack.Pop();
                    //stack.Pop();
                    //int arg1=stack.Pop();
                    //stack.result_register=Random.Range(1,stack.at(arg1)+1);
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = Random.Range(1, stack.at(args[0]) + 1);
                    break;
                }

            case "show_inv":
                {
                    //stack.Pop();
                    int arg1 = stack.at(stack.stackptr - 2);
                    int arg2 = stack.at(stack.stackptr - 3);
                    stack.result_register = show_inv(arg1, arg2);

                    break;
                }

            case "give_to_npc":
                {
                    //stack.Pop();
                    //int arg1= stack.Pop();	
                    //int arg2= stack.Pop();
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    stack.result_register = give_to_npc(npc, args[0], stack.at(args[1]));
                    break;
                }

            case "take_from_npc":
                {
                    //stack.Pop();
                    //int arg1 = stack.Pop();

                    int arg = stack.at(stack.stackptr - 2);//ptr to value

                    stack.result_register = take_from_npc(npc, stack.at(arg));
                    break;
                }

            case "setup_to_barter":
                {
                    //stack.Pop();
                    setup_to_barter(npc);
                    break;
                }

            case "do_offer":
                {//TODO:review use of argsarray
                 //int noOfArgs=stack.Pop();
                 //int[] args = argsArray();

                    switch (stack.TopValue)//(args.GetUpperBound(0))
                    {
                        case 7:
                            {
                                int[] args = new int[7];
                                args[0] = stack.at(stack.stackptr - 2);//ptr to value
                                args[1] = stack.at(stack.stackptr - 3);//ptr to value
                                args[2] = stack.at(stack.stackptr - 4);//ptr to value
                                args[3] = stack.at(stack.stackptr - 5);//ptr to value
                                args[4] = stack.at(stack.stackptr - 6);//ptr to value
                                args[5] = stack.at(stack.stackptr - 7);//ptr to value
                                args[6] = stack.at(stack.stackptr - 8);//ptr to value

                                yield return StartCoroutine(do_offer(npc, stack.at(args[0]), stack.at(args[1]), stack.at(args[2]), stack.at(args[3]), stack.at(args[4]), stack.at(args[5]), stack.at(args[6])));
                                break;
                            }
                        case 5:
                            {
                                int[] args = new int[5];
                                args[0] = stack.at(stack.stackptr - 2);//ptr to value
                                args[1] = stack.at(stack.stackptr - 3);//ptr to value
                                args[2] = stack.at(stack.stackptr - 4);//ptr to value
                                args[3] = stack.at(stack.stackptr - 5);//ptr to value
                                args[4] = stack.at(stack.stackptr - 5);//ptr to value
                                yield return StartCoroutine(do_offer(npc, stack.at(args[0]), stack.at(args[1]), stack.at(args[2]), stack.at(args[3]), stack.at(args[4]), -1, -1));
                                break;
                            }


                        default:
                            {
                                Debug.Log("uniplemented version of do_offer " + stack.TopValue);
                                break;
                            }
                    }
                    break;
                }

            case "do_demand":
                {//TODO:review use of argsarray
                 //int[] args = argsArray();
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    yield return StartCoroutine(do_demand(npc, stack.at(args[0]), stack.at(args[1])));
                    break;
                }

            case "do_judgement":
                {
                    //stack.Pop();
                    yield return StartCoroutine(do_judgement(npc));
                    break;
                }

            case "do_decline":
                {
                    do_decline(npc);
                    break;
                }

            case "gronk_door":
                {//TODO:review use of argsarray
                 //int[] args = argsArray();
                 //stack.Pop();//no of args
                    int[] args = new int[3];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    stack.result_register = gronk_door(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]));
                    break;
                }


            case "x_obj_stuff":
                {//This is totally wrong!
                 //x_obj_stuff( 9,locals[9], locals[8], locals[7], locals[2], locals[6], locals[5], locals[4], locals[3], param2 );

                    //int[] args = argsArrayPtr();
                    //	int[] args=new int[4];
                    //stack.Pop();//no of args


                    int[] args = new int[9];
                    //stack.Pop();//no of args
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    args[3] = stack.at(stack.stackptr - 5);//ptr to value
                    args[4] = stack.at(stack.stackptr - 6);//ptr to value
                    args[5] = stack.at(stack.stackptr - 7);//ptr to value
                    args[6] = stack.at(stack.stackptr - 8);//ptr to value
                    args[7] = stack.at(stack.stackptr - 9);//ptr to value
                    args[8] = stack.at(stack.stackptr - 10);//ptr to value
                    x_obj_stuff(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);

                    break;
                }

            case "find_inv":
                {//TODO:review use of argsarray
                 //int[] args=argsArray();
                    int[] args = new int[2];
                    //stack.Pop();//no of args
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    stack.result_register = find_inv(npc, stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "identify_inv":
                {

                    //Pop No of args.
                    //stack.Pop();
                    //pop that array ptr
                    // identify_inv( 4,locals,  17, 15, 16, locals[6+i] );
                    int[] args = new int[4];
                    //stack.Pop();//no of args
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    args[3] = stack.at(stack.stackptr - 5);//ptr to value
                                                           //int[] args=argsArray();
                    stack.result_register = identify_inv(args[0], args[1], args[2], args[3]);
                    break;
                }

            case "contains":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    stack.result_register = contains(args[0], args[1]);
                    break;
                }

            case "set_race_attitude":
                {
                    //TODO:review use of argsarray
                    //int[] args=argsArray();
                    int[] args = new int[3];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    set_race_attitude(npc, stack.at(args[0]), stack.at(args[1]), stack.at(args[2]));
                    break;
                }

            case "set_attitude":
                {//TODO:review use of argsarray
                 //int[] args=argsArray();
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    set_attitude(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "compare":
                {//TODO:review use of argsarray
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    stack.result_register = compare(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "count_inv":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = count_inv(stack.at(args[0]));
                    break;
                }

            case "remove_talker":
                {
                    remove_talker(npc);
                    break;
                }
            case "give_ptr_npc":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    give_ptr_npc(npc, (short)stack.at(args[0]), args[1]);
                    break;
                }
            case "take_from_npc_inv":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = take_from_npc_inv(npc, stack.at(args[0]));
                    break;
                }

            case "take_id_from_npc":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = take_id_from_npc(npc, stack.at(args[0]));
                    break;
                }

            case "find_barter":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = find_barter(stack.at(args[0]));
                    break;
                }

            case "length":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value	
                    stack.result_register = length(stack.at(args[0]));
                    break;
                }


            case "find_barter_total":
                {
                    int[] args = new int[4];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    args[3] = stack.at(stack.stackptr - 5);//ptr to value
                    stack.result_register = find_barter_total(args[0], args[1], args[2], stack.at(args[3]));
                    break;
                }

            case "do_inv_create":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = do_inv_create(npc, stack.at(args[0]));
                    break;
                }

            case "place_object":
                {
                    int[] args = new int[3];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value	
                    place_object(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]));
                    break;
                }

            case "do_inv_delete":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    do_inv_delete(npc, stack.at(args[0]));
                    break;
                }

            case "x_traps":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value	
                    if (stack.at(args[0]) == 10001)
                    {
                        stack.result_register = x_traps(stack.at(args[0]), stack.at(args[1]));
                    }
                    else
                    {
                        x_traps(stack.at(args[0]), stack.at(args[1]));
                    }
                    break;
                }

            case "switch_pic":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    switch_pic(stack.at(args[0]));
                    break;
                }

            case "x_clock":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value	
                    x_clock(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "x_exp":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    x_exp(stack.at(args[0]));
                    break;
                }

            case "check_inv_quality":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    stack.result_register = check_inv_quality(stack.at(args[0]));
                    break;
                }

            case "set_inv_quality":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    set_inv_quality(stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "x_obj_pos":
                {
                    int[] args = new int[5];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value
                    args[3] = stack.at(stack.stackptr - 5);//ptr to value		
                    args[4] = stack.at(stack.stackptr - 5);//ptr to value	
                    x_obj_pos(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]), stack.at(args[3]), stack.at(args[4]));
                    break;
                }

            case "teleport_talker":
                {
                    int[] args = new int[2];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    teleport_talker(npc, stack.at(args[0]), stack.at(args[1]));
                    break;
                }

            case "babl_hack":
                {
                    int typeofbablhack = stack.at(stack.at(stack.stackptr - 2));
                    switch (typeofbablhack)//babl_hack mode
                    {
                        case 0://Challenge a fighter
                            {
                                SettingUpFight = true;
                                //Quest.ArenaOpponents[0] = npc.objInt().BaseObjectData.index;
                                Quest.SetArenaOpponent(0, npc.ObjectIndex);
                                Quest.FightingInArena = true;
                                break;
                            }
                        case 1://Are you in a fight
                            {
                                if (Quest.FightingInArena)
                                {
                                    stack.result_register = 1;
                                }
                                else
                                {
                                    stack.result_register = 0;
                                }
                                break;
                            }
                        case 2://Set up arena fight.
                            {
                                int[] args = new int[4];
                                args[0] = stack.at(stack.stackptr - 2);//ptr to value
                                args[1] = stack.at(stack.stackptr - 3);//ptr to value
                                args[2] = stack.at(stack.stackptr - 4);//ptr to value
                                args[3] = stack.at(stack.stackptr - 5);//ptr to value
                                babl_hackSetUpFight(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]), stack.at(args[3]));
                                break;
                            }
                        case 3://Jospur arena debt
                            {
                                int[] args = new int[1];
                                args[0] = stack.at(stack.stackptr - 2);//ptr to value
                                babl_hackJospurDebt(stack.at(args[0]));
                                break;
                            }

                        default:
                            {
                                Debug.Log("Unimplemented babl hack " + typeofbablhack);
                                break;
                            }
                    }
                    break;
                }

            case "teleport_player":
                {
                    int[] args = new int[3];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    args[1] = stack.at(stack.stackptr - 3);//ptr to value
                    args[2] = stack.at(stack.stackptr - 4);//ptr to value	
                    teleport_player(stack.at(args[0]), stack.at(args[1]), stack.at(args[2]));
                    break;
                }
            case "pause":
                {
                    int[] args = new int[1];
                    args[0] = stack.at(stack.stackptr - 2);//ptr to value
                    yield return StartCoroutine(pause(stack.at(args[0])));
                    break;
                }
            default:
                {
                    Debug.Log("Conversation : " + npc.npc_whoami + "unimplemented function " + func.functionName + " instr at " + stack.instrp);
                    break;
                }
        }
        yield return 0;
    }

    /// <summary>
    /// Waits for a number of seconds
    /// </summary>
    /// <param name="waittime"></param>
    /// <returns></returns>
    public IEnumerator pause(int waittime)
    {
        yield return new WaitForSecondsRealtime(waittime);
    }


    public IEnumerator babl_menu(int Start)
    {
        UWHUD.instance.MessageScroll.Clear();
        yield return new WaitForSecondsRealtime(0.2f);
        usingBablF = false;
        MaxAnswer = 0;
        int j = 1;
        ClearConversationOptions();
        for (int i = Start; i <= stack.Upperbound(); i++)
        {
            if (stack.at(i) > 0)
            {
                string TextLine = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(i));
                if (TextLine.Contains("@"))
                {
                    TextLine = TextSubstitute(TextLine);
                }

                UWHUD.instance.ConversationOptions[j - 1].SetText(j + "." + TextLine + "");
                UWHUD.instance.EnableDisableControl(UWHUD.instance.ConversationOptions[j - 1], true);
                j++;
                MaxAnswer++;
            }
            else
            {
                break;
            }
        }
        yield return StartCoroutine(WaitForInput());
        int AnswerIndex = stack.at(Start + PlayerAnswer - 1);
        yield return StartCoroutine(say_op(AnswerIndex, PC_SAY));
        stack.result_register = PlayerAnswer;
        yield return 0;
    }

    /// <summary>
    /// Removes and disables conversation option text
    /// </summary>
    private static void ClearConversationOptions()
    {
        for (int i = 0; i <= UWHUD.instance.ConversationOptions.GetUpperBound(0); i++)
        {
            UWHUD.instance.ConversationOptions[i].SetText("");
            UWHUD.instance.EnableDisableControl(UWHUD.instance.ConversationOptions[i], false);
        }
    }


    /// <summary>
    /// Dialog menu with choices that may or may not show based on the flags
    /// </summary>
    /// <param name="unknown">Unknown.</param>
    /// <param name="localsArray">Array of local variables from the conversation</param>
    /// <param name="Start">Index to start taking values from the array</param>
    /// <param name="flagIndex">Index to start flagging if a value is allowed from the array</param>
    public IEnumerator babl_fmenu(int Start, int flagIndex)
    {
        ClearConversationOptions();
        UWHUD.instance.MessageScroll.Clear();
        yield return new WaitForSecondsRealtime(0.2f);
        usingBablF = true;
        for (int i = 0; i <= bablf_array.GetUpperBound(0); i++)
        {//Reset the answers array
            bablf_array[i] = 0;
        }

        int j = 1;
        MaxAnswer = 0;
        for (int i = Start; i <= stack.Upperbound(); i++)
        {
            if (stack.at(i) != 0)
            {
                if (stack.at(flagIndex++) != 0)
                {
                    string TextLine = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(i));
                    if (TextLine.Contains("@"))
                    {
                        TextLine = TextSubstitute(TextLine);
                    }


                    bablf_array[j - 1] = stack.at(i);
                    //tmp = tmp + j++ + "." + StringController.instance.GetString(StringBlock,localsArray[i]) + "\n";
                    // UWHUD.instance.MessageScroll.Add(j++ + "." + TextLine + "");
                    UWHUD.instance.ConversationOptions[j - 1].SetText(j + "." + TextLine + "");
                    UWHUD.instance.EnableDisableControl(UWHUD.instance.ConversationOptions[j - 1], true);
                    j++;
                    MaxAnswer++;
                }
            }
            else
            {
                break;
            }
        }
        yield return StartCoroutine(WaitForInput());

        yield return StartCoroutine(say_op(bablf_array[bablf_ans - 1], PC_SAY));

        stack.result_register = PlayerAnswer;
        yield return 0;
    }


    /// <summary>
    /// Sets up for typed input
    /// </summary>
    public IEnumerator babl_ask()
    {
        PlayerTypedAnswer = "";
        //tl_input.Set(">");
        //PlayerInput.text = ">";
        for (int j = 0; j <= UWHUD.instance.ConversationOptions.GetUpperBound(0); j++)
        {
            UWHUD.instance.ConversationOptions[j].SetText("");
        }
        UWHUD.instance.ConversationOptions[0].SetText(">");

        InputField inputctrl = UWHUD.instance.InputControl;
        inputctrl.gameObject.SetActive(true);
        inputctrl.gameObject.GetComponent<InputHandler>().target = this.gameObject;
        inputctrl.gameObject.GetComponent<InputHandler>().currentInputMode = InputHandler.InputConversationWords;
        inputctrl.contentType = InputField.ContentType.Standard;
        inputctrl.text = "";
        inputctrl.Select();
        yield return StartCoroutine(WaitForTypedInput());
        yield return StartCoroutine(say_op(PlayerTypedAnswer, PC_SAY));
        inputctrl.text = "";
        UWHUD.instance.MessageScroll.Clear();
        stack.result_register = StringController.instance.AddString(conv[currConv].StringBlock, PlayerTypedAnswer);
    }

    /// <summary>
    /// Responds to player typed input submission.
    /// </summary>
    /// <param name="PlayerTypedAnswerIN">The string the player has typed</param>
    public static void OnSubmitPickup(string PlayerTypedAnswerIN)
    {
        InputField inputctrl = UWHUD.instance.InputControl;
        WaitingForTyping = false;
        inputctrl.gameObject.SetActive(false);
        PlayerTypedAnswer = PlayerTypedAnswerIN;
    }



    /// <summary>
    /// Waits for input in babl_menu and bablf_menu
    /// </summary>
    IEnumerator WaitForInput()
    {
        WaitingForInput = true;
        while (WaitingForInput)
        { yield return null; }
    }

    /// <summary>
    /// Waits for typed input from the player when in babl_ask
    /// </summary>
    IEnumerator WaitForTypedInput()
    {
        WaitingForTyping = true;
        while (WaitingForTyping)
        {
            UWHUD.instance.ConversationOptions[0].SetText(">" + UWHUD.instance.InputControl.text);
            yield return null;
        }
    }

    /// <summary>
    /// Processes key presses from the player when waiting for input.
    /// </summary>
    void OnGUI()
    {
        if (EnteringQty == true)
        {
            return;
        }

        if (WaitingForInput)
        {
            if (CurrentObjectInHand != null) { return; }//no conversation options allowed while holding an item
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CheckAnswer(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CheckAnswer(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CheckAnswer(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                CheckAnswer(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                CheckAnswer(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                CheckAnswer(6);
            }
        }
        else if (WaitingForMore)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WaitingForMore = false;
            }
        }
    }




    /// <summary>
    /// Checks the answer the player has entered to see if it in within the bounds of the valid options.
    /// Sets the PlayerAnswer variable for checking within the conversations
    /// </summary>
    /// <param name="AnswerNo">The answer number from the menu entered by the player</param>
    public void CheckAnswer(int AnswerNo)
    {
        if (usingBablF == false)
        {
            if ((AnswerNo > 0) && (AnswerNo <= MaxAnswer))
            {
                PlayerAnswer = AnswerNo;
                WaitingForInput = false;
            }
        }
        else
        {
            if ((AnswerNo > 0) && (AnswerNo <= MaxAnswer))
            {
                ///For babl_fmenus convert the answer using the bablf_array
                bablf_ans = AnswerNo;
                PlayerAnswer = bablf_array[AnswerNo - 1];
                WaitingForInput = false;
                usingBablF = false;
            }
        }
    }


    /// <summary>
    /// Gets the quest variable.
    /// </summary>
    /// <returns>The quest.</returns>
    /// <param name="unk">Unk.</param>
    /// <param name="QuestNo">Quest no to lookup</param>
    public int get_quest(int QuestNo)
    {

        return (Quest.GetQuestVariable(QuestNo));
        //if (_RES == GAME_UW2)
        //{
        //    Debug.Log("Checking Quest no " + QuestNo + " it's value is " + Quest.GetQuestVariable(QuestNo));
        //}
        //if (QuestNo > Quest.QuestVariablesOBSOLETE.GetUpperBound(0))
        //{
        //    Debug.Log("invalid quest no " + QuestNo);
        //    return 0;
        //}
        //return Quest.QuestVariablesOBSOLETE[QuestNo];
    }

    /// <summary>
    /// Sets the quest variable
    /// </summary>
    /// <param name="value">Value to change to</param>
    /// <param name="QuestNo">Quest no to change</param>
    public void set_quest(int value, int QuestNo)
    {
        //if (_RES == GAME_UW2)
        //{
        //    Debug.Log("Setting Quest no " + QuestNo + " to " + value);
        //}
        //if (QuestNo > Quest.QuestVariablesOBSOLETE.GetUpperBound(0))
        //{
        //    Debug.Log("Setting invalid quest no " + QuestNo);
        //    return;
        //}
        //Quest.QuestVariablesOBSOLETE[QuestNo] = value;
        Quest.SetQuestVariable(QuestNo, value);
    }

    /// <summary>
    /// Probably returns the skill value at the index.
    /// </summary>
    /// <returns>The skills.</returns>
    /// <param name="index">Index.</param>
    /// IN UW2 the return value seems to indicate if the skill gain occurred.
    public int x_skills(int mode, int skillToChange, int val3, int val4)
    {
        Debug.Log("X_skills (" + mode + "," + skillToChange + "," + val3 + "," + val4 + ")");
        if (_RES != GAME_UW2)
        {
            switch (mode)
            {
                case 10001:
                    Debug.Log("Returning skill " + Skills.GetSkillName(skillToChange + 1));
                    return Skills.GetSkill(skillToChange);
                case 10000:
                    Debug.Log("Possibly setting skill to " + Skills.GetSkillName(skillToChange + 1) + " " + 6);
                    Skills.AdvanceSkill(skillToChange, 6);
                    return Skills.GetSkill(skillToChange);
                default:
                    Debug.Log("Possibly setting skill to " + Skills.GetSkillName(skillToChange + 1) + " " + mode);
                    Skills.AdvanceSkill(skillToChange, mode);
                    return Skills.GetSkill(skillToChange);
            }
        }
        else
        {//In uw2 skill numbers are zero based?
            skillToChange++;
            switch (mode)
            {
                case 9999://Return the skill value
                    return Skills.GetSkill(skillToChange);
                case 10001: //Increase the skill if points are available. Returns 1 if sucess 0 if fail
                    if (UWCharacter.Instance.TrainingPoints > 0)
                    {
                        Skills.AdvanceSkill(skillToChange, 1);
                        UWCharacter.Instance.TrainingPoints--;
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                default://Set the skill to the specified value.
                    Skills.SetSkill(skillToChange, mode);
                    return Skills.GetSkill(skillToChange);
            }
        }
    }

    /// <summary>
    /// Sets the likes dislikes.
    /// </summary>
    /// <param name="index1">Index1.</param>
    /// <param name="index2">Index2.</param>
    public void set_likes_dislikes(int index1, int index2)
    {
        Debug.Log("set_likes_dislikes(" + index1 + "," + index2 + ")");
    }



    /// <summary>
    /// Copies inventory item ids and slot positions into the array indices specified.
    /// </summary>
    /// <returns>The number of items found</returns>
    /// <param name="startObjectPos">Start index of object position.</param>
    /// <param name="startObjectIDs">Start index of object Ids.</param>
    public int show_inv(int startObjectPos, int startObjectIDs)
    {
        int j = 0;
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            TradeSlot pcSlot = UWHUD.instance.playerTrade[i];
            if (pcSlot.isSelected())
            {
                stack.Set(startObjectPos + j, pcSlot.objectInSlot.ObjectIndex);  // FindObjectIndexInObjectList(pcSlot.objectInSlot.name));
                stack.Set(startObjectIDs + j, pcSlot.objectInSlot.item_id);
                j++;
            }
        }
        return j;
    }



    /// <summary>
    /// Gives the items at the specified positions in the array to the NPC
    /// </summary>
    /// <returns>1 if something given</returns>
    /// <param name="start">Start index of slots to transfer from</param>
    /// <param name="NoOfItems">No of items to transfer</param>
    public int give_to_npc(NPC npc, int start, int NoOfItems)
    {
        Container cn = npc.gameObject.GetComponent<Container>();
        bool SomethingGiven = false;
        for (int i = 0; i < NoOfItems; i++)
        {
            int slotNo = stack.at(start + i);

            ObjectInteraction demanded = UWHUD.instance.playerTrade[slotNo].objectInSlot;  // FindGameObjectInObjectList(slotNo);

            //Give the item to the npc
            if (Container.GetFreeSlot(cn) != -1)
            {
                if (demanded != null)
                {
                    ClearTradeSlotWithObject(slotNo);
                    cn.AddItemToContainer(demanded);
                    ObjectInteraction.SetPosition(demanded, GameWorldController.instance.InventoryMarker.transform.position,false);
                 }
                SomethingGiven = true;
            }
            else
            {
                if (demanded != null)
                {
                    ClearTradeSlotWithObject(slotNo);
                    ObjectInteraction.SetPosition(demanded, npc.NPC_Launcher.transform.position);
                    SomethingGiven = false;
                }
            }
        }

        if (SomethingGiven == true)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// transfers an item from npc inventory to player inventory,based on an item id. 
    /// </summary>
    /// when the value is > 1000, all items of		
    /// a category are copied. category item start = (arg1-1000)*16
    /// My currenty example is lanugo (10) who is passing a value greater than 1000*/
    /// Until It get more examples I'm doing the following*/
    /// Get the items in the npcs container. If their ITEM id is between the calculated category value and +16 of that I take that item*/
    /// Another example goldthirst who specifically has an item id to pass.
    /// 
    /// <returns>1: ok, 2: player has no space left</returns>
    /// <param name="unk">Unk.</param>
    /// <param name="arg1">Arg1.</param>
    public int take_from_npc(NPC npc, int arg1)
    {
        /*
   id=0015 name="take_from_npc" ret_type=int
   parameters:   arg1: item id (can also be an item category value, > 1000)
   description:  transfers an item from npc inventory to player inventory,
                 based on an item id. when the value is > 1000, all items of
                 a category are copied. category item start = (arg1-1000)*16
   return value: 1: ok, 2: player has no space left
    */
        /*My currenty example is lanugo (10) who is passing a value greater than 1000*/
        /*Until It get more examples I'm doing the following*/
        /*Get the items in the npcs container. If their ITEM id is between the calculated category value and +16 of that I take that item*/
        //Debug.Log ("Item Category is " + (arg1-1000)*16);
        //Another example goldthirst who specifically has an item id to pass.
        int playerHasSpace = 1;
        Container cn = npc.gameObject.GetComponent<Container>();
        Container PlayerContainer = UWCharacter.Instance.gameObject.GetComponent<Container>();

        if (arg1 < 1000)
        {//I'm taking a specific item.
            for (short i = 0; i <= cn.MaxCapacity(); i++)
            {
                if (cn.GetItemAt(i) != null)
                {
                    ObjectInteraction objInt = cn.GetItemAt(i);//cn.GetGameObjectAt(i).GetComponent<ObjectInteraction>(); //GameObject.Find (itemName).GetComponent<ObjectInteraction>();
                                                               //lastObjectTraded=objInt;
                    if (objInt != null)
                    {
                        if (objInt.item_id == arg1)
                        {
                            playerHasSpace = TakeItemFromNPCCOntainer(npc, PlayerContainer, i);
                            return playerHasSpace;
                        }
                    }
                }
            }
        }
        else
        {
            int rangeS = (arg1 - 1000) * 16;
            int rangeE = rangeS + 16;

            for (short i = 0; i <= cn.MaxCapacity(); i++)
            {
                if (cn.GetItemAt(i) != null)
                {
                    ObjectInteraction objInt = cn.GetItemAt(i);// cn.GetGameObjectAt(i).GetComponent<ObjectInteraction>(); //GameObject.Find (itemName).GetComponent<ObjectInteraction>();
                                                               //lastObjectTraded=objInt;
                    if (
                            ((arg1 >= 1000) && (objInt.item_id >= rangeS) && (objInt.item_id <= rangeE))
                            ||
                            ((arg1 < 1000) && (objInt.item_id == arg1))
                    )
                    {
                        playerHasSpace = TakeItemFromNPCCOntainer(npc, PlayerContainer, i);
                    }
                }
            }
        }
        return playerHasSpace;
    }

    static int TakeItemFromNPCCOntainer(NPC npc, Container PlayerContainer, int index)
    {
        //Give to PC
        ObjectInteraction demanded = npc.GetComponent<Container>().GetItemAt((short)index);
        if (Container.GetFreeSlot(PlayerContainer) != -1)//Is there space in the container.
        {
            npc.GetComponent<Container>().RemoveItemFromContainer(demanded);
            UWCharacter.Instance.Pickup(demanded.GetComponent<ObjectInteraction>(), UWCharacter.Instance.playerInventory);
            return 1;
        }
        else
        {
            npc.GetComponent<Container>().RemoveItemFromContainer(demanded);
            ObjectInteraction.SetPosition(demanded, npc.NPC_Launcher.transform.position);
            if (demanded.GetComponent<Container>())
            {
                Container cn = demanded.GetComponent<Container>();
                for (short i = 0; i <= cn.MaxCapacity(); i++)
                {
                    ObjectInteraction containerItem = cn.GetItemAt(i);
                    if (containerItem != null)
                    {
                        npc.GetComponent<Container>().RemoveItemFromContainer(containerItem);
                    }
                }
            }
            CurrentObjectInHand = demanded;
            GameWorldController.MoveToInventory(demanded);
            return 2;
        }
    }


    /// <summary>
    /// Waits for the player to press any key when printing text
    /// </summary>
    IEnumerator WaitForMore()
    {
        WaitingForMore = true;
        while (WaitingForMore)
        { yield return null; }
    }





    /// <summary>
    /// Setups the barter windows
    /// </summary>
    /// TODO: Figure out where generic NPCs get their inventory for trading from???? Is there a loot list somewhere?
    public void setup_to_barter(NPC npc)
    {
        /*
   id=001f name="setup_to_barter" ret_type=void
   parameters:   none
   description:  starts bartering; shows npc items in npc bartering area
 */

        Container cn = npc.gameObject.GetComponent<Container>();
        int itemCount = 0;
        npc.SetupNPCInventory();
        //Debug.Log ("Setup to barter. Based on characters inventory at the moment.");
        for (short i = 0; i <= cn.MaxCapacity(); i++)
        {
            ObjectInteraction itemToTrade = cn.GetItemAt(i);
            if (itemToTrade != null)
            {
                if (itemCount <= TradeSlot.TradeSlotUBound)
                {//Just take the first four items
                    //ObjectInteraction itemToTrade = cn.GetGameObjectAt(i).GetComponent<ObjectInteraction>(); //GameObject.Find (cn.GetItemAt(i)).GetComponent<ObjectInteraction>();
                    TradeSlot ts = UWHUD.instance.npcTrade[itemCount++];//GameObject.Find ("Trade_NPC_Slot_" + itemCount++).GetComponent<TradeSlot>();
                    ts.objectInSlot = itemToTrade;//.gameObject.name;
                    ts.SlotImage.texture = itemToTrade.GetInventoryDisplay().texture;
                    int qty = itemToTrade.GetQty();
                    if (qty <= 1)
                    {
                        ts.Quantity.text = "";
                    }
                    else
                    {
                        ts.Quantity.text = qty.ToString();
                    }
                }
            }
        }

        return;
    }


    /// <summary>
    /// Does the appraisal of the worth of the items in the trade area
    /// TODO: figure out how this works. for the moment just base it out quantity of objects selected
    /// Current implemenation is based on qty of objects in the trade area.
    /// </summary>
    /// <returns>The judgement.</returns>
    /// <param name="unk">Unk.</param>

    public IEnumerator do_judgement(NPC npc)
    {
        /*
id=0021 name="do_judgement" ret_type=void
parameters:   none
description:  judges current trade (using the "appraise" skill) and prints result
 */
        //Debug.Log ("Do Judgment");

        int playerObjectCount = 0; int npcObjectCount = 0;
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            TradeSlot npcSlot = UWHUD.instance.npcTrade[i];//GameObject.Find ("Trade_NPC_Slot_" + i).GetComponent<TradeSlot>();
            TradeSlot pcSlot = UWHUD.instance.playerTrade[i];// GameObject.Find ("Trade_Player_Slot_" + i).GetComponent<TradeSlot>();
            if (npcSlot.isSelected()) { npcObjectCount++; }
            if (pcSlot.isSelected()) { playerObjectCount++; }
        }
        if (playerObjectCount < npcObjectCount)
        {
            yield return StartCoroutine(say_op("Player has the better deal", PRINT_SAY));
        }
        else if (playerObjectCount == npcObjectCount)
        {
            yield return StartCoroutine(say_op("It is an even deal", PRINT_SAY));
        }
        else
        {
            yield return StartCoroutine(say_op("NPC has the better deal", PRINT_SAY));
        }
        //return;
    }

    /// <summary>
    /// Declines the trade offer
    /// Moves items back into respective inventories
    /// </summary>
    /// <param name="unk">Unk.</param>
    public void do_decline(NPC npc)
    {
        /*
id=0022 name="do_decline" ret_type=void
parameters:   none
description:  declines trade offer (?)
*/

        Container cn = UWCharacter.Instance.playerInventory.currentContainer;
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            TradeSlot pcSlot = UWHUD.instance.playerTrade[i];
            if (pcSlot.objectInSlot != null)
            {//Move the object to the players container or to the ground
                if (Container.GetFreeSlot(cn) != -1)//Is there space in the container.
                {
                    var moved = GameWorldController.MoveToInventory(pcSlot.objectInSlot);
                    cn.AddItemToContainer(moved);                    
                    UWCharacter.Instance.GetComponent<PlayerInventory>().Refresh();
                }
                else
                {
                    var moved = GameWorldController.MoveToWorld(pcSlot.objectInSlot);//ok
                    ObjectInteraction.SetPosition(moved, UWCharacter.Instance.transform.position, true);
                }
                pcSlot.clear();
            }
        }

        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {//Clear out the trade slots.
            UWHUD.instance.npcTrade[i].clear();
        }
        return;
    }



    /// <summary>
    /// Checks the offer.
    /// checks if the deal is acceptable for the npc, based on the
    /// selected items in both bartering areas. the values in arg1
    /// to arg5 are probably values of the items that are
    /// acceptable for the npc.
    /// the function is sometimes called with 7 args, but arg6 and
    /// arg7 are always set to -1.
    /// </summary>
    /// <returns>1 if the deal is acceptable, 0 if not</returns>
    /// <param name="unk">Unk.</param>
    /// <param name="arg1">Arg1.</param>
    /// <param name="arg2">Arg2.</param>
    /// <param name="arg3">Arg3.</param>
    /// <param name="arg4">Arg4.</param>
    /// <param name="arg5">Arg5.</param>
    /// <param name="arg6">Arg6.</param>
    /// <param name="arg7">Arg7.</param>
    /// I think the values passed are actually the strings for how the npc reacts to the offer. The value judgment is done elsewhere
    /// In the prototype I randomise the decision. And then randomise a response based on that decision.
    /// Arg5 is the yes answer
    public IEnumerator do_offer(NPC npc, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7)
    {
        /*
id=0018 name="do_offer" ret_type=int
parameters:   arg1 ... arg5: unknown
         [arg6, arg7]: unknown
description:  checks if the deal is acceptable for the npc, based on the
         selected items in both bartering areas. the values in arg1
         to arg5 are probably values of the items that are
         acceptable for the npc.
         the function is sometimes called with 7 args, but arg6 and
         arg7 are always set to -1.
return value: 1 if the deal is acceptable, 0 if not*/


        //PlayerAnswer = 
        int NpcAnswer = Random.Range(0, 2);
        stack.result_register = NpcAnswer;
        if (NpcAnswer == 1)
        {
            yield return StartCoroutine(say_op(arg5));
            //for the moment move to the player's backpack or if no room there drop them on the ground
            for (int i = 0; i <= 3; i++)
            {
                TakeFromNPC(npc, i);//Takes from the NPC slot if selected
            }


            //Now give the item to the NPC.
            //cn =npc.gameObject.GetComponent<Container>();
            for (int i = 0; i <= 3; i++)
            {
                TakeFromPC(npc, i);
            }
        }
        else
        {
            //Debug.Log ("offer declined");
            switch (Random.Range(1, 5))
            {
                case 1:
                    yield return StartCoroutine(say_op(arg1)); break;
                case 2:
                    yield return StartCoroutine(say_op(arg2)); break;
                case 3:
                    yield return StartCoroutine(say_op(arg3)); break;
                case 4:
                    yield return StartCoroutine(say_op(arg4)); break;
            }
        }
    }

    /// <summary>
    /// Does the offer.
    /// </summary>
    public IEnumerator do_offer(NPC npc, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6)
    {
        yield return StartCoroutine(do_offer(npc, arg1, arg2, arg3, arg4, arg5, arg6, -1));
    }

    /// <summary>
    /// Player demands the items from the npc
    /// </summary>
    /// <returns>returns 1 when player persuaded the NPC, 0 else</returns>
    /// <param name="unk">Unk.</param>
    /// <param name="arg1">string id with text to print if NPC is not willing to give the item</param>
    /// <param name="arg2">string id with text if NPC gives the player the item</param>
    public IEnumerator do_demand(NPC npc, int arg1, int arg2)
    {
        /*
id=0019 name="do_demand" ret_type=int
parameters:   arg1: string id with text to print if NPC is not willing
               to give the item
         arg2: string id with text if NPC gives the player the item
description:  decides if the player can "persuade" the NPC to give away
         the items in barter area, e.g. using karma.
return value: returns 1 when player persuaded the NPC, 0 else
 */
        int DemandResult = Random.Range(0, 2);
        if (DemandResult == 1)
        {
            //Demand sucessfull
            for (int i = 0; i <= 3; i++)
            {
                TakeFromNPC(npc, i);//Takes from the NPC slot if selected
            }
            yield return StartCoroutine(say_op(arg2));
        }
        else
        {
            //Unsucessful
            yield return StartCoroutine(say_op(arg1));
        }

        //Move the players items back to his inventory;
        RestorePCsInventory(npc);
        stack.result_register = DemandResult;
    }








    /// <summary>
    /// Gives the selected items to the NPC
    /// </summary>
    /// <param name="SlotNo">Slot no.</param>
    /// Use only in bartering as this does not refer to the master object list!
    private void TakeFromNPC(NPC npc, int SlotNo)
    {
        Container cn = UWCharacter.Instance.playerInventory.currentContainer;
        TradeSlot npcSlot = UWHUD.instance.npcTrade[SlotNo];
        //GameObject.Find ("Trade_NPC_Slot_" + i).GetComponent<TradeSlot>();
        if (npcSlot.isSelected())
        {
            npc.GetComponent<Container>().RemoveItemFromContainer(npcSlot.objectInSlot);
            //Move the object to the container or to the ground
            if (Container.GetFreeSlot(cn) != -1)//Is there space in the container.
            {                
                var movedObj=GameWorldController.MoveToInventory(npcSlot.objectInSlot);
                cn.AddItemToContainer(movedObj);
                npcSlot.clear();
                UWCharacter.Instance.GetComponent<PlayerInventory>().Refresh();
            }
            else
            {                
                npc.GetComponent<Container>().RemoveItemFromContainer(npcSlot.objectInSlot);
                ObjectInteraction.SetPosition(npcSlot.objectInSlot, npc.NPC_Launcher.transform.position,true);
                npcSlot.clear();
            }
        }
        return;
    }
    /// <summary>
    /// Takes from PCs selected items and gives them to the NPC.
    /// </summary>
    /// <param name="slotNo">Slot no.</param>
    void TakeFromPC(NPC npc, int slotNo)
    {
        Container cn = npc.GetComponent<Container>();
        TradeSlot TradeSlotToUse = UWHUD.instance.playerTrade[slotNo];
        if (TradeSlotToUse.isSelected())
        {
            //Move the object to the container or to the ground
            if (Container.GetFreeSlot(cn) != -1)//Is there space in the container.
            {
                cn.AddItemToContainer(TradeSlotToUse.objectInSlot);
                ObjectInteraction.SetPosition(TradeSlotToUse.objectInSlot, GameWorldController.instance.InventoryMarker.transform.position);
                //TradeSlotToUse.objectInSlot.transform.position = new Vector3(119f, 2.1f, 119f);
                TradeSlotToUse.clear();
            }
            else
            {
                ObjectInteraction.SetPosition(TradeSlotToUse.objectInSlot, npc.NPC_Launcher.transform.position, true);
                TradeSlotToUse.clear();
            }
        }
    }

    /// <summary>
    /// Restores the Pcs inventory out of the trade area and back into the current container.
    /// </summary>
    void RestorePCsInventory(NPC npc)
    {
        Container cn = UWCharacter.Instance.playerInventory.currentContainer;
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            TradeSlot pcSlot = UWHUD.instance.playerTrade[i];
            if (pcSlot.objectInSlot != null)
            {
                //Move the object to the players container or to the ground
                if (Container.GetFreeSlot(cn) != -1)//Is there space in the container.
                {
                    var moved = GameWorldController.MoveToInventory(pcSlot.objectInSlot);
                    cn.AddItemToContainer(moved);
                    pcSlot.clear();
                    UWCharacter.Instance.GetComponent<PlayerInventory>().Refresh();
                }
                else
                {
                    pcSlot.clear();
                    //demanded.transform.parent = GameWorldController.instance.DynamicObjectMarker ();
                    ObjectInteraction.SetPosition(pcSlot.objectInSlot, npc.NPC_Launcher.transform.position);        
                }
            }
        }
    }


    /// <summary>
    /// Gronks the door open
    /// </summary>
    /// <returns> appears to have something to do with if the door is broken or not.</returns>
    /// <param name="Action">close/open flag (0 means open)</param>
    /// <param name="tileY">y tile coordinate with door to open</param>
    /// <param name="tileX">x tile coordinate</param>
    public int gronk_door(int Action, int tileY, int tileX)
    {
        /*
id=0025 name="gronk_door" ret_type=int
parameters:   arg1: x tile coordinate with door to open
arg2: y tile coordinate
arg3: close/open flag (0 means open)
description:  opens/closes door or portcullis
return value: unknown
return value appears to have something to do with if the door is broken or not.
*/

        ///Finds the door than needs to be opened
        ///Based on the action used the door control functions as appropiate.
        GameObject dr = GameObject.Find("door_" + tileX.ToString("D3") + "_" + tileY.ToString("D3"));
        if (dr != null)
        {
            DoorControl DC = dr.GetComponent<DoorControl>();
            if (DC != null)
            {
                if (Action == 0)
                {
                    DC.UnlockDoor(false);//Possibly Npcs don't actually unlock the door
                    DC.OpenDoor(DoorControl.DefaultDoorTravelTime);
                }
                else
                {
                    DC.CloseDoor(DoorControl.DefaultDoorTravelTime);
                    DC.LockDoor();
                }
                if (DC.quality == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                Debug.Log(stack.instrp + " Unable to find doorcontrol to gronk " + " at " + tileX + " " + tileY);
                return 0;
            }

        }
        else
        {
            Debug.Log("Unable to find door to gronk " + " at " + tileX + " " + tileY);
            return 0;
        }

        //GameObject door = Var.findDoor(Var.triggerX,Var.triggerY);
    }



    public void x_obj_stuff(int arg1, int arg2, int arg3, int link, int arg5, int owner, int quality, int item_id, int pos)
    {
        pos = stack.at(pos);

        //Debug.Log("x_obj_stuff");		
        //id=002f name="x_obj_stuff" ret_type=void
        //	parameters:   arg1: not used in uw1
        //		arg2: not used in uw1
        //		arg3: 0, (upper bit of quality field?)
        //		arg4: quantity/special field, 115
        //		arg5: not used in uw1
        //		arg6: not used in uw1
        //		arg7: quality?
        //		arg8: identified flag? or is it owner or item_id
        //		arg9: position in inventory object list
        //		description:  sets object properties for object in inventory object list.
        //			if a property shouldn't be set, -1 is passed for the
        //         property value.
        //If -1 then it returns the value in the array?
        /*This may be implemented wrongly*/

        /*	if (pos<=7)//Item is in a trade slot  Assuming I'll never have to change an object at the top of the inventory list. Another bad hack.
{
pos -=TradeAreaOffset;//Take the offset off to get back to a trade slot.
pos--;
if (pos<0)
{
    return;
}
if (pos<=3)
{//Item is in players trade area.
    obj= UWHUD.instance.playerTrade[pos].GetGameObjectInteraction();

}
else if (pos <=7)
{//item in npc trade area
    obj= UWHUD.instance.npcTrade[pos-4].GetGameObjectInteraction();					
}	
}
else
{//Item is in the object masterlist
obj = CurrentObjectList().objInfo[pos].instance;
}	*/

        ObjectInteraction obj = ObjectLoader.getObjectIntAt(pos);// FindObjectInteractionInObjectList(pos);

        if (obj == null)
        {
            Debug.Log("Obj not found in x_obj_stuff. Trying the last traded object");
            //obj=lastObjectTraded;
            if (obj == null)
            {
                return;
            }
        }

        if (stack.at(link) <= 0)
        {
            //locals[link]=obj.link-512; 
            if (_RES == GAME_UW2)
            {//Looks like UW2 does not offset by 512 for testing links
                stack.Set(link, obj.link);
            }
            else
            {
                stack.Set(link, obj.link - 512);
            }

        }
        else
        {
             if (obj.isQuantityBln)
            {
                obj.link = (short)stack.at(link);
            }
            else
            {
                obj.link = (short)(stack.at(link) + 512);
            }

        }

        if (stack.at(owner) <= 0)
        {
            stack.Set(owner, obj.owner);
        }
        else
        {
            obj.owner = (short)stack.at(owner);
        }


        if (stack.at(quality) <= 0)
        {
            stack.Set(quality, obj.quality);
        }
        else
        {
            obj.quality = (short)stack.at(quality);
        }

        if (stack.at(item_id) <= 0)
        {
            stack.Set(item_id, obj.item_id);
        }
        //else
        //{
        //	obj.item_id=stack.at(item_id);
        //}

    }





    /// <summary>
    /// Finds item in PC or NPC inventory
    /// </summary>
    /// <returns>position in master object list, or 0 if not found</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="arg1">0: npc inventory; 1: player inventory</param>
    /// <param name="item_id">Item type ID of the object to find.</param>
    public int find_inv(NPC npc, int targetInventory, int item_id)
    {
        //id=0030 name="find_inv" ret_type=int
        //	parameters:   arg1: 0: npc inventory; 1: player inventory
        //	arg2: item id
        //		description:  searches item in npc or player inventory
        //		return value: position in master object list, or 0 if not found
        switch (targetInventory)
        {
            case 0://NPC inventory
                {
                    Container npcCont = npc.gameObject.GetComponent<Container>();

                    for (short i = 0; i <= npcCont.items.GetUpperBound(0); i++)
                    {
                        ObjectInteraction obj = npcCont.GetItemAt(i);
                        if (obj != null)
                        {
                            if (obj.item_id == item_id)
                            {
                                return obj.ObjectIndex;
                            }
                        }
                    }
                    break;
                }

            case 1://PC Search
                {
                    int rangeS = (item_id - 1000) * 16;
                    int rangeE = rangeS + 16;
                    ObjectInteraction obj = null;
                    if (item_id >= 1000)
                    {
                        for (int i = rangeS; i <= rangeE; i++)
                        {
                            obj = UWCharacter.Instance.playerInventory.findObjInteractionByID(i);
                            if (obj != null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        obj = UWCharacter.Instance.playerInventory.findObjInteractionByID(item_id);
                    }

                    Debug.Log("PC version of find_inv."); //will happen in judy's conversation

                    if (obj != null)
                    {
                        return obj.ObjectIndex;
                    }
                    else
                    {
                        return 0;
                    }
                }

        }
        return 0;
    }


    /// <summary>
    /// Identifies the item at the specified trade slot
    /// </summary>
    /// <param name="unk1">Unk1.</param>
    /// <param name="unk2">pStrPtr. Location to save the string pointer for this item.</param>
    /// <param name="unk3">ItemId Sets the item id of the item into the locals array</param>
    /// <param name="unk4">Unk4.</param>
    /// <param name="inventorySlotIndex">Trade slot index.</param>
    public int identify_inv(int pUNK1, int pStrPtr, int pUNK3, int pTradeSlot)
    {
        //id=0017 name="identify_inv" ret_type=int
        //	parameters:   arg1:
        //arg2:
        //arg3:
        //arg4: inventory item position
        //description:  unknown TODO
        //return value: unknown

        int ItemPos = stack.at(pTradeSlot);

        ObjectInteraction objInt = ObjectLoader.getObjectIntAt(ItemPos);//  FindObjectInteractionInObjectList(ItemPos);

        if (objInt != null)
        {
            int unitValue = GameWorldController.instance.commonObject.properties[objInt.item_id].Value;
            int qty = objInt.GetQty();
            if (pStrPtr >= 0)
            {
                string objName;
                if (objInt.GetComponent<enchantment_base>())
                {//This is done so Zoranthus can id the scepter of deadly seeker properly. He searches for the word deadly seeker
                    objName = objInt.GetComponent<enchantment_base>().DisplayEnchantment;
                }
                else
                {
                    objName = StringController.instance.GetSimpleObjectNameUW(objInt);
                }

                //Add temporary string to string controller
                stack.Set(
                        pStrPtr,
                        StringController.instance.AddString(
                                conv[currConv].StringBlock,
                                objName));
            }

            return unitValue * qty;
        }
        else
        {
            return 0;
        }
    }


    /// <summary>
    /// checks if the first string contains the second string,
    /// </summary>
    /// <returns>returns 1 when the string was found, 0 when not</returns>
    public int contains(int pStringSearch1, int pStringFind2)
    {//pString2 is the string memory.
     //id=0007 name="contains" ret_type=int
     //parameters:   arg1: pointer to first string id
     //arg2: pointer to second string id
     //description:  checks if the first string contains the second string,
     //case-independent.
     //return value: returns 1 when the string was found, 0 when not
        string StringToFind = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(pStringFind2));
        string StringToSearch = StringController.instance.GetString(conv[currConv].StringBlock, stack.at(pStringSearch1));
        Debug.Log("checking to see if " + StringToSearch + " contains " + StringToFind);

        if (StringToSearch.Trim() == "")
        {
            return 0;//no cheating...
        }
        if (StringToSearch.ToUpper().Contains(StringToFind.ToUpper()))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }


    /// <summary>
    /// Sets the race attitude.
    /// </summary>
    /// <param name="unk1">Unk1.</param>
    /// <param name="gtarg">Gtarg.</param>
    /// <param name="attitude">Attitude.</param>
    /// <param name="unk2">Unk2.</param>
    /// Seems to set attitude for all npcs with the whoami of the same value.
    public void set_race_attitude(NPC npc, int unk1, int attitude, int Race)
    {
        //Used in Bandit chief conversation Level3
        //id=0026 name="set_race_attitude" ret_type=void
        //parameters:   unknown
        //description:  sets attitude for a whole race (?)
        //Seems to set attitude for all npcs with the whoami of the same value.
        //Debug.Log ("set_race_attitude " + attitude);
        /*	NPC[] foundNPCs=GameWorldController.instance.DynamicObjectMarker().GetComponentsInChildren<NPC>();
    for (int i=0; i<foundNPCs.GetUpperBound(0);i++)
    {
        if (foundNPCs[i].npc_whoami== npc.npc_whoami)
        {
            Debug.Log("Setting attitude= " + attitude + " for " + foundNPCs[i].name);
            foundNPCs[i].npc_attitude=attitude;
            foundNPCs[i].npc_gtarg=gtarg;
        }
    }*/
        //theory only works on local npcs???
        Debug.Log(npc.name + " Set Race attitude " + unk1 + " " + attitude + " " + Race);
        foreach (Collider Col in Physics.OverlapSphere(npc.transform.position, 4.0f))
        {
            if (Col.gameObject.GetComponent<NPC>() != null)
            {
                if (Col.gameObject.GetComponent<NPC>().GetRace() == Race)
                //if (
                //	(AreNPCSAllied(this,Col.gameObject.GetComponent<NPC>()))	
                //	||
                //	(AreNPCSAllied(Col.gameObject.GetComponent<NPC>(),this))	
                //)
                {
                    Col.gameObject.GetComponent<NPC>().npc_attitude = (short)attitude;

                    if (attitude == 0)
                    {
                        Col.gameObject.GetComponent<NPC>().npc_gtarg = 5;
                        //XG  Col.gameObject.GetComponent<NPC>().gtarg = UWCharacter.Instance.gameObject;
                        //Col.gameObject.GetComponent<NPC>().gtargName = UWCharacter.Instance.gameObject.name;
                        Col.gameObject.GetComponent<NPC>().npc_goal = (byte)NPC.npc_goals.npc_goal_attack_5;
                    }
                }
            }
        }


    }


    /// <summary>
    /// Sets the attitude of a target NPC
    /// </summary>
    /// <param name="Attitude">Attitude.</param>
    /// <param name="target_whoami">The NPC whoami to find</param>
    public void set_attitude(int attitude, int target_whoami)
    {

        NPC[] foundNPCs = GameWorldController.instance.DynamicObjectMarker().GetComponentsInChildren<NPC>();
        for (int i = 0; i < foundNPCs.GetUpperBound(0); i++)
        {
            if (foundNPCs[i].npc_whoami == target_whoami)
            {
                foundNPCs[i].npc_attitude = (short)attitude;
            }
        }
    }


    /// <summary>
    /// compares strings for equality, case independent
    /// </summary>
    /// <returns>returns 1 when strings are equal, 0 when not</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="StringIndex">String index.</param>
    /// <param name="StringIn">String in.</param>
    public int compare(int StringIndex1, int StringIndex2)
    {
        //id=0004 name="compare" ret_type=int
        //	parameters:   arg1: string id
        //	arg2: string id
        //	description:  compares strings for equality, case independent
        //	return value: returns 1 when strings are equal, 0 when not
        //Debug.Log("Comparing :" + StringController.instance.GetString(conv[currConv].StringBlock,StringIndex1).ToUpper() + " to " + StringController.instance.GetString(conv[currConv].StringBlock,StringIndex2).ToUpper() );
        //In this implemention I get the string at stringindex compare with string memory (i'm assuming I'm only comparing with babl_ask)
        if (StringController.instance.GetString(conv[currConv].StringBlock, StringIndex1).ToUpper() == StringController.instance.GetString(conv[currConv].StringBlock, StringIndex2).ToUpper())
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }


    /// <summary>
    /// counts number of items in inventory position.
    /// </summary>
    /// <returns>item number</returns>
    /// <param name="ItemPos">Item position.</param>
    public int count_inv(int ItemPos)
    {
        //id=001e name="count_inv" ret_type=int
        //parameters:   unknown
        //description:  counts number of items in inventory
        //return value: item number
        //int total =0;

        ObjectInteraction objInt = ObjectLoader.getObjectIntAt(ItemPos); //FindObjectInteractionInObjectList(ItemPos); // objInslot.GetComponent<ObjectInteraction>();
        if (objInt != null)
        {
            return objInt.GetQty();
        }
        //}
        return 0;//if not found.
    }



    /// <summary>
    ///  removes npc the player is talking to (?)
    /// </summary>
    public void remove_talker(NPC npc)
    {
        //   id=002a name="remove_talker" ret_type=void
        //parameters:   none
        //		description:  removes npc the player is talking to (?)
        if (npc.Agent != null)
        {
            npc.Agent.enabled = false;
        }
        ObjectInteraction.SetPosition(npc.objInt(), GameWorldController.instance.InventoryMarker.transform.position, false);
    }


    /// <summary>
    /// Copies item from player inventory to npc inventory
    /// </summary>
    /// <param name="unk1">Unk1.</param>
    /// <param name="Quantity">Quantity.</param>
    /// <param name="slotNo">Slot no.</param>
    public void give_ptr_npc(NPC npc, short Quantity, int ptrSlotNo)
    {
        /*
id=0014 name="give_ptr_npc" ret_type=int
parameters:   arg1: quantity (?), or -1 for ignore
arg2: inventory object list pos
description:  copies item from player inventory to npc inventory
return value: none
*/

        int slotNo = stack.at(ptrSlotNo);
        Container cn = npc.gameObject.GetComponent<Container>();
        ObjectInteraction objGiven = UWHUD.instance.playerTrade[slotNo].objectInSlot;  //FindGameObjectInObjectList(slotNo);

        if (objGiven != null)
        {
            if ((Quantity == -1))
            {
                ClearTradeSlotWithObject(slotNo);
                objGiven.transform.parent = GameWorldController.instance.DynamicObjectMarker().transform;
                cn.AddItemToContainer(objGiven);
            }
            else
            {//Clone the object and give the clone to the npc
             //
                if (objGiven != null)
                {
                    if ((objGiven.isQuantityBln == true)
                            &&
                            (objGiven.link > 1)
                            &&
                            (objGiven.isEnchanted == false)
                            &&
                            (objGiven.link != Quantity)
                    )
                    {//Object is a quantity or is a quantity less than the number already there.
                        var Split = ObjectLoader.Clone(objGiven);
                        Split.link = Quantity;
                        objGiven.link = (short)(objGiven.link - Quantity);
                        cn.AddItemToContainer(Split);
                    }
                    else
                    {//Object is not a quantity or is the full amount.
                        ClearTradeSlotWithObject(slotNo);
                        cn.AddItemToContainer(objGiven);
                    }
                }
            }
        }
        //After moving update my ptr to reflect the new object index assuming it's had to change for some reason.
        stack.Set(ptrSlotNo, objGiven.ObjectIndex);//  FindObjectIndexInObjectList(objGiven.name));
    }


    /// <summary>
    /// transfers item to player, per id (?)
    /// </summary>
    /// <returns>1: ok, 2: player has no space left</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="ItemPos">Item position.</param>
    public int take_id_from_npc(NPC npc, int index)
    {
        //id=0016 name="take_id_from_npc" ret_type=int
        //parameters:   arg1: inventory object list pos (from take_from_npc_inv)
        //description:  transfers item to player, per id (?)
        //return value: 1: ok, 2: player has no space left
        if (index > CurrentObjectList().objInfo.GetUpperBound(0))
        {
            Debug.Log("Index out of range in take_id_from_npc");
            return 2;
        }
        ObjectInteraction objInt = CurrentObjectList().objInfo[index].instance;
        Container playerContainer = UWCharacter.Instance.gameObject.GetComponent<Container>();
        if (objInt == null) { return 1; }

        int playerHasSpace;
        //Give to PC
        if (Container.GetFreeSlot(playerContainer) != -1)//Is there space in the container.
        {
            npc.GetComponent<Container>().RemoveItemFromContainer(objInt);

            var tomove = GameWorldController.MoveToInventory(objInt);
            playerContainer.AddItemToContainer(tomove);
            UWCharacter.Instance.GetComponent<PlayerInventory>().Refresh();
            playerHasSpace = 1;
        }
        else
        {
            playerHasSpace = 2;
            npc.GetComponent<Container>().RemoveItemFromContainer(objInt);
            ObjectInteraction.SetPosition(objInt, UWCharacter.Instance.transform.position);  
        }
        return playerHasSpace;
    }


    /// <summary>
    /// Gets the index of the item in the specified slot of the NPC inventory (I think)
    /// </summary>
    /// <returns>The object list index of the item</returns>
    /// <param name="arg1">Arg1.</param>
    public int take_from_npc_inv(NPC npc, int pos)
    {
        pos--;
        ObjectInteraction obj = npc.GetComponent<Container>().GetItemAt((short)pos);
        if (obj != null)
        {
            return obj.BaseObjectData.index;
        }
        return 0;
    }


    /// <summary>
    /// searches for item in barter area
    /// </summary>
    /// <returns>This returns slot number + 1. Take this into account when retrieving the items in later functions</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="itemID">item id to find</param>
    public int find_barter(int itemID)
    {//This returns slot number + 1. Take this into account when retrieving the items in later functions
     //id=0031 name="find_barter" ret_type=int
     //	parameters:   arg1: item id to find
     //	description:  searches for item in barter area
     //	return value: returns pos in inventory object list, or 0 if not found
     // if arg1 > 1000 return Item Category is = + (arg1-1000)*16);
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            if (UWHUD.instance.playerTrade[i].isSelected())
            {
                ObjectInteraction objInt = UWHUD.instance.playerTrade[i].objectInSlot;
                if (objInt != null)
                {
                    if (itemID < 1000)
                    {
                        if (objInt.item_id == itemID)
                        {
                            return objInt.ObjectIndex;
                        }
                    }
                    else
                    {
                        if ((objInt.item_id >= (itemID - 1000) * 16) && (objInt.item_id < ((itemID + 1) - 1000) * 16))
                        {
                            return objInt.ObjectIndex;
                        }
                    }
                }
            }

        }

        return 0;
    }


    /// <summary>
    /// Length of the specified string.
    /// </summary>
    /// <param name="str">String.</param>
    public int length(int str)
    {
        //id=000b name="length" ret_type=int
        //	parameters:   arg1: string id
        //	description:  calculates length of string
        //	return value: length of string
        return StringController.instance.GetString(conv[currConv].StringBlock, str).Length;
    }


    /// <summary>
    /// Gets the string no that identifies this players gender.
    /// </summary>
    /// <param name="unknown">Unknown.</param>
    /// <param name="ParamFemale">female string</param>
    /// <param name="ParamMale">male string</param>
    public int sex(int ParamFemale, int ParamMale)
    {
        if (UWCharacter.Instance.isFemale == true)
        {
            return ParamFemale;
        }
        else
        {
            return ParamMale;
        }
    }



    /// <summary>
    /// Finds the barter total.
    /// </summary>
    /// <returns>The barter total.</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="ptrCount">Ptr count.</param>
    /// <param name="ptrSlot">Ptr slot.</param>
    /// <param name="ptrNoOfSlots">Ptr no of slots.</param>
    /// <param name="item_id">Item identifier.</param>
    /// <param name="variables">Variables.</param>
    /// My implementation 
    /// find the total number of matching items
    /// keep a list of slots where they are found
    /// keep a number of found slots.
    public int find_barter_total(int ptrCount, int ptrSlot, int ptrNoOfSlots, int item_id)//TODO:I swapped ptrslot around with ptrcount.
    {
        int slotFoundCounter = 0;
        stack.Set(ptrNoOfSlots, 0);
        stack.Set(ptrCount, 0);

        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            if (UWHUD.instance.playerTrade[i].isSelected())
            {
                /*
        id=0032 name="find_barter_total" ret_type=int
        parameters:   s[0]: ???
                 s[1]: pointer to number of found items
                 s[2]: pointer to
                 s[3]: pointer to
                 s[4]: pointer to item ID to find
        description:  searches for item in barter area
        return value: 1 when found (?)

        */

                ObjectInteraction objInt = UWHUD.instance.playerTrade[i].GetGameObjectInteraction();

                if (objInt != null)
                {
                    int founditem_id = objInt.item_id;
                    int itemqt = objInt.GetQty();

                    if (item_id == founditem_id)
                    {
                        stack.Set(ptrCount, stack.at(ptrCount) + itemqt);
                        stack.Set(ptrSlot + slotFoundCounter++, objInt.ObjectIndex);
                        stack.Set(ptrNoOfSlots, stack.at(ptrNoOfSlots) + 1);
                    }
                }
            }
        }
        return stack.StackValues[ptrCount];

    }



    /// <summary>
    /// creates item in npc inventory
    /// </summary>
    /// <returns>inventory object list position</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="item_id">Item identifier.</param>
    public int do_inv_create(NPC npc, int item_id)
    {
        //id=001a name="do_inv_create" ret_type=int
        //parameters:   arg1: item id
        //description:  creates item in npc inventory
        //return value: inventory object list position

        ObjectLoaderInfo newobjt = ObjectLoader.newWorldObject(item_id, 0, 0, 1, 256);
        newobjt.is_quant = 1;      
        ObjectInteraction myObj = ObjectInteraction.CreateNewObject(CurrentTileMap(), newobjt, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, GameWorldController.instance.InventoryMarker.transform.position);
        npc.GetComponent<Container>().AddItemToContainer(myObj);
        return myObj.ObjectIndex;
    }



    /// <summary>
    ///  places a generated object in underworld
    /// </summary>
    /// <param name="unk1">Unk1.</param>
    /// <param name="tileY">Tile y.</param>
    /// <param name="tileX">Tile x.</param>
    /// <param name="invSlot">inventory item slot number (from do_inv_create)</param>
    public void place_object(int tileY, int tileX, int index)
    {
        /*
id=0027 name="place_object" ret_type=void
parameters:   arg1: x tile pos
 arg2: y tile pos
 arg3: inventory item slot number (from do_inv_create)
description:  places a generated object in underworld
 used in Judy's conversation, #23

    */
        ObjectInteraction objInt = CurrentObjectList().objInfo[index].instance;
        if (objInt != null)
        {
            //string objName = UWHUD.instance.npcTrade[invSlot].objectInSlot;
            GameObject obj = objInt.gameObject;
            obj.transform.position = CurrentTileMap().getTileVector(tileX, tileY);
        }
        return;
    }

    /// <summary>
    // description:  Deletes item from npc inventory
    /// </summary>
    /// <param name="item_id">Item identifier.</param>
    public void do_inv_delete(NPC npc, int item_id)
    {
        //id=001b name="do_inv_delete" ret_type=int
        //	parameters:   arg1: item id
        //		description:  deletes item from npc inventory
        //		return value: none

        //Debug.Log ("do_inv_delete(" + item_id + ")");

        Container npcContainer = npc.GetComponent<Container>();
        if (npcContainer != null)
        {
            ObjectInteraction Item = npcContainer.findItemOfType(item_id);
            if (Item != null)
            {
                npcContainer.RemoveItemFromContainer(Item);
                //GameObject itemObj = GameObject.Find(Item);
                //if (itemObj != null)
                //{
                Item.consumeObject();
                //}
            }
        }
    }


    /// <summary>
    /// UWformats has no info on this. Based on usage in conversation 220 and on level 7 of prison tower I think it means it looks at the same variables as the check variable traps
    /// </summary>
    /// <returns>The traps.</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="VariableIndex">Variable index.</param>

    public int x_traps(int VariableValue, int VariableIndex)
    {
        Debug.Log("x_traps :" + VariableValue + " " + VariableIndex);
        //if (VariableValue <= Quest.variables.GetUpperBound(0))
        //{
        //    Quest.variables[VariableIndex] = VariableValue;
        //}
        //return Quest.variables[VariableIndex];

        Quest.SetVariable(VariableIndex,VariableValue);
        return Quest.GetVariable(VariableIndex);
    }



    /// <summary>
    /// Switchs the portrait used in the conversation.
    /// </summary>
    /// <param name="PortraitNo">Portrait no.</param>
    void switch_pic(int PortraitNo)
    {
        GRLoader grCharHead = new GRLoader(GRLoader.CHARHEAD_GR);
        RawImage npcPortrait = UWHUD.instance.ConversationPortraits[1];
        npcPortrait.texture = grCharHead.LoadImageAt(PortraitNo - 1);
        UWHUD.instance.NPCName.text = StringController.instance.GetString(7, PortraitNo + 16);
    }

    /// <summary>
    /// x_clock.
    /// </summary>
    /// <returns>The clock.</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="unk2">Unk2.</param>
    /// Reckon it has something to do with game world state changes (possibly linked to scd.ark) and plot. Used alot in Britannia...
    /// Parameters seem to support setting and retrieving of values.
    /// A unk1 = 10001 is retrieve value at index unk2
    /// other wise set unk1 to unk2
    /// I think...
    void x_clock(int unk1, int unk2)
    {
        //Debug.Log("x_clock " + unk1 + " " + unk2 + " at instruction " + stack.instrp);
        if (unk1 == 10001)
        {
            Debug.Log("x_clock returning: " + Quest.GetX_Clock(unk2) + " from " + unk2);
            stack.result_register = Quest.GetX_Clock(unk2);
        }
        else
        {//Should this be an increment???
            Debug.Log("x_clock setting: " + (unk2) + " to " + unk1);
            // Quest.x_clocks[unk2] = unk1;
            Quest.SetX_Clock(unk2, unk1);
        }
    }

    /// <summary>
    /// Adds EXP to player.
    /// </summary>
    /// <param name="xpToAdd">Xp to add.</param>
    void x_exp(int xpToAdd)
    {
        UWCharacter.Instance.AddXP(xpToAdd);
    }

    void PrintImportedVariable(int index, int newValue)
    {
        //Find the variable name
        for (int i = 0; i <= conv[currConv].functions.GetUpperBound(0); i++)
        {
            if ((conv[currConv].functions[i].ID_or_Address == index) && (conv[currConv].functions[i].import_type == import_variable))
            {
                //Debug.Log("Setting " + conv[currConv].functions[i].functionName + " to " + newValue + " was " + stack.at(index));
                return;
            }
        }
        //Debug.Log("Setting variable " + index + " to " + newValue + " was " + stack.at(index));
    }


    /// <summary>
    /// Checks the inv quality.
    /// </summary>
    /// <returns> returns "quality" field of npc? inventory item</returns>
    /// <param name="unk1">Unk1.</param>
    /// <param name="itemPos">Item position.</param>
    public int check_inv_quality(int itemPos)
    {
        //id=001c name="check_inv_quality" ret_type=int
        //parameters:   arg1: inventory item position
        //description:  returns "quality" field of npc? inventory item
        //return value: "quality" field

        ObjectInteraction objInslot = ObjectLoader.getObjectIntAt(itemPos);
        if (objInslot != null)
        {
            return objInslot.quality;
        }
        else
        {
            return 0;
        }
    }


    void x_obj_pos(int arg1, int arg2, int arg3, int arg4, int arg5)
    {
        Debug.Log("x_obj_pos (" + arg1 + "," + arg2 + "," + arg3 + "," + arg4 + "," + arg5);
    }

    /// <summary>
    /// Teleports the talker.
    /// </summary>
    /// <param name="npc">Npc.</param>
    /// <param name="tileY">Tile y.</param>
    /// <param name="tileX">Tile x.</param>
    void teleport_talker(NPC npc, int tileY, int tileX)
    {
        Debug.Log("moving " + npc.name + " to " + tileX + " " + tileY);
        //npc.transform.position = CurrentTileMap().getTileVector(tileX, tileY);
        npc.Agent.Warp(CurrentTileMap().getTileVector(tileX, tileY));
    }

    /// <summary>
    /// Babls the hack.
    /// </summary>
    /// <param name="arg">Argument.</param>
    /// Jospur arena debt
    void babl_hackJospurDebt(int arg)
    {
        //Arena debt
        stack.result_register = Quest.GetQuestVariable(133);
    }

    /// <summary>
    /// Babls the hack.
    /// </summary>
    /// <param name="arg">Argument.</param>
    /// <param name="NoOfFighters">No of fighters.</param>
    /// <param name="arena">Arena.</param>
    /// <param name="unk">Unk.</param>
    void babl_hackSetUpFight(int arg, int NoOfFighters, int arena, int unk)
    {
        Debug.Log("Setting up a fight with " + NoOfFighters + " in arena " + arena);
        SettingUpFight = true;
        //Quest.QuestVariablesOBSOLETE[133] = NoOfFighters * 5;
        Quest.SetQuestVariable(133, NoOfFighters * 5);
        Quest.FightingInArena = true;
        int[] tileX = new int[5];
        int[] tileY = new int[5];
        switch (arena)
        {
            case 0://water
                tileX[0] = 37;
                tileY[0] = 25;
                tileX[1] = 36;
                tileY[1] = 35;
                tileX[2] = 35;
                tileY[2] = 35;
                tileX[3] = 35;
                tileY[3] = 36;
                tileX[4] = 35;
                tileX[4] = 37;
                break;
            case 1://air
                tileX[0] = 27;
                tileY[0] = 37;
                tileX[1] = 27;
                tileY[1] = 36;
                tileX[2] = 27;
                tileY[2] = 35;
                tileX[3] = 26;
                tileY[3] = 35;
                tileX[4] = 25;
                tileX[4] = 35;
                break;
            case 2://fire
                tileX[0] = 25;
                tileY[0] = 27;
                tileX[1] = 26;
                tileY[1] = 27;
                tileX[2] = 27;
                tileY[2] = 27;
                tileX[3] = 27;
                tileY[3] = 26;
                tileX[4] = 27;
                tileX[4] = 25;
                break;
            case 3://earth
            default:
                tileX[0] = 36;//,35,36,35,37};
                tileY[0] = 27;//,26,27,26,27};
                tileX[1] = 35;
                tileY[1] = 26;
                tileX[2] = 36;
                tileY[2] = 27;
                tileX[3] = 35;
                tileY[3] = 26;
                tileX[4] = 37;
                tileX[4] = 27;
                break;
        }

        for (int i = 0; i < NoOfFighters; i++)
        {
            ObjectLoaderInfo objNew = ObjectLoader.newWorldObject(Random.Range(120, 122), 36, 27, 0, 1);
            Vector3 pos = CurrentTileMap().getTileVector(tileX[i], tileY[i]);
            ObjectInteraction objI = ObjectInteraction.CreateNewObject(CurrentTileMap(), objNew, CurrentObjectList().objInfo, GameWorldController.instance.DynamicObjectMarker().gameObject, pos);
            objI.GetComponent<NPC>().npc_attitude = 0;
            objI.GetComponent<NPC>().npc_goal = (byte)NPC.npc_goals.npc_goal_attack_5;
            objI.GetComponent<NPC>().npc_hp = 49;
            objI.GetComponent<NPC>().npc_xhome = (short)tileX[i];
            objI.GetComponent<NPC>().npc_yhome = (short)tileY[i];
            objI.GetComponent<NPC>().npc_whoami = 102;
            //Quest.ArenaOpponents[i] = objNew.index;
            Quest.SetArenaOpponent(i, objNew.index);
        }
    }

    /// <summary>
    /// Teleports the player.
    /// </summary>
    /// <param name="level">Level.</param>
    /// <param name="tileX">Tile x.</param>
    /// <param name="tileY">Tile y.</param>
    void teleport_player(int level, int tileY, int tileX)
    {
        Debug.Log("teleporting to " + level + " " + tileX + " " + tileY);
        Teleport = true;
        TeleportLevel = level - 1;
        TeleportTileX = tileX;
        TeleportTileY = tileY;
    }


    /// <summary>
    /// Sets the inv quality of the item at the specified item list position
    /// </summary>
    /// <param name="NewQuality">New quality.</param>
    /// <param name="itemIndex">Item position in object list</param>
    public void set_inv_quality(int NewQuality, int itemIndex)
    {
        //id=001d name="set_inv_quality" ret_type=int
        //parameters:   arg1: quality value
        //arg2: inventory object list position
        //description:  sets quality for an item in inventory
        //return value: none
        //GameObject objInslot = GameObject.Find(UWHUD.instance.npcTrade[itemPos].objectInSlot);
        if (itemIndex <= CurrentObjectList().objInfo.GetUpperBound(0))
        {
            if (CurrentObjectList().objInfo[itemIndex].instance != null)
            {
                CurrentObjectList().objInfo[itemIndex].instance.quality = (short)NewQuality;
            }
        }
        else
        {
            Debug.Log("itemIndex out of range in set_inv_quality");
        }
    }



    //public static void BuildObjectList()
    //{
    //    ObjectLoader.RebuildObjectListUW(CurrentTileMap(), CurrentObjectList());
    //    int noOfInventoryItems = GameWorldController.instance.InventoryMarker.transform.childCount;
    //    ObjectMasterList = new string[1024 + noOfInventoryItems + 1];
    //    ObjectLoaderInfo[] objList = CurrentObjectList().objInfo;
    //    for (int i = 0; i < 1024; i++)
    //    {
    //        if (objList[i].instance != null)
    //        {
    //            ObjectMasterList[i] = objList[i].instance.name;
    //        }
    //        else
    //        {
    //            ObjectMasterList[i] = "";
    //        }
    //    }
    //    for (int i = 1024; i < 1024 + noOfInventoryItems; i++)
    //    {
    //        ObjectMasterList[i] = GameWorldController.instance.InventoryMarker.transform.GetChild(i - 1024).gameObject.name;
    //    }
    //}

    //static int FindObjectIndexInObjectList(string objectName)
    //{
    //    for (int i = 0; i <= ObjectMasterList.GetUpperBound(0); i++)
    //    {
    //        if (ObjectMasterList[i] == objectName)
    //        {
    //            return i;
    //        }
    //    }
    //    return 0;
    //}

    //static ObjectInteraction FindObjectInteractionInObjectList(int index)
    //{
    //    ObjectInteraction obj = FindGameObjectInObjectList(index);
    //    return obj;
    //}

    //static ObjectInteraction FindGameObjectInObjectList(int index)
    //{
    //    string objName = ObjectMasterList[index];
    //    if (objName != "")
    //    {
    //        GameObject obj = GameObject.Find(objName);
    //        return obj.GetComponent<ObjectInteraction>(); ;
    //    }
    //    return null;
    //}

    static void ClearTradeSlotWithObject(int index)
    {
        TradeSlot toClear = FindTradeSlotWithItem(index);
        if (toClear != null)
        {
            toClear.clear();
        }
    }

    static TradeSlot FindTradeSlotWithItem(int index)
    {
       // string objName = ObjectMasterList[index];
        //if (objName == "") { return null; }
        for (int i = 0; i <= TradeSlot.TradeSlotUBound; i++)
        {
            if (UWHUD.instance.playerTrade[i].objectInSlot != null)
            {
                if (UWHUD.instance.playerTrade[i].objectInSlot.ObjectIndex == index)
                {
                    return UWHUD.instance.playerTrade[i];
                }
            }
        }
        for (int i = 0; i <= UWHUD.instance.npcTrade.GetUpperBound(0); i++)
        {
            if (UWHUD.instance.npcTrade[i].objectInSlot != null)
            {
                if (UWHUD.instance.npcTrade[i].objectInSlot.ObjectIndex == index)
                {
                    return UWHUD.instance.npcTrade[i];
                }
            }
        }
        return null;
    }
}