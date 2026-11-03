using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;

namespace TIA_ToolKit
{
    /// <summary>
    /// Prijava za zaščiten TIA projekt (UMAC). Geslo se vrne kot SecureString in se nikjer ne shrani.
    /// </summary>
    sealed class LoginDialog : Form
    {
        readonly TextBox _tbUser, _tbPassword;
        readonly RadioButton _rbProject, _rbGlobal;

        LoginDialog(string projectName)
        {
            Text = "Prijava v zaščiten projekt";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);

            var t = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Fill };
            t.Controls.Add(new Label { Text = "Projekt:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            t.Controls.Add(new Label { Text = projectName, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Left }, 1, 0);

            t.Controls.Add(new Label { Text = "Uporabnik:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _tbUser = new TextBox { Width = 240 };
            t.Controls.Add(_tbUser, 1, 1);

            t.Controls.Add(new Label { Text = "Geslo:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _tbPassword = new TextBox { Width = 240, UseSystemPasswordChar = true };
            t.Controls.Add(_tbPassword, 1, 2);

            t.Controls.Add(new Label { Text = "Vrsta uporabnika:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            var types = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            _rbProject = new RadioButton { Text = "Projektni", Checked = true, AutoSize = true };
            _rbGlobal = new RadioButton { Text = "Globalni (UMC / Windows)", AutoSize = true };
            types.Controls.Add(_rbProject);
            types.Controls.Add(_rbGlobal);
            t.Controls.Add(types, 1, 3);

            var note = new Label
            {
                Text = "Geslo se preda neposredno TIA Portalu in se ne shrani.\nProjekt mora biti v TIA Portalu zaprt (TIA naj ostane odprt).",
                AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 8)
            };
            t.Controls.Add(note, 0, 4);
            t.SetColumnSpan(note, 2);

            var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            var ok = new Button { Text = "Odpri", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Prekliči", DialogResult = DialogResult.Cancel, AutoSize = true };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            t.Controls.Add(buttons, 0, 5);
            t.SetColumnSpan(buttons, 2);

            Controls.Add(t);
            AcceptButton = ok;
            CancelButton = cancel;
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_tbUser.Text)) { DialogResult = DialogResult.None; _tbUser.Focus(); }
            };
        }

        public static bool Ask(IWin32Window owner, string projectName, out string user, out SecureString password, out bool globalUser)
        {
            user = null; password = null; globalUser = false;
            using (var d = new LoginDialog(projectName))
            {
                if (d.ShowDialog(owner) != DialogResult.OK) return false;
                user = d._tbUser.Text.Trim();
                globalUser = d._rbGlobal.Checked;
                password = new SecureString();
                foreach (char c in d._tbPassword.Text) password.AppendChar(c);
                password.MakeReadOnly();
                d._tbPassword.Clear();
                return true;
            }
        }
    }
}
