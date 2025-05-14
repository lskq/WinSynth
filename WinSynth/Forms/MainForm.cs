using WinSynth.Synth;
using Timer = System.Windows.Forms.Timer;

namespace WinSynth.Forms;

public class MainForm : Form
{
    public Timer Timer = new() { Interval = 1000 };
    public Piano PianoModel = new();
    public PlaybackRecorder PlaybackRecorder = new();

    public MainForm()
    {
        InitializeComponent();
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
        var playbackPanel = Controls.Find("PlaybackPanel", true)[0];

        playbackPanel.Controls.Clear();

        for (int i = 0; i < PlaybackRecorder.Tracks.Length; i++)
        {
            var trackFile = PlaybackRecorder.Tracks[i].Item1; ;
            var trackImage = Visualizer.Visualize(trackFile);

            var trackPictureBox = new PictureBox
            {
                Image = trackImage,
                Name = $"Track {i}",
                Tag = i,
                Size = trackImage.Size,
            };

            var trackCloseButton = new Button
            {
                Size = new Size(ClientSize.Height / 20, ClientSize.Height / 20),
                Text = "X",
                TextAlign = ContentAlignment.MiddleCenter,
            };

            trackCloseButton.Click += (o, e) =>
            {
                PlaybackRecorder.RemoveTrack((int)trackPictureBox.Tag);

                UpdatePlaybackPanel();
            };

            trackPictureBox.Controls.Add(trackCloseButton);

            trackPictureBox.Location = new Point(0, trackPictureBox.Height * i);

            playbackPanel.Controls.Add(trackPictureBox);
        }

        PlaybackRecorder.Stop();
        Timer.Start();
    }

    public void InitializeComponent()
    {
        // Init Form
        Text = "WinSynth";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(854, 480);

        // Init Timer
        Timer.Tick += Timer_Tick;

        // Init Menu Strip
        MainMenuStrip = new MenuStrip();

        var fileDropDownButton = new ToolStripDropDownButton() { Text = "File" };
        var fileDropDown = new ToolStripDropDown();
        var newMIDIButton = new ToolStripButton() { Text = "New MIDI Track" };
        var importButton = new ToolStripButton() { Text = "Import Track from ..." };
        var exportButton = new ToolStripButton() { Text = "Export Tracks as ..." };

        importButton.Click += ImportButton_Click;

        fileDropDownButton.DropDown = fileDropDown;
        fileDropDownButton.ShowDropDownArrow = false;

        fileDropDown.Items.AddRange([newMIDIButton, importButton, exportButton]);

        MainMenuStrip.Items.Add(fileDropDownButton);

        Controls.Add(MainMenuStrip);

        // Init Playback Bar
        var playbackBar = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = SystemColors.ControlLightLight,
            Location = new Point(0, ClientSize.Height / 20),
            Size = new Size(ClientSize.Width, ClientSize.Height / 20),
        };

        Controls.Add(playbackBar);

        var timerLabel = new Label
        {
            Dock = DockStyle.Left,
            Name = "TimerLabel",
            Text = "00:00/00:00",
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(playbackBar.Width / 12, playbackBar.Height),
        };

        var playButton = new Button { Name = "PlayButton", Text = "⏯" };
        var stopButton = new Button { Name = "StopButton", Text = "⏹" };
        var recordButton = new Button { Name = "RecordButton", Text = "⏺" };

        Button[] playbackButtons = [recordButton, stopButton, playButton];
        foreach (var button in playbackButtons)
        {
            button.BackColor = Color.Transparent;
            button.Dock = DockStyle.Left;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Size = new Size(playbackBar.Height, playbackBar.Height);
        }

        playButton.Click += PlayButton_Click;
        stopButton.Click += StopButton_Click;
        recordButton.Click += RecordButton_Click;

        var pianoCombobox = new ComboBox
        {
            Dock = DockStyle.Left,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };

        pianoCombobox.Items.AddRange
        ([
            "MIDI",
            "Sine",
            "Triangle",
            "Square",
            "Sawtooth",
            "Pink Noise",
            "White Noise"
        ]);

        pianoCombobox.SelectedIndex = 0;

        pianoCombobox.SelectedIndexChanged += PianoCombobox_SelectedIndexChanged;

        var gainPanel = new Panel
        {
            Dock = DockStyle.Left,
            Size = new Size(playbackBar.Width / 10, playbackBar.Height),
        };

