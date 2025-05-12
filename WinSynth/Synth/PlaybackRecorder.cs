using NAudio.Wave;

namespace WinSynth.Synth;

public class PlaybackRecorder
{
    public (AudioFileReader, WaveOutEvent)[] Tracks { get; set; } = [];

    public bool Playing = false;

    public void AddTrack(string filepath)
    {
        var input = new AudioFileReader(filepath);
        var output = new WaveOutEvent();
        output.Init(input);
        Tracks = [.. Tracks, (input, output)];
    }

    public void RemoveTrack(int i)
    {
        if (Tracks.Length < i + 1) return;

        Tracks[i].Item1.Dispose();
        Tracks[i].Item2.Dispose();

        if (i == 0)
            Tracks = Tracks[1..];
        else if (i == Tracks.Length - 1)
            Tracks = Tracks[..^1];
        else
            Tracks = [.. Tracks[..i], .. Tracks[(i + 1)..]];
    }

    public void Play()
    {
        if (!Playing && Tracks.Length > 0)
        {
            for (int i = 0; i < Tracks.Length; i++) Tracks[i].Item2.Play();
            Playing = true;
        }
        else
        {
            for (int i = 0; i < Tracks.Length; i++) Tracks[i].Item2.Pause();
            Playing = false;
        }
    }

    public void Stop()
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            Tracks[i].Item2.Stop();
            Tracks[i].Item1.Position = 0;
        }
        Playing = false;
    }
}