using UnityEngine;

public class ReadableTrap : object_base
{

    //Explodes in your face book.	public override bool use ()
    public override bool use()
    {
        if (CurrentObjectInHand == null)
        {
            UWHUD.instance.MessageScroll.Add("The book explodes in your face!");
            UWCharacter.Instance.ApplyDamage(Random.Range(1, 20));
            //Quest.QuestVariablesOBSOLETE[8] = 1;
            if (_RES==GAME_UW1)
            {//For Bronus/Morlock quest.
                Quest.SetQuestVariable(8, 1);
            }
            objInt().consumeObject();
            return true;
        }
        else
        {
            return ActivateByObject(CurrentObjectInHand);
        }
    }
}
