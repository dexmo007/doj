using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DOJ
{
    public partial class MainApp : Form
    {
        private User _loggedUser;
        private List<User> _users;
        private BindingSource _bindSource;
        private bool _unsavedChange = false;
        private bool _logoutFlag;
        private bool _wasFocused;

        private const string GoogleSearch = "http://www.google.com/search?hl=en&q=";

        public MainApp(User user)
        {
            InitializeComponent();
            _loggedUser = user;
            timeLabel.Text = DateTime.Now.ToString("F");
            greetingsLabel.Text = "Welcome " + _loggedUser.Name + "!";
            switch (_loggedUser.Role)
            {
                case User.FullView:
                    // searches only
                    tabControl.TabPages.Remove(adminTab);
                    break;
                case User.Admin:
                    // full  access
                    InitUserAdmin();
                    break;
                default:
                    // no access at all
                    tabControl.TabPages.Remove(adminTab);
                    tabControl.TabPages.Remove(peopleTab);
                    tabControl.TabPages.Remove(fileTab);
                    break;
            }
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            timeLabel.Text = DateTime.Now.ToString("F");
        }

        #region web search
        private void urlTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)ConsoleKey.Enter)
            {
                goButton_Click(null, null);
            }
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            var text = urlTextBox.Text.Trim();
            if (text == "")
            {
                return;
            }
            var url = text;
            if (!char.IsLetter(text[0]) || !text.Contains(".") || text.Contains(" "))
            {
                url = GoogleSearch + text.Replace(" ", "+");
            }
            webBrowser1.Navigate(url);
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            backButton.Enabled = webBrowser1.CanGoBack;
            forwardButton.Enabled = webBrowser1.CanGoForward;
            urlTextBox.Text = webBrowser1.Url.ToString();
            webBrowser1.Select();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private void homeButton_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("www.justice.gov");
        }

        private void urlTextBox_Click(object sender, EventArgs e)
        {
            urlTextBox.ForeColor = Color.Black;
            urlTextBox.Font = new Font(urlTextBox.Font, FontStyle.Regular);
            if (!_wasFocused)
            {
                _wasFocused = true;
                urlTextBox.SelectAll();
            }
        }

        private void urlTextBox_Leave(object sender, EventArgs e)
        {
            _wasFocused = false;
            urlTextBox.ForeColor = Color.Gray;
            urlTextBox.Font = new Font(urlTextBox.Font, FontStyle.Italic);
        }

        #endregion

        #region user admin
        private void InitUserAdmin()
        {
            var dsm = new DojSecurityManager();
            _users = dsm.ReadUsers();
            _bindSource = new BindingSource { DataSource = _users };
            dataGridView.DataSource = _bindSource;
        }

        private void dataGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            _unsavedChange = true;
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var username = e.Row.Cells[0].Value;
            var confirm = MessageBox.Show("Are you sure you want to delete user \"" + username + "\"?",
                "User Administration", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                _unsavedChange = true;
            }
        }

        private void dataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            var firstIndex = e.RowIndex;
            var rows = dataGridView.Rows;
            for (int i = firstIndex; i < dataGridView.RowCount; i++)
            {
                rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            var firstIndex = e.RowIndex;
            var rows = dataGridView.Rows;
            for (int i = firstIndex; i < dataGridView.RowCount; i++)
            {
                rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            var currentRow = dataGridView.CurrentRow;
            if (e.KeyCode == Keys.Delete && currentRow != null)
            {
                var username = currentRow.Cells[0].Value;
                var confirm = MessageBox.Show("Are you sure you want to delete user \"" + username + "\"?",
                    "User Administration", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                {
                    dataGridView.Rows.RemoveAt(currentRow.Index);
                    _unsavedChange = true;
                }
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                var plain = dataGridView[e.ColumnIndex, e.RowIndex].Value;
                dataGridView[e.ColumnIndex, e.RowIndex].Value = plain.ToString().ToUpper();
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            var res = SaveData();
            if (res == -1)
            {
                MessageBox.Show("Userbase saved.", "User Administration", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var username = _users[res].Username;
                MessageBox.Show("User \"" + username + "\" (row " + res + ") is not valid!",
                    "User Administration", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private int SaveData()
        {
            for (int i = 0; i < _users.Count; i++)
            {
                var user = _users[i];
                if (!user.IsValid())
                {
                    return i;
                }
            }
            var dsm = new DojSecurityManager();
            dsm.WriteUsers(_users);
            _unsavedChange = false;
            return -1;
        }
        #endregion

        private void MainApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_unsavedChange)
            {
                var res = MessageBox.Show("You have a few unsaved changes. Do you want to save those?",
                    "User Administration", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (res)
                {
                    case DialogResult.Yes:
                        var dataRes = SaveData();
                        if (dataRes != -1)
                        {
                            var username = _users[dataRes].Username;
                            MessageBox.Show("User \"" + username + "\" (row " + dataRes + ") is not valid!",
                                "User Administration", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                        break;
                    case DialogResult.No:
                        e.Cancel = false;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            if (_logoutFlag == false)
            {
                Application.Exit();
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            _logoutFlag = true;
            Owner.Show();
            Close();
        }
    }
}
