using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WinSynth.Synth;

public class Piano
{
    public WaveOutEvent[] Channels;

    public static double[] Notes =
    [
        16.35,
        17.32,
        18.35,
        19.45,
        20.6,
        21.83,
        23.12,
        24.5,
        25.96,
        27.5,
        29.14,
        30.87
    ];

    public Piano()
    {
        Channels = new WaveOutEvent[12];
        for (int i = 0; i < 12; i++)
        {
            Channels[i] = new WaveOutEvent();
        }
    }

    public void Play(int note, int octave, SignalGeneratorType waveType = SignalGeneratorType.Square)
    {
        var generator = new SignalGenerator()
        {
            Gain = 0.1,
            Frequency = Math.Pow(Notes[note], 1 + octave),
            Type = waveType
        };

        var wo = Channels[note];
        wo.Init(generator);
        wo.Play();
    }

    public void Stop(int note)
    {
        Channels[note].Stop();
    }
}