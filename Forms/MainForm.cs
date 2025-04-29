namespace WinSynth.Forms;

public class MainForm : Form
{
    public int KeyWidth => ClientSize.Width / 7;
    public int KeyHeight => ClientSize.Height;
    public Panel Piano = new();

    public MainForm()
    {
        InitializeComponent();
    }

    override protected void OnResize(EventArgs e)
    {
        ResizePiano();
    }

    public void InitializeComponent()
    {
        Text = "WinSynth";
        MinimumSize = new Size(800, 400);

        InitializePiano();

        OnResize(EventArgs.Empty);

        Piano.Focus();
    }

    public void ResizePiano()
    {
        Piano.Size = new Size(ClientSize.Width, ClientSize.Height);

        foreach (var control in Piano.Controls)
        {
            var button = (Button)control;

            if (button.Name.Length == 1)
            {
                button.Width = KeyWidth;
                button.Height = KeyHeight;
                button.Font = new Font("", KeyHeight / 10);
            }
            else
            {
                button.Width = KeyWidth / 2;
                button.Height = KeyHeight * 3 / 4;
                button.Font = new Font("", KeyHeight / 20);
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

    public void InitializePiano()
    {
        Piano.Dock = DockStyle.Fill;

        void Play(Button button)
        {
            button.BackColor = Color.FromArgb(255, 255, 0);
            button.ForeColor = Color.FromArgb(15, 15, 15);
        }

        void Stop(Button button)
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
        }

        Piano.KeyDown += (s, e) =>
        {
            foreach (var control in Piano.Controls)
            {
                if (control.GetType() == typeof(Button))
                {
                    var button = (Button)control;
                    if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                    {
                        Play(button);
                    }
                }
            }
        };

        Piano.KeyUp += (s, e) =>
        {
            foreach (var control in Piano.Controls)
            {
                if (control.GetType() == typeof(Button))
                {
                    var button = (Button)control;

                    if (button.Tag != null && e.KeyCode == (Keys)button.Tag)
                    {
                        Stop(button);
                    }
                }
            }
        };

        Button[] blackKeys = [];
        string[] name = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        string[] text = ["a", "w", "s", "e", "d", "f", "t", "g", "y", "h", "u", "j"];
        Keys[] keys = [Keys.A, Keys.W, Keys.S, Keys.E, Keys.D, Keys.F, Keys.T, Keys.G, Keys.Y, Keys.H, Keys.U, Keys.J];

        for (int i = 0; i < 12; i++)
        {
            var button = new Button
            {
                TabIndex = i,
                Name = name[i],
                Text = text[i],
                Tag = keys[i],
                TextAlign = ContentAlignment.BottomCenter,
            };

            button.MouseDown += (s, e) => Play(button);
            button.MouseUp += (s, e) => Stop(button);
            button.GotFocus += (s, e) => Piano.Focus();

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

            Piano.Controls.Add(button);
        }

        foreach (var key in blackKeys)
        {
            key.BringToFront();
        }

        Controls.Add(Piano);
    }
}