
namespace MK.MobileDevice.Lite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Linq;
    using MK.Plist;
    using System.Xml;
    using System.Xml.Linq;
    
	
    public class iOSDeviceMK
    {
        private bool connected;
        private string current_directory;
        private ITMDDeviceNotificationCallback dnc;
        
        internal unsafe void* hAFC;
        internal unsafe void* hAFC_original;
        internal IntPtr AFChandle;
        internal unsafe void* hService;
        internal unsafe void* iPhoneHandle;
        private static char[] path_separators = new char[] { '/' };
        private bool wasAFC2;
        
        private bool recovery;

        public event ITMDConnectEventHandler Connect;

        public event ITMDConnectEventHandler Disconnect;
		
        public iOSDeviceMK()
        {
            this.doConstruction();            
        }
        
        private void getDeviceHandle()
        {
        	
        }

        private unsafe bool ConnectToPhone()
        {
            if (MobileDevice.AMDeviceConnect(this.iPhoneHandle) == 1)
            {
                throw new Exception("Phone in recovery mode, support not yet implemented");
            }
            if (MobileDevice.AMDeviceIsPaired(this.iPhoneHandle) == 0)
            {
                return false;
            }
            if (MobileDevice.AMDeviceValidatePairing(this.iPhoneHandle) != 0)
            {
                return false;
            }
            if (MobileDevice.AMDeviceStartSession(this.iPhoneHandle) == 1)
            {
                return false;
            }
            if (0 != MobileDevice.AMDeviceStartService(this.iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc2")), ref this.hService, null))
            {
                if (0 != MobileDevice.AMDeviceStartService(this.iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCString("com.apple.afc")), ref this.hService, null))
                {
                    return false;
                }
            }
            else
            {
                this.wasAFC2 = true;
            }
            if (MobileDevice.AFCConnectionOpen(this.hService, 0, ref this.hAFC) != 0)
            {
                return false;
            }
            this.connected = true;
            return true;
        }

        public void Copy(string sourceName, string destName)
        {
        	throw new NotImplementedException("Copy not yet implemented.");
        }
        
        public unsafe bool CreateDirectory(string path)
        {
            string str = this.FullPath(this.current_directory, path);
            if (MobileDevice.AFCDirectoryCreate(this.hAFC, str) != 0)
            {
                return false;
            }
            return true;
        }

        public unsafe void DeleteDirectory(string path)
        {
            string str = this.FullPath(this.current_directory, path);
            if (this.IsDirectory(str))
            {
                MobileDevice.AFCRemovePath(this.hAFC, str);
            }
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            if (!recursive)
            {
                this.DeleteDirectory(path);
            }
            else
            {
                string str = this.FullPath(this.current_directory, path);
                if (this.IsDirectory(str))
                {
                    this.InternalDeleteDirectory(path);
                }
            }
        }

        public unsafe void DeleteFile(string path)
        {
            string str = this.FullPath(this.current_directory, path);
            if (this.Exists(str))
            {
                MobileDevice.AFCRemovePath(this.hAFC, str);
            }
        }

        
        private unsafe void doConstruction()
        {
            void* voidPtr;
            this.dnc = new ITMDDeviceNotificationCallback(this.NotifyCallback);
            int num = MobileDevice.AMDeviceNotificationSubscribe(this.dnc, 0, 0, 0, out voidPtr);
            
            if (num != 0)
            {
                throw new Exception("AMDeviceNotificationSubscribe failed with error " + num);
            }
            
            this.current_directory = "/";
        }

        public unsafe bool Exists(string path)
        {
            void* dict = null;
            int num = MobileDevice.AFCFileInfoOpen(this.hAFC, path, ref dict);
            if (num == 0)
            {
                MobileDevice.AFCKeyValueClose(dict);
            }
            return (num == 0);
        }

        public ulong FileSize(string path)
        {
            ulong num;
            bool flag;
            this.GetFileInfo(path, out num, out flag);
            return num;
        }

        internal string FullPath(string path1, string path2)
        {
            string[] strArray;
            if ((path1 == null) || (path1 == string.Empty))
            {
                path1 = "/";
            }
            if ((path2 == null) || (path2 == string.Empty))
            {
                path2 = "/";
            }
            if (path2[0] == '/')
            {
                strArray = path2.Split(path_separators);
            }
            else if (path1[0] == '/')
            {
                strArray = (path1 + "/" + path2).Split(path_separators);
            }
            else
            {
                strArray = ("/" + path1 + "/" + path2).Split(path_separators);
            }
            string[] strArray2 = new string[strArray.Length];
            int count = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == "..")
                {
                    if (count > 0)
                    {
                        count--;
                    }
                }
                else if (!(strArray[i] == ".") && !(strArray[i] == ""))
                {
                    strArray2[count++] = strArray[i];
                }
            }
            return ("/" + string.Join("/", strArray2, 0, count));
        }

        private string Get_st_ifmt(string path)
        {
            return this.GetFileInfo(path)["st_ifmt"];
        }

        public string GetCurrentDirectory()
        {
            return this.current_directory;
        }

        public unsafe string[] GetDirectories(string path)
        {
            if (!this.IsConnected)
            {
                throw new Exception("Not connected to phone");
            }
            void* dir = null;
            string s = this.FullPath(this.CurrentDirectory, path);
            if (MobileDevice.AFCDirectoryOpen(this.hAFC, Encoding.UTF8.GetBytes(s), ref dir) != 0)
            {
                throw new Exception("Path does not exist");
            }
            string buffer = null;
            ArrayList list = new ArrayList();
            MobileDevice.AFCDirectoryRead(this.hAFC, dir, ref buffer);
            while (buffer != null)
            {
                if (((buffer != ".") && (buffer != "..")) && this.IsDirectory(this.FullPath(s, buffer)))
                {
                    list.Add(buffer);
                }
                MobileDevice.AFCDirectoryRead(this.hAFC, dir, ref buffer);
            }
            MobileDevice.AFCDirectoryClose(this.hAFC, dir);
            return (string[]) list.ToArray(typeof(string));
        }

        public string GetDirectoryRoot(string path)
        {
            return "/";
        }

        public unsafe Dictionary<string, string> GetFileInfo(string path)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            void* dict = null;
            if ((MobileDevice.AFCFileInfoOpen(this.hAFC, path, ref dict) == 0) && (dict != null))
            {
                void* voidPtr2;
                void* voidPtr3;
                while (((MobileDevice.AFCKeyValueRead(dict, out voidPtr2, out voidPtr3) == 0) && (voidPtr2 != null)) && (voidPtr3 != null))
                {
                    string key = Marshal.PtrToStringAnsi(new IntPtr(voidPtr2));
                    string str2 = Marshal.PtrToStringAnsi(new IntPtr(voidPtr3));
                    dictionary.Add(key, str2);
                }
                MobileDevice.AFCKeyValueClose(dict);
            }
            return dictionary;
        }

        public unsafe void GetFileInfo(string path, out ulong size, out bool directory)
        {
            string str;
            Dictionary<string, string> fileInfo = this.GetFileInfo(path);
            size = fileInfo.ContainsKey("st_size") ? ulong.Parse(fileInfo["st_size"]) : ((ulong) 0L);
            bool flag = false;
            directory = false;
            if (fileInfo.ContainsKey("st_ifmt") && ((str = fileInfo["st_ifmt"]) != null))
            {
                if (!(str == "S_IFDIR"))
                {
                    if (str == "S_IFLNK")
                    {
                        flag = true;
                    }
                }
                else
                {
                    directory = true;
                }
            }
            if (flag)
            {
                void* dir = null;
                if (directory = MobileDevice.AFCDirectoryOpen(this.hAFC, Encoding.UTF8.GetBytes(path), ref dir) == 0)
                {
                    MobileDevice.AFCDirectoryClose(this.hAFC, dir);
                }
            }
        }

        public unsafe string[] GetFiles(string path)
        {
            if (!this.connected)
            {
                throw new Exception("Not connected to device");
            }
            string s = this.FullPath(this.current_directory, path);
            void* dir = null;
            if (MobileDevice.AFCDirectoryOpen(this.hAFC, Encoding.UTF8.GetBytes(s), ref dir) != 0)
            {
                throw new Exception("Path does not exist");
            }
            string buffer = null;
            ArrayList list = new ArrayList();
            MobileDevice.AFCDirectoryRead(this.hAFC, dir, ref buffer);
            while (buffer != null)
            {
                if (!this.IsDirectory(this.FullPath(s, buffer)))
                {
                    list.Add(buffer);
                }
                MobileDevice.AFCDirectoryRead(this.hAFC, dir, ref buffer);
            }
            MobileDevice.AFCDirectoryClose(this.hAFC, dir);
            return (string[]) list.ToArray(typeof(string));
        }

        private void InternalDeleteDirectory(string path)
        {
            string str = this.FullPath(this.current_directory, path);
            string[] files = this.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                this.DeleteFile(str + "/" + files[i]);
            }
            files = this.GetDirectories(path);
            for (int j = 0; j < files.Length; j++)
            {
                this.InternalDeleteDirectory(str + "/" + files[j]);
            }
            this.DeleteDirectory(path);
        }

        public bool IsDirectory(string path)
        {
            ulong num;
            bool flag;
            this.GetFileInfo(path, out num, out flag);
            return flag;
        }

        public bool IsFile(string path)
        {
            return (this.Get_st_ifmt(path) == "S_IFREG");
        }

        public bool IsLink(string path)
        {
            return (this.Get_st_ifmt(path) == "S_IFLNK");
        }

        private unsafe void NotifyCallback(ref ITMDAMDeviceNotificationCallbackInfo callback)
        {
            if (callback.msg == NotificationMessage.Connected)
            {
                this.iPhoneHandle = (void*)callback.dev;
                if (this.ConnectToPhone())
                {
                    this.OnConnect(new ITMDConnectEventArgs(callback));
                }
            }
            else if (callback.msg == NotificationMessage.Disconnected)
            {
                this.connected = false;
                this.OnDisconnect(new ITMDConnectEventArgs(callback));
            }
        }

        protected void OnConnect(ITMDConnectEventArgs args)
        {
            ITMDConnectEventHandler connect = this.Connect;
            if (connect != null)
            {
                connect(this, args);
            }
        }

        
        protected void OnDisconnect(ITMDConnectEventArgs args)
        {
            ITMDConnectEventHandler disconnect = this.Disconnect;
            if (disconnect != null)
            {
                disconnect(this, args);
            }
        }

        public unsafe void ReConnect()
        {
            MobileDevice.AFCConnectionClose(this.hAFC);
            MobileDevice.AMDeviceStopSession(this.iPhoneHandle);
            MobileDevice.AMDeviceDisconnect(this.iPhoneHandle);
            this.ConnectToPhone();
        }

        

        public unsafe bool Rename(string sourceName, string destName)
        {
            return (MobileDevice.AFCRenamePath(this.hAFC, this.FullPath(this.current_directory, sourceName), this.FullPath(this.current_directory, destName)) == 0);
        }

        public unsafe void sendCommandToDevice(string Command)
        {
            MobileDevice.sendCommandToDevice(this.iPhoneHandle, MobileDevice.__CFStringMakeConstantString(MobileDevice.StringToCFString(Command)), 0);
        }

        public void sendFileToDevice(string Filename)
        {
        }

        public void SetCurrentDirectory(string path)
        {
            string str = this.FullPath(this.current_directory, path);
            if (!this.IsDirectory(str))
            {
                throw new Exception("Invalid directory specified");
            }
            this.current_directory = str;
        }

        public unsafe string ActivationState
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "ActivationState");
            }
        }

        public unsafe void* AFCHandle
        {
            get
            {
                return this.hAFC;
            }
        }

        public string CurrentDirectory
        {
            get
            {
                return this.current_directory;
            }
            set
            {
                this.current_directory = value;
            }
        }

        public unsafe void* Device
        {
            get
            {
                return this.iPhoneHandle;
            }
        }

        public unsafe string DeviceBasebandBootloaderVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "BasebandBootloaderVersion");
            }
        }

        public unsafe string DeviceBasebandVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "BasebandVersion");
            }
        }

        public unsafe string DeviceBuildVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "BuildVersion");
            }
        }

        public unsafe string DeviceFirmwareVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "FirmwareVersion");
            }
        }

        public unsafe string DeviceId
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "UniqueDeviceID");
            }
        }

        public unsafe string DeviceIntegratedCircuitCardIdentity
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "IntegratedCircuitCardIdentity");
            }
        }

        public unsafe string DeviceiTunesHasConnected
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "iTunesHasConnected");
            }
        }

        public unsafe string DeviceModelNumber
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "ModelNumber");
            }
        }
        
        public unsafe string DeviceName
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "DeviceName");
            }
            set
            {
            	MobileDevice.AMDeviceSetValue(this.iPhoneHandle, 0, "DeviceName", value);
            }
        }
        

        public unsafe string DevicePhoneNumber
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "PhoneNumber");
            }
        }

        public unsafe string DeviceProductType
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "ProductType");
            }
        }

        public unsafe string DeviceSerial
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "SerialNumber");
            }
        }

        public unsafe string DeviceSIMStatus
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "SIMStatus");
            }
        }

        public unsafe string DeviceType
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "DeviceClass");
            }
        }

        public unsafe string DeviceVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "ProductVersion");
            }
        }

        public unsafe string DeviceWiFiAddress
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "WiFiAddress");
            }
        }

        public unsafe string IInternationalMobileSubscriberIdentity
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "InternationalMobileSubscriberIdentity");
            }
        }

        public unsafe string InternationalMobileEquipmentIdentity
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "InternationalMobileEquipmentIdentity");
            }
        }

        public bool IsConnected
        {
            get
            {
                return this.connected;
            }
        }

        public bool IsJailbreak
        {
            get
            {
                if (this.wasAFC2)
                {
                    return true;
                }
                if (!this.connected)
                {
                    return false;
                }
                return this.Exists("/Applications");
            }
        }

        public unsafe string ProductVersion
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "ih8sn0w");
            }
        }

        public unsafe string UniqueChipID
        {
            get
            {
                return MobileDevice.AMDeviceCopyValue(this.iPhoneHandle, 0, "UniqueChipID");
            }
        }
    }
}

