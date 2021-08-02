using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class GameSelect : GuiBase {

	public string RES;
	public bool Game_Found;
	public Text PathStatus;
	public string exe;
	public bool FolderTestPassed = false;

	string[] UW1RequiredFiles = { "uw.exe", "data\\lev.ark" };
	string[] UW2RequiredFiles = { "uw2.exe", "data\\lev.ark" };

	public override void Start ()
	{
		base.Start ();
		CheckPath();
		switch(RES)
        {
			case GAME_UW1:
				UWHUD.instance.InputPathUW1.text = GameWorldController.instance.config.paths.PATH_UW1;
				break;
			case GAME_UW2:
				UWHUD.instance.InputPathUW2.text = GameWorldController.instance.config.paths.PATH_UW2;
				break;
			case GAME_UWDEMO:
				UWHUD.instance.InputPathUWDemo.text = GameWorldController.instance.config.paths.PATH_UWDEMO;
				break;
		}
	}

	public void CheckPath()
	{
		string Path="";
		switch(RES)
		{
		case GAME_UWDEMO: Path=GameWorldController.instance.path_uw0;break;
		case GAME_UW1: Path=GameWorldController.instance.path_uw1;break;
		case GAME_UW2:Path=GameWorldController.instance.path_uw2;break;
		case GAME_SHOCK:Path=GameWorldController.instance.path_shock;break;
		case GAME_TNOVA:Path=GameWorldController.instance.path_tnova;break;
		}

		Game_Found = (Directory.Exists(Path));
		if (Game_Found)
		{			
			PathStatus.text="Folder found at " + Path ;//+ exe; 
			FolderTestPassed = FolderTest();
		}
		else
		{
			PathStatus.text= "Folder not found at " + Path ;//+ exe;
			FolderTestPassed = false;	
		}
	}


	bool FolderTest()
    {
		switch(RES)
        {
			case GAME_UW1:
                {
                    return CheckRequiredFiles(GameWorldController.instance.path_uw1, UW1RequiredFiles);
                }
			case GAME_UW2:
				{
					return CheckRequiredFiles(GameWorldController.instance.path_uw2, UW2RequiredFiles);
				}
			default:
				return true;
        }
    }

    private bool CheckRequiredFiles(string path, string[] requiredfiles)
    {
        foreach (var item in requiredfiles)
        {
            var test = Path.Combine(path, item);
            if (!File.Exists(test))
            {
                PathStatus.text += "\n" + test + " not found!";
                return false;
            }
        }
        PathStatus.text += "\nKnown required files found";
		PathStatus.text += "\nOnly testing .exe and lev.ark";
		return true;
    }

    public void OnClick()
	{
		if (!FolderTestPassed) { return; }
		if (!Game_Found){return;}
		switch (RES)
		{
		case GAME_SHOCK:
			GameObject selectObj = GameObject.Find("SSLevelSelect");
			if (selectObj!=null)
			{
				Dropdown selLevel = selectObj.GetComponent<Dropdown>();
				if (selLevel!=null)
				{
					GameWorldController.instance.startLevel=(short)selLevel.value;
				}
			}
			break;
		}
		GameWorldController.instance.Begin(RES);
	}

	public void onHoverEnter()
	{
		if (!Game_Found){return;}
		RawImage img = this.GetComponent<RawImage>();
		if	(img!=null)
		{
			img.color = Color.white;
		}
	}

	public void onHoverExit()
	{
		if (!Game_Found){return;}				
		RawImage img = this.GetComponent<RawImage>();
		if	(img!=null)
		{
			img.color = Color.grey;
		}
	}
	
}
