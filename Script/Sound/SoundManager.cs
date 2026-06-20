using Godot;

public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }

    [ExportGroup("Music")]
    [Export] public AudioStreamPlayer MusicPlayer;
    [Export] public AudioStream MenuMusic;
    [Export] public AudioStream LevelMusic;

    [ExportGroup("Sound Effects")]
    [Export] public AudioStreamPlayer ButtonPlayer;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void PlayButtonSound()
    {
        PlaySfx(ButtonPlayer);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(MenuMusic);
    }

    public void PlayLevelMusic()
    {
        PlayMusic(LevelMusic);
    }

    public void StopMusic()
    {
        if (MusicPlayer != null)
            MusicPlayer.Stop();
    }

    private void PlayMusic(AudioStream music)
    {
        if (MusicPlayer == null || music == null)
            return;

        // Make imported audio loop if supported
        if (music is AudioStreamOggVorbis ogg)
            ogg.Loop = true;
        else if (music is AudioStreamMP3 mp3)
            mp3.Loop = true;
        else if (music is AudioStreamWav wav)
            wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;

        if (MusicPlayer.Stream == music && MusicPlayer.Playing)
            return;

        MusicPlayer.Stop();
        MusicPlayer.Stream = music;
        MusicPlayer.Play();
    }

    private void PlaySfx(AudioStreamPlayer player)
    {
        if (player == null)
            return;

        player.Play();
    }
}