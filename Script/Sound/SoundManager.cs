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
    [Export] public AudioStreamPlayer ExplosionPlayer;
    [Export] public AudioStreamPlayer ExplosionPlayer2;
    [Export] public AudioStreamPlayer ExplosionPlayer3;
    [Export] public AudioStreamPlayer ExplosionPlayer4;
    [Export] public AudioStreamPlayer WinPlayer;
    [Export] public AudioStreamPlayer ThrowPlayer;
    [Export] public AudioStreamPlayer GrabPlayer;
    [Export] public AudioStreamPlayer HurtPlayer;
    [Export] public AudioStreamPlayer HitPlayer;

    public bool SwapMenuAndLevelMusic { get; private set; } = false;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void SetSwapMenuAndLevelMusic(bool enabled)
    {
        SwapMenuAndLevelMusic = enabled;
    }

    public void ToggleSwapMenuAndLevelMusic(bool enabled)
    {
        SetSwapMenuAndLevelMusic(enabled);
        PlayMenuMusic();
    }

    public void PlayButtonSound()
    {
        PlaySfx(ButtonPlayer);
    }

    public void PlayExplosionSound()
    {
        PlaySfx(ExplosionPlayer);
    }

    public void PlayExplosionSound2()
    {
        PlaySfx(ExplosionPlayer2);
    }

    public void PlayExplosionSound3()
    {
        PlaySfx(ExplosionPlayer3);
    }

    public void PlayExplosionSound4()
    {
        PlaySfx(ExplosionPlayer4);
    }

    public void PlayWinSound()
    {
        PlaySfx(WinPlayer);
    }

    public void PlayThrowSound()
    {
        PlaySfx(ThrowPlayer);
    }

    public void PlayGrabSound()
    {
        PlaySfx(GrabPlayer);
    }

    public void PlayHurtSound()
    {
        PlaySfx(HurtPlayer);
    }

    public void PlayHitSound()
    {
        PlaySfx(HitPlayer);
    }

    public void PlayMenuMusic()
    {
        AudioStream music = SwapMenuAndLevelMusic ? LevelMusic : MenuMusic;
        PlayMusic(music);
    }

    public void PlayLevelMusic()
    {
        AudioStream music = SwapMenuAndLevelMusic ? MenuMusic : LevelMusic;
        PlayMusic(music);
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