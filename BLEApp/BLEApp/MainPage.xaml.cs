using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BLEApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        BinaryWriter bluetoothWriter;
        CustomStreamReader bluetoothReader;
        BackgroundWorker backgroundWorker;

        public MainPage()
        {
            InitializeComponent();
            Task.Factory.StartNew(StartUp);
        }

        async void StartUp()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status != Xamarin.Essentials.PermissionStatus.Granted)
            {
                var statusRequest = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (statusRequest != Xamarin.Essentials.PermissionStatus.Granted)
                {
                    _ = this.DisplayAlert("Notice", "Need permission.", "Ok");
                }
            }
        }

        private async void Button_Start_Clicked(object sender, EventArgs e)
        {
            CrossBluetooth.Adaptor.DeviceDiscovered += Adaptor_DeviceDiscovered;
            CrossBluetooth.Adaptor.StartDiscovery();
            await Task.Delay(3000);
            CrossBluetooth.Adaptor.CancelDiscovery();
            listView_DeviceList.ItemsSource = CrossBluetooth.Adaptor.GetListOfDiscoveredDevices();
            listView_PairedDeviceList.ItemsSource = CrossBluetooth.Adaptor.GetPairedDevices();

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
        }

        private void Adaptor_DeviceDiscovered(object sender, DeviceDiscoveredEventArgs e)
        {
            Debug.WriteLine($"New Device <{e.BluetoothDevice.Name}> |{e.BluetoothDevice.Address}|");
        }

        private void Button_State_Clicked(object sender, EventArgs e)
        {
            this.DisplayAlert("Notice", CrossBluetooth.Adaptor.IsEnabled().ToString(), "Ok");
        }

        private void Button_Connect_Clicked(object sender, EventArgs e)
        {
            if (CrossBluetooth.Adaptor.ConnectToDeviceByName(Entry_BluetoothDeviceName.Text))
            {
                bluetoothWriter = new BinaryWriter(CrossBluetooth.Adaptor.GetTransmitStream());
                bluetoothReader = new CustomStreamReader(CrossBluetooth.Adaptor.GetReceiveStream());
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void Button_Transmit_Clicked(object sender, EventArgs e)
        {
            bluetoothWriter.Write(Entry_TxData.Text);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                var str = bluetoothReader.ReadTo('>');
                Device.BeginInvokeOnMainThread(() => Editor_RxData.Text += str);
            }
        }
    }
}
