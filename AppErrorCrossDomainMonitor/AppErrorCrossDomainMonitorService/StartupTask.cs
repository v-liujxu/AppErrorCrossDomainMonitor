using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace AppErrorCrossDomainMonitorService
{
    public sealed class AppErrorCrossDomainMonitorTask : IBackgroundTask
    {
        IBackgroundTaskInstance _taskInstance = null;
        private BackgroundTaskDeferral serviceDeferral;
        private AppServiceConnection connection;
        private GpioController gpioController;
        private static GpioPin ledPin = null;
        private GpioOpenStatus pinStatus = GpioOpenStatus.UnknownError;
        private const int LED_PIN = 4;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            //Take a service deferral so the service isn't terminated
            try
            {
                serviceDeferral = taskInstance.GetDeferral();

                taskInstance.Canceled += OnTaskCanceled;
                _taskInstance = taskInstance;

                pinStatus = GpioOpenStatus.UnknownError;
                gpioController = GpioController.GetDefault();

                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                if(details != null)
                {
                    connection = details.AppServiceConnection;

                    //Listen for incoming app service requests
                    connection.RequestReceived += OnRequestReceived;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                //Complete the service deferral
                serviceDeferral.Complete();
                serviceDeferral = null;
            }

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();

            try
            {
                var input = args.Request.Message;
                string appName = input["AppName"].ToString();
                string actionName = input["ActionName"].ToString();

                //Create the response
                var result = new ValueSet();

                if (gpioController != null)
                {
                    if(ledPin == null)
                    {
                        gpioController.TryOpenPin(LED_PIN, GpioSharingMode.Exclusive, out ledPin, out pinStatus);
                        if (ledPin != null)
                        {
                            ledPin.SetDriveMode(GpioPinDriveMode.Output);
                        }
                    }                    
                }

                if (actionName == "error")
                {
                    //Open LED
                    ledPin.Write(GpioPinValue.High);
                    result.Add("led-status", "ON");
                }

                if (actionName == "clear")
                {
                    //Close LED
                    ledPin.Write(GpioPinValue.Low);
                    result.Add("led-status", "OFF");
                }

                //Send the response
                await args.Request.SendResponseAsync(result);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {                
                //Complete the message deferral so the platform knows we're done responding
                messageDeferral.Complete();
            }
        }
    }
}
