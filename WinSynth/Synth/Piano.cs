using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WinSynth.Synth;

public class Piano
{
    public MidiOut MidiOut = new(0);
    public WaveOutEvent[] SignalOut = new WaveOutEvent[12];
    public bool[] NotePlaying = new bool[12];

    public int Mode = 0;
    public double Gain = 0.5;
    public int Frequency = 4;
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
        for (int i = 0; i < 12; i++)
        {
            SignalOut[i] = new WaveOutEvent();
        }
    }

    public void Play(int note)
    {
        if (NotePlaying[note]) return;

        if (Mode == 0)
        {
            int noteNumber = note + (12 * (Frequency + 1));
            var noteOnEvent = new NoteOnEvent(0, 1, noteNumber, (int)(100 * Gain), 50);

            MidiOut.Send(noteOnEvent.GetAsShortMessage());
        }
        else
        {
            var waveType = Mode switch
            {
                1 => SignalGeneratorType.Sin,
                2 => SignalGeneratorType.Triangle,
                3 => SignalGeneratorType.Square,
                4 => SignalGeneratorType.SawTooth,
                5 => SignalGeneratorType.Pink,
                6 => SignalGeneratorType.White,
                _ => throw new Exception()
            };

            var generator = new SignalGenerator()
            {
                Gain = Gain,
                Frequency = Notes[note] * Math.Pow(2, Frequency),
                Type = waveType
            };

            var wo = SignalOut[note];
            wo.Init(generator);
            wo.Play();
        }

        NotePlaying[note] = true;
    }

    public void Stop(int note)
    {
        NotePlaying[note] = false;

        int noteNumber = note + (12 * (Frequency + 1));
        var noteOnEvent = new NoteOnEvent(0, 1, noteNumber, 0, 0);

        MidiOut.Send(noteOnEvent.GetAsShortMessage());

        SignalOut[note].Stop();
    }
}