using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.AppService;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ButtonException_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new AppServiceConnection())
            {
                connection.AppServiceName = "com.microsoft.errorcrossdomainmonitor";
                connection.PackageFamilyName = "AppErrorCrossDomainMonitorService-uwp_jcbbyxpds0z24";

                AppServiceConnectionStatus status = await connection.OpenAsync();
                if(status == AppServiceConnectionStatus.Success)
                {
                    var inputs = new ValueSet();
                    inputs.Add("AppName", "SampleApp1");
                    inputs.Add("ActionName", "error");
                    AppServiceResponse response = await connection.SendMessageAsync(inputs);

                    if (response.Status == AppServiceResponseStatus.Success &&
                        response.Message.ContainsKey("led-status"))
                    {
                        var result = response.Message["led-status"].ToString();
                    }
                }
            }
        }

        private async void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new AppServiceConnection())
            {
                connection.AppServiceName = "com.microsoft.errorcrossdomainmonitor";
                connection.PackageFamilyName = "AppErrorCrossDomainMonitorService-uwp_jcbbyxpds0z24";

                AppServiceConnectionStatus status = await connection.OpenAsync();
                if (status == AppServiceConnectionStatus.Success)
                {
                    var inputs = new ValueSet();
                    inputs.Add("AppName", "SampleApp1");
                    inputs.Add("ActionName", "clear");
                    AppServiceResponse response = await connection.SendMessageAsync(inputs);

                    if (response.Status == AppServiceResponseStatus.Success &&
                        response.Message.ContainsKey("led-status"))
                    {
                        var result = response.Message["led-status"].ToString();
                    }
                }
            }
        }
    }
}
