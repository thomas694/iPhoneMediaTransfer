namespace MK.MobileDevice.Lite
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct AMDeviceNotificationCallbackInfo
    {
        internal unsafe IntPtr dev_ptr;
        public NotificationMessage msg;
        public unsafe IntPtr dev
        {
            get
            {
                return this.dev_ptr;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ITMDAMDeviceNotificationCallbackInfo
    {
        internal unsafe void* dev_ptr;
        public NotificationMessage msg;
        public unsafe void* dev
        {
            get
            {
                return this.dev_ptr;
            }
        }
    }
}

