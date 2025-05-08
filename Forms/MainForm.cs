using WinSynth.Synth;

namespace WinSynth.Forms;

public class MainForm : Form
{
    public int KeyWidth => PianoPanel.Size.Width / 7;
    public int KeyHeight => PianoPanel.Size.Height;

    public MenuStrip MenuBar = new();
    public Panel ControlPanel = new();
    public Panel PianoPanel = new();

    public Piano PianoModel = new();
    public PlaybackRecorder PlaybackRecorder = new();

    public MainForm()
    {
        InitializeComponent();
    }

    override protected void OnResize(EventArgs e)
    {
        ResizeControlPanel();
        ResizePianoPanel();
    }

    public void InitializeComponent()
    {
        Text = "WinSynth";
        MinimumSize = new Size(800, 600);

        InitializeMainMenu();
        InitializeControlPanel();
        InitializePianoPanel();

        OnResize(EventArgs.Empty);

        PianoPanel.Focus();
    }

    public void ResizeControlPanel()
    {
        ControlPanel.Size = new Size(ClientSize.Width, ClientSize.Height / 16);
        ControlPanel.Location = new Point(0, MenuBar.Height);

        for (int i = 0; i < ControlPanel.Controls.Count; i++)
        {
            var control = ControlPanel.Controls[i];
            int startX = i == 0 ? 0 : ControlPanel.Controls[i - 1].Location.X + ControlPanel.Controls[i - 1].Width;

            if (control.GetType() == typeof(Button))
            {
                control.Size = new Size(ControlPanel.Height, ControlPanel.Height);
                control.Location = new Point(startX, 0);

                var fontSize = control.Height / 2 == 0 ? 1 : control.Height / 2;
                control.Font = new Font("", fontSize);
            }
            else if (control.GetType() == typeof(ComboBox))
            {
                control.Location = new Point(startX, ControlPanel.Height / 4);
            }
            else if (control.GetType() == typeof(Panel))
            {
                int width = ControlPanel.Width / 6;


                control.Size = new Size(width, ControlPanel.Height);
                control.Location = new Point(startX, 0);

                for (int j = 0; j < control.Controls.Count; j++)
                {
                    var subcontrol = control.Controls[j];

                    subcontrol.Size = new Size(control.Width, control.Height / 2);
                    subcontrol.Location = new Point(0, control.Height * j * 2 / 4);

                    var fontSize = control.Height / 4 == 0 ? 1 : control.Height / 4;
                    subcontrol.Font = new Font("", fontSize);
                }
            }
        }
    }

    public void ResizePianoPanel()
    {
        PianoPanel.Size = new Size(ClientSize.Width, ClientSize.Height - ControlPanel.Height - MenuBar.Height);
        PianoPanel.Location = new Point(0, ControlPanel.Location.Y + ControlPanel.Height);

        foreach (var control in PianoPanel.Controls)
        {
            var button = (Button)control;

            if (button.Name.Length == 1)
            {
                button.Width = KeyWidth;
                button.Height = KeyHeight;

                var fontSize = KeyHeight / 10 == 0 ? 1 : KeyHeight / 10;
                button.Font = new Font("", fontSize);
            }
            else
            {
                button.Width = KeyWidth / 2;
                button.Height = KeyHeight * 3 / 4;

                var fontSize = KeyHeight / 20 == 0 ? 1 : KeyHeight / 20;
                button.Font = new Font("", fontSize);
            }

            button.Location = button.Name switch
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
        }
    }

    public void InitializeMainMenu()
    {
        MainMenuStrip = MenuBar;

        var fileDropDownButton = new ToolStripDropDownButton() { Text = "File" };
        var fileDropDown = new ToolStripDropDown();
        var fileNewMIDI = new ToolStripButton() { Text = "New MIDI Track" };
        var fileImport = new ToolStripButton() { Text = "Import Track from ..." };
        var fileExport = new ToolStripButton() { Text = "Export Tracks as ..." };

        fileImport.Click += (o, e) =>
        {
            var op = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "MP3 files (*.mp3)|*.mp3|" +
                         "WAV files (*.wav)|*.wav|" +
                         "AIFF files (*.aiff)|*.aiff" +
                         "All files (*.*)|*.*",
                FilterIndex = 4,
                RestoreDirectory = true
            };

            if (op.ShowDialog() == DialogResult.OK)
            {
                var filepath = op.FileName;
                PlaybackRecorder.AddTrack(filepath);
            }
            ;
        };

        fileDropDownButton.DropDown = fileDropDown;
        fileDropDownButton.ShowDropDownArrow = false;

        fileDropDown.Items.AddRange([fileNewMIDI, fileImport, fileExport]);

        MenuBar.Items.Add(fileDropDownButton);

