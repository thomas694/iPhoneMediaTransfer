/*

 */
using System;

namespace MK.MobileDevice.Lite
{
	/// <summary>
	/// iPhoneException.
	/// </summary>
	class iPhoneException : Exception
    {
        public iPhoneException()
            : base() { }

        public iPhoneException(string message)
            : base(message) { }

        public iPhoneException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public iPhoneException(string message, Exception innerException)
            : base(message, innerException) { }

        public iPhoneException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