        var gainLabel = new Label
        {
            Name = "GainLabel",
            Text = "Gain: 0.5",
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 0),
            Size = new Size(gainPanel.Width, gainPanel.Height / 2),
        };

        gainPanel.Controls.Add(gainLabel);

        var gainTrackbar = new TrackBar
        {
            Minimum = 0,
            Maximum = 10,
            Value = 5,
            TickFrequency = 1,
            Location = new Point(0, gainPanel.Height / 2),
            Size = new Size(gainPanel.Width, gainPanel.Height / 2),
        };

        gainTrackbar.Scroll += GainTrackbar_Scroll;

        gainPanel.Controls.Add(gainTrackbar);

        var freqPanel = new Panel
        {
            Dock = DockStyle.Left,
            Size = new Size(playbackBar.Width / 10, playbackBar.Height),
        };

        var freqLabel = new Label
        {
            Name = "FreqLabel",
            Text = "Octave: 4",
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 0),
            Size = new Size(freqPanel.Width, freqPanel.Height / 2),
        };

        freqPanel.Controls.Add(freqLabel);

        var freqTrackbar = new TrackBar
        {
            Minimum = 0,
            Maximum = 8,
            Value = 4,
            TickFrequency = 1,
            Location = new Point(0, freqPanel.Height / 2),
            Size = new Size(freqPanel.Width, freqPanel.Height / 2),
        };

        freqTrackbar.Scroll += FreqTrackbar_Scroll;

        freqPanel.Controls.Add(freqTrackbar);

        playbackBar.Controls.Add(freqPanel);
        playbackBar.Controls.Add(gainPanel);
        playbackBar.Controls.Add(pianoCombobox);
        playbackBar.Controls.AddRange(playbackButtons);
        playbackBar.Controls.Add(timerLabel);

        // Init Piano Panel
        var pianoPanel = new Panel
        {
            Anchor = AnchorStyles.Bottom,
            Location = new Point(ClientSize.Width / 3, ClientSize.Height * 2 / 3),
            Size = new Size(ClientSize.Width / 3, ClientSize.Height / 3)
        };

        pianoPanel.KeyDown += PianoPanel_KeyDown;
        pianoPanel.KeyUp += PianoPanel_KeyUp;

        Controls.Add(pianoPanel);

        // Init Piano Keys
        Button[] blackKeys = [];

        (string, string, Keys)[] keyData =
        [
            // name, text, tag
            ("C", "a", Keys.A),
            ("C#", "w", Keys.W),
            ("D", "s", Keys.S),
            ("D#", "e", Keys.E),
            ("E", "d", Keys.D),
            ("F", "f", Keys.F),
            ("F#", "t", Keys.T),
            ("G", "g", Keys.G),
            ("G#", "y", Keys.Y),
            ("A", "h", Keys.H),
            ("A#", "u", Keys.U),
            ("B", "j", Keys.J)
        ];

        for (int i = 0; i < 12; i++)
        {
            var pianoKey = new Button
            {
                TabIndex = i,
                Name = keyData[i].Item1,
                Text = keyData[i].Item2,
                Tag = keyData[i].Item3,
                TextAlign = ContentAlignment.BottomCenter,
                FlatStyle = FlatStyle.Flat,
            };

            pianoKey.FlatAppearance.BorderSize = 1;

            pianoKey.MouseDown += PianoKey_MouseDown;
            pianoKey.MouseUp += PianoKey_MouseUp;
            pianoKey.GotFocus += PianoKey_GotFocus;

            if (pianoKey.Name.Length == 1)
            {
                pianoKey.BackColor = Color.FromArgb(255, 255, 255);
                pianoKey.ForeColor = Color.FromArgb(15, 15, 15);
            }
            else
            {
                pianoKey.BackColor = Color.FromArgb(15, 15, 15);
                pianoKey.ForeColor = Color.FromArgb(255, 255, 255);
                pianoKey.FlatAppearance.BorderColor = Color.FromArgb(15, 15, 15);
                blackKeys = [.. blackKeys, pianoKey];
            }

            var KeyWidth = pianoPanel.Size.Width / 7;
            var KeyHeight = pianoPanel.Size.Height;

            if (pianoKey.Name.Length == 1)
            {
                pianoKey.Width = KeyWidth;
                pianoKey.Height = KeyHeight;

                var fontSize = KeyHeight / 10 == 0 ? 1 : KeyHeight / 10;
                pianoKey.Font = new Font("", fontSize);
            }
            else
            {
                pianoKey.Width = KeyWidth / 2;
                pianoKey.Height = KeyHeight * 3 / 4;

                var fontSize = KeyHeight / 20 == 0 ? 1 : KeyHeight / 20;
                pianoKey.Font = new Font("", fontSize);
            }

            pianoKey.Location = pianoKey.Name switch
            {
                "C" => new Point(0, 0),
                "C#" => new Point(KeyWidth * 3 / 4, 0),
                "D" => new Point(KeyWidth, 0),
                "D#" => new Point(KeyWidth * 7 / 4, 0),
                "E" => new Point(KeyWidth * 2, 0),
                "F" => new Point(KeyWidth * 3, 0),
                "F#" => new Point(KeyWidth * 15 / 4, 0),
                "G" => new Point(KeyWidth * 4, 0),
                "G#" => new Point(KeyWidth * 19 / 4, 0),
                "A" => new Point(KeyWidth * 5, 0),
                "A#" => new Point(KeyWidth * 23 / 4, 0),
                "B" => new Point(KeyWidth * 6, 0),
                _ => throw new Exception()
            };

            pianoPanel.Controls.Add(pianoKey);
        }

        foreach (var key in blackKeys)
        {
            key.BringToFront();
        }

        // Init Playback Panel
        var PlaybackPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoScroll = true,
            BackColor = SystemColors.ControlLight,
            Name = "PlaybackPanel",
            Location = new Point(0, MainMenuStrip.Height + playbackBar.Height),
            Size = new Size(ClientSize.Width, ClientSize.Height - MainMenuStrip.Height - playbackBar.Height - pianoPanel.Height),
        };

        Controls.Add(PlaybackPanel);
    }
}