        Controls.Add(MenuBar);
    }

    public void InitializeControlPanel()
    {
        ControlPanel.BackColor = SystemColors.ControlLight;

        var playButton = new Button { Name = "Play", Text = "⏯" };
        var stopButton = new Button { Name = "Stop", Text = "⏹" };
        var recordButton = new Button { Name = "Record", Text = "⏺" };

        var inactiveColor = Color.Transparent;
        var activeColor = SystemColors.ControlDark;

        Button[] buttons = [playButton, stopButton, recordButton];
        foreach (var button in buttons)
        {
            button.BackColor = inactiveColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        playButton.Click += (o, e) =>
        {
            PlaybackRecorder.Play();

            playButton.BackColor = activeColor;
            stopButton.BackColor = inactiveColor;
            recordButton.BackColor = inactiveColor;

            PianoPanel.Focus();
        };

        stopButton.Click += (o, e) =>
        {
            PlaybackRecorder.Stop();

            playButton.BackColor = inactiveColor;
            stopButton.BackColor = activeColor;
            recordButton.BackColor = inactiveColor;

            PianoPanel.Focus();
        };

        recordButton.Click += (o, e) =>
        {
            //PlaybackRecorder.Record();

            playButton.BackColor = inactiveColor;
            stopButton.BackColor = inactiveColor;
            recordButton.BackColor = activeColor;

            PianoPanel.Focus();
        };

        ControlPanel.Controls.AddRange(buttons);

        var dropdown = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
        };

        dropdown.Items.AddRange
        ([
            "MIDI",
            "Sine",
            "Triangle",
            "Square",
            "Sawtooth",
            "Pink Noise",
            "White Noise"
        ]);

        dropdown.SelectedIndex = 0;

        dropdown.SelectedIndexChanged += (o, e) =>
        {
            PianoModel.Mode = dropdown.SelectedIndex;
            PianoPanel.Focus();
        };

        ControlPanel.Controls.Add(dropdown);

        var gainPanel = new Panel();

        var gainLabel = new Label { Text = "Gain: 0.5", TextAlign = ContentAlignment.MiddleCenter };

        gainPanel.Controls.Add(gainLabel);

        var gainBar = new TrackBar
        {
            Minimum = 0,
            Maximum = 10,
            Value = 5,
            TickFrequency = 1
        };

        gainBar.Scroll += (o, e) =>
        {
            var gain = gainBar.Value / 10d;

            PianoModel.Gain = gain;
            gainLabel.Text = $"Gain: {gain:0.0}";
        };

        gainBar.MouseUp += (o, e) => PianoPanel.Focus();

        gainPanel.Controls.Add(gainBar);

        ControlPanel.Controls.Add(gainPanel);

        var freqPanel = new Panel();

        var freqLabel = new Label { Text = "Octave: 4", TextAlign = ContentAlignment.MiddleCenter };

        freqPanel.Controls.Add(freqLabel);

        var freqBar = new TrackBar
        {
            Minimum = 0,
            Maximum = 8,
            Value = 4,
            TickFrequency = 1
        };

        freqBar.Scroll += (o, e) =>
        {
            PianoModel.Frequency = freqBar.Value;
            freqLabel.Text = $"Octave: {freqBar.Value}";
        };

        freqBar.MouseUp += (o, e) => PianoPanel.Focus();

        freqPanel.Controls.Add(freqBar);

        ControlPanel.Controls.Add(freqPanel);

        Controls.Add(ControlPanel);
    }

    public void InitializePianoPanel()
    {
        void Down(Button button)
        {
            button.BackColor = Color.FromArgb(255, 255, 0);
            button.ForeColor = Color.FromArgb(15, 15, 15);

            PianoModel.Play(button.TabIndex);
        }

        void Up(Button button)
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

        PianoPanel.KeyDown += (s, e) =>
        {
            foreach (var control in PianoPanel.Controls)
            {
                if (control.GetType() == typeof(Button))
                {
                    var button = (Button)control;
                    if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                    {
                        Down(button);
                    }
                }
            }
        };

        PianoPanel.KeyUp += (s, e) =>
        {
            foreach (var control in PianoPanel.Controls)
            {
                if (control.GetType() == typeof(Button))
                {
                    var button = (Button)control;
                    if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                    {
                        Up(button);
                    }
                }
            }
        };

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
            var button = new Button
            {
                TabIndex = i,
                Name = keyData[i].Item1,
                Text = keyData[i].Item2,
                Tag = keyData[i].Item3,
                TextAlign = ContentAlignment.BottomCenter,
            };

            button.MouseDown += (s, e) => Down(button);
            button.MouseUp += (s, e) => Up(button);
            button.GotFocus += (s, e) => PianoPanel.Focus();

            if (button.Name.Length == 1)
            {
                button.BackColor = Color.FromArgb(255, 255, 255);
                button.ForeColor = Color.FromArgb(15, 15, 15);
            }
            else
            {
                button.BackColor = Color.FromArgb(15, 15, 15);
                button.ForeColor = Color.FromArgb(255, 255, 255);
                blackKeys = [.. blackKeys, button];
            }

            PianoPanel.Controls.Add(button);
        }

        foreach (var key in blackKeys)
        {
            key.BringToFront();
        }

        Controls.Add(PianoPanel);
    }
}