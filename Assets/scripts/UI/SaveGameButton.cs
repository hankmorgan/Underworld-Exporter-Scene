﻿using UnityEngine;
using UnityEngine.UI;

public class SaveGameButton : GuiBase
{

    public MainMenuHud SubmitTarget;
    public int slotNo;

    public static Color32 On = new Color32(255, 213, 64, 255);
    public static Color32 Off = new Color32(187, 123, 1, 255);


    public Text label;

    public override void Start()
    {
        base.Start();
        label.color = Off;
    }


    public void OnHoverEnter()
    {
        //ButtonBG.texture=ButtonOn;
        label.color = On;

    }

    public void OnHoverExit()
    {
        //ButtonBG.texture=ButtonOff;
        label.color = Off;
    }

    public virtual void OnClick()
    {
        SubmitTarget.LoadSave(slotNo);
    }

}
