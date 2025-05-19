using NAudio.Wave;

namespace WinSynth.Synth;

public class PlaybackRecorder
{
    public Track? LongestTrack = null;
    public Track[] Tracks { get; set; } = [];

    public bool Playing = false;

    public void AddTrack(string filepath)
    {
        var track = new Track(filepath);
        Tracks = [.. Tracks, track];

        SetLongestTrack();
    }

    public void RemoveTrack(AudioFileReader file)
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            if (file == Tracks[i].AudioFile)
            {
                var track = Tracks[i];

                if (i == 0)
                    Tracks = Tracks[1..];
                else if (i == Tracks.Length - 1)
                    Tracks = Tracks[..^1];
                else
                    Tracks = [.. Tracks[..i], .. Tracks[(i + 1)..]];

                track.Dispose();

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
            Tracks[i].AudioFile.CurrentTime = pos;
        }
    }

    public void PlayPause()
    {
        if (!Playing && Tracks.Length > 0)
        {
            for (int i = 0; i < Tracks.Length; i++) Tracks[i].WaveOut.Play();
            Playing = true;
        }
        else
        {
            for (int i = 0; i < Tracks.Length; i++) Tracks[i].WaveOut.Pause();
            Playing = false;
        }
    }

    public void Stop()
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            Tracks[i].WaveOut.Stop();
            Tracks[i].AudioFile.Position = 0;
        }
        Playing = false;
    }

    public TimeSpan GetCurrentTime()
    {
        if (LongestTrack != null)
            return LongestTrack.AudioFile.CurrentTime;
        else
            return new TimeSpan(0);
    }

    public TimeSpan GetTotalTime()
    {
        if (LongestTrack != null)
            return LongestTrack.AudioFile.TotalTime;
        else
            return new TimeSpan(0);
    }

    public void SetLongestTrack()
    {
        if (Tracks.Length == 0)
        {
            LongestTrack = null;
        }
        else
        {
            var longest = Tracks[0];
            foreach (var track in Tracks[1..])
            {
                if (track.AudioFile.TotalTime > longest.AudioFile.TotalTime)
                {
                    longest = track;
                }
            }

            LongestTrack = longest;
        }
    }
}