public class ApplyPaths : GuiBase
{
    public void SetPaths()
    {
        GameWorldController.instance.config.paths.PATH_UW1 = UWHUD.instance.InputPathUW1.text;
        UWHUD.instance.StatusUW1.CheckPath();

        GameWorldController.instance.config.paths.PATH_UW2 = UWHUD.instance.InputPathUW2.text;
        UWHUD.instance.StatusUW2.CheckPath();

        GameWorldController.instance.config.paths.PATH_UWDEMO = UWHUD.instance.InputPathUWDemo.text;
        UWHUD.instance.StatusUWDemo.CheckPath();
    }
}
