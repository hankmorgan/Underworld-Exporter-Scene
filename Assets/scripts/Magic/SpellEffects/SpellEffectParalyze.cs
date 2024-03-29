﻿using UnityEngine;
/// <summary>
/// Freezes the NPC
/// </summary>
public class SpellEffectParalyze : SpellEffect
{
    public bool isNPC;//The effect is applied to an npc
    public NPC npc;//THe NPC the spell is applied to.
    ///The state the npc was in before the spell was cast at them
    //public int state;
    public Animator anim;

    public override void ApplyEffect()
    {
        if (isNPC == false)
        {//Apply to player
         //if (UWCharacter.Instance==null)
         //{
         //		UWCharacter.Instance= this.GetComponent<UWCharacter>();
         //}
           // UWCharacter.Instance.Paralyzed = true;
            UWCharacter.Instance.ParalyzeTimer = 4 + Random.Range(0, 16);
        }
        else
        {
            if (npc == null)
            {
                npc = this.GetComponent<NPC>();
                npc.Paralyzed = true;
            }
            else
            {
                npc.Paralyzed = true;
            }

            //state = this.GetComponent<NPC>().state;
            //anim = this.GetComponent<NPC>().anim;
            //if (anim!=null)
            //{
            //		anim.enabled=false;
            //}
        }
        base.ApplyEffect();
    }

    public override void CancelEffect()
    {
        if (isNPC == false)
        {
            //UWCharacter.Instance.Paralyzed = false;
            UWCharacter.Instance.ParalyzeTimer = 0;
            UWCharacter.Instance.walkSpeed = 3.0f;
        }
        else
        {
            npc.Paralyzed = false;
        }
        base.CancelEffect();
    }

    public void Update()
    {//Maintain the effect
        if (isNPC == true)
        {
            if (npc != null)
            {
                npc.Paralyzed = true;
            }
            else
            {
                npc = this.GetComponent<NPC>();
                npc.Paralyzed = true;
            }
        }
        else
        {
            //UWCharacter.Instance.Paralyzed = true;
        }
    }

}
