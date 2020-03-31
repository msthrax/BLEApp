using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BLEApp
{
    public static class CrossBluetooth
    {
        public static msBluetooth Adaptor;
        public static void SetPlatformBluetoothAdaptor(msBluetooth msBluetoothAdaptor)
        {
            Adaptor = msBluetoothAdaptor;
        }
    }

    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public DeviceDiscoveredEventArgs(CrossBluetoothDevice BluetoothDevice)
        {
            this.BluetoothDevice = BluetoothDevice;
        }

        public CrossBluetoothDevice BluetoothDevice { get; }
    }

    public class CrossBluetoothDevice
    {
        public CrossBluetoothDevice(string Name, string Address)
        {
            this.Name = Name;
            this.Address = Address;
        }
        public string Name { get;}
        public string Address { get;}

        public override string ToString()
        {
            return $"{Name} | {Address}";
        }
    }

    //
    // Summary:
    //     Enumerates values returned by several types.
    //
    // Remarks:
    //     Portions of this page are modifications based on work created and shared by the
    //     Android Open Source Project and used according to terms described in the Creative
    //     Commons 2.5 Attribution License.
    public enum msBluetoothState
    {
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Disconnected = 0,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Connecting = 1,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Connected = 2,
        //
        // Summary:
        //     To be added.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Disconnecting = 3,
        //
        // Summary:
        //     Indicates the local Bluetooth adapter is off.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Off = 10,
        //
        // Summary:
        //     Indicates the local Bluetooth adapter is turning on.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        TurningOn = 11,
        //
        // Summary:
        //     Indicates the local Bluetooth adapter is on, and ready for use.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        On = 12,
        //
        // Summary:
        //     Indicates the local Bluetooth adapter is turning off.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        TurningOff = 13
    }

    //
    // Summary:
    //     Enumerates values returned by several types.
    //
    // Remarks:
    //     Portions of this page are modifications based on work created and shared by the
    //     Android Open Source Project and used according to terms described in the Creative
    //     Commons 2.5 Attribution License.
    public enum msBluetoothScanMode
    {
        //
        // Summary:
        //     Indicates that both inquiry scan and page scan are disabled on the local Bluetooth
        //     adapter.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        None = 20,
        //
        // Summary:
        //     Indicates that inquiry scan is disabled, but page scan is enabled on the local
        //     Bluetooth adapter.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        Connectable = 21,
        //
        // Summary:
        //     Indicates that both inquiry scan and page scan are enabled on the local Bluetooth
        //     adapter.
        //
        // Remarks:
        //     Portions of this page are modifications based on work created and shared by the
        //     Android Open Source Project and used according to terms described in the Creative
        //     Commons 2.5 Attribution License.
        ConnectableDiscoverable = 23
    }

    public delegate void DeviceDiscoveredEventHandler(object sender, DeviceDiscoveredEventArgs e);

    public interface msBluetooth
    {
        bool IsEnabled();
        msBluetoothState GetState();
        msBluetoothScanMode GetScanMode();

        bool StartDiscovery();
        bool CancelDiscovery();
        bool IsDiscovering();
        List<CrossBluetoothDevice> GetListOfDiscoveredDevices();
        List<CrossBluetoothDevice> GetPairedDevices();

        bool ConnectToDevice(CrossBluetoothDevice crossBluetoothDevice);
        bool ConnectToDeviceByName(string crossBluetoothDeviceName);
        bool ConnectToDeviceByAddress(string crossBluetoothDeviceAddress);

        //BinaryWriter GetTransmitStream();
        //BinaryReader GetReceiveStream();

        Stream GetTransmitStream();
        Stream GetReceiveStream();

        string GetBluetoothDeviceName();

        event DeviceDiscoveredEventHandler DeviceDiscovered;
    }

    public class CustomStreamReader
    {
        StreamReader streamReader;

        public CustomStreamReader(Stream stream)
        {
            streamReader = new StreamReader(stream);
        }

        public CustomStreamReader(StreamReader streamReader)
        {
            this.streamReader = streamReader;
        }

        public string ReadTo(char Postamble)
        {
            if (streamReader != null)
            {
                int PostambleCounter = 0;
                StringBuilder stringBuilder = new StringBuilder();
                while (true)
                {
                    var recb = streamReader.Read();
                    
                    stringBuilder.Append(ASCIIEncoding.ASCII.GetChars(new byte[] { (byte)recb }));
                    if (recb == Postamble)
                    {
                        if (PostambleCounter == 3 /*4 Postamble needed*/)
                        {
                            return stringBuilder.ToString();
                        }
                        else PostambleCounter++;
                    }
                }
            }
            else
            {
                return String.Empty;
            }
        }

        public void Close()
        {
            if(streamReader != null)
            {
                streamReader.Close();
            }
        }
    }
}
