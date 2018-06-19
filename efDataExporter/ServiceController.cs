using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;


namespace efDataExtporter
{
    partial class ServiceController : ServiceBase
    {
        public ServiceHost serviceHost = null;
        private ProcessStartup _processStartup;

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        public ServiceController()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _processStartup = new ProcessStartup(); // Pass the token into the task.
            Task.Run(() => _processStartup.Init(_cancellationTokenSource));
        }

        protected override void OnStop()
        {
            _processStartup.Shutdown();
        }
    }
}
