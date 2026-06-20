using Game.Items;
using Godot;

namespace Game.Enemy
{

    [GlobalClass]
    public partial class EnemyData : Resource
    {
        [Export] public float MaxHealth { get; set; } = 100f;

        [Export] public float MoveSpeed { get; set; } = 4.5f;
        [Export] public float Acceleration { get; set; } = 14f;
        [Export] public float Friction { get; set; } = 18f;
        [Export] public float Gravity { get; set; } = 24f;
    }
}

//EnemyBase = shared enemy body
//EnemyController = brain / state machine
//EnemyStats = data
//Components = reusable behavior
//Types = enemy-specific overrides
//Scenes = composed prefab variants