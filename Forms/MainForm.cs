using WinSynth.Synth;

namespace WinSynth.Forms;

public class MainForm : Form
{
    public int KeyWidth => PianoPanel.Size.Width / 7;
    public int KeyHeight => PianoPanel.Size.Height;

    public Panel ModPanel = new();
    public Panel PianoPanel = new();
    public Piano PianoModel = new();

    public MainForm()
    {
        InitializeComponent();
    }

    override protected void OnResize(EventArgs e)
    {
        ResizePianoPanel();
        ResizeModPanel();
    }

    public void InitializeComponent()
    {
        Text = "WinSynth";
        MinimumSize = new Size(800, 400);

        InitializePianoPanel();
        InitializeModPanel();

        OnResize(EventArgs.Empty);

        PianoPanel.Focus();
    }

    public void ResizeModPanel()
    {
        ModPanel.Size = new Size(ClientSize.Width, ClientSize.Height / 6);
        ModPanel.Location = new Point(0, 0);

        for (int i = 0; i < ModPanel.Controls.Count; i++)
        {
            var control = ModPanel.Controls[i];

            control.Size = new Size(ModPanel.Width / 5, ModPanel.Height / 5);
            control.Location = new Point(ModPanel.Width * (1 + i * 5) / 10, ModPanel.Height / 3);
        }
    }

    public void ResizePianoPanel()
    {
        PianoPanel.Size = new Size(ClientSize.Width, ClientSize.Height * 5 / 6);
        PianoPanel.Location = new Point(0, ClientSize.Height / 6);

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

    public void InitializeModPanel()
    {
        ModPanel.BackColor = Color.FromArgb(255, 0, 0);

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

        ModPanel.Controls.Add(dropdown);

        var trackbar = new TrackBar
        {
            Minimum = 1,
            Maximum = 10,
            Value = 5,
            TickFrequency = 1
        };

        trackbar.Scroll += (o, e) =>
        {
            PianoModel.Volume = trackbar.Value * 10;
        };

        trackbar.MouseUp += (o, e) => PianoPanel.Focus();

        ModPanel.Controls.Add(trackbar);

        Controls.Add(ModPanel);
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