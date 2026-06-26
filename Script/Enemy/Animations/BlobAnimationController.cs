using Godot;

namespace Game.Enemy
{
    public partial class BlobAnimationController : Node
    {
        [Export] public AnimationPlayer AnimationPlayer;

        [ExportGroup("Animation Names")]
        [Export] public string DeathAnimation = "CharacterArmature|Death";
        [Export] public string DanceAnimation = "CharacterArmature|Dance";
        [Export] public string IdleAnimation = "CharacterArmature|Idle";
        [Export] public string JumpAnimation = "CharacterArmature|Jump";
        [Export] public string NoAnimation = "CharacterArmature|No";
        [Export] public string WalkAnimation = "CharacterArmature|Walk";
        [Export] public string YesAnimation = "CharacterArmature|Yes";

        private string _currentAnimation = "";

        public void PlayIdle()
        {
            GD.Print("BlobAnimationController: PlayIdle called.");
            PlayAnimation(DanceAnimation, true);
        }
        public void PlayWalk() => PlayAnimation(WalkAnimation, true);
        public void PlayJump() => PlayAnimation(JumpAnimation, false);
        public void PlayDance() => PlayAnimation(DanceAnimation, true);
        public void PlayHit() => PlayAnimation(YesAnimation, false);
        public void PlayDeath() => PlayAnimation(DeathAnimation, false);

        private void PlayAnimation(string animationName, bool loop)
        {
            GD.Print($"BlobAnimationController: Trying to play '{animationName}'");

            if (AnimationPlayer == null)
            {
                GD.PrintErr("BlobAnimationController: AnimationPlayer is null.");
                return;
            }

            GD.Print($"BlobAnimationController: AnimationPlayer found: {AnimationPlayer.Name}");

            if (string.IsNullOrEmpty(animationName))
            {
                GD.PrintErr("BlobAnimationController: animationName is empty.");
                return;
            }

            if (!AnimationPlayer.HasAnimation(animationName))
            {
                GD.PrintErr($"BlobAnimationController: Missing animation: '{animationName}'");

                GD.Print("Available animations:");
                foreach (StringName name in AnimationPlayer.GetAnimationList())
                {
                    GD.Print($"- {name}");
                }

                return;
            }

            Animation animation = AnimationPlayer.GetAnimation(animationName);

            if (animation != null)
                animation.LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;

            if (_currentAnimation == animationName && AnimationPlayer.IsPlaying())
            {
                GD.Print($"BlobAnimationController: Already playing '{animationName}'");
                return;
            }

            _currentAnimation = animationName;
            AnimationPlayer.Play(animationName);

            GD.Print($"BlobAnimationController: Now playing '{animationName}'");
        }
    }
}