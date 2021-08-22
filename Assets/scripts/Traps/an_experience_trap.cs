/// <summary>
/// An experience trap.
/// </summary>
///Adds experience points to the character. 
/// I'm guessing the value in owner.
public class an_experience_trap : trap_base
{

    public override void ExecuteTrap(object_base src, int triggerX, int triggerY, int State)
    {
		//TODO: Calculation from reverse engineering is more complex than this.
        UWCharacter.Instance.AddXP(owner);
    }
}
