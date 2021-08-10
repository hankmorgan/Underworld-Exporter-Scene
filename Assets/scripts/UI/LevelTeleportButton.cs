public class LevelTeleportButton : GuiBase
{

    public GameWorldController.UW2_LevelNos levelToLoad;

    public void Load()
    {
        GameWorldController.instance.SwitchLevel((short)levelToLoad);
    }
}
