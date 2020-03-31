using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System.Threading.Tasks;
using Android.Content;
using System.Collections.Generic;
using System.Threading;
using BLEApp;
using System.Linq;
using System.IO;
using Java.Util;

namespace BLEApp.Droid
{
    [Activity(Label = "BLEApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            //Task.Factory.StartNew(StartBle);

            androidBluetooth = new msBluetoothDroid(BluetoothAdapter.DefaultAdapter, ApplicationContext);
            androidBluetooth.Init();

            _receiver = new BluetoothDeviceReceiver();
            _receiver.SetLocalAndroidBluetooth(androidBluetooth);
            RegisterReceiver(_receiver, new IntentFilter(BluetoothDevice.ActionFound));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private BluetoothDeviceReceiver _receiver;
        public msBluetoothDroid androidBluetooth;

        public class msBluetoothDroid : msBluetooth
        {
            public msBluetoothDroid(BluetoothAdapter bt)
            {
                bluetoothAdapter = bt;
                IsInit = false;
            }

            public msBluetoothDroid(BluetoothAdapter bt, Context context)
            {
                bluetoothAdapter = bt;
                this.context = context;
                IsInit = false;
            }

            public BluetoothAdapter bluetoothAdapter;

            private BluetoothSocket bluetoothTxSocket;
            private BluetoothSocket bluetoothRxSocket;
            private BluetoothServerSocket bluetoothServerSocket;

            public List<BluetoothDevice> deviceList = new List<BluetoothDevice>();

            private Context context;

            //private BinaryWriter binaryWriter;
            //private BinaryReader binaryReader;

            private Stream streamWriter;
            private Stream streamReader;

            private bool IsInit;

            public bool IsEnabled()
            {
                bool result = false;
                if (bluetoothAdapter != null)
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                        result = bluetoothAdapter.IsEnabled;
                    }
                }
                return result;
            }
            public msBluetoothState GetState()
            {
                return (msBluetoothState)bluetoothAdapter.State;
            }
            public msBluetoothScanMode GetScanMode()
            {
                return (msBluetoothScanMode)bluetoothAdapter.ScanMode;
            }

            public bool StartDiscovery()
            {
                bool result = false;
                if (bluetoothAdapter != null)
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                        if(bluetoothAdapter.IsDiscovering == false)
                        {
                            bluetoothAdapter.StartDiscovery();
                            result = true;
                        }
                    }
                }
                return result;
            }
            public bool CancelDiscovery()
            {
                bool result = false;
                if (bluetoothAdapter != null)
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                        if (bluetoothAdapter.IsDiscovering == true)
                        {
                            bluetoothAdapter.CancelDiscovery();
                            result = true;
                        }
                    }
                }
                return result;
            }
            public bool IsDiscovering()
            {
                bool result = false;
                if (bluetoothAdapter != null)
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                        result = bluetoothAdapter.IsDiscovering;
                    }
                }
                return result;
            }
            public List<CrossBluetoothDevice> GetListOfDiscoveredDevices()
            {
                List<CrossBluetoothDevice> res = new List<CrossBluetoothDevice>();
                foreach (var item in deviceList)
                {
                    res.Add(new CrossBluetoothDevice(item.Name,item.Address));
                }
                return res;
            }
            public List<CrossBluetoothDevice> GetPairedDevices()
            {
                List<CrossBluetoothDevice> res = new List<CrossBluetoothDevice>();
                foreach (var item in bluetoothAdapter.BondedDevices)
                {
                    res.Add(new CrossBluetoothDevice(item.Name, item.Address));
                }
                return res;
            }   

            public string GetBluetoothDeviceName()
            {
                return bluetoothAdapter.Name;
            }

            public bool ConnectToDevice(CrossBluetoothDevice crossBluetoothDevice)
            {
                bool result = false;
                if(deviceList.Count > 0)
                {
                    var pairedDeviceSearchCount = bluetoothAdapter.BondedDevices.Where(x => x.Name == crossBluetoothDevice.Name && x.Address == crossBluetoothDevice.Address).Count();
                    var discoveredDeviceSearchCount = deviceList.Where(x => x.Name == crossBluetoothDevice.Name && x.Address == crossBluetoothDevice.Address).Count();
                    if (pairedDeviceSearchCount > 0)
                    {
                        if(pairedDeviceSearchCount == 1)
                        {
                            var bluetoothDevice = bluetoothAdapter.BondedDevices.First(x => x.Name == crossBluetoothDevice.Name && x.Address == crossBluetoothDevice.Address);
                            var bluetoothGatt = bluetoothDevice.ConnectGatt(context, true, new BGattCallback());
                            var bluetoothGattServices = bluetoothGatt.Services;
                            result = true;
                            int i = 0;
                        }
                    }
                    else
                    {
                        if (discoveredDeviceSearchCount > 0)
                        {
                            if (discoveredDeviceSearchCount == 1)
                            {
                                var bluetoothDevice = deviceList.First(x => x.Name == crossBluetoothDevice.Name && x.Address == crossBluetoothDevice.Address);
                                var bluetoothGatt = bluetoothDevice.ConnectGatt(context, true, new BGattCallback());
                                var bluetoothGattServices = bluetoothGatt.Services;
                                result = true;
                                int i = 0;
                            }
                        }
                    }
                    
                }
                return result;
            }
            public bool ConnectToDeviceByName(string crossBluetoothDeviceName)
            {
                bool result = false;
                if (deviceList.Count > 0 || bluetoothAdapter.BondedDevices.Count > 0)
                {
                    var pairedDeviceSearchCount = bluetoothAdapter.BondedDevices.Where(x => x.Name == crossBluetoothDeviceName).Count();
                    var discoveredDeviceSearchCount = deviceList.Where(x => x.Name == crossBluetoothDeviceName).Count();
                    if (pairedDeviceSearchCount > 0)
                    {
                        if (pairedDeviceSearchCount == 1)
                        {
                            var bluetoothDevice = bluetoothAdapter.BondedDevices.First(x => x.Name == crossBluetoothDeviceName);
                            bluetoothTxSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                            bluetoothTxSocket.Connect();
                            streamWriter = bluetoothTxSocket.OutputStream;

                            bluetoothServerSocket = bluetoothAdapter.ListenUsingRfcommWithServiceRecord(context.PackageName, UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                            bluetoothRxSocket = bluetoothServerSocket.Accept();
                            streamReader = bluetoothRxSocket.InputStream;
                            result = true;
                            int i = 0;
                        }
                    }
                    else
                    {
                        if (discoveredDeviceSearchCount > 0)
                        {
                            if (discoveredDeviceSearchCount == 1)
                            {
                                var bluetoothDevice = deviceList.First(x => x.Name == crossBluetoothDeviceName); 
                                 bluetoothTxSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                                bluetoothTxSocket.Connect();
                                streamWriter = bluetoothTxSocket.OutputStream;
                                
                                bluetoothServerSocket = bluetoothAdapter.ListenUsingRfcommWithServiceRecord(context.PackageName, UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                                bluetoothRxSocket = bluetoothServerSocket.Accept();
                                streamReader = bluetoothRxSocket.InputStream;
                                result = true;
                                int i = 0;
                            }
                        }
                    }

                }
                return result;
            }
            public bool ConnectToDeviceByAddress(string crossBluetoothDeviceAddress)
            {
                bool result = false;
                if (deviceList.Count > 0)
                {
                    var pairedDeviceSearchCount = bluetoothAdapter.BondedDevices.Where(x => x.Address == crossBluetoothDeviceAddress).Count();
                    var discoveredDeviceSearchCount = deviceList.Where(x => x.Address == crossBluetoothDeviceAddress).Count();
                    if (pairedDeviceSearchCount > 0)
                    {
                        if (pairedDeviceSearchCount == 1)
                        {
                            var bluetoothDevice = bluetoothAdapter.BondedDevices.First(x => x.Address == crossBluetoothDeviceAddress);
                            var bluetoothGatt = bluetoothDevice.ConnectGatt(context, true, new BGattCallback());
                            var bluetoothGattServices = bluetoothGatt.Services;
                            result = true;
                            int i = 0;
                        }
                    }
                    else
                    {
                        if (discoveredDeviceSearchCount > 0)
                        {
                            if (discoveredDeviceSearchCount == 1)
                            {
                                var bluetoothDevice = deviceList.First(x => x.Address == crossBluetoothDeviceAddress);
                                var bluetoothGatt = bluetoothDevice.ConnectGatt(context, true, new BGattCallback());
                                var bluetoothGattServices = bluetoothGatt.Services;
                                result = true;
                                int i = 0;
                            }
                        }
                    }

                }
                return result;
            }

            public Stream GetTransmitStream()
            {
                //if (binaryWriter != null)
                //    return binaryWriter;
                //else return null;
                if (streamWriter != null)
                    return streamWriter;
                else return null;
            }

            public Stream GetReceiveStream()
            {
                //if (binaryReader != null)
                //    return binaryReader;
                //else return null;
                if (streamReader != null)
                    return streamReader;
                else return null;
            }

            public bool Init()
            {
                bool result = false;
                if (IsInit == false)
                {
                    if (bluetoothAdapter != null)
                    {
                        if (bluetoothAdapter.IsEnabled)
                        {
                            BLEApp.CrossBluetooth.SetPlatformBluetoothAdaptor(this);
                            IsInit = true;
                            result = true;
                        }
                    }
                }
                return result;
            }

            public event DeviceDiscoveredEventHandler DeviceDiscovered;
            protected virtual void OnDeviceDiscovered(CrossBluetoothDevice BluetoothDevice)
            {
                if (DeviceDiscovered != null) DeviceDiscovered(this, new DeviceDiscoveredEventArgs(BluetoothDevice));
            }
            public void AddNewDevice(BluetoothDevice bluetoothDevice)
            {
                deviceList.Add(bluetoothDevice);
                OnDeviceDiscovered(new CrossBluetoothDevice(bluetoothDevice.Name, bluetoothDevice.Address));
            }

            public void SetContext(Context context)
            {
                this.context = context;
            }
        }

        // Implements callback methods for GATT events that the app cares about.  For example,
        // connection change and services discovered.
        class BGattCallback : BluetoothGattCallback
        {
            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                String intentAction;
                if (newState == ProfileState.Connected)
                {

                }
                else if (newState == ProfileState.Disconnected)
                {

                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                if (status == GattStatus.Success)
                {
                    
                }
                else
                {
                    
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
            {
                if (status == GattStatus.Success)
                {
                    
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                
            }
        }

        public class BluetoothDeviceReceiver : BroadcastReceiver
        {
            private msBluetoothDroid localAndroidBluetooth;

            public void SetLocalAndroidBluetooth(msBluetoothDroid localAndroidBluetooth)
            {
                this.localAndroidBluetooth = localAndroidBluetooth;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                var action = intent.Action;

                if (action != BluetoothDevice.ActionFound)
                {
                    return;
                }

                // Get the device
                var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (device.BondState != Bond.Bonded)
                {
                    localAndroidBluetooth.AddNewDevice(device);
                }
            }
        }
    }
}