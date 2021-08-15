using UnityEngine;

/// <summary>
/// Changes the variable controlling the vending machine selection
/// </summary>
public class a_hack_trap_vendingselect : a_hack_trap
{

    public override void ExecuteTrap(object_base src, int triggerX, int triggerY, int State)
    {
        Quest.variables[owner]++;
        if (Quest.variables[owner] >= 8)
        {
            Quest.variables[owner] = 0;
        }
    }

    public override void PostActivate(object_base src)
    {
        Debug.Log("Overridden PostActivate to test " + this.name);
        base.PostActivate(src);
    }
}
