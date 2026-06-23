public readonly struct SpinThrowModifier
{
    public readonly int PowerStep;
    public readonly float ForceMultiplier;
    public readonly float DamageMultiplier;

    public SpinThrowModifier(
        int powerStep,
        float forceMultiplier,
        float damageMultiplier)
    {
        PowerStep = powerStep;
        ForceMultiplier = forceMultiplier;
        DamageMultiplier = damageMultiplier;
    }
}
