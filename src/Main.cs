using Microsoft.Win32;
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
    public partial class Main : Form
    {
        private Dictionary<string, RegistryKey> rootRegistry = new Dictionary<string,RegistryKey>()
        {
            { "HKEY_CLASSES_ROOT", Registry.ClassesRoot },
            { "HKEY_CURRENT_USER", Registry.CurrentUser },
            { "HKEY_LOCAL_MACHINE", Registry.LocalMachine },
            { "HKEY_USERS", Registry.Users },
            { "HKEY_CURRENT_CONFIG", Registry.CurrentConfig }
        };

        public Main()
        {
            InitializeComponent();

            this.tvRegistry.BeforeExpand += tvRegistry_BeforeExpand;
            this.tvRegistry.AfterSelect += tvRegistry_AfterSelect;

            ColumnHeader ch = new ColumnHeader();
            ch.Text = "Имя";
            this.lvRegistry.Columns.Add(ch);

            ch = new ColumnHeader();
            ch.Text = "Тип";
            this.lvRegistry.Columns.Add(ch);

            ch = new ColumnHeader();
            ch.Text = "Значение";
            this.lvRegistry.Columns.Add(ch);

            for (int i = 0; i < this.lvRegistry.Columns.Count; i++)
            {
                lvRegistry.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }

            this.FillRoot();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region TREE

        private void tvRegistry_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = e.Node;
            RegistryKey rKey = t.Tag as RegistryKey;
            if (rKey == null) { return; }

            this.UpdateList(rKey);
        }

        private void tvRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode t = e.Node;

            if (t.Text == "REGISTRY") { return; }

            t.Nodes.Clear();
            RegistryKey rKey = t.Tag as RegistryKey;
            if (rKey == null) { return; }

            string[] names = rKey.GetSubKeyNames();

            foreach (string name in names)
            {
                try
                {
                    RegistryKey tmpKey = rKey.OpenSubKey(name, true);
                    TreeNode tmpNode = new TreeNode(name, 0, 1);
                    tmpNode.Tag = tmpKey;
                    tmpNode.Nodes.Add("^_^");
                    t.Nodes.Add(tmpNode);
                }
                catch { }
            }
        }

        private void tvRegistry_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) { return; }

            TreeNode tn = this.tvRegistry.GetNodeAt(e.Location);
            if (tn != null)
            {
                this.tvRegistry.SelectedNode = tn;
            }
        }

        private void cmnAddTree_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.tvRegistry.SelectedNode;
            if (selectedNode == null || selectedNode.Parent == null) { return; }

            RegistryKey currentRegKey = this.tvRegistry.SelectedNode.Tag as RegistryKey;

            if (currentRegKey == null || rootRegistry.Values.Contains(currentRegKey)) { return; }

            SubAdd addForm = new SubAdd();
            if (addForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    currentRegKey.CreateSubKey(addForm.SubName);
                    if (!selectedNode.IsExpanded) { selectedNode.Expand(); }
                    this.UpdateNode(selectedNode, currentRegKey);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void cmnDeleteTree_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.tvRegistry.SelectedNode;
            TreeNode parentNode = selectedNode.Parent;
            if (selectedNode == null || parentNode == null || parentNode.Parent == null) { return; }

            RegistryKey currentRegKey = parentNode.Tag as RegistryKey;

            if (currentRegKey == null) { return; }

            try
            {
                currentRegKey.DeleteSubKey(selectedNode.Text, true);
                this.UpdateNode(parentNode, currentRegKey);
                this.UpdateList(currentRegKey);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateNode(TreeNode selectedNode, RegistryKey currentRegKey)
        {
            selectedNode.Nodes.Clear();
            string[] names = currentRegKey.GetSubKeyNames();
            foreach (string name in names)
            {
                try
                {
                    RegistryKey tmpKey = currentRegKey.OpenSubKey(name, true);
                    TreeNode tmpNode = new TreeNode(name);
                    tmpNode.Tag = tmpKey;
                    tmpNode.Nodes.Add("^_^");
                    selectedNode.Nodes.Add(tmpNode);
                }
                catch { }
            }
        }

        #endregion

        #region LISTVIEW

        private void cmnAddList_Click(object sender, EventArgs e)
        {
            TreeNode currentNode = this.tvRegistry.SelectedNode;
            if (currentNode == null || currentNode.Parent == null) { return; }

            RegistryKey currentRegKey = currentNode.Tag as RegistryKey;
            if (currentRegKey == null || this.rootRegistry.Values.Contains(currentRegKey)) { return; }

            ValueForm form = new ValueForm();

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    currentRegKey.SetValue(form.ValueName, form.ValueValue, form.ValueType);
                    this.UpdateList(currentRegKey);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void cmnDeleteList_Click(object sender, EventArgs e)
        {
            if (this.lvRegistry.SelectedIndices.Count == 0) { return; }

            RegistryKey currentRegKey = this.tvRegistry.SelectedNode.Tag as RegistryKey;
            if (currentRegKey == null) { return; }

            string name = lvRegistry.SelectedItems[0].Text;
            try
            {
                currentRegKey.DeleteValue(name);
                this.UpdateList(currentRegKey);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmnEditList_Click(object sender, EventArgs e)
        {
            if (this.lvRegistry.SelectedIndices.Count == 0) { return; }

            RegistryKey currentRegKey = this.tvRegistry.SelectedNode.Tag as RegistryKey;
            if (currentRegKey == null) { return; }

            string name = lvRegistry.SelectedItems[0].Text;
            RegistryValueKind type = currentRegKey.GetValueKind(name);
            object value = currentRegKey.GetValue(name);

            ValueForm form = new ValueForm(name, type, value);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                currentRegKey.SetValue(form.ValueName, form.ValueValue, form.ValueType);
                this.UpdateList(currentRegKey);
            }
        }

        private void UpdateList(RegistryKey rKey)
        {
            this.lvRegistry.Items.Clear();
            string[] names = rKey.GetValueNames();
            foreach (string name in names)
            {
                ListViewItem lvi = new ListViewItem(name);
                RegistryValueKind rType = rKey.GetValueKind(name);
                lvi.Tag = rType;

                string value = null;
                if (rType == RegistryValueKind.Binary)
                {
                    byte[] valueObj = rKey.GetValue(name) as byte[];
                    foreach (byte item in valueObj)
                    {
                        value += item.ToString("X2") + " ";
                    }
                }
                else
                {
                    value = rKey.GetValue(name).ToString();
                }

                string type = rType.ToString();
                lvi.SubItems.Add(type);
                lvi.SubItems.Add(value.Trim());
                this.lvRegistry.Items.Add(lvi);
            }

            for (int i = 0; i < this.lvRegistry.Columns.Count; i++)
            {
                if (this.lvRegistry.Items.Count > 0)
                {
                    this.lvRegistry.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
                else
                {
                    this.lvRegistry.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
        }
        
        #endregion

        private void FillRoot()
        {
            TreeNode root = new TreeNode("REGISTRY");
            foreach (KeyValuePair<string, RegistryKey> item in rootRegistry)
            {
                TreeNode t = new TreeNode(item.Key, 0, 1);
                t.Tag = item.Value;
                t.Nodes.Add("^_^");
                root.Nodes.Add(t);
            }
            this.tvRegistry.Nodes.Add(root);
        }
    }
}