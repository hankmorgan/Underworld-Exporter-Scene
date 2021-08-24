using UnityEngine;

public class Potion : enchantment_base
{

    public ObjectInteraction linkedspell;

    public override bool Eat()
    {
        return use();
    }

    public override bool use()
    {
        if (ConversationVM.InConversation) { return false; }
        if ((CurrentObjectInHand == null) || ((CurrentObjectInHand == this.objInt())))
        {
            if (linkedspell != null)
            {
                switch (linkedspell.item_id)
                {
                    case 384://A damage trap
                        linkedspell.gameObject.GetComponent<trap_base>().Activate(this, 0, 0, 0);
                        objInt().consumeObject();
                        return true;
                    default://A spell trap
                        UWCharacter.Instance.PlayerMagic.CastEnchantment(UWCharacter.Instance.gameObject, null, linkedspell.gameObject.GetComponent<a_spell>().link - 256, Magic.SpellRule_TargetSelf, Magic.SpellRule_Consumable);
                        objInt().consumeObject();
                        return true;
                }
            }
            else
            {
                int UseString = StringController.str_you_quaff_the_potion_in_one_gulp_;

                UWHUD.instance.MessageScroll.Add(StringController.instance.GetString(1, UseString));
                UWCharacter.Instance.PlayerMagic.CastEnchantment(UWCharacter.Instance.gameObject, null, GetActualSpellIndex(), Magic.SpellRule_TargetSelf, Magic.SpellRule_Consumable);
                objInt().consumeObject();
                return true;
            }
        }
        else
        {
            return ActivateByObject(CurrentObjectInHand);
        }
    }

    protected override int GetActualSpellIndex()
    {
        if (linkedspell != null)
        {
            switch (linkedspell.item_id)
            {
                case 384://A damage trap
                    return 116;
                default: //spell trap
                    return linkedspell.link - 256;
            }
        }
        else
        {
            return base.GetActualSpellIndex(); //link - 256;//527;
        }
    }

    public override bool ApplyAttack(short damage)
    {
        quality -= damage;
        if (quality <= 0)
        {
            ChangeType(213);//Change to debris.
            this.gameObject.AddComponent<enchantment_base>();//Add a generic object base for behaviour. THis is the famous magic debris
                                                             //objInt().objectloaderinfo.InUseFlag=0;
            Destroy(this);//Remove the potion enchantment.
        }
        return true;
    }


    public override string UseVerb()
    {
        return "quaff"; //why not quaff a potion!
    }


    //To support potions that are linked to spells/damage traps
    public override void MoveToWorldEvent()
    {
        if ((isquant == 0) && (link < 256) && (link > 0))
        {//Object links to a spell.
            if (linkedspell != null)
            {
                bool match = false;
                //Try and find a spell already in the level that matches the characteristics of this spell
                ObjectLoaderInfo[] objList = CurrentObjectList().objInfo;
                for (short i = 0; i <= objList.GetUpperBound(0); i++)
                {
                    if (objList[i].GetItemType() == linkedspell.GetItemType())//Find a matching item type
                    {
                        if (objList[i].instance != null)
                        {
                            if ((objList[i].link == linkedspell.link) && (objList[i].owner == linkedspell.owner) && (objList[i].quality == linkedspell.quality))
                            {//Point to that instance if found instead.
                                Destroy(linkedspell.gameObject);
                                linkedspell = objList[i].instance;
                                link = i;
                                match = true;
                                break;
                            }
                        }
                    }
                }

                if (!match)
                {
                    GameWorldController.MoveToWorld(linkedspell,true);
                }
            }
        }
    }

    public override void MoveToInventoryEvent()
    {
        if (linkedspell != null)
        {
            bool DoNotDestroy = false;
            var objList = CurrentObjectList();
            //Try and find another object that links to the original spell.
            for (int i = 2; i < 1024; i++)
            {
                if (DoNotDestroy) { break; }
                if (objList.objInfo[i].instance != null)
                {
                    var linked = objList.objInfo[i].instance.link;
                    var linkedtype = ObjectLoader.GetItemTypeAt(linked);
                    switch (linkedtype)
                    {
                        case ObjectInteraction.POTIONS:
                        case ObjectInteraction.WAND:
                        case ObjectInteraction.A_DAMAGE_TRAP:
                            if (linked == this.linkedspell.ObjectIndex)
                            {
                                DoNotDestroy = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!DoNotDestroy)
            {//Move the existing spell over.
                Debug.Log("Moving a spell. Check this works.");
                var spell = GameWorldController.MoveToInventory(linkedspell.gameObject);
                linkedspell = spell;
            }
            else
            {
                Debug.Log("Copying a spell. Check this works.");
                //Create a copy in the inventory list instead.
                var cloneLinkedSpell = ObjectLoader.Clone(linkedspell);
               // clonelinkedspell.name = "a_spell_for_" + this.name; // ObjectInteraction.UniqueObjectName(clonelinkedspell.GetComponent<ObjectInteraction>());
                linkedspell = cloneLinkedSpell;
                GameWorldController.MoveToInventory(cloneLinkedSpell);
            }
        }
    }

    void MagicFood()
    {

    }

}
