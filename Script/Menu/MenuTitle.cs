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
    [Export] private HSlider volumeSlider;
    [Export] private Label volumeLabel;
    [Export] private CheckBox battleMusicCheckBox;

    private bool _usingControllerFocus;

    private const string MasterBusName = "Master";

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Input.MouseMode = Input.MouseModeEnum.Visible;

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

        if (volumeSlider != null)
        {
            volumeSlider.MinValue = 0.0f;
            volumeSlider.MaxValue = 100.0f;
            volumeSlider.Step = 1.0f;
            volumeSlider.Value = 100.0f;
            volumeSlider.ValueChanged += OnVolumeChanged;

            ApplyVolume(100.0f);
        }

        if (battleMusicCheckBox != null)
        {
            battleMusicCheckBox.ButtonPressed =
                SoundManager.Instance != null &&
                SoundManager.Instance.SwapMenuAndLevelMusic;

            battleMusicCheckBox.Toggled += OnBattleMusicToggled;
        }

        ShowMainMenu(false);

        SoundManager.Instance?.PlayMenuMusic();
    }

    private void OnVolumeChanged(double value)
    {
        ApplyVolume((float)value);
    }

    private void ApplyVolume(float percent)
    {
        percent = Mathf.Clamp(percent, 0.0f, 100.0f);

        int busIndex = AudioServer.GetBusIndex(MasterBusName);
        if (busIndex == -1)
            return;

        if (percent <= 0.0f)
        {
            AudioServer.SetBusMute(busIndex, true);
        }
        else
        {
            AudioServer.SetBusMute(busIndex, false);

            float linearVolume = percent / 100.0f;
            float dbVolume = Mathf.LinearToDb(linearVolume);

            AudioServer.SetBusVolumeDb(busIndex, dbVolume);
        }

        if (volumeLabel != null)
            volumeLabel.Text = $"Volume: {Mathf.RoundToInt(percent)}%";
    }

    private void OnBattleMusicToggled(bool enabled)
    {
        SoundManager.Instance?.ToggleSwapMenuAndLevelMusic(enabled);
    }

    private void ShowMainMenu(bool grabFocusIfController = true)
    {
        if (mainPanel != null)
            mainPanel.Visible = true;

        if (optionsPanel != null)
            optionsPanel.Visible = false;

        if (grabFocusIfController && _usingControllerFocus)
            playButton?.GrabFocus();
    }

    private void ShowOptionsMenu()
    {
        if (mainPanel != null)
            mainPanel.Visible = false;

        if (optionsPanel != null)
            optionsPanel.Visible = true;

        if (_usingControllerFocus)
            volumeSlider?.GrabFocus();
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (IsControllerMenuInput(@event))
        {
            EnableControllerFocus();

            if (optionsPanel != null && optionsPanel.Visible && @event.IsActionPressed("ui_cancel"))
            {
                ShowMainMenu(false);
                GetViewport().SetInputAsHandled();
            }
        }

        if (@event is InputEventMouseMotion || @event is InputEventMouseButton)
        {
            _usingControllerFocus = false;

            Control focused = GetViewport().GuiGetFocusOwner();
            focused?.ReleaseFocus();
        }
    }

    private bool IsControllerMenuInput(InputEvent @event)
    {
        return @event is InputEventJoypadButton ||
               @event is InputEventJoypadMotion ||
               @event.IsActionPressed("ui_up") ||
               @event.IsActionPressed("ui_down") ||
               @event.IsActionPressed("ui_left") ||
               @event.IsActionPressed("ui_right") ||
               @event.IsActionPressed("ui_accept") ||
               @event.IsActionPressed("ui_cancel");
    }

    private void EnableControllerFocus()
    {
        if (_usingControllerFocus)
            return;

        _usingControllerFocus = true;

        if (optionsPanel != null && optionsPanel.Visible)
            volumeSlider?.GrabFocus();
        else
            playButton?.GrabFocus();
    }
}