using Godot;
using System;

public partial class MenuTitle : Node
{
    public override void _Ready()
    {
        Button playButton = GetNode<Button>("PlayButton"); //name of button in scene
        playButton.Pressed += OnStartButtonPressed;

        SoundManager.Instance.PlayMenuMusic();
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Levels/test_map.tscn");
    }
}