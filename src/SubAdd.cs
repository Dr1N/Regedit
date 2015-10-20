using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Regedit
{
    public partial class SubAdd : Form
    {
        public string SubName { get; set; }

        private ErrorProvider ep = new ErrorProvider();

        public SubAdd()
        {
            InitializeComponent();
            this.FormClosing += SubAdd_FormClosing;
        }

        private void SubAdd_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (tbName.Text.Trim().Length == 0)
                {
                    ep.SetError(tbName, "Введите имя");
                    e.Cancel = true;
                }
                else
                {
                    ep.SetError(tbName, "");
                    SubName = this.tbName.Text.Trim();
                }
            }
        }
    }
}