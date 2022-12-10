namespace MK.MobileDevice.Lite
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DeviceNotificationCallback(ref AMDeviceNotificationCallbackInfo callback_info);
    internal delegate void ITMDDeviceNotificationCallback(ref ITMDAMDeviceNotificationCallbackInfo callback_info);
}

