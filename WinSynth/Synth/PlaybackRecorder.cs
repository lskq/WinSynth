using NAudio.Wave;

namespace WinSynth.Synth;

public class PlaybackRecorder
{
    public (AudioFileReader?, WaveOutEvent?) LongestTrack = (null, null);
    public (AudioFileReader, WaveOutEvent)[] Tracks { get; set; } = [];

    public bool Playing = false;

    public void AddTrack(string filepath)
    {
        var input = new AudioFileReader(filepath);
        var output = new WaveOutEvent();
        output.Init(input);
        Tracks = [.. Tracks, (input, output)];

        SetLongestTrack();
    }

    public void RemoveTrack(AudioFileReader file)
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            if (file == Tracks[i].Item1)
            {
                var track = Tracks[i];

                if (i == 0)
                    Tracks = Tracks[1..];
                else if (i == Tracks.Length - 1)
                    Tracks = Tracks[..^1];
                else
                    Tracks = [.. Tracks[..i], .. Tracks[(i + 1)..]];

                track.Item1.Dispose();
                track.Item2.Dispose();

                break;
            }
        }

        SetLongestTrack();
    }

    public void MoveToPos(int milliseconds)
    {
        var pos = new TimeSpan(0, 0, 0, 0, milliseconds);

        for (int i = 0; i < Tracks.Length; i++)
        {
            Tracks[i].Item1.CurrentTime = pos;
        }
    }

    public void PlayPause()
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

    public TimeSpan GetCurrentTime()
    {
        if (LongestTrack.Item1 != null)
            return LongestTrack.Item1.CurrentTime;
        else
            return new TimeSpan(0);
    }

    public TimeSpan GetTotalTime()
    {
        if (LongestTrack.Item1 != null)
            return LongestTrack.Item1.TotalTime;
        else
            return new TimeSpan(0);
    }

    public void SetLongestTrack()
    {
        if (Tracks.Length == 0)
        {
            LongestTrack = (null, null);
        }
        else
        {
            var longest = Tracks[0];
            foreach (var track in Tracks[1..])
            {
                if (track.Item1.TotalTime > longest.Item1.TotalTime)
                {
                    longest = track;
                }
            }

            LongestTrack = longest;
        }
    }
}