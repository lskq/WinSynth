using NAudio.Wave;

namespace WinSynth.Synth;

public class PlaybackRecorder
{
    public AudioFileReader Input { get; set; } = new(Filepath);
    public WaveOutEvent Output { get; set; } = new();

    public bool Playing = false;

    public static string Filepath { get; set; } = "example.mp3";

    public PlaybackRecorder()
    {
        Output.Init(Input);
    }

    public void Play()
    {
        if (!Playing)
        {
            Output.Play();
            Playing = true;
        }
        else
        {
            Output.Pause();
            Playing = false;
        }
    }

    public void Stop()
    {
        Output.Stop();
        Input.Position = 0;
        Playing = false;
    }
}