using UnityEngine;

public class a_hack_trap : trap_base
{
    //Hack trap is the term for a do_trap in UW2
	//qual=2 a_do_trap camera
	//qual=3 a_do_trap_platform
    //qual=5 is a trespass trap.
    //qual=10 is the awarding of class specific items at the start of the game
	//qual=11 Probably the forcefield that only allows passage when fraznium gloves are equipped (somewhere in the academy/tower?)
    //qual=12 is an oscillator row of tiles (i think). Owner of the trap increments on each call. -unimplemented
    //qual=14 cycles wall/floor colours in a room in talorus, owner to/from zpos the textures, runs along the x axis for y tiles
    //qual=17 is used to collapse cracked ice floors (linked to timer triggers)
    //qual=18 Scintillus 5 switch puzzle reset
    //qual=19 scintullus 7 platform puzzle reset
    //qual=20 used for rising platforms on level 42 (scintilus)
    //qual=21 is the moving switches in loths tomb.  
	//qual=22 Not found in the wild yet but appears to be a variant of moving switches where owner is forced to be 1
    //qual=23 is a variant of the tmap change
    //qual=24 is the same id as the bullfrog trap. Used in lvl 42 scint and the pits to change graffiti.			
    //qual=25 is the bly scup chamber puzzle
    //qual=26 is the force field on scint level 5
    //qual=27 Changes the quality of the linked object to the trap owner value
    //qual=28 is change tmap objects to use a different texture, 
	//         in practice this is a change to the owner value so this is probably more likely a trap that changes owner rather than just change TMAP
    //Qual=29 is randomly flick buttons. (talorus 1)
    //qual=30 if the avatar is a coward trap in the pits
    //qual=31 something in the arena of fire? Possibly unused. linked to a timer trigger and appears to involve teleporting warriers around the arena.
    //qual=32 is the qbert puzzle in the void. - Used on both the pyramid and the teleports that take you to it (from red hell at least)
    //qual=33 is used to recycle empty bottles! 
	//qual=34 Probably the castle water runs out. Affects plants, mushrooms and fountains.
    //qual=35 is recharge light crystals	
    //qual=36 Called after first LB conversation. Moves all NPCs to their proper locations. Possibly used to manage schedules. Only implemented for the first xclock 
    //qual=37 Not seen in the wild yet. Seems to involve scd.ark
	//qual=38 Used in the tombs to swap your potion of cure poison with a potion of poison (via a linked damage trap)
    //qual=39 is change object visability
    //qual=40 is the vending machine selection
    //qual=41 is the vending machine spawning
    //qual=42 is the vending machine sign (in uw1 is is the talking door!)
    //qual=43 is to change the goal of a (type) of Npc. Used in Tombs level 1 by the skeletons who attack when you pick up the map piece 
    //qual=44 is a go to sleep trap used by "bridge based" beds. (eg prison tower straw beds)
	//qual=45 not seen in the wild yet. refers to automap data??
    //qual=50 is to trigger the conversation with the troll #251 in tybals lair after you are imprisoned.
	//qual=54 is to rotate the gem faces
	//qual=55 is used to teleport to other worlds
    //qual=62 is used in Britannia, prison tower and Kilorn 1 for an unknown purpose .  Also appears in UW1 Level 3
	//		Update aug 2021 seems to set a goal for the npc linked to by the trap using the owner value of the trap.
	//				LBs trap has owner=12 which is stand still. Does this goal have another meaning?
    //      In Britannia where is is triggered by quest variables 109 and 112
    //      which are have you talked to british and have you been arrested
    //      possibly this trap is related to these events. 
    //      The britannia traps link to LBs index and the avatars index "1" (maybe used to manipulate the player/npc in certain scenarios)
    //      In the kilorn and the prison tower the move triggers that call it are disabled. They are linked to one of the humans

    public override void ExecuteTrap(object_base src, int triggerX, int triggerY, int State)
    {
        Debug.Log("Hack Trap " + objInt().BaseObjectData.index + " qual = " + quality + " triggers:" + triggerX + "," + triggerY);
    }

    protected override void Start()
    {
        Debug.Log("Default Hack Trap: Quality=" + quality + " " + this.name);
        //if (
        //    ((GameWorldController.instance.dungeon_level != 0) && (GameWorldController.instance.dungeon_level != 16))
        //    && (quality == 62)
        //    )
        //{
        //    Debug.Log(this.name+ "oh hey another instance of that hack trap I'm trying to figure out");
        //}

    }
}
