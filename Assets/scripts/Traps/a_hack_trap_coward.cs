using UnityEngine;

/// <summary>
/// Used when the avatar flees the arena in the pits of carnage and ends combat with hostile duelists
/// </summary>
public class a_hack_trap_coward : a_hack_trap
{

    public override void ExecuteTrap(object_base src, int triggerX, int triggerY, int State)
    {
        int OpponentsFound = 0;
        int OpponentIndex = 0;
        for (int i = 0; i <= Quest.ArenaOpponents.GetUpperBound(0); i++)
        {
            if (Quest.ArenaOpponents[i] != 0)
            {
                OpponentsFound++;
                OpponentIndex = Quest.ArenaOpponents[i];
                ObjectInteraction objI = ObjectLoader.getObjectIntAt(OpponentIndex);
                if (objI != null)
                {
                    if (objI.GetComponent<NPC>() != null)
                    {//Make NPC calmer.
                        objI.GetComponent<NPC>().npc_attitude = 1;
                        objI.GetComponent<NPC>().npc_goal = (short)NPC.npc_goals.npc_goal_wander_8;
                    }
                }
            }
            Quest.ArenaOpponents[i] = 0;
        }
        //Reduce player reputation.
        if (OpponentsFound > 0)
        {
            //Update win loss record to record a loss
            // Quest.QuestVariablesOBSOLETE[129] = Mathf.Max(Quest.GetQuestVariable(129) - OpponentsFound, 0);
            Quest.SetQuestVariable(129, Mathf.Max(Quest.GetQuestVariable(129) - OpponentsFound, 0));
            //Quest.QuestVariablesOBSOLETE[133] = 0;
            Quest.SetQuestVariable(133, 0);
            if (OpponentIndex > 0)
            {//Begin taunting conversation.
                ObjectInteraction objI = ObjectLoader.getObjectIntAt(OpponentIndex);
                if (objI != null)
                {
                    objI.TalkTo();
                }
            }
            else
            {
                Quest.FightingInArena = false;
            }
        }
    }

    public override void PostActivate(object_base src)
    {
        Debug.Log("Overridden PostActivate to test " + this.name);
        base.PostActivate(src);
    }
}