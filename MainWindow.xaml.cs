using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace SocketInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Messenger Messenger { get; set; } = new Messenger();

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            Messenger.Register<String>(this, OnStateChanged);
            Address.Text = "127.0.0.1";
            Send.Text = ""
                + "{Set encoding,1251}\n"
                + "{Load task,LINE0001,ADIDAS}\n"
                + "{Set user elements,LINE0001,1,3654299463,2,3604269156,3,71,4,5,5,25.05.21,6,86911497,7,РУ_НАБЕРЕЖНЫЕ ЧЕЛНЫ_ТорговыйКвартал.CS_RB new,8,C40L,9,*1204X*}\n"
                + "{Start task,LINE0001}";
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var addr = IPAddress.Parse(Address.Text);
            var cmds = Send.Text;

            Recv.Text = "";

            Task.Run(() =>
            {
                try
                {
                    using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                    {
                        byte[] bytes = new byte[1024];

                        socket.Connect(new IPEndPoint(addr, 2202));

                        Regex.Split(cmds, "\n").ToList().ForEach(cmd =>
                        {
                            var bytes = Encoding.GetEncoding(1251).GetBytes(cmd);

                            Messenger.Send<String>(cmd);
                            socket.Send(bytes);

                            bytes = new byte[1024];
                            var receive = socket.Receive(bytes);
                            var reply = Encoding.GetEncoding(1251).GetString(bytes, 0, receive);

                            Messenger.Send<String>($"\t--> {reply}");
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    Messenger.Send<String>(ex.Message);
                }
            });
        }

        void OnStateChanged(String s)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Recv.Text += $"{s}\n";
                Recv.ScrollToEnd();
            }));
        }
    }
}
