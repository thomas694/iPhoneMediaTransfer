namespace MK.MobileDevice.Lite
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate uint t_sendCommandToDevice(IntPtr conn, IntPtr cfs, int block);
}

