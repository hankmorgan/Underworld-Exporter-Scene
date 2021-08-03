using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class Configuration
{

    [Serializable]
    public struct MouseSettings
    {
        public float mouseX;
        public float mouseY;
    }

    [Serializable]
    public struct PathSettings
    {
        public string PATH_UW1;
        public string PATH_UW2;
        public string PATH_UWDEMO;
        public string PATH_SHOCK;
        public string PATH_TNOVA;
    }

    [Serializable]
    public struct CameraSettings
    {
        public float DefaultLightLevel;
        public float FOV;
    }

    [Serializable]
    public struct CheatSettings
    {
        public bool GodMode;
        public bool InfiniteMana;
        public bool BashAllDoors;
    }

    [Serializable]
    public struct UISettings
    {
        public bool ContextUIEnabled;
        public bool AutoKey;
        public bool AutoEat;
    }

    [Serializable]
    public struct DevSettings
    {
        public bool GenerateReports;
        public bool ShowOnlyInUse;
    }

    [Serializable]
    public struct AudioSetting
    {
        public string UW1_SOUNDBANK;
        public string UW2_SOUNDBANK;
    }

    [Serializable]
    public struct KeySettings
    {
        public string flyup;
        public string flydown;
        public string togglemouselook;
        public string togglefullscreen;
        public string interactionoptions;
        public string interactiontalk;
        public string interactionpickup;
        public string interactionlook;
        public string interactionattack;
        public string interactionuse;
        public string castspell;
        public string trackskill;
    }

    //Keycodes in use with their default values.
    [System.NonSerialized]
    public KeyCode FlyUp = KeyCode.R;
    [System.NonSerialized]
    public KeyCode FlyDown = KeyCode.V;
    [System.NonSerialized]
    public KeyCode ToggleMouseLook = KeyCode.E;
    [System.NonSerialized]
    public KeyCode ToggleFullScreen = KeyCode.F;
    [System.NonSerialized]
    public KeyCode InteractionOptions = KeyCode.F1;
    [System.NonSerialized]
    public KeyCode InteractionTalk = KeyCode.F2;
    [System.NonSerialized]
    public KeyCode InteractionPickup = KeyCode.F3;
    [System.NonSerialized]
    public KeyCode InteractionLook = KeyCode.F4;
    [System.NonSerialized]
    public KeyCode InteractionAttack = KeyCode.F5;
    [System.NonSerialized]
    public KeyCode InteractionUse = KeyCode.F6;
    [System.NonSerialized]
    public KeyCode CastSpell = KeyCode.Q;
    [System.NonSerialized]
    public KeyCode TrackSkill = KeyCode.T;

    private static readonly string path = "config.json";

    public MouseSettings mouse;
    public PathSettings paths;
    public CameraSettings camera;
    public CheatSettings cheats;
    public UISettings ui;
    public DevSettings dev;
    public AudioSetting audio;
    //public KeyBindings keys;
    public KeySettings keybindings;

    public Configuration()
    {
        //Default mouse settings
        mouse.mouseX = 15f;
        mouse.mouseY = 15f;
        keybindings.flyup = "R";
        keybindings.flydown = "v";
        keybindings.togglemouselook = "e";
        keybindings.togglefullscreen = "f";
        keybindings.interactionoptions = "f1";
        keybindings.interactiontalk = "f2";
        keybindings.interactionpickup = "f3";
        keybindings.interactionlook = "f4";
        keybindings.interactionattack = "f5";
        keybindings.interactionuse = "f6";
        keybindings.castspell = "q";
        keybindings.trackskill = "t";

        //keys = new KeyBindings() ;        
    }

    public static Configuration Read()
    {
        Configuration instance = new Configuration();

        if (File.Exists(path))
        {
            StreamReader reader = new StreamReader(path);
            string json = reader?.ReadToEnd();
            reader.Close();
            JsonUtility.FromJsonOverwrite(json, instance);
            instance = JsonUtility.FromJson<Configuration>(json);

            //Apply settings to certain properties
            instance.ApplyKeyCodes();
            GameWorldController.instance.MouseX.sensitivityX = instance.mouse.mouseX;
            GameWorldController.instance.MouseY.sensitivityY = instance.mouse.mouseY;

            Camera.main.fieldOfView = instance.camera.FOV;

        }
        return instance;
    }


    public static void Save(Configuration instance)
    {
        string json = JsonUtility.ToJson(instance, true);
        //Write some text to the test.txt file
        File.WriteAllText(path, json);
    }


    void ApplyKeyCodes()
    {
        FlyUp = GetKeyCode(keybindings.flyup);
        FlyDown = GetKeyCode(keybindings.flydown);
        ToggleMouseLook = GetKeyCode(keybindings.togglemouselook);
        ToggleFullScreen = GetKeyCode(keybindings.togglefullscreen);
        InteractionOptions = GetKeyCode(keybindings.interactionoptions);
        InteractionTalk = GetKeyCode(keybindings.interactiontalk);
        InteractionPickup = GetKeyCode(keybindings.interactionpickup);
        InteractionLook = GetKeyCode(keybindings.interactionlook);
        InteractionAttack = GetKeyCode(keybindings.interactionattack);
        InteractionUse = GetKeyCode(keybindings.interactionuse);
        CastSpell = GetKeyCode(keybindings.castspell);
        TrackSkill = GetKeyCode(keybindings.trackskill);

        ////Apply to UI (moved to UWHUD)
        //UWHUD.instance.InteractionControlUW1.ControlItems[0].ShortCutKey = InteractionOptions;
        //UWHUD.instance.InteractionControlUW1.ControlItems[1].ShortCutKey = InteractionTalk;
        //UWHUD.instance.InteractionControlUW1.ControlItems[2].ShortCutKey = InteractionPickup;
        //UWHUD.instance.InteractionControlUW1.ControlItems[3].ShortCutKey = InteractionLook;
        //UWHUD.instance.InteractionControlUW1.ControlItems[4].ShortCutKey = InteractionAttack;
        //UWHUD.instance.InteractionControlUW1.ControlItems[5].ShortCutKey = InteractionUse;
        //UWHUD.instance.InteractionControlUW2.ControlItems[0].ShortCutKey = InteractionOptions;
        //UWHUD.instance.InteractionControlUW2.ControlItems[1].ShortCutKey = InteractionTalk;
        //UWHUD.instance.InteractionControlUW2.ControlItems[2].ShortCutKey = InteractionPickup;
        //UWHUD.instance.InteractionControlUW2.ControlItems[3].ShortCutKey = InteractionLook;
        //UWHUD.instance.InteractionControlUW2.ControlItems[4].ShortCutKey = InteractionAttack;
        //UWHUD.instance.InteractionControlUW2.ControlItems[5].ShortCutKey = InteractionUse;
    }

    KeyCode GetKeyCode(string key)
    {
        chartoKeycode.TryGetValue(key.ToLower(), out KeyCode keyCodeToUse);
        return keyCodeToUse;
    }




    //Dictionary Sourced from https://gist.github.com/b-cancel/c516990b8b304d47188a7fa8be9a1ad9
    //Change to use strings and added function keys.

    //NOTE: This is only a dictionary with MOST characters to keycode bindings (NOT A WORKING CS FILE)
    //ITS USEFUL: when you are reading in your control scheme from a file

    //NOTE: some keys create the same characters
    //since this is a dictionary, only 1 character is bound to 1 keycode
    //EX: * from the keyboard will be read the same as * from the keypad 
    //Because they produce the same character in a text file

    //NOTE: not ALL characters are bound to a keycode due to Unity"s Limitations
    public Dictionary<string, KeyCode> chartoKeycode = new Dictionary<string, KeyCode>()
        {

                  {".", KeyCode.KeypadPeriod},
                      {"/", KeyCode.KeypadDivide},
                {"*", KeyCode.KeypadMultiply},
				 // {"-", KeyCode.KeypadMinus},
				    {"+", KeyCode.KeypadPlus},
				 // {"=", KeyCode.KeypadEquals},


				//other REGULAR
				{"`", KeyCode.BackQuote},
                {"-", KeyCode.Minus},
                {"=", KeyCode.Equals},
                {"[", KeyCode.LeftBracket},
                {"]", KeyCode.RightBracket},
                {"\\'", KeyCode.Backslash}, //remember the special forward slash rule... this isnt wrong
				{";", KeyCode.Semicolon},
                {"\'", KeyCode.Quote}, //neither is this
				{",", KeyCode.Comma},
				//{".", KeyCode.Period},
				//{"/", KeyCode.Slash},




				/*
				//other with SHIFT
				//{"~", KeyCode.tilde}, //inaccesible for some reason...
				{"_", KeyCode.Underscore},
				{"+", KeyCode.Plus},
				//{"{", KeyCode.LeftCurlyBrace}, //inaccesible for some reason...
				//{"}", KeyCode.RightCurlyBrace}, //inaccesible for some reason...
				//{"|", KeyCode.Line}, //inaccesible for some reason...
				{":", KeyCode.Colon},
				{"\"", KeyCode.DoubleQuote},
				{"<", KeyCode.Less},
				{">", KeyCode.Greater},
				{"?", KeyCode.Question},

				//KeyBoard Top Numbers - will not register as numbers but rather as (shift + number) = symbol
				{"!", KeyCode.Exclaim}, //1
				{"@", KeyCode.At},
				{"#", KeyCode.Hash},
				{"$", KeyCode.Dollar},
				//{"%", KeyCode.percent}, //inaccesible for some reason...
				{"^", KeyCode.Caret},
				{"&", KeyCode.Ampersand},
				{"*", KeyCode.Asterisk},
				{"(", KeyCode.LeftParen}, //9
				{")", KeyCode.RightParen}, //0
				*/


				//KeyPad Numbers
				{"1", KeyCode.Keypad1},
                {"2", KeyCode.Keypad2},
                {"3", KeyCode.Keypad3},
                {"4", KeyCode.Keypad4},
                {"5", KeyCode.Keypad5},
                {"6", KeyCode.Keypad6},
                {"7", KeyCode.Keypad7},
                {"8", KeyCode.Keypad8},
                {"9", KeyCode.Keypad9},
                {"0", KeyCode.Keypad0},

				//regular letters
				{"a", KeyCode.A},
                {"b", KeyCode.B},
                {"c", KeyCode.C},
                {"d", KeyCode.D},
                {"e", KeyCode.E},
                {"f", KeyCode.F},
                {"g", KeyCode.G},
                {"h", KeyCode.H},
                {"i", KeyCode.I},
                {"j", KeyCode.J},
                {"k", KeyCode.K},
                {"l", KeyCode.L},
                {"m", KeyCode.M},
                {"n", KeyCode.N},
                {"o", KeyCode.O},
                {"p", KeyCode.P},
                {"q", KeyCode.Q},
                {"r", KeyCode.R},
                {"s", KeyCode.S},
                {"t", KeyCode.T},
                {"u", KeyCode.U},
                {"v", KeyCode.V},
                {"w", KeyCode.W},
                {"x", KeyCode.X},
                {"y", KeyCode.Y},
                {"z", KeyCode.Z},

				//Added function keys
				{"f1", KeyCode.F1},
                {"f2", KeyCode.F2},
                {"f3", KeyCode.F3},
                {"f4", KeyCode.F4},
                {"f5", KeyCode.F5},
                {"f6", KeyCode.F6},
                {"f7", KeyCode.F7},
                {"f8", KeyCode.F8},
                {"f9", KeyCode.F9},
                {"f10", KeyCode.F10},
                {"f11", KeyCode.F11},
                {"f12", KeyCode.F12}
        };



}

