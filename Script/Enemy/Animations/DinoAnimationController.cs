using Godot;

namespace Game.Enemy
{
    public partial class DinoAnimationController : Node
    {
        [Export] public AnimationPlayer AnimationPlayer;

        [ExportGroup("Animation Names")]
        [Export] public string IdleAnimation = "Armature|Parasaurolophus_Idle";
        [Export] public string WalkAnimation = "Armature|Parasaurolophus_Walk";
        [Export] public string AttackAnimation = "Armature|Parasaurolophus_Attack";
        [Export] public string DeathAnimation = "Armature|Parasaurolophus_Death";
        [Export] public string RunAnimation = "Armature|Parasaurolophus_Run";
        [Export] public string JumpAnimation = "Armature|Parasaurolophus_Jump";

        private string _currentAnimation = "";

        public void PlayIdle() => PlayAnimation(IdleAnimation, true);
        public void PlayWalk() => PlayAnimation(WalkAnimation, true);
        public void PlayRun() => PlayAnimation(RunAnimation, true);
        public void PlayJump() => PlayAnimation(JumpAnimation, true);

        public void PlayAttack() => PlayAnimation(AttackAnimation, false);
        public void PlayDeath() => PlayAnimation(DeathAnimation, false);

        private void PlayAnimation(string animationName, bool loop)
        {
            if (AnimationPlayer == null)
                return;

            if (string.IsNullOrEmpty(animationName))
                return;

            if (!AnimationPlayer.HasAnimation(animationName))
            {
                GD.PrintErr($"Missing animation: {animationName}");
                return;
            }

            Animation animation = AnimationPlayer.GetAnimation(animationName);

            if (animation != null)
                animation.LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;

            if (_currentAnimation == animationName && AnimationPlayer.IsPlaying())
                return;

            _currentAnimation = animationName;
            AnimationPlayer.Play(animationName);
        }
    }
}