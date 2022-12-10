

namespace MK.MobileDevice.Lite
{
    using System;

    public class ITMDConnectEventArgs : EventArgs
    {
        private unsafe void* device;
        private NotificationMessage message;

        internal unsafe ITMDConnectEventArgs(ITMDAMDeviceNotificationCallbackInfo cbi)
        {
            this.message = cbi.msg;
            this.device = cbi.dev;
        }

        public unsafe void* Device
        {
            get
            {
                return this.device;
            }
        }

        public NotificationMessage Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

