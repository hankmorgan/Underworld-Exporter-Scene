using UnityEngine;
public class SilverSeed : object_base
{

    public override bool use()
    {
        if (CurrentObjectInHand == null)
        {
            if ((objInt().PickedUp == true))
            {
                ObjectLoaderInfo newtree = ObjectLoader.newWorldObject(458, 40, 16, 1, 256);
                newtree.is_quant = 1;
                ObjectInteraction.CreateNewObject
                    (
                    CurrentTileMap(),
                    newtree,
                    CurrentObjectList().objInfo,
                    GameWorldController.instance.DynamicObjectMarker().gameObject,
                    CurrentTileMap().getTileVector(TileMap.visitTileX, TileMap.visitTileY)
                    );

                //Plant the seed message
                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, 12));
                Debug.Log("Silver seed has been planted but there is no check for valid soil made");
                UWCharacter.Instance.ResurrectPosition = UWCharacter.Instance.transform.position;
                UWCharacter.Instance.ResurrectLevel = (short)(GameWorldController.instance.dungeon_level + 1);
                objInt().consumeObject();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return ActivateByObject(CurrentObjectInHand);
        }
    }

    public override string UseVerb()
    {
        return "plant";
    }

    public override bool CanBePickedUp()
    {
        return true;
    }
}
