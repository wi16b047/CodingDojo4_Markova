using CodingDojo4_DataHandler;
using CodingDojo4_Markova.Communication;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodingDojo4_Markova.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Server server;
        private const int port = 10100;
        private const string ip = "127.0.0.1";
        private bool isConnected = false;
        private DataHandler dHandler;

        public RelayCommand StartBtnClickCmd { get; set; }
        public RelayCommand StopBtnClickCmd { get; set; }
        public RelayCommand DropClientBtnClickCmd { get; set; }
        public RelayCommand SaveToLogBtnClickCmd { get; set; }
        public ObservableCollection<string> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public string SelectedUser { get; set; }

        #region Bonus Task Properties
        public ObservableCollection<string> LogFiles
        {
            get
            {
                return new ObservableCollection<string>(dHandler.QueryFilesFromFolder());
            }
        }

        public ObservableCollection<string> LogMessages { get; set; }
        public string SelectedLogFile { get; set; }

        public RelayCommand OpenLogFileBtnClickCmd { get; set; }
        public RelayCommand DropLogFileBtnClickCmd { get; set; }
        #endregion 

        public int NoOfReceivedMessages
        {
            get
            {
                return Messages.Count;
            }
        }


        public MainViewModel()
        {

            dHandler = new DataHandler();
            Messages = new ObservableCollection<string>();
            Users = new ObservableCollection<string>();
            LogMessages = new ObservableCollection<string>();

            //set command for start button
            StartBtnClickCmd = new RelayCommand(
                () =>
                {
                    server = new Server(ip, port, UpdateGuiWithNewMessage);
                    server.StartAccepting();
                    isConnected = true;
                },
                () => { return !isConnected; });

            //set command for stop button
            StopBtnClickCmd = new RelayCommand(
                //action for execute
                () =>
                {
                    server.StopAccepting();
                    isConnected = false;
                },
                //can execute
                () => { return isConnected; });

            //init Command for Drop button with CanExecute statement
            DropClientBtnClickCmd = new RelayCommand(() =>
            {
                server.DisconnectSpecificClient(SelectedUser);
                Users.Remove(SelectedUser); // remove from GUI listbox
            },
                () => { return (SelectedUser != null); });

            //init Command for SaveToLogFIle button with CanExecute statement
            SaveToLogBtnClickCmd = new RelayCommand(
                () =>
                {
                    dHandler.Save(Messages.ToList());
                    RaisePropertyChanged("LogFiles"); //to update the list in the log section
                },
                () => { return Messages.Count >= 1; }
                );
            //init Command for OpenLogFile button with CanExecute statement
            OpenLogFileBtnClickCmd = new RelayCommand(
                () =>
                {
                    LogMessages = new ObservableCollection<string>(dHandler.Load(SelectedLogFile));
                    RaisePropertyChanged("LogMessages");
                },
                () => { return SelectedLogFile != null; }
                );
            //init Command for Drop Log files button with CanExecute statement
            DropLogFileBtnClickCmd = new RelayCommand(
                () =>
                {
                    dHandler.Delete(SelectedLogFile);
                    RaisePropertyChanged("LogFiles"); //to update the list in the log section},
                },
                () => { return SelectedLogFile != null; });


        }

        public void UpdateGuiWithNewMessage(string message)
        {
            //switch thread to GUI thread to write to GUI
            App.Current.Dispatcher.Invoke(() =>
            {
                string name = message.Split(':')[0];
                if (!Users.Contains(name))
                {//not in list => add it
                    Users.Add(name);
                }
                if (message.Contains("@quit"))
                {
                    server.DisconnectSpecificClient(name);
                }
                //write message
                Messages.Add(message);
                //do this to inform the GUI about the update of the received message counter!
                RaisePropertyChanged("NoOfReceivedMessages");
            });



        }
    }
}