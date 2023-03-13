﻿/// <summary>
/// Applies the effect.
/// </summary>
/// Sets the NPC attitude and states
/// Does not apply if allied or confused
public class SpellEffectFear : SpellEffect
{

    /// Backup the original state of the Npc
    //public int OriginalState;
    /// Backup the original attitude and goals of the npc
    public short OriginalAttitude;
    public short OriginalGtarg;
    public short OriginalGoal;

    private NPC npc;
    public bool WasActive;

    /// <summary>
    /// Applies the effect.
    /// </summary>
    /// Sets the NPC attitude and states
    /// Does not apply if allied or afraid
    public override void ApplyEffect()
    {
        if ((this.GetComponent<SpellEffectAlly>() == null) && (this.GetComponent<SpellEffectConfusion>() == null))
        {//Only one or the other.
            npc = this.GetComponent<NPC>();
            if (npc != null)
            {
                //OriginalState= npc.state;
                OriginalAttitude = npc.npc_attitude;
                OriginalGoal = npc.npc_goal;
                OriginalGtarg = npc.npc_gtarg;


                //npc.state=NPC.AI_STATE_IDLERANDOM;	//Temporarily just wander around
                npc.npc_attitude = NPC.AI_ATTITUDE_UPSET;

                //Makes the NPC Run away
                npc.npc_goal = (byte)NPC.npc_goals.npc_goal_attack_6;
                npc.WaitTimer = 0f;
                npc.npc_gtarg = 1;

                WasActive = true;
            }
        }
        base.ApplyEffect();
    }

    public override void CancelEffect()
    {
        if (WasActive == true)
        {
            //npc.state=OriginalState;
            npc.npc_attitude = OriginalAttitude;
            npc.npc_goal = (byte)OriginalGoal;
            npc.npc_gtarg = OriginalGtarg;
        }
        base.CancelEffect();
    }
}
