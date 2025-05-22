using NAudio.Wave;

namespace WinSynth.Synth;

public class Track
{
    public AudioFileReader AudioFile { get; }
    public WaveOutEvent WaveOut { get; }

    public Track(string filepath)
    {
        AudioFile = new AudioFileReader(filepath);
        WaveOut = new WaveOutEvent();
        WaveOut.Init(AudioFile);
    }

    public void Dispose()
    {
        AudioFile.Dispose();
        WaveOut.Dispose();
    }
}