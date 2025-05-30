using NAudio.Wave;
using WinSynth.Synth;
using Timer = System.Windows.Forms.Timer;

namespace WinSynth.Forms;

partial class MainForm : Form
{
    public Timer Timer = new() { Interval = 100 };
    public Piano PianoModel = new();
    public PlaybackRecorder PlaybackRecorder = new();

    public MainForm()
    {
        InitializeComponent();
    }

    public void PlaybackPanelHeaderTick_MouseUp(object? sender, MouseEventArgs e)
    {
        if (sender == null) return;
        var offset = ((Control)sender).Location.X + e.Location.X;

        PlaybackRecorder.MoveToPos(offset * 100);

        UpdateTime();
    }

    public void PlaybackPanelHeader_MouseUp(object? sender, MouseEventArgs e)
    {
        PlaybackRecorder.MoveToPos(e.Location.X * 100);

        UpdateTime();
    }

    public void PlaybackPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        if (sender == null) return;
        var playbackPanel = (Panel)sender;

        PlaybackRecorder.MoveToPos((e.Location.X - playbackPanel.AutoScrollPosition.X) * 100);

        UpdateTime();
    }

    public void TrackCloseButton_Click(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var trackPictureBox = ((Control)sender).Parent;

        if (trackPictureBox == null || trackPictureBox.Tag == null) return;
        PlaybackRecorder.RemoveTrack((AudioFileReader)trackPictureBox.Tag);

        UpdatePlaybackPanel();
    }

    public void ImportButton_Click(object? sender, EventArgs e)
    {
        var op = new OpenFileDialog
        {
            InitialDirectory = Directory.GetCurrentDirectory(),
            Filter = "MP3 files (*.mp3)|*.mp3|" +
                     "WAV files (*.wav)|*.wav|" +
                     "AIFF files (*.aiff)|*.aiff|" +
                     "All files (*.*)|*.*",
            FilterIndex = 4,
            Multiselect = true,
            RestoreDirectory = true,

        };

        if (op.ShowDialog() == DialogResult.OK)
        {
            foreach (var filename in op.FileNames)
            {
                PlaybackRecorder.AddTrack(filename);
                Timer.Start();
                UpdatePlaybackPanel();
            }
        }
    }

    public void FreqTrackbar_Scroll(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var freqTrackbar = (TrackBar)sender;

        PianoModel.Frequency = freqTrackbar.Value;
        Controls.Find("FreqLabel", true)[0].Text = $"Octave: {freqTrackbar.Value}";
    }

    public void GainTrackbar_Scroll(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var gainTrackbar = (TrackBar)sender;
        var gain = gainTrackbar.Value / 10d;

        PianoModel.Gain = gain;
        Controls.Find("GainLabel", true)[0].Text = $"Gain: {gain:0.0}";
    }

    public void PianoCombobox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var pianoCombobox = (ComboBox)sender;

        PianoModel.Mode = pianoCombobox.SelectedIndex;
    }

    public void PlayButton_Click(object? sender, EventArgs e)
    {
        PlaybackRecorder.PlayPause();
        Timer.Start();
    }

    public void StopButton_Click(object? sender, EventArgs e)
    {
        PlaybackRecorder.Stop();
        Timer.Start();
    }

    public void RecordButton_Click(object? sender, EventArgs e)
    {
        //PlaybackRecorder.Record();
    }

    public void Timer_Tick(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var timer = (Timer)sender;

        UpdateTime();

        if (!PlaybackRecorder.Playing)
        {
            timer.Stop();
            return;
        }
    }

    public void PianoKey_MouseDown(object? sender, MouseEventArgs e)
    {
        if (sender == null) return;
        var pianoKey = (Button)sender;
        PianoModel_PlayNote(pianoKey);
    }

    public void PianoKey_MouseUp(object? sender, MouseEventArgs e)
    {
        if (sender == null) return;
        var pianoKey = (Button)sender;
        PianoModel_StopNote(pianoKey);
    }

    public void PianoKey_GotFocus(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var pianoKey = (Button)sender;
        if (pianoKey.Parent != null)
            pianoKey.Parent.Focus();
    }

    public void PianoPanel_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender == null) return;

        var pianoPanel = (Panel)sender;

        foreach (var control in pianoPanel.Controls)
        {
            if (control.GetType() == typeof(Button))
            {
                var button = (Button)control;
                if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                {
                    PianoModel_PlayNote(button);
                }
            }
        }
    }

    public void PianoPanel_KeyUp(object? sender, KeyEventArgs e)
    {
        if (sender == null) return;

        var pianoPanel = (Panel)sender;

        foreach (var control in pianoPanel.Controls)
        {
            if (control.GetType() == typeof(Button))
            {
                var button = (Button)control;
                if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                {
                    PianoModel_StopNote(button);
                }
            }
        }
    }

    public void PianoModel_PlayNote(Button button)
    {
        button.BackColor = Color.FromArgb(255, 255, 0);
        button.ForeColor = Color.FromArgb(15, 15, 15);

        PianoModel.Play(button.TabIndex);
    }

    public void PianoModel_StopNote(Button button)
    {
        if (button.Name.Length == 1)
        {
            button.BackColor = Color.FromArgb(255, 255, 255);
            button.ForeColor = Color.FromArgb(15, 15, 15);
        }
        else
        {
            button.BackColor = Color.FromArgb(15, 15, 15);
            button.ForeColor = Color.FromArgb(255, 255, 255);
        }

        PianoModel.Stop(button.TabIndex);
    }

    public void UpdateTime()
    {
        var maxTimeSpan = PlaybackRecorder.GetTotalTime();
        var currentTimeSpan = PlaybackRecorder.GetCurrentTime();

        Controls.Find("TimerLabel", true)[0].Text = $"{currentTimeSpan.Minutes:00}:{currentTimeSpan.Seconds:00}/{maxTimeSpan.Minutes:00}:{maxTimeSpan.Seconds:00}";

        var playbackPanel = (Panel)Controls.Find("PlaybackPanel", false)[0];
        playbackPanel.Controls.Find("PlaybackPanelLine", true)[0].Location = new Point((int)currentTimeSpan.TotalMilliseconds / (1000 / Visualizer.PixelsPerSecond) + playbackPanel.AutoScrollPosition.X, 0);
    }

    public void UpdatePlaybackPanel()
    {
        var playbackPanel = Controls.Find("PlaybackPanel", false)[0];
        var headerOffset = playbackPanel.Controls.Find("PlaybackPanelHeader", false)[0].Height;

        var numTracks = PlaybackRecorder.Tracks.Length;
        var numPictureBoxes = playbackPanel.Controls.Count - 2;

        if (numTracks > numPictureBoxes)
        {
            // A track has been added
            var id = numTracks - 1;
            var trackFile = new AudioFileReader(PlaybackRecorder.Tracks[id].Item1.FileName);
            var trackImage = Visualizer.Visualize(trackFile);
            trackFile.Dispose();

            var trackPictureBox = new PictureBox
            {
                Image = trackImage,
                Name = $"Track {id}",
                Tag = PlaybackRecorder.Tracks[id].Item1,
                Size = trackImage.Size,
            };

            var trackCloseButton = new Button
            {
                Size = new Size(ClientSize.Height / 20, ClientSize.Height / 20),
                Text = "X",
                TextAlign = ContentAlignment.MiddleCenter,
            };

            trackCloseButton.Click += TrackCloseButton_Click;

            trackPictureBox.Controls.Add(trackCloseButton);

            trackPictureBox.Location = new Point(0, headerOffset + trackPictureBox.Height * id);

            playbackPanel.Controls.Add(trackPictureBox);
        }
        else if (numTracks < numPictureBoxes)
        {
            // A track has been removed
            bool removed = false;
            for (int i = 0; i < numPictureBoxes; i++)
            {
                var pictureBox = playbackPanel.Controls[i + 2];

                if (pictureBox == null || pictureBox.Tag == null) return;

                if (!removed)
                {
                    if (numTracks == 0 || i == numPictureBoxes - 1 || (AudioFileReader)pictureBox.Tag != PlaybackRecorder.Tracks[i].Item1)
                    {
                        playbackPanel.Controls.Remove(pictureBox);
                        removed = true;
                        numPictureBoxes--;
                        i--;
                    }
                }
                else
                {
                    pictureBox.Name = $"Track {i}";
                    pictureBox.Location = new Point(0, headerOffset + pictureBox.Height * i);
                }
            }
        }

        ResizePlaybackPanel();

        PlaybackRecorder.Stop();
        Timer.Start();
    }

    public void ResizePlaybackPanel()
    {
        var controls = Controls.Find("PlaybackPanel", false);
        if (controls.Length == 0) return;

        Panel playbackPanel = (Panel)controls[0];

        var line = playbackPanel.Controls.Find("PlaybackPanelLine", false)[0];
        if (playbackPanel.HorizontalScroll.Visible)
            line.Height = playbackPanel.Height - SystemInformation.HorizontalScrollBarHeight;
        else
            line.Height = playbackPanel.Height;

        var header = playbackPanel.Controls.Find("PlaybackPanelHeader", false)[0];
        var length = header.Width;

        if (PlaybackRecorder.LongestTrack.Item1 != null)
        {
            var longestTrackLength = (int)PlaybackRecorder.LongestTrack.Item1.TotalTime.TotalSeconds * 10;
            length = longestTrackLength > length ? longestTrackLength : length;
        }

        var pixelsPerTenSeconds = Visualizer.PixelsPerSecond;
        if (header.Controls.Count < length / pixelsPerTenSeconds)
        {
            Control[] labels = [];

            for (int i = pixelsPerTenSeconds + pixelsPerTenSeconds * header.Controls.Count; i < length; i += pixelsPerTenSeconds)
            {
                var time = new TimeSpan(0, 0, i / Visualizer.PixelsPerSecond);

                Control tick;
                if (time.TotalSeconds % 30 == 0)
                {
                    tick = new Label()
                    {
                        BackColor = Color.Transparent,
                        Text = $"{time.Minutes:00}:{time.Seconds:00}",
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(34, header.Height),
                    };

                    tick.Location = new Point(i - (tick.Size.Width / 2), 0);

                    labels = [.. labels, tick];
                }
                else
                {
                    tick = new Panel() { BackColor = Color.FromArgb(15, 15, 15) };

                    if (time.TotalSeconds % 10 == 0)
                    {
                        tick.Location = new Point(i, header.Height / 10);
                        tick.Size = new Size(1, header.Height * 4 / 5);
                    }
                    else if (time.TotalSeconds % 5 == 0)
                    {
                        tick.Location = new Point(i, header.Height / 5);
                        tick.Size = new Size(1, header.Height * 3 / 5);
                    }
                    else
                    {
                        tick.Location = new Point(i, header.Height * 3 / 10);
                        tick.Size = new Size(1, header.Height * 2 / 5);
                    }
                }

                tick.Name = Name = $"{time.Seconds}s";
                tick.MouseUp += PlaybackPanelHeaderTick_MouseUp;

                header.Controls.Add(tick);
            }

            foreach (var label in labels)
            {
                label.BringToFront();
            }
        }
    }
}