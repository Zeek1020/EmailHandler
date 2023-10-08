using MailManagerLibrary;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace EmailHandler.Models
{
    public class MailManagerAsync : MailManager
    {
        public new string Username { get => base.Username; protected set => base.Username = value; }
        public MailManagerAsync(string _username, string _password, string _host, int _port = 993, AuthMethod _login = AuthMethod.Login, bool _ssl = true) : base(_username, _password, _host, _port, _login, _ssl)
        {
        }

        public IEnumerable<MailMessage> getMessagesFromList(IEnumerable<uint> list)
        {
            return Client.GetMessages(list, Options, Seen, MailBox);
        }

    }

    public class TickingTimer : Timer
    {
        public long NumTicks = 0;

        public TickingTimer() : base()
        {
            base.Tick += (object s, EventArgs e) => { NumTicks = NumTicks == long.MaxValue ? 1 : NumTicks + 1; };
        }
    }
}
