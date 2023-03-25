using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHud : GuiBase
{
    readonly string[] saveNames = { "", "", "", "" };
    public Texture2D CursorIcon;
    public Rect CursorPosition;


    //References to hud elements
    public GameObject CharGen;//panel
    public GameObject OpScr;
    public GameObject IntroductionButton;
    public GameObject CreateCharacterButton;
    public GameObject CreditsButton;
    public GameObject JourneyOnButton;
    public GameObject[] SaveGameButtons;
    public Text CharName;
    public Text CharGender;
    public Text CharClass;
    public Text CharStr;
    public Text CharDex;
    public Text CharInt;
    public Text CharVit;
    public Text[] CharSkillName;
    public Text[] CharSkillVal;
    public InputField EnterCharName;
    public int MenuMode = 0; //0=main, 1 = chargen, 2= save games
    public int chargenStage = 0;

    //0 = Gender
    //1 = Handeness
    //2 = Class
    //3,4,5,6,7 = Skills. Some are skipped over.
    //8 is portrait/race
    //9 is difficulty.
    //10 is name
    //11 is confirm.

    public Text CharGenQuestion;
    //private string CharNameAns;
    private int CharClassAns;
    private int SkillSeed;
    public RawImage CharGenBody;

    protected int cursorSizeX = 64;
    protected int cursorSizeY = 64;
    GRLoader chrBtns;

    public static MainMenuHud instance;

    public void InitChargenScreen()
    {
        CharName.text = "";
        CharGender.text = "";
        CharStr.text = "";
        CharDex.text = "";
        CharInt.text = "";
        CharVit.text = "";
        CharClass.text = "";
        for (int i = 0; i < 5; i++)
        {
            CharSkillName[i].text = "";
            CharSkillVal[i].text = "";
            if (_RES == GAME_UW2)
            {
                CharSkillVal[i].color = Color.white;
                CharSkillName[i].color = Color.white;
            }
        }
        CharGenBody.texture = Resources.Load<Texture2D>(_RES + "/Sprites/texture_blank");
    }

    public override void Start()
    {
        instance = this;
        if (GameWorldController.instance.AtMainMenu)
        {
            WindowDetectUW.SwitchFromMouseLook();
            //Initialize the open screens from the game files
            switch (_RES)
            {
                case GAME_UW2:
                    OpScr.GetComponent<RawImage>().texture = GameWorldController.instance.bytloader.LoadImageAt(BytLoader.UW2MAIN_BYT);
                    CharGenQuestion.color = Color.white;
                    CharName.color = Color.white;
                    CharGender.color = Color.white;
                    CharClass.color = Color.white;
                    CharStr.color = Color.white;
                    CharDex.color = Color.white;
                    CharInt.color = Color.white;
                    CharVit.color = Color.white;

                    break;
                default:
                    OpScr.GetComponent<RawImage>().texture = GameWorldController.instance.bytloader.LoadImageAt(BytLoader.OPSCR_BYT);

                    break;
            }
            CharGen.GetComponent<RawImage>().texture = GameWorldController.instance.bytloader.LoadImageAt(BytLoader.CHARGEN_BYT);

            CursorIcon = GameWorldController.instance.grCursors.LoadImageAt(0);
            CursorPosition = new Rect(
                    0.0f,
                    0.0f,
                    cursorSizeX,
                    cursorSizeY);
            //Play the splash screens.
            CharGen.SetActive(false);
            UWHUD.instance.CutScenesFull.SetAnimationFile = "FadeToBlackSleep";
            UWHUD.instance.CutScenesFull.End();
            Cutscene_Splash ci = UWHUD.instance.gameObject.AddComponent<Cutscene_Splash>();
            UWHUD.instance.CutScenesFull.cs = ci;
            UWHUD.instance.CutScenesFull.Begin();
        }
    }


    void OnGUI()
    {
        CursorPosition.center = Event.current.mousePosition;
        if (CursorIcon != null)
        {
            GUI.DrawTexture(CursorPosition, CursorIcon);
        }
        if ((MenuMode == 1) || (MenuMode == 2))
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                MenuMode = 0;
                chargenStage = 0;
                OpScr.SetActive(true);
                CharGen.SetActive(false);
                ButtonClickMainMenu(4);
            }
        }
    }

    public void ButtonClickMainMenu(int option)
    {//Button clicks on front menu.

        if (MenuMode == 0)
        {
            switch (option)
            {
                case 0: //PLay introduction
                        //UWHUD.instance.CutScenesFull.SetAnimation="cs013_n01";
                    Cutscene_Intro ci = UWHUD.instance.gameObject.AddComponent<Cutscene_Intro>();
                    UWHUD.instance.CutScenesFull.cs = ci;
                    UWHUD.instance.CutScenesFull.Begin();

                    break;

                case 1: // Create Character
                    MenuMode = 1;
                    CharGen.SetActive(true);
                    OpScr.SetActive(false);
                    CharGenQuestion.text = getQuestion(0);
                    InitChargenScreen();
                    chrBtns = new GRLoader(GRLoader.CHRBTNS_GR)
                    {
                        PaletteNo = 9
                    };
                    PlaceButtons(Chargen.GetChoices(Chargen.STAGE_GENDER, -1), false);
                    break;

                case 2:// Acknowledgements
                    Cutscene_Credits cc = UWHUD.instance.gameObject.AddComponent<Cutscene_Credits>();
                    UWHUD.instance.CutScenesFull.cs = cc;
                    UWHUD.instance.CutScenesFull.Begin();
                    break;
                case 3:// Journey onwards. In the future will be a link to load menu
                    MenuMode = 2;
                    DisplaySaveGames();
                    break;
                case 4://Reset MainMenu
                    IntroductionButton.SetActive(true);
                    CreateCharacterButton.SetActive(true);
                    CreditsButton.SetActive(true);
                    JourneyOnButton.SetActive(true);
                    for (int i = 0; i <= SaveGameButtons.GetUpperBound(0); i++)
                    {
                        SaveGameButtons[i].SetActive(false);
                    }
                    OpScr.SetActive(true);
                    CharGen.SetActive(false);
                    break;
            }
        }
        else
        {//Chargen
            ChargenClick(option);
        }
    }

    void DisplaySaveGames()
    {
        IntroductionButton.SetActive(false);
        CreateCharacterButton.SetActive(false);
        CreditsButton.SetActive(false);
        JourneyOnButton.SetActive(false);

        //List the save names
        UWHUD.instance.MessageScroll.Clear();

        for (int i = 1; i <= 4; i++)
        {
            var toLoad = Path.Combine(Loader.BasePath, "SAVE" + i, "DESC");
            saveNames[i - 1] = "";
            if (File.Exists(toLoad))
            {
                var fileDesc = System.IO.File.ReadAllBytes(toLoad);
               // if (Loader.ReadStreamFile(toLoad, out byte[] fileDesc))
               // {
                    saveNames[i - 1] = System.Text.Encoding.Default.GetString(fileDesc); //new string(fileDesc));
                                                                                       // }
            }
        }

        for (int i = 0; i <= saveNames.GetUpperBound(0); i++)
        {
            if (saveNames[i] != "")
            {
                SaveGameButtons[i].SetActive(true);
                SaveGameButtons[i].GetComponent<Text>().text = saveNames[i];
            }
            else
            {
                SaveGameButtons[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Loads the save at the specified slot
    /// </summary>
    /// <param name="SlotNo">Slot no.</param>
    public void LoadSave(int SlotNo)
    {
        if (SlotNo == -2)
        {
            if (EditorMode == true)
            {
                UWHUD.instance.editorButtonLabel.text = "Enable Editor";
                EditorMode = false;
            }
            else
            {
                UWHUD.instance.editorButtonLabel.text = "Editor Enabled";
                EditorMode = true;
            }
            return;
        }

        if (SlotNo == -1)
        {//Speedstart
            GameWorldController.instance.Lev_Ark_File_Selected = Path.Combine("DATA", "LEV.ARK");
            GameWorldController.instance.SCD_Ark_File_Selected = Path.Combine("DATA", "SCD.ARK");
            GameWorldController.instance.InitBGlobals(0);
            //GameClock.instance._day = 0;
            //GameClock.instance._minute = 51;
            //GameClock.instance._second = 15;
            //GameClock.Clock1 = 3; GameClock.Clock2 = 12; //To Confirm;
            GameClock.ClockSet(3075);
            UWCharacter.Instance.MaxVIT = 255;
            UWCharacter.Instance.CurVIT = 255;
            Skills.STR = 30;
            Skills.DEX = 30;
            Skills.INT= 30;
            UWCharacter.Instance.CharName = "Speedstart";
            Skills.Unarmed = 30;
            Skills.Attack = 30;
            Skills.Defense = 30;
            Skills.Casting = 30;
            Skills.ManaSkill = 30;
            Skills.Acrobat= 30;
            JourneyOnwards();
            return;
        }

        //Load a save file
        //Set the level file
        GameWorldController.instance.Lev_Ark_File_Selected = Path.Combine("SAVE" + (SlotNo + 1), "LEV.ARK");
        GameWorldController.instance.SCD_Ark_File_Selected = Path.Combine("SAVE" + (SlotNo + 1), "SCD.ARK");
        if (_RES != GAME_UW2)
        {
            //Read in the character data
            SaveGame.LoadPlayerDatUW1(SlotNo + 1);
        }
        else
        {
            SaveGame.LoadPlayerDatUW2(SlotNo + 1);
        }

        //Read in bglob
        GameWorldController.instance.InitBGlobals(SlotNo + 1);


        //Load up the map
        JourneyOnwards();
        UWCharacter.Instance.playerInventory.Refresh();
        //000~001~162~Restore Game Complete. \n
        UWHUD.instance.MessageScroll.Set(StringController.instance.GetString(1, StringController.str_restore_game_complete_));
        return;
    }



    public void ChargenClick(int option)
    {
        //0 = Gender
        //1 = Handeness
        //2 = Class
        //3,4,5,6,7 = Skills. Some are skipped over.
        //8 is portrait/race
        //9 is difficulty.
        //10 is name
        //11 is confirm.

        switch (chargenStage)
        {
            case Chargen.STAGE_GENDER:
                //Gender
                Skills.InitSkills();//Set all skills to zero

                if (option == 0)
                {
                    UWCharacter.Instance.isFemale = false;
                }
                else
                {
                    UWCharacter.Instance.isFemale = true;
                }
                CharGender.text = StringController.instance.GetString(2, Chargen.GetChoices(chargenStage, -1)[option]);
                chargenStage++;
                PlaceButtons(Chargen.GetChoices(chargenStage, -1), false);

                break;
            case Chargen.STAGE_HANDENESS:
                //Handedness
                if (option == 0)
                {
                    UWCharacter.Instance.isLefty = true;
                }
                else
                {
                    UWCharacter.Instance.isLefty = false;
                }
                chargenStage++;
                PlaceButtons(Chargen.GetChoices(chargenStage, -1), false);
                break;
            case Chargen.STAGE_CLASS:
                UWCharacter.Instance.CharClass = option;

                CharClassAns = option;
                SkillSeed = Chargen.getSeed(option);
                //Set str, int and dex here.
                //Max attribute is 30. Min is 12.
                Skills.STR = Mathf.Min(Mathf.Max(Chargen.getBaseSTR(option) + Random.Range(1, SkillSeed), 12), 30);
                Skills.INT = Mathf.Min(Mathf.Max(Chargen.getBaseINT(option) + Random.Range(1, SkillSeed), 12), 30);
                Skills.DEX = Mathf.Min(Mathf.Max(Chargen.getBaseDEX(option) + Random.Range(1, SkillSeed), 12), 30);
                CharStr.text = "Str:   " + Skills.STR.ToString();
                CharInt.text = "Int:   " + Skills.INT.ToString();
                CharDex.text = "Dex:   " + Skills.DEX.ToString();
                CharClass.text = getClass(UWCharacter.Instance.CharClass);
                // UWCharacter.Instance.MaxVIT = (Skills.STR * 2);
                //UWCharacter.Instance.CurVIT = (Skills.STR * 2);
                UWCharacter.Instance.MaxVIT = 30 + ((Skills.STR * 1) / 5);
                UWCharacter.Instance.CurVIT = UWCharacter.Instance.MaxVIT;

                CharVit.text = "Vit:   " + UWCharacter.Instance.MaxVIT.ToString();
                //todo
                chargenStage++;
                if (Chargen.GetChoices(chargenStage, CharClassAns).GetUpperBound(0) == 0)
                {           //Only one choice. Accept it by default.					
                            //	chargenStage++;
                    CharGenQuestion.text = getQuestion(chargenStage);
                    ChargenClick(0);
                    return;
                }
                else
                {
                    PlaceButtons(Chargen.GetChoices(chargenStage, CharClassAns), false);
                }

                break;

            case Chargen.STAGE_SKILLS_1:
            case Chargen.STAGE_SKILLS_2:
            case Chargen.STAGE_SKILLS_3:
            case Chargen.STAGE_SKILLS_4:
                //Set skills here if possible.
                AdvanceSkill(option, chargenStage);
                chargenStage++;
                if (Chargen.GetChoices(chargenStage, CharClassAns).GetUpperBound(0) == 0)
                {       //Only one choice. Accept it by default.					
                        //chargenStage++;
                    CharGenQuestion.text = getQuestion(chargenStage);
                    ChargenClick(0);
                    return;
                }
                else
                {
                    PlaceButtons(Chargen.GetChoices(chargenStage, CharClassAns), false);
                }
                break;
            case Chargen.STAGE_SKILLS_5:
                //Assume I will always have a choice here.
                //Set skills here if possible.
                AdvanceSkill(option, chargenStage);
                chargenStage++;
                PlaceButtons(Chargen.GetChoices(chargenStage, CharClassAns), true);//Moving to protrait.
                break;
            case Chargen.STAGE_PORTRAIT:
                chargenStage++;
                PlaceButtons(Chargen.GetChoices(chargenStage, -1), false);
                UWCharacter.Instance.Body = option;
                GRLoader chrBdy = new GRLoader(GRLoader.BODIES_GR);
                //Show the matching body.
                //Update the paperdoll.
                if (UWCharacter.Instance.isFemale)
                {
                    //CharGenBody.texture = (Texture2D)Resources.Load(_RES +"/Hud/Chargen/chrbtns_" + (22+option).ToString("0000"));		
                    CharGenBody.texture = chrBtns.LoadImageAt(22 + option);
                    //UWHUD.instance.playerBody.texture =(Texture2D)Resources.Load(_RES +"/Hud/Bodies/bodies_" + (5+option).ToString("0000"));		
                    UWHUD.instance.playerBody.texture = chrBdy.LoadImageAt(5 + option);
                }
                else
                {
                    //CharGenBody.texture = (Texture2D)Resources.Load(_RES +"/Hud/Chargen/chrbtns_" + (17+option).ToString("0000"));		
                    CharGenBody.texture = chrBtns.LoadImageAt(17 + option);
                    //UWHUD.instance.playerBody.texture =(Texture2D)Resources.Load(_RES +"/Hud/Bodies/bodies_" + (option).ToString("0000"));		
                    UWHUD.instance.playerBody.texture = chrBdy.LoadImageAt(option);
                }

                break;
            case Chargen.STAGE_DIFFICULTY:
                //Not implemented.
                //Show the name input box.
                //Remove buttons.
                chargenStage++;
                GameWorldController.instance.difficulty = option;
                RemoveButtons();
                EnterCharName.gameObject.SetActive(true);
                EnterCharName.GetComponent<RawImage>().texture = chrBtns.LoadImageAt(2);
                EnterCharName.Select();
                break;
            case Chargen.STAGE_NAME:
                //Set the player name.
                //UWCharacter.Instance.CharName=CharNameAns;
                chargenStage++;
                EnterCharName.gameObject.SetActive(false);
                PlaceButtons(Chargen.GetChoices(chargenStage, CharClassAns), false);
                break;
            case Chargen.STAGE_CONFIRM:
                if (option == 0)
                {
                    //Start a new game
                    UWCharacter.Instance.EXP = 50;
                    UWCharacter.Instance.CharLevel = 1;
                    UWCharacter.Instance.TrainingPoints = 1;
                    UWCharacter.Instance.PlayerMagic.MaxMana = (Skills.INT * Skills.ManaSkill) >> 3;
                    UWCharacter.Instance.PlayerMagic.CurMana = UWCharacter.Instance.PlayerMagic.MaxMana;
                    UWCharacter.Instance.PlayerMagic.TrueMaxMana = UWCharacter.Instance.PlayerMagic.MaxMana;
                    GameWorldController.instance.InitBGlobals(0);

                    switch (_RES)
                    {
                        case GAME_UW1:
                        case GAME_UWDEMO:
                            Quest.TalismansRemaining = 8;
                            Quest.DayGaramonDream = 0;
                            Quest.GaramonDream = 0;
                            Quest.IncenseDream = 0;
                            Quest.IsGaramonBuried = false;
                            Quest.IsTybalsOrbDestroyed = false;
                            Quest.IsCupOfWonderFound = false;
                            break;
                        case GAME_UW2:
                            //Why am I setting these!!!
                            //Quest.variables[101] = 255;                            
                            //Quest.variables[102] = 255;
                            //Quest.variables[103] = 255;
                            //Quest.variables[104] = 255;
                            //Quest.variables[105] = 255;
                            //Quest.variables[106] = 255;
                            Quest.SetVariable(101, 255);
                            Quest.SetVariable(102, 255);
                            Quest.SetVariable(103, 255);
                            Quest.SetVariable(104, 255);
                            Quest.SetVariable(105, 255);
                            Quest.SetVariable(106, 255);
                            break;
                    }
                    //GameClock.instance._day = 0;
                    //GameClock.instance._minute = 51;
                    //GameClock.instance._second = 15;
                    //GameClock.Clock1 = 3; GameClock.Clock2 = 12;
                    GameClock.ClockSet(3075);
                    UWCharacter.Instance.Fatigue = 20;
                    UWCharacter.Instance.FoodLevel = 192;
                    JourneyOnwards();
                }
                else
                {
                    //restart chargen
                    chargenStage = Chargen.STAGE_GENDER;
                    InitChargenScreen();
                    PlaceButtons(Chargen.GetChoices(chargenStage, -1), false);
                }
                break;
        }

        //Set next question.
        CharGenQuestion.text = getQuestion(chargenStage);

    }

    public void EnterCharNameEvent()
    {
        //chargenStage++;
        //CharNameAns=EnterCharName.text;
        if (EnterCharName.text.TrimEnd() == "")
        {
            EnterCharName.text = "Avatar";
        }
        CharName.text = EnterCharName.text;
        UWCharacter.Instance.CharName = EnterCharName.text;
        EnterCharName.gameObject.SetActive(false);
        ChargenClick(0);
    }

    public void AdvanceSkill(int option, int Stage)
    {
        int actualSkillNo = Chargen.GetChoices(Stage, CharClassAns)[option] - 30; //was -31
                                                                                  //actualSkillNo++;
                                                                                  //Increase the skill by up to (no more than the skill seed) Apply the attibute bonus if needed.
        int SkillScore = Mathf.Min(Random.Range(1, SkillSeed) + Skills.getSkillAttributeBonus(actualSkillNo), SkillSeed);
        Skills.AdvanceSkill(actualSkillNo, SkillScore);
        string skillname = StringController.instance.GetString(2, Chargen.GetChoices(Stage, CharClassAns)[option]);
        for (int i = 0; i < 5; i++)//Update the display
        {
            if (CharSkillName[i].text == "")
            {//First free slot
                CharSkillName[i].text = skillname;
                CharSkillVal[i].text = Skills.GetSkill(actualSkillNo).ToString();
                return;
            }
            else if (CharSkillName[i].text == skillname)
            {//Skill found add to it.
                CharSkillVal[i].text = Skills.GetSkill(actualSkillNo).ToString();
                return;
            }
        }
    }

    public string getClass(int option)
    {
        return StringController.instance.GetString(2, 23 + option);
    }

    public string getQuestion(int option)
    {
        //0 = Gender
        //1 = Handeness
        //2 = Class
        //3,4,5,6,7 = Skills. Some are skipped over.
        //8 is portrait/race
        //9 is difficulty.
        //10 is name
        //11 is confirm.

        switch (option)
        {
            case 0:
            case 1:
            case 2:
                return StringController.instance.GetString(2, 1 + option);
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                return StringController.instance.GetString(2, 4);
            case 8:
                return "";//no question about portait.
            case 9:
            case 10:
            case 11:
                return StringController.instance.GetString(2, option - 3);
            default:
                return "UNKNOWN Option!";
        }
    }

    public void RemoveButtons()
    {
        //Destroy existing buttons.
        foreach (Transform child in CharGen.transform)
        {
            //Debug.Log(child.name.Substring(0,4) );
            if (child.name.Substring(0, 5) == "_Char")
            {
                Destroy(child.transform.gameObject);
            }
        }
    }

    public void PlaceButtons(int[] buttons, bool isImageButton)
    {
        RemoveButtons();


        if (isImageButton)
        {//JUST A VERTICAL LIST OF PORTRAITS
            int GenderPortraitIndex = 7;
            if (UWCharacter.Instance.isFemale)
            {
                GenderPortraitIndex = 12;
            }
            for (int i = 0; i <= buttons.GetUpperBound(0); i++)
            {
                GameObject myObj = (GameObject)Instantiate(Resources.Load("Prefabs/_CharGenImageButton"));
                myObj.transform.SetParent(CharGen.transform);
                myObj.GetComponent<ChargenButton>().ButtonBG.texture = chrBtns.LoadImageAt(4);
                myObj.GetComponent<ChargenButton>().ButtonOff = chrBtns.LoadImageAt(4);
                myObj.GetComponent<ChargenButton>().ButtonOn = chrBtns.LoadImageAt(5);
                myObj.GetComponent<ChargenButton>().SubmitTarget = this;
                myObj.GetComponent<ChargenButton>().Value = i;
                myObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(70f, 70 - i * 35f);
                myObj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
                //Load the protraits					
                //myObj.GetComponent<ChargenButton>().ButtonImage.texture = (Texture2D)Resources.Load(_RES +"/Hud/Chargen/chrbtns_" + (GenderPortraitIndex+i).ToString("0000"));
                myObj.GetComponent<ChargenButton>().ButtonImage.texture = chrBtns.LoadImageAt(GenderPortraitIndex + i);
            }
        }
        else
        {
            //Pick a configuration to use.
            if (buttons.GetUpperBound(0) <= 8)
            {//One Column
                for (int i = 0; i <= buttons.GetUpperBound(0); i++)
                {
                    GameObject myObj = (GameObject)Instantiate(Resources.Load("Prefabs/_CharGenTextButton"));
                    myObj.GetComponent<ChargenButton>().ButtonBG.texture = chrBtns.LoadImageAt(2);
                    myObj.GetComponent<ChargenButton>().ButtonOff = chrBtns.LoadImageAt(2);
                    myObj.GetComponent<ChargenButton>().ButtonOn = chrBtns.LoadImageAt(6);
                    myObj.transform.SetParent(CharGen.transform);
                    myObj.GetComponent<ChargenButton>().SubmitTarget = this;
                    myObj.GetComponent<ChargenButton>().Value = i;
                    myObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(70f, 60 - i * 20f);
                    myObj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
                    myObj.GetComponent<ChargenButton>().ButtonText.text = StringController.instance.GetString(2, buttons[i]);
                }

            }
            else
            {
                //Two Columns
                for (int i = 0; i <= buttons.GetUpperBound(0); i++)
                {//First 5 buttons
                    GameObject myObj = (GameObject)Instantiate(Resources.Load("Prefabs/_CharGenTextButton"));
                    myObj.GetComponent<ChargenButton>().ButtonBG.texture = chrBtns.LoadImageAt(2);
                    myObj.GetComponent<ChargenButton>().ButtonOff = chrBtns.LoadImageAt(2);
                    myObj.GetComponent<ChargenButton>().ButtonOn = chrBtns.LoadImageAt(6);
                    myObj.transform.SetParent(CharGen.transform);
                    myObj.GetComponent<ChargenButton>().SubmitTarget = this;
                    myObj.GetComponent<ChargenButton>().Value = i;
                    if (i < 5)
                    {//First 4 buttons
                        myObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, 60 - i * 20f);
                    }
                    else
                    {
                        myObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(110f, 60 - (i - 5) * 20f);
                    }
                    myObj.GetComponent<ChargenButton>().ButtonText.text = StringController.instance.GetString(2, buttons[i]);
                    myObj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
                }
            }
        }
    }

    /// <summary>
    /// Brings the player into the gameworld when starting a new game.
    /// </summary>
    public void JourneyOnwards()
    {
        GameWorldController.LoadingGame = true;
        GameWorldController.instance.SwitchLevel(GameWorldController.instance.startLevel);

        UWCharacter.Instance.transform.position = GameWorldController.instance.StartPos;
        UWHUD.instance.gameObject.SetActive(true);
        UWCharacter.Instance.playerController.enabled = true;
        UWCharacter.Instance.Death = false;
        UWCharacter.Instance.playerMotor.enabled = true;
        GameWorldController.instance.AtMainMenu = false;
        UWCharacter.Instance.playerInventory.Refresh();
        UWCharacter.Instance.playerInventory.UpdateLightSources();
        UWHUD.instance.RefreshPanels(UWHUD.HUD_MODE_INVENTORY);
        instance.gameObject.SetActive(false);
        if (EditorMode)
        {
            //GameWorldController.instance.PositionDetect();
            UWHUD.instance.editor.SelectCurrentTile();
        }
        GameWorldController.LoadingGame = false;
    }
}