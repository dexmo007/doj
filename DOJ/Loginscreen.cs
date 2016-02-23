using System;
using System.Linq;
using System.Windows.Forms;

namespace DOJ
{
    public partial class LoginScreen : Form
    {
        private const char Dot = '•';

        public LoginScreen()
        {
            InitializeComponent();
            usernameBox.Select();
            passwordBox.PasswordChar = Dot;

        }

        private void usernameBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) ConsoleKey.Enter)
            {
                loginButton_Click(null, null);
            }
        }

        private void passwordBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) ConsoleKey.Enter)
            {
                loginButton_Click(null, null);
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            var dsm = new DojSecurityManager();
            var userBase = dsm.ReadUsers();
            var usermap = userBase.ToDictionary(user => user.Username);
            var userName = usernameBox.Text;
            if (usermap.ContainsKey(userName) && passwordBox.Text == usermap[userName].Password)
            {
                usernameBox.Clear();
                passwordBox.Clear();
                usernameBox.Select();
                Hide();
                var mainApp = new MainApp(usermap[userName]);
                mainApp.Show(this);
            }
            else
            {
                MessageBox.Show("Login failed!", "U.S. Department Of Justice", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                passwordBox.Clear();
                passwordBox.Select();
            }
        }
    }
}
