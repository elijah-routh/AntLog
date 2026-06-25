using Godot;

namespace Game.Enemy
{
    public enum EnemyAttackType
    {
        Charge,
        Explode
    }

    [GlobalClass]
    public partial class AttackFeedbackData : Resource
    {
        [Export] public EnemyAttackType AttackType;

        [ExportGroup("Audio")]
        [Export] public AudioStream WindupSound;
        [Export] public AudioStream AttackSound;
        [Export] public AudioStream ImpactSound;

        [ExportGroup("VFX")]
        [Export] public PackedScene WindupVfx;
        [Export] public PackedScene AttackVfx;
        [Export] public PackedScene ImpactVfx;
    }
}