using Godot;
using System.Text;

namespace Game.UI
{
    public partial class ScorePopup : RichTextLabel
    {
        [Export] public float RiseDistance = 60f;
        [Export] public float Duration = 0.85f;
        [Export] public float StartScale = 0.75f;
        [Export] public float PopScale = 1.35f;
        [Export] public float EndScale = 1.0f;

        private static readonly string[] RainbowColors =
        {
            "#ff3b30", // red
            "#ff9500", // orange
            "#5856d6", // blue/purple
            "#ff2d55"  // pink
        };

        public void Play(int points)
        {
            Text = MakeRainbowText($"+{points}");
            Scale = Vector2.One * StartScale;
            Modulate = Colors.White;

            PivotOffset = Size * 0.5f;

            Vector2 startPosition = Position;
            Vector2 endPosition = startPosition + new Vector2(0, -RiseDistance);

            Tween tween = CreateTween();
            tween.SetParallel(true);

            tween.TweenProperty(this, "position", endPosition, Duration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);

            tween.TweenProperty(this, "scale", Vector2.One * PopScale, Duration * 0.25f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

            tween.TweenProperty(this, "scale", Vector2.One * EndScale, Duration * 0.65f)
                .SetDelay(Duration * 0.25f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);

            tween.TweenProperty(this, "modulate:a", 0f, Duration * 0.35f)
                .SetDelay(Duration * 0.55f);

            tween.Finished += QueueFree;
        }

        private string MakeRainbowText(string value)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                string color = RainbowColors[i % RainbowColors.Length];
                builder.Append($"[color={color}]{value[i]}[/color]");
            }

            return builder.ToString();
        }
    }
}