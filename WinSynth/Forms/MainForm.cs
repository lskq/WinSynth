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

    public void TrackCloseButton_Click(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var trackPictureBox = ((Control)sender).Parent;

        if (trackPictureBox == null || trackPictureBox.Tag == null) return;
        PlaybackRecorder.RemoveTrack((int)trackPictureBox.Tag);

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
            RestoreDirectory = true
        };

        if (op.ShowDialog() == DialogResult.OK)
        {
            var filepath = op.FileName;
            PlaybackRecorder.AddTrack(filepath);
            Timer.Start();

            UpdatePlaybackPanel();
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
        PlaybackRecorder.Play();
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

        var maxTimeSpan = PlaybackRecorder.GetTotalTime();
        var currentTimeSpan = PlaybackRecorder.GetCurrentTime();

        Controls.Find("TimerLabel", true)[0].Text = $"{currentTimeSpan.Minutes:00}:{currentTimeSpan.Seconds:00}/{maxTimeSpan.Minutes:00}:{maxTimeSpan.Seconds:00}";

        Controls.Find("PlaybackPanelLine", true)[0].Location = new Point((int)currentTimeSpan.TotalMilliseconds / 100, 0);

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

    public void UpdatePlaybackPanel()
    {
        var playbackPanel = Controls.Find("PlaybackPanel", false)[0];
        var headerOffset = playbackPanel.Controls[0].Height;

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
                Tag = id,
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
                    if ((int)pictureBox.Tag != i || numTracks == 0 || numTracks == i)
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
                    pictureBox.Tag = i;
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

        if (header.Controls.Count < length / 300)
        {
            for (int i = 300 + 300 * header.Controls.Count; i < length; i += 300)
            {
                var time = new TimeSpan(0, 0, i / 10);

                var timeLabel = new Label()
                {
                    Name = $"{time.Seconds}s",
                    Text = $"{time.Minutes:00}:{time.Seconds:00}",
                    TextAlign = ContentAlignment.MiddleCenter,
                };

                timeLabel.Location = new Point(i - (timeLabel.Size.Width / 2), 0);

                header.Controls.Add(timeLabel);
            }
        }
    }
}