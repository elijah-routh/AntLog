using Godot;

public partial class MenuTitle : Node
{
    [ExportGroup("Main Buttons")]
    [Export] private Button playButton;
    [Export] private Button optionsButton;
    [Export] private Button exitButton;

    [ExportGroup("Panels")]
    [Export] private Control mainPanel;
    [Export] private Control optionsPanel;

    [ExportGroup("Options")]
    [Export] private Button backButton;
    [Export] private CheckBox fullscreenCheckBox;

    public override void _Ready()
    {
        if (playButton != null)
            playButton.Pressed += OnStartButtonPressed;

        if (optionsButton != null)
            optionsButton.Pressed += OnOptionsButtonPressed;

        if (exitButton != null)
            exitButton.Pressed += OnExitButtonPressed;

        if (backButton != null)
            backButton.Pressed += OnBackButtonPressed;

        if (fullscreenCheckBox != null)
        {
            fullscreenCheckBox.ButtonPressed = IsFullscreen();
            fullscreenCheckBox.Toggled += OnFullscreenToggled;
        }

        ShowMainMenu();

        SoundManager.Instance.PlayMenuMusic();
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Levels/IslandMap.tscn");
    }

    private void OnOptionsButtonPressed()
    {
        ShowOptionsMenu();
    }

    private void OnBackButtonPressed()
    {
        ShowMainMenu();
    }

    private void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    private void ShowMainMenu()
    {
        if (mainPanel != null)
            mainPanel.Visible = true;

        if (optionsPanel != null)
            optionsPanel.Visible = false;
    }

    private void ShowOptionsMenu()
    {
        if (mainPanel != null)
            mainPanel.Visible = false;

        if (optionsPanel != null)
            optionsPanel.Visible = true;
    }

    private void OnFullscreenToggled(bool enabled)
    {
        DisplayServer.WindowSetMode(
            enabled
                ? DisplayServer.WindowMode.Fullscreen
                : DisplayServer.WindowMode.Windowed
        );
    }

    private bool IsFullscreen()
    {
        DisplayServer.WindowMode mode = DisplayServer.WindowGetMode();

        return mode == DisplayServer.WindowMode.Fullscreen ||
               mode == DisplayServer.WindowMode.ExclusiveFullscreen;
    }
}