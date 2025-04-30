using NAudio.Midi;

namespace WinSynth.Synth;

public class Piano
{
    public MidiOut MidiOut = new(0);
    public bool[] NotePlaying = new bool[12];

    public void Play(int note, int octave)
    {
        if (!NotePlaying[note])
        {
            NotePlaying[note] = true;

            int noteNumber = (note + 12) * (octave + 1);
            var noteOnEvent = new NoteOnEvent(0, 1, noteNumber, 100, 50);

            MidiOut.Send(noteOnEvent.GetAsShortMessage());
        }
    }

    public void Stop(int note, int octave)
    {
        NotePlaying[note] = false;

        int noteNumber = (note + 12) * (octave + 1);
        var noteOnEvent = new NoteOnEvent(0, 1, noteNumber, 0, 0);

        MidiOut.Send(noteOnEvent.GetAsShortMessage());
    }
}