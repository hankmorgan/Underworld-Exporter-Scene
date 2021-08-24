using UnityEngine;

public class a_create_object_trap : trap_base
{
    /*
    Per uw-formats.txt
        0187  a_create object trap
        creates a new object using the object referenced by the "quantity"
        field as a template. the object is created only when a random number
        between 0 and 3f is greater than the "quality" field value.

    Typically the object is stored off map in the room at 99,99 and cloned from there
    Examples of usage
    Level 1 at the north end of the level near the staircase. Two goblins will spawn when the player steps on the move triggers in that area
    */

    //private bool TrapFired=false;
    //public string NavMeshRegion;//Which nav mesh should apply to cloned objects if they are npcs. No longer needed here!
    //public int Room;
    /// <summary>
    /// The last object created. Used to force the garamon converation.
    /// </summary>
    public static string LastObjectCreated = "";

    public override void ExecuteTrap(object_base src, int triggerX, int triggerY, int State)
    {
        ObjectInteraction objToClone = ObjectLoader.getObjectIntAt(link);
        if (objToClone != null)
        {
            GameObject NewObject = CloneObject(objToClone, triggerX, triggerY, true);
            LastObjectCreated = NewObject.name;
            string created = NewObject.name;
            if (objToClone.GetComponent<Container>() != null)
            {//Clone the items on this object
                for (short i = 0; i <= objToClone.GetComponent<Container>().MaxCapacity(); i++)
                {
                    ObjectInteraction obj = objToClone.GetComponent<Container>().GetItemAt(i);
                    if (obj != null)
                    {
                        GameObject CloneContainerItem = CloneObject(obj, triggerX, triggerY, false);
                        NewObject.GetComponent<Container>().items[i] = CloneContainerItem.GetComponent<ObjectInteraction>();
                    }
                }
            }
        }
    }

    public GameObject CloneObject(ObjectInteraction objToClone, int triggerX, int triggerY, bool MoveItem)
    {
        var cloneObj = ObjectLoader.Clone(objToClone);

        if (MoveItem)
        {
            if (this.gameObject.transform.position.x >= 100.0f)
            {
                ObjectInteraction.SetPosition(cloneObj, CurrentTileMap().getTileVector(triggerX, triggerY), true);
            }
            else
            {
                Vector3 newpos = CurrentTileMap().getTileVector(triggerX, triggerY);
                ObjectInteraction.SetPosition(cloneObj, new Vector3(newpos.x, this.gameObject.transform.position.y, newpos.z), true);
            }
        }
        return cloneObj.gameObject;
    }


    public override bool Activate(object_base src, int triggerX, int triggerY, int State)
    {
        //Do what it needs to do.
        ExecuteTrap(this, triggerX, triggerY, State);

        //It's link is the object it is creating so no activation of more traps/triggers
        PostActivate(src);
        return true;
    }
}
