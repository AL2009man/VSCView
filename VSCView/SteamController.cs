﻿using HidLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VSCView
{
    public class SteamController
    {
        private const int VendorId = 0x28DE; // 10462
        private const int ProductIdWireless = 0x1142; // 4418;
        private const int ProductIdWired = 0x1102; // 4354

        SteamControllerState State = new SteamControllerState();
        SteamControllerState OldState = new SteamControllerState();
        public bool SensorsEnabled;
        private HidDevice _device;

        int stateUsageLock = 0, internalStateLock = 0;

        #region DATA STRUCTS
        public enum VSCEventType
        {
            CONTROL_UPDATE = 0x01,
            CONNECTION_DETAIL = 0x03,
            BATTERY_UPDATE = 0x04,
        }

        public enum ConnectionState
        {
            DISCONNECT = 0x01,
            CONNECT = 0x02,
            PAIRING = 0x03,
        }

        public class SteamControllerButtons : ICloneable
        {
            public bool A { get; set; }
            public bool B { get; set; }
            public bool X { get; set; }
            public bool Y { get; set; }

            public bool LeftBumper { get; set; }
            public bool LeftTrigger { get; set; }

            public bool RightBumper { get; set; }
            public bool RightTrigger { get; set; }

            public bool LeftGrip { get; set; }
            public bool RightGrip { get; set; }

            public bool Start { get; set; }
            public bool Steam { get; set; }
            public bool Select { get; set; }

            public bool Down { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Up { get; set; }

            public bool StickClick { get; set; }
            public bool LeftPadTouch { get; set; }
            public bool LeftPadClick { get; set; }
            public bool RightPadTouch { get; set; }
            public bool RightPadClick { get; set; }

            public virtual object Clone()
            {
                SteamControllerButtons buttons = (SteamControllerButtons)base.MemberwiseClone();

                buttons.A = A;
                buttons.B = B;
                buttons.X = X;
                buttons.Y = Y;

                buttons.LeftBumper = LeftBumper;
                buttons.LeftTrigger = LeftTrigger;

                buttons.RightBumper = RightBumper;
                buttons.RightTrigger = RightTrigger;

                buttons.LeftGrip = LeftGrip;
                buttons.RightGrip = RightGrip;

                buttons.Start = Start;
                buttons.Steam = Steam;
                buttons.Select = Select;

                buttons.Down = Down;
                buttons.Left = Left;
                buttons.Right = Right;
                buttons.Up = Up;

                buttons.StickClick = StickClick;
                buttons.LeftPadTouch = LeftPadTouch;
                buttons.LeftPadClick = LeftPadClick;
                buttons.RightPadTouch = RightPadTouch;
                buttons.RightPadClick = RightPadClick;

                return buttons;
            }
        }

        public class SteamControllerState
        {
            public SteamControllerButtons Buttons { get; set; }

            public byte LeftTrigger { get; set; }
            public byte RightTrigger { get; set; }

            public Int32 LeftStickX { get; set; }
            public Int32 LeftStickY { get; set; }
            public Int32 LeftPadX { get; set; }
            public Int32 LeftPadY { get; set; }
            public Int32 RightPadX { get; set; }
            public Int32 RightPadY { get; set; }

            public Int16 AccelerometerX { get; set; }
            public Int16 AccelerometerY { get; set; }
            public Int16 AccelerometerZ { get; set; }
            public Int16 AngularVelocityX { get; set; }
            public Int16 AngularVelocityY { get; set; }
            public Int16 AngularVelocityZ { get; set; }
            public Int16 OrientationW { get; set; }
            public Int16 OrientationX { get; set; }
            public Int16 OrientationY { get; set; }
            public Int16 OrientationZ { get; set; }

            public bool DataStuck { get; set; }
        }

        public SteamControllerState GetState()
        {
            if (0 == Interlocked.Exchange(ref stateUsageLock, 1))
            {
                SteamControllerState newState = new SteamControllerState();
                newState.Buttons = (SteamControllerButtons)Buttons.Clone();

                newState.LeftTrigger = LeftTrigger;
                newState.RightTrigger = RightTrigger;

                newState.LeftStickX = LeftStickX;
                newState.LeftStickY = LeftStickY;
                newState.LeftPadX = LeftPadX;
                newState.LeftPadY = LeftPadY;
                newState.RightPadX = RightPadX;
                newState.RightPadY = RightPadY;

                newState.AccelerometerX = AccelerometerX;
                newState.AccelerometerY = AccelerometerY;
                newState.AccelerometerZ = AccelerometerZ;
                newState.AngularVelocityX = AngularVelocityX;
                newState.AngularVelocityY = AngularVelocityY;
                newState.AngularVelocityZ = AngularVelocityZ;
                newState.OrientationW = OrientationW;
                newState.OrientationX = OrientationX;
                newState.OrientationY = OrientationY;
                newState.OrientationZ = OrientationZ;

                newState.DataStuck = DataStuck;

                State = newState;
                Interlocked.Exchange(ref stateUsageLock, 0);
            }
            return State;
        }
        #endregion

        SteamControllerButtons Buttons = new SteamControllerButtons();

        byte LeftTrigger { get; set; }
        byte RightTrigger { get; set; }

        Int32 LeftStickX { get; set; }
        Int32 LeftStickY { get; set; }
        Int32 LeftPadX { get; set; }
        Int32 LeftPadY { get; set; }
        Int32 RightPadX { get; set; }
        Int32 RightPadY { get; set; }

        Int16 AccelerometerX { get; set; }
        Int16 AccelerometerY { get; set; }
        Int16 AccelerometerZ { get; set; }
        Int16 AngularVelocityX { get; set; }
        Int16 AngularVelocityY { get; set; }
        Int16 AngularVelocityZ { get; set; }
        Int16 OrientationW { get; set; }
        Int16 OrientationX { get; set; }
        Int16 OrientationY { get; set; }
        Int16 OrientationZ { get; set; }

        bool DataStuck { get; set; }

        bool Initalized;

        public delegate void StateUpdatedEventHandler(object sender, SteamControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(SteamControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        public SteamController(HidDevice device)
        {
            _device = device;

            Initalized = false;
        }

        public void Initalize()
        {
            if (Initalized) return;

            //_device.OpenDevice();

            //_device.Inserted += DeviceAttachedHandler;
            //_device.Removed += DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = true;

            Initalized = true;

            //_attached = _device.IsConnected;

            _device.ReadReport(OnReport);
        }

        public void DeInitalize()
        {
            if (!Initalized) return;

            //_device.Inserted -= DeviceAttachedHandler;
            //_device.Removed -= DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = false;

            Initalized = false;
            //ResetGyroSensors();
            _device.CloseDevice();
        }

        public bool EnableGyroSensors()
        {
            if (!SensorsEnabled)
            {
                byte[] reportData = new byte[64];
                reportData[1] = 0x87; // 0x87 = register write command
                reportData[2] = 0x03; // 0x03 = length of data to be written (data + 1 empty bit)
                reportData[3] = 0x30; // 0x30 = register of Gyro data
                reportData[4] = 0x10 | 0x08 | 0x04; // enable raw Gyro, raw Accel, and Quaternion data
                Debug.WriteLine("Attempting to reenable MPU accelerometer sensor");
                var result = _device.WriteFeatureData(reportData);
                SensorsEnabled = true;
                return result;
            }
            return false;
        }

        public bool ResetGyroSensors()
        {
            if (SensorsEnabled)
            {
                byte[] reportData = new byte[64];
                reportData[1] = 0x87; // 0x87 = register write command
                reportData[2] = 0x03; // 0x03 = length of data to be written (data + 1 empty bit)
                reportData[3] = 0x30; // 0x30 = register of Gyro data
                reportData[4] = 0x10 | 0x04; // enable raw Gyro, raw Accel, and Quaternion data
                Debug.WriteLine("Attempting to restore default sensor state");
                var result = _device.WriteFeatureData(reportData);
                SensorsEnabled = false;
                return result;
            }
            return false;
        }

        public bool CheckSensorDataStuck()
        {
            return (OldState != null &&
                State.AccelerometerX == 0 &&
                State.AccelerometerY == 0 &&
                State.AccelerometerZ == 0 ||
                State.AccelerometerX == OldState.AccelerometerX &&
                State.AccelerometerY == OldState.AccelerometerY &&
                State.AccelerometerZ == OldState.AccelerometerZ ||
                State.AngularVelocityX == OldState.AngularVelocityX &&
                State.AngularVelocityY == OldState.AngularVelocityY &&
                State.AngularVelocityZ == OldState.AngularVelocityZ
            );
        }

        public string GetDevicePath()
        {
            return _device.DevicePath;
        }

        public static SteamController[] GetControllers()
        {
            List<HidDevice> _devices = HidDevices.Enumerate(VendorId, ProductIdWireless, ProductIdWired).ToList();
            List<HidDevice> HidDeviceList = new List<HidDevice>();
            string wired_m = "&pid_1102&mi_02";
            string dongle_m = "&pid_1142&mi_01";

            // we should never have holes, this entire dictionary is just because I don't know if I can trust the order I get the HID devices
            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] != null)
                {
                    HidDevice _device = _devices[i];
                    string devicePath = _device.DevicePath.ToString();
                    if (devicePath.Contains(wired_m) || devicePath.Contains(dongle_m))
                        HidDeviceList.Add(_device);
                }
            }
            return HidDeviceList.Select(dr => new SteamController(dr)).ToArray();
        }

        private void OnReport(HidReport report)
        {
            if (!Initalized) return;

            if (0 == Interlocked.Exchange(ref internalStateLock, 1))
            {
                //if (_attached == false) { return; }
                OldState = GetState();

                byte Unknown1 = report.Data[0]; // always 0x01?
                byte Unknown2 = report.Data[1]; // always 0x00?
                VSCEventType EventType = (VSCEventType)report.Data[2];

                switch (EventType)
                {
                    case 0: // not sure what this is but wired controllers do it
                        break;
                    case VSCEventType.CONTROL_UPDATE:
                        {
                            //report.Data[3] // 0x3C?

                            UInt32 PacketIndex = BitConverter.ToUInt32(report.Data, 4);

                            Buttons.A = (report.Data[8] & 128) == 128;
                            Buttons.X = (report.Data[8] & 64) == 64;
                            Buttons.B = (report.Data[8] & 32) == 32;
                            Buttons.Y = (report.Data[8] & 16) == 16;
                            Buttons.LeftBumper = (report.Data[8] & 8) == 8;
                            Buttons.RightBumper = (report.Data[8] & 4) == 4;
                            Buttons.LeftTrigger = (report.Data[8] & 2) == 2;
                            Buttons.RightTrigger = (report.Data[8] & 1) == 1;

                            Buttons.LeftGrip = (report.Data[9] & 128) == 128;
                            Buttons.Start = (report.Data[9] & 64) == 64;
                            Buttons.Steam = (report.Data[9] & 32) == 32;
                            Buttons.Select = (report.Data[9] & 16) == 16;

                            Buttons.Down = (report.Data[9] & 8) == 8;
                            Buttons.Left = (report.Data[9] & 4) == 4;
                            Buttons.Right = (report.Data[9] & 2) == 2;
                            Buttons.Up = (report.Data[9] & 1) == 1;

                            bool LeftAnalogMultiplexMode = (report.Data[10] & 128) == 128;
                            Buttons.StickClick = (report.Data[10] & 64) == 64;
                            bool Unknown = (report.Data[10] & 32) == 32; // what is this?
                            Buttons.RightPadTouch = (report.Data[10] & 16) == 16;
                            bool LeftPadTouch = (report.Data[10] & 8) == 8;
                            Buttons.RightPadClick = (report.Data[10] & 4) == 4;
                            bool ThumbOrLeftPadPress = (report.Data[10] & 2) == 2; // what is this even for?
                            Buttons.RightGrip = (report.Data[10] & 1) == 1;

                            LeftTrigger = report.Data[11];
                            RightTrigger = report.Data[12];

                            if (LeftAnalogMultiplexMode)
                            {
                                if (LeftPadTouch)
                                {
                                    Buttons.LeftPadTouch = true;
                                    Buttons.LeftPadClick = ThumbOrLeftPadPress;
                                    LeftPadX = BitConverter.ToInt16(report.Data, 16);
                                    LeftPadY = BitConverter.ToInt16(report.Data, 18);
                                }
                                else
                                {
                                    LeftStickX = BitConverter.ToInt16(report.Data, 16);
                                    LeftStickY = BitConverter.ToInt16(report.Data, 18);
                                }
                            }
                            else
                            {
                                if (LeftPadTouch)
                                {
                                    Buttons.LeftPadTouch = true;
                                    LeftPadX = BitConverter.ToInt16(report.Data, 16);
                                    LeftPadY = BitConverter.ToInt16(report.Data, 18);
                                }
                                else
                                {
                                    Buttons.LeftPadTouch = false;
                                    LeftStickX = BitConverter.ToInt16(report.Data, 16);
                                    LeftStickY = BitConverter.ToInt16(report.Data, 18);
                                    LeftPadX = 0;
                                    LeftPadY = 0;
                                }

                                Buttons.LeftPadClick = ThumbOrLeftPadPress && !Buttons.StickClick;
                            }

                            RightPadX = BitConverter.ToInt16(report.Data, 20);
                            RightPadY = BitConverter.ToInt16(report.Data, 22);

                            /*
                            DataStuck = CheckSensorDataStuck();
                            if (!SensorsEnabled || DataStuck) { EnableGyroSensors(); }
                            */

                            AccelerometerX = BitConverter.ToInt16(report.Data, 28);
                            AccelerometerY = BitConverter.ToInt16(report.Data, 30);
                            AccelerometerZ = BitConverter.ToInt16(report.Data, 32);
                            AngularVelocityX = BitConverter.ToInt16(report.Data, 34);
                            AngularVelocityY = BitConverter.ToInt16(report.Data, 36);
                            AngularVelocityZ = BitConverter.ToInt16(report.Data, 38);
                            OrientationW = BitConverter.ToInt16(report.Data, 40);
                            OrientationX = BitConverter.ToInt16(report.Data, 42);
                            OrientationY = BitConverter.ToInt16(report.Data, 44);
                            OrientationZ = BitConverter.ToInt16(report.Data, 46);
                        }
                        break;

                    case VSCEventType.CONNECTION_DETAIL:
                        {
                            //report.Data[3] // 0x01?

                            // Connection detail. 0x01 for disconnect, 0x02 for connect, 0x03 for pairing request.
                            ConnectionState ConnectionStateV = (ConnectionState)report.Data[4];

                            if (report.Data[4] == 0x01)
                            {
                                byte[] tmpBytes = new byte[4];
                                tmpBytes[1] = report.Data[5];
                                tmpBytes[2] = report.Data[6];
                                tmpBytes[3] = report.Data[7];

                                //BitConverter.ToUInt32(tmpBytes, 0); // Timestamp
                            }
                        }
                        break;

                    case VSCEventType.BATTERY_UPDATE:
                        {
                            //report.Data[3] // 0x0B?

                            UInt32 PacketIndex = BitConverter.ToUInt32(report.Data, 4);

                            // only works if controller is configured to send this data

                            // millivolts
                            UInt16 BatteryVoltage = BitConverter.ToUInt16(report.Data, 8);
                            //BitConverter.ToUInt16(report.Data, 10); // UNKNOWN, stuck at 100
                        }
                        break;

                    default:
                        {
                            Console.WriteLine("Unknown Packet Type " + EventType);
                        }
                        break;
                }

                SteamControllerState NewState = GetState();
                OnStateUpdated(NewState);

                _device.ReadReport(OnReport);

                Interlocked.Exchange(ref internalStateLock, 0);
            }
        }

        /*private void DeviceAttachedHandler()
        {
            lock (controllerStateLock)
            {
                _attached = true;
                Console.WriteLine("VSC Address Attached");
                _device.ReadReport(OnReport);
            }
        }

        private void DeviceRemovedHandler()
        {
            lock (controllerStateLock)
            {
                _attached = false;
                Console.WriteLine("VSC Address Removed");
            }
        }*/
    }
}