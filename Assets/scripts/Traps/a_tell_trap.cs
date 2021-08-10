using UnityEngine;

public class a_tell_trap : trap_base
{

    protected override void Start()
    {
        base.Start();
        Debug.Log("Oh hey a tell trap!");
    }

}
