using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TIA_HMIColorTool
{
    /// <summary>Izbira kategorij za "Export all". Brez TIA klicev - samo UI.</summary>
    sealed class ExportAllDialog : Form
    {
        readonly CheckedListBox _list;

        public ExportAllDialog(string targetDir)
        {
            Text = "Export all - izbira kategorij";
            Size = new Size(620, 470);
            MinimumSize = new Size(520, 400);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            MinimizeBox = MaximizeBox = false;
            ShowInTaskbar = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(layout);

            layout.Controls.Add(new Label
            {
                AutoSize = true, MaximumSize = new Size(580, 0), Padding = new Padding(0, 0, 0, 6),
                Text = "Izvoz v: " + targetDir + "\n" +
                       "Samo branje - projekt se ne prevaja, ne uvaža in ne shrani. " +
                       "Ni izvozljivo prek Openness: HMI alarmi, recepti, uporabniki, master kopije."
            }, 0, 0);

            _list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false };
            foreach (var c in ExportCategories.All)
                _list.Items.Add(new CategoryItem(c), ExportCategories.DefaultOn(c));
            layout.Controls.Add(_list, 0, 1);

            var sel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            sel.Controls.Add(MakeButton("Vse", (s, e) => SetAll(true)));
            sel.Controls.Add(MakeButton("Nič", (s, e) => SetAll(false)));
            layout.Controls.Add(sel, 0, 2);

            var ok = MakeButton("Začni export", null);
            ok.DialogResult = DialogResult.OK;
            var cancel = MakeButton("Prekliči", null);
            cancel.DialogResult = DialogResult.Cancel;
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            bottom.Controls.Add(cancel);
            bottom.Controls.Add(ok);
            layout.Controls.Add(bottom, 0, 3);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        public List<ExportCategory> SelectedCategories
        {
            get { return _list.CheckedItems.Cast<CategoryItem>().Select(i => i.Category).ToList(); }
        }

        void SetAll(bool value)
        {
            for (int i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, value);
        }

        static Button MakeButton(string text, System.EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 28 };
            if (click != null) b.Click += click;
            return b;
        }

        sealed class CategoryItem
        {
            public readonly ExportCategory Category;
            public CategoryItem(ExportCategory c) { Category = c; }
            public override string ToString() { return ExportCategories.Label(Category); }
        }
    }
}
