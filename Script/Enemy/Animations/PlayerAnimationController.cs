using Godot;


    public partial class PlayerAnimationController : Node
    {
        [Export] public AnimationPlayer AnimationPlayer;

        [ExportGroup("Animation Names")]
        [Export] public string DeathAnimation = "CharacterArmature|Death";
        [Export] public string DuckAnimation = "CharacterArmature|Duck";
        [Export] public string IdleAnimation = "CharacterArmature|Idle";
        [Export] public string JumpAnimation = "CharacterArmature|Jump";
        [Export] public string JumpIdleAnimation = "CharacterArmature|Jump_Idle";
        [Export] public string JumpLandAnimation = "CharacterArmature|Jump_Land";
        [Export] public string NoAnimation = "CharacterArmature|No";
        [Export] public string PunchAnimation = "CharacterArmature|Punch";
        [Export] public string RunAnimation = "CharacterArmature|Run";
        [Export] public string WalkAnimation = "CharacterArmature|Walk";
        [Export] public string WaveAnimation = "CharacterArmature|Wave";
        [Export] public string YesAnimation = "CharacterArmature|Yes";
        [Export] public string WeaponAnimation = "CharacterArmature|Weapon";

    private string _currentAnimation = "";

        public void PlayIdle() => PlayAnimation(IdleAnimation, true);
        public void PlayWalk() => PlayAnimation(WalkAnimation, true);
        public void PlayRun() => PlayAnimation(RunAnimation, true);
        public void PlayJump() => PlayAnimation(JumpIdleAnimation, true);

        public void PlayThrow() => PlayAnimation(WeaponAnimation, false);
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
