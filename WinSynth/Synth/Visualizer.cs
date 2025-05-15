using NAudio.Wave;
using NAudio.WaveFormRenderer;

namespace WinSynth.Synth;

public static class Visualizer
{
    public static Image Visualize(WaveStream audioFile)
    {


        var maxPeakProvider = new MaxPeakProvider();
        var rmsPeakProvider = new RmsPeakProvider(200);
        var samplingPeakProvider = new SamplingPeakProvider(200);
        var averagePeakProvider = new AveragePeakProvider(4);

        var myPeakProvider = maxPeakProvider;

        var myRendererSettings = new StandardWaveFormRendererSettings
        {
            Width = (int)audioFile.TotalTime.TotalSeconds * 10,
            TopHeight = 48,
            BottomHeight = 48
        };

        var renderer = new WaveFormRenderer();
        return renderer.Render(audioFile, myPeakProvider, myRendererSettings);


    }
}
