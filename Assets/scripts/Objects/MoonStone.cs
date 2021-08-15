using UnityEngine;
/// <summary>
/// Used by the Gate Travel spell as a teleport target
/// </summary>
public class MoonStone : object_base
{

    protected override void Start()
    {
        base.Start();
        if ((objInt().ObjectTileX<=64) && (objInt().ObjectTileY <= 64))
        {
            UWCharacter.Instance.MoonGateLevel = (short)(GameWorldController.instance.dungeon_level + 1);
            UWCharacter.Instance.MoonGatePosition = this.transform.position;
        }
    }

    /// <summary>
    /// Updates the location of the moonstone
    /// </summary>
    /// TODO: only update when position changes?
    /// Or find when on spell cast or Update when leaving level.
    public override void Update()
    {
        base.Update();
        //if (objInt().PickedUp == false)
        //{
        //    UWCharacter.Instance.MoonGateLevel = (short)(GameWorldController.instance.dungeon_level + 1);
        //    UWCharacter.Instance.MoonGatePosition = this.transform.position;
        //}
        //else
        //{
        //    UWCharacter.Instance.MoonGatePosition = Vector3.zero;
        //}
    }

    public override bool DropEvent()
    {
        if ((objInt().ObjectTileX <= 64) && (objInt().ObjectTileY <= 64))
        {
            UWCharacter.Instance.MoonGateLevel = (short)(GameWorldController.instance.dungeon_level + 1);
            UWCharacter.Instance.MoonGatePosition = this.transform.position;
        }
        return base.DropEvent();
    }

    public override bool PickupEvent()
    {
        UWCharacter.Instance.MoonGateLevel = 0;
        UWCharacter.Instance.MoonGatePosition = Vector3.zero;
        return base.PickupEvent();
    }

}
