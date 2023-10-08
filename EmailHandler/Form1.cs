using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmailHandler
{
    public partial class Form1 : Form
    {

        public string username { get; set; }
        public string password { get; set; }
        public string host { get; set; }
        public bool valid { get; set; }

        protected void globalKeyPressHandler(object sender,  KeyPressEventArgs e ) {
            switch((Keys)e.KeyChar)
            {
                case Keys.Enter:
                    this.btnLogin.PerformClick();
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }

        public Form1() {
            InitializeComponent();
            this.KeyPress += globalKeyPressHandler;
            foreach( Control control in this.Controls ){
                control.KeyPress += globalKeyPressHandler;
            }
        }

        protected bool isUsernameValid(  string username ){
            bool allowed = true;

            Regex r = new Regex(@".+@.+\..+", RegexOptions.IgnoreCase);

            if( string.IsNullOrEmpty( username ))
            {
                allowed = false;
            }else if( !r.IsMatch( username ) )
            {
                allowed = false;
            }
            return allowed;
        }

        protected bool isPasswordValid( string password) {
            bool allowed = true;
            if( string.IsNullOrEmpty( password ))
            {
                allowed = false;
            }
            return allowed;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string _username = this.txtUsername.Text.Trim();
            string _password = this.txtPassword.Text.Trim();
            string _host = this.comboBox1.Text.Trim();

            bool allowed = true;

            if( !isUsernameValid( _username ))
            {
                allowed = false;
                this.txtUsername.BackColor = Color.Red;
            } else
            {
                this.txtUsername.BackColor = Color.White;
            }
            if( !isPasswordValid(_password))
            {
                allowed = false;
                this.txtPassword.BackColor = Color.Red;
            } else
            {
                this.txtPassword.BackColor = Color.White;
            }




            if (allowed){
                this.valid = true;
                this.username = _username;
                this.password = _password;
                this.host = _host;
                this.Close();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
