using Godot;
using System.Collections.Generic;

namespace Game.Enemy
{
    public partial class AttackFeedbackComponent : Node3D
    {
        [Export] public AttackFeedbackData[] FeedbackProfiles;

        [ExportGroup("Audio Players")]
        [Export] public AudioStreamPlayer3D WindupPlayer;
        [Export] public AudioStreamPlayer3D AttackPlayer;
        [Export] public AudioStreamPlayer3D ImpactPlayer;

        [ExportGroup("Spawn Points")]
        [Export] public Node3D DefaultWindupPoint;
        [Export] public Node3D DefaultAttackPoint;
        [Export] public Node3D DefaultImpactPoint;

        private readonly Dictionary<EnemyAttackType, AttackFeedbackData> _profiles = new();

        public override void _Ready()
        {
            _profiles.Clear();

            if (FeedbackProfiles == null)
                return;

            foreach (AttackFeedbackData profile in FeedbackProfiles)
            {
                if (profile == null)
                    continue;

                _profiles[profile.AttackType] = profile;
            }
        }

        public void PlayWindup(EnemyAttackType attackType)
        {
            if (!TryGetProfile(attackType, out AttackFeedbackData profile))
                return;

            PlaySound(WindupPlayer, profile.WindupSound);
            SpawnVfx(profile.WindupVfx, DefaultWindupPoint);
        }

        public void PlayAttack(EnemyAttackType attackType)
        {
            if (!TryGetProfile(attackType, out AttackFeedbackData profile))
                return;

            PlaySound(AttackPlayer, profile.AttackSound);
            SpawnVfx(profile.AttackVfx, DefaultAttackPoint);
        }

        public void PlayImpact(EnemyAttackType attackType, Vector3 worldPosition)
        {
            if (!TryGetProfile(attackType, out AttackFeedbackData profile))
                return;

            PlaySound(ImpactPlayer, profile.ImpactSound);
            SpawnVfxAtWorldPosition(profile.ImpactVfx, worldPosition);
        }

        private bool TryGetProfile(
            EnemyAttackType attackType,
            out AttackFeedbackData profile)
        {
            return _profiles.TryGetValue(attackType, out profile) && profile != null;
        }

        private void PlaySound(AudioStreamPlayer3D player, AudioStream stream)
        {
            if (player == null || stream == null)
                return;

            player.Stream = stream;
            player.Play();
        }

        private void SpawnVfx(PackedScene scene, Node3D spawnPoint)
        {
            if (scene == null)
                return;

            Node3D instance = scene.Instantiate<Node3D>();

            Node parent = GetTree().CurrentScene;
            parent.AddChild(instance);

            Node3D point = spawnPoint ?? this;
            instance.GlobalPosition = point.GlobalPosition;
            instance.GlobalRotation = point.GlobalRotation;
        }

        private void SpawnVfxAtWorldPosition(PackedScene scene, Vector3 worldPosition)
        {
            if (scene == null)
                return;

            Node3D instance = scene.Instantiate<Node3D>();

            Node parent = GetTree().CurrentScene;
            parent.AddChild(instance);

            instance.GlobalPosition = worldPosition;
        }
    }
}