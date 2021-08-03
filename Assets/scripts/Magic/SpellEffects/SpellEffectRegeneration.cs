public class SpellEffectRegeneration : SpellEffect
{

    ///The amount of regen per counter tick.
    public int DOT;

    public override void ApplyEffect()
    {
        DOT = Value / counter;
        base.ApplyEffect();
    }
}
