using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class GameSelect : GuiBase
{

	public string RES;
	public bool Game_Found;
	public Text PathStatus;
	public string exe;
	public bool FolderTestPassed = false;

	string[] UW1RequiredFiles = { "uw.exe",
								"DATA\\3DWIN.GR",
								"DATA\\ALLPALS.DAT",
								"DATA\\ANIMO.GR",
								"DATA\\ARMOR_F.GR",
								"DATA\\ARMOR_M.GR",
								"DATA\\BABGLOBS.DAT",
								"DATA\\BLNKMAP.BYT",
								"DATA\\BODIES.GR",
								"DATA\\BUTTONS.GR",
								"DATA\\CHAINS.GR",
								"DATA\\CHARGEN.BYT",
								"DATA\\CHARHEAD.GR",
								"DATA\\CHRBTNS.GR",
								"DATA\\CHRGEN.DAT",
								"DATA\\CMB.DAT",
								"DATA\\CNV.ARK",
								"DATA\\COMOBJ.DAT",
								"DATA\\COMPASS.GR",
								"DATA\\CONV.BYT",
								"DATA\\CONVERSE.GR",
								"DATA\\CURSORS.GR",
								"DATA\\DOORS.GR",
								"DATA\\DRAGONS.GR",
								"DATA\\EYES.GR",
								"DATA\\F16.TR",
								"DATA\\F32.TR",
								"DATA\\FLASKS.GR",
								"DATA\\FONT4X5P.SYS",
								"DATA\\FONT5X6I.SYS",
								"DATA\\FONT5X6P.SYS",
								"DATA\\FONTBIG.SYS",
								"DATA\\FONTBUTN.SYS",
								"DATA\\FONTCHAR.SYS",
								"DATA\\GENHEAD.GR",
								"DATA\\GRAVE.DAT",
								"DATA\\HEADS.GR",
								"DATA\\INV.GR",
								"DATA\\LEV.ARK",
								"DATA\\LFTI.GR",
								"DATA\\LIGHT.DAT",
								"DATA\\LIGHTS.DAT",
								"DATA\\MAIN.BYT",
								"DATA\\MONO.DAT",
								"DATA\\OBJECTS.DAT",
								"DATA\\OBJECTS.GR",
								"DATA\\OPBTN.GR",
								"DATA\\OPSCR.BYT",
								"DATA\\OPTB.GR",
								"DATA\\OPTBTNS.GR",
								"DATA\\PALS.DAT",
								"DATA\\PANELS.GR",
								"DATA\\PLAYER.DAT",
								"DATA\\POWER.GR",
								"DATA\\PRES1.BYT",
								"DATA\\PRES2.BYT",
								"DATA\\QUESTION.GR",
								"DATA\\SCRLEDGE.GR",
								"DATA\\SKILLS.DAT",
								"DATA\\SPELLS.GR",
								"DATA\\STRINGS.PAK",
								"DATA\\TERRAIN.DAT",
								"DATA\\TMFLAT.GR",
								"DATA\\TMOBJ.GR",
								"DATA\\UW.CFG",
								"DATA\\VIEWS.GR",
								"DATA\\W16.TR",
								"DATA\\W64.TR",
								"DATA\\WEAPONS.CM",
								"DATA\\WEAPONS.DAT",
								"DATA\\WEAPONS.GR",
								"DATA\\WIN1.BYT",
								"DATA\\WIN2.BYT",
								"DATA\\XFER.DAT",
								"CRIT\\ASSOC.ANM",
								"CRIT\\CR00PAGE.N00",
								"CRIT\\CR00PAGE.N01",
								"CRIT\\CR01PAGE.N00",
								"CRIT\\CR01PAGE.N01",
								"CRIT\\CR02PAGE.N00",
								"CRIT\\CR02PAGE.N01",
								"CRIT\\CR03PAGE.N00",
								"CRIT\\CR03PAGE.N01",
								"CRIT\\CR04PAGE.N00",
								"CRIT\\CR04PAGE.N01",
								"CRIT\\CR05PAGE.N00",
								"CRIT\\CR05PAGE.N01",
								"CRIT\\CR06PAGE.N00",
								"CRIT\\CR06PAGE.N01",
								"CRIT\\CR07PAGE.N00",
								"CRIT\\CR07PAGE.N01",
								"CRIT\\CR10PAGE.N00",
								"CRIT\\CR10PAGE.N01",
								"CRIT\\CR11PAGE.N00",
								"CRIT\\CR11PAGE.N01",
								"CRIT\\CR12PAGE.N00",
								"CRIT\\CR12PAGE.N01",
								"CRIT\\CR13PAGE.N00",
								"CRIT\\CR13PAGE.N01",
								"CRIT\\CR14PAGE.N00",
								"CRIT\\CR14PAGE.N01",
								"CRIT\\CR15PAGE.N00",
								"CRIT\\CR15PAGE.N01",
								"CRIT\\CR16PAGE.N00",
								"CRIT\\CR16PAGE.N01",
								"CRIT\\CR17PAGE.N00",
								"CRIT\\CR17PAGE.N01",
								"CRIT\\CR20PAGE.N00",
								"CRIT\\CR20PAGE.N01",
								"CRIT\\CR21PAGE.N00",
								"CRIT\\CR21PAGE.N01",
								"CRIT\\CR22PAGE.N00",
								"CRIT\\CR22PAGE.N01",
								"CRIT\\CR23PAGE.N00",
								"CRIT\\CR23PAGE.N01",
								"CRIT\\CR24PAGE.N00",
								"CRIT\\CR24PAGE.N01",
								"CRIT\\CR25PAGE.N00",
								"CRIT\\CR25PAGE.N01",
								"CRIT\\CR26PAGE.N00",
								"CRIT\\CR26PAGE.N01",
								"CRIT\\CR27PAGE.N00",
								"CRIT\\CR27PAGE.N01",
								"CRIT\\CR30PAGE.N00",
								"CRIT\\CR30PAGE.N01",
								"CRIT\\CR31PAGE.N00",
								"CRIT\\CR31PAGE.N01",
								"CRIT\\CR32PAGE.N00",
								"CRIT\\CR32PAGE.N01",
								"CRIT\\CR33PAGE.N00",
								"CRIT\\CR33PAGE.N01",
								"CRIT\\CR34PAGE.N00",
								"CRIT\\CR34PAGE.N01",
								"CRIT\\CR35PAGE.N00",
								"CRIT\\CR35PAGE.N01",
								"CRIT\\CR36PAGE.N00",
								"CRIT\\CR36PAGE.N01",
								"CRIT\\CR37PAGE.N00",
								"CRIT\\CR37PAGE.N01",
								"CUTS\\CS000.N00",
								"CUTS\\CS000.N01",
								"CUTS\\CS000.N02",
								"CUTS\\CS000.N03",
								"CUTS\\CS000.N04",
								"CUTS\\CS000.N05",
								"CUTS\\CS000.N06",
								"CUTS\\CS000.N07",
								"CUTS\\CS000.N10",
								"CUTS\\CS000.N11",
								"CUTS\\CS000.N12",
								"CUTS\\CS000.N13",
								"CUTS\\CS000.N14",
								"CUTS\\CS000.N15",
								"CUTS\\CS000.N16",
								"CUTS\\CS000.N17",
								"CUTS\\CS000.N20",
								"CUTS\\CS000.N21",
								"CUTS\\CS000.N22",
								"CUTS\\CS000.N23",
								"CUTS\\CS000.N24",
								"CUTS\\CS000.N25",
								"CUTS\\CS001.N00",
								"CUTS\\CS001.N01",
								"CUTS\\CS001.N02",
								"CUTS\\CS001.N03",
								"CUTS\\CS001.N04",
								"CUTS\\CS001.N05",
								"CUTS\\CS001.N06",
								"CUTS\\CS001.N07",
								"CUTS\\CS001.N10",
								"CUTS\\CS002.N00",
								"CUTS\\CS002.N01",
								"CUTS\\CS002.N02",
								"CUTS\\CS002.N03",
								"CUTS\\CS002.N04",
								"CUTS\\CS003.N00",
								"CUTS\\CS003.N01",
								"CUTS\\CS003.N02",
								"CUTS\\CS011.N00",
								"CUTS\\CS011.N01",
								"CUTS\\CS012.N00",
								"CUTS\\CS012.N01",
								"CUTS\\CS013.N00",
								"CUTS\\CS013.N01",
								"CUTS\\CS014.N00",
								"CUTS\\CS014.N01",
								"CUTS\\CS015.N00",
								"CUTS\\CS015.N01",
								"CUTS\\CS030.N00",
								"CUTS\\CS031.N00",
								"CUTS\\CS032.N00",
								"CUTS\\CS033.N00",
								"CUTS\\CS034.N00",
								"CUTS\\CS035.N00",
								"CUTS\\CS036.N00",
								"CUTS\\CS037.N00",
								"CUTS\\CS040.N00",
								"CUTS\\CS041.N00",
								"CUTS\\CS400.N00",
								"CUTS\\CS400.N01",
								"CUTS\\CS401.N00",
								"CUTS\\CS401.N01",
								"CUTS\\CS402.N00",
								"CUTS\\CS402.N01",
								"CUTS\\CS403.N00",
								"CUTS\\CS403.N01",
								"CUTS\\CS403.N02",
								"CUTS\\CS404.N00",
								"CUTS\\CS404.N01",
								"CUTS\\CS410.N00",
								"CUTS\\CS410.N01"
					};

	string[] UW2RequiredFiles = { "uw2.exe",
		"DATA\\3DWIN.GR",
		"DATA\\ALLPALS.DAT",
		"DATA\\ANIMO.GR",
		"DATA\\ARMOR_F.GR",
		"DATA\\ARMOR_M.GR",
		"DATA\\BABGLOBS.DAT",
		"DATA\\BODIES.GR",
		"DATA\\BUTTONS.GR",
		"DATA\\BYT.ARK",
		"DATA\\CHAINS.GR",
		"DATA\\CHARHEAD.GR",
		"DATA\\CHRBTNS.GR",
		"DATA\\CHRGEN.DAT",
		"DATA\\CMB.DAT",
		"DATA\\CNV.ARK",
		"DATA\\COMOBJ.DAT",
		"DATA\\COMPASS.GR",
		"DATA\\CONTROLS.DAT",
		"DATA\\CONVERSE.GR",
		"DATA\\CURSORS.GR",
		"DATA\\DL.DAT",
		"DATA\\DL.DAT.bak",
		"DATA\\DOORS.GR",
		"DATA\\DRAGONS.GR",
		"DATA\\EYES.GR",
		"DATA\\FLASKS.GR",
		"DATA\\FONT4X5P.SYS",
		"DATA\\FONT5X6I.SYS",
		"DATA\\FONT5X6P.SYS",
		"DATA\\FONTBIG.SYS",
		"DATA\\FONTBUTN.SYS",
		"DATA\\FONTCHAR.SYS",
		"DATA\\GEMPT.GR",
		"DATA\\GENHEAD.GR",
		"DATA\\GHED.GR",
		"DATA\\GRAVE.DAT",
		"DATA\\HEADS.GR",
		"DATA\\INV.GR",
		"DATA\\LEV.ARK",
		"DATA\\LFTI.GR",
		"DATA\\LIGHT.DAT",
		"DATA\\LIGHTING.DAT",
		"DATA\\LIGHTS.DAT",
		"DATA\\MONO.DAT",
		"DATA\\OBJECTS.DAT",
		"DATA\\OBJECTS.GR",
		"DATA\\OPBTN.GR",
		"DATA\\OPTB.GR",
		"DATA\\OPTBTNS.GR",
		"DATA\\PALS.DAT",
		"DATA\\PANELS.GR",
		"DATA\\PLAYER.DAT",
		"DATA\\POWER.GR",
		"DATA\\QUESTION.GR",
		"DATA\\SCD.ARK",
		"DATA\\SCRLEDGE.GR",
		"DATA\\SKILLS.DAT",
		"DATA\\SPELLS.GR",
		"DATA\\STRINGS.PAK",
		"DATA\\T64.TR",
		"DATA\\TERRAIN.DAT",
		"DATA\\TMFLAT.GR",
		"DATA\\TMOBJ.GR",
		"DATA\\UW.CFG",
		"DATA\\VIEWS.GR",
		"DATA\\WEAP.CM",
		"DATA\\WEAP.DAT",
		"DATA\\WEAP.GR",
		"DATA\\XFER.DAT",
		"CRIT\\AS.AN",
		"CRIT\\CR.AN",
		"CRIT\\CR00.00",
		"CRIT\\CR01.00",
		"CRIT\\CR01.01",
		"CRIT\\CR02.00",
		"CRIT\\CR02.01",
		"CRIT\\CR02.02",
		"CRIT\\CR03.00",
		"CRIT\\CR04.00",
		"CRIT\\CR04.01",
		"CRIT\\CR05.00",
		"CRIT\\CR05.01",
		"CRIT\\CR06.00",
		"CRIT\\CR06.01",
		"CRIT\\CR06.02",
		"CRIT\\CR07.00",
		"CRIT\\CR07.01",
		"CRIT\\CR07.02",
		"CRIT\\CR10.00",
		"CRIT\\CR10.01",
		"CRIT\\CR11.00",
		"CRIT\\CR12.00",
		"CRIT\\CR12.01",
		"CRIT\\CR12.02",
		"CRIT\\CR13.00",
		"CRIT\\CR13.01",
		"CRIT\\CR13.02",
		"CRIT\\CR13.03",
		"CRIT\\CR14.00",
		"CRIT\\CR14.01",
		"CRIT\\CR15.00",
		"CRIT\\CR15.01",
		"CRIT\\CR16.00",
		"CRIT\\CR17.00",
		"CRIT\\CR17.01",
		"CRIT\\CR17.02",
		"CRIT\\CR20.00",
		"CRIT\\CR20.01",
		"CRIT\\CR20.02",
		"CRIT\\CR20.03",
		"CRIT\\CR20.04",
		"CRIT\\CR20.05",
		"CRIT\\CR21.00",
		"CRIT\\CR21.01",
		"CRIT\\CR21.02",
		"CRIT\\CR21.03",
		"CRIT\\CR21.04",
		"CRIT\\CR21.05",
		"CRIT\\CR22.00",
		"CRIT\\CR22.01",
		"CRIT\\CR22.02",
		"CRIT\\CR23.00",
		"CRIT\\CR23.01",
		"CRIT\\CR24.00",
		"CRIT\\CR24.01",
		"CRIT\\CR24.02",
		"CRIT\\CR25.00",
		"CRIT\\CR25.01",
		"CRIT\\CR26.00",
		"CRIT\\CR26.01",
		"CRIT\\CR27.00",
		"CRIT\\CR27.01",
		"CRIT\\CR30.00",
		"CRIT\\CR30.01",
		"CRIT\\CR31.00",
		"CRIT\\CR31.01",
		"CRIT\\CR32.00",
		"CRIT\\CR32.01",
		"CUTS\\CS000.N00",
		"CUTS\\CS000.N01",
		"CUTS\\CS000.N02",
		"CUTS\\CS000.N03",
		"CUTS\\CS000.N04",
		"CUTS\\CS000.N05",
		"CUTS\\CS000.N06",
		"CUTS\\CS000.N07",
		"CUTS\\CS000.N10",
		"CUTS\\CS000.N11",
		"CUTS\\CS000.N12",
		"CUTS\\CS000.N13",
		"CUTS\\CS000.N14",
		"CUTS\\CS000.N15",
		"CUTS\\CS001.N00",
		"CUTS\\CS001.N01",
		"CUTS\\CS001.N02",
		"CUTS\\CS001.N03",
		"CUTS\\CS001.N04",
		"CUTS\\CS001.N05",
		"CUTS\\CS002.N00",
		"CUTS\\CS002.N01",
		"CUTS\\CS002.N02",
		"CUTS\\CS002.N03",
		"CUTS\\CS002.N04",
		"CUTS\\CS002.N05",
		"CUTS\\CS002.N06",
		"CUTS\\CS002.N07",
		"CUTS\\CS002.N10",
		"CUTS\\CS004.N00",
		"CUTS\\CS005.N00",
		"CUTS\\CS006.N00",
		"CUTS\\CS007.N00",
		"CUTS\\CS011.N00",
		"CUTS\\CS011.N01",
		"CUTS\\CS012.N00",
		"CUTS\\CS012.N01",
		"CUTS\\CS030.N00",
		"CUTS\\CS030.N01",
		"CUTS\\CS031.N00",
		"CUTS\\CS031.N01",
		"CUTS\\CS032.N00",
		"CUTS\\CS032.N01",
		"CUTS\\CS033.N00",
		"CUTS\\CS033.N01",
		"CUTS\\CS034.N00",
		"CUTS\\CS034.N01",
		"CUTS\\CS035.N00",
		"CUTS\\CS035.N01",
		"CUTS\\CS036.N00",
		"CUTS\\CS036.N01",
		"CUTS\\CS037.N01",
		"CUTS\\CS040.N00",
		"CUTS\\CS040.N01",
		"CUTS\\CS403.N00",
		"CUTS\\CS403.N01",
		"CUTS\\CS403.N02",
		"CUTS\\LBACK000.BYT",
		"CUTS\\LBACK001.BYT",
		"CUTS\\LBACK002.BYT",
		"CUTS\\LBACK003.BYT",
		"CUTS\\LBACK004.BYT",
		"CUTS\\LBACK005.BYT",
		"CUTS\\LBACK006.BYT",
		"CUTS\\LBACK007.BYT"
	};

	public override void Start()
	{
		base.Start();
		CheckPath();
		switch (RES)
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
		string Path = "";
		switch (RES)
		{
			case GAME_UWDEMO: Path = GameWorldController.instance.path_uw0; break;
			case GAME_UW1: Path = GameWorldController.instance.path_uw1; break;
			case GAME_UW2: Path = GameWorldController.instance.path_uw2; break;
			case GAME_SHOCK: Path = GameWorldController.instance.path_shock; break;
			case GAME_TNOVA: Path = GameWorldController.instance.path_tnova; break;
		}

		Game_Found = (Directory.Exists(Path));
		if (Game_Found)
		{
			PathStatus.text = "Folder found at " + Path;//+ exe; 
			FolderTestPassed = FolderTest();
		}
		else
		{
			PathStatus.text = "Folder not found at " + Path;//+ exe;
			FolderTestPassed = false;
		}
	}

	bool FolderTest()
	{
		switch (RES)
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
			var test = Path.Combine(path, item.Replace("\\",Path.DirectorySeparatorChar.ToString()));
			if (!File.Exists(test))
			{
				PathStatus.text += "\n" + test + " not found!";
				return false;
			}
		}
		PathStatus.text += "\nKnown required files found";
		return true;
	}

	public void OnClick()
	{
		if (!FolderTestPassed) { return; }
		if (!Game_Found) { return; }
		switch (RES)
		{
			case GAME_SHOCK:
				GameObject selectObj = GameObject.Find("SSLevelSelect");
				if (selectObj != null)
				{
					Dropdown selLevel = selectObj.GetComponent<Dropdown>();
					if (selLevel != null)
					{
						GameWorldController.instance.startLevel = (short)selLevel.value;
					}
				}
				break;		}
		GameWorldController.instance.Begin(RES);
	}

	public void onHoverEnter()
	{
		if (!Game_Found) { return; }
		RawImage img = this.GetComponent<RawImage>();
		if (img != null)
		{
			img.color = Color.white;
		}
	}

	public void onHoverExit()
	{
		if (!Game_Found) { return; }
		RawImage img = this.GetComponent<RawImage>();
		if (img != null)
		{
			img.color = Color.grey;
		}
	}

}
