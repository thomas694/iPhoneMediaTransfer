namespace MK.MobileDevice.Lite
{
    using System;
    using System.IO;

    public class iPhoneFile : Stream
    {
        private long handle;
        private OpenMode mode;
        private iOSDeviceMK phone;
        private string path;

        private iPhoneFile(iOSDeviceMK phone, long handle, OpenMode mode, string path)
        {
            this.phone = phone;
            this.mode = mode;
            this.handle = handle;
            this.path = path;
        }

        protected override unsafe void Dispose(bool disposing)
        {
            if (disposing && (this.handle != 0L))
            {
                //MobileDevice.AFCFileRefClose(this.phone.AFCHandle, this.handle);
                this.handle = 0L;
            }
            base.Dispose(disposing);
        }

        public override unsafe void Flush()
        {
        	throw new NotImplementedException();
            //MobileDevice.AFCFlushData(this.phone.AFCHandle, this.handle);
        }

        public static unsafe iPhoneFile Open(iOSDeviceMK phone, string path, FileAccess openmode)
        {
            long num;
            OpenMode none = OpenMode.None;
            switch (openmode)
            {
                case FileAccess.Read:
                    none = OpenMode.Read;
                    break;

                case FileAccess.Write:
                    none = OpenMode.Write;
                    break;

                case FileAccess.ReadWrite:
                    throw new NotImplementedException("Read+Write not (yet) implemented");
            }
            string str = phone.FullPath(phone.GetCurrentDirectory(), path);
            int num2 = MobileDevice.AFCFileRefOpen(phone.AFCHandle, str, (int) none, 0, out num);
            if (num2 != 0)
            {
                throw new IOException("AFCFileRefOpen failed with error " + num2.ToString());
            }
            return new iPhoneFile(phone, num, none, path);
        }

        public static iPhoneFile OpenRead(iOSDeviceMK phone, string path)
        {
            return Open(phone, path, FileAccess.Read);
        }

        public static iPhoneFile OpenWrite(iOSDeviceMK phone, string path)
        {
            return Open(phone, path, FileAccess.Write);
        }

        private const int FILEREAD_BUFFER_SIZE = 4096;

        public unsafe byte[] ReadAll()
        {
            void* dict = null;
            MobileDevice.AFCFileInfoOpen(this.phone.AFCHandle, this.path, ref dict);

            var fileSize = this.phone.FileSize(this.path);
            byte[] buffer = new byte[FILEREAD_BUFFER_SIZE];
            uint total = 0;
            uint len = 0;

            using (var ms = new MemoryStream((int)fileSize))
            {
                while (total < fileSize)
                {
                    len = (uint)Math.Min(fileSize - total, FILEREAD_BUFFER_SIZE);
                    int num2 = MobileDevice.AFCFileRefRead(phone.AFCHandle, handle, buffer, ref len);
                    if (num2 != 0)
                    {
                        throw new IOException("AFCFileRefRead error = " + num2.ToString());
                    }
                    ms.Write(buffer, 0, (int)len);
                    total += len;
                }

                return ms.GetBuffer();
            }
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
        	throw new NotImplementedException();
        	/*
            byte[] buffer2;
            if (this.mode != OpenMode.Read)
            {
                throw new NotImplementedException("Stream open for writing only");
            }
            if (offset == 0)
            {
                buffer2 = buffer;
            }
            else
            {
                buffer2 = new byte[count];
            }
            uint len = (uint) count;
            int num2 = MobileDevice.AFCFileRefRead(this.phone.AFCHandle, this.handle, buffer2, ref len);
            if (num2 != 0)
            {
                throw new IOException("AFCFileRefRead error = " + num2.ToString());
            }
            if (buffer2 != buffer)
            {
                Buffer.BlockCopy(buffer2, 0, buffer, offset, (int) len);
            }
            return (int) len;
            */
        }

        public override unsafe long Seek(long offset, SeekOrigin origin)
        {
        	throw new NotImplementedException();
        	/*
            int num = MobileDevice.AFCFileRefSeek(this.phone.AFCHandle, this.handle, (uint) offset, 0);
            Console.WriteLine("ret = {0}", num);
            return offset;
            */
        }

        public override unsafe void SetLength(long value)
        {
        	throw new NotImplementedException();
            //MobileDevice.AFCFileRefSetFileSize(this.phone.AFCHandle, this.handle, (uint) value);
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
        	throw new NotImplementedException();
        	/*
            byte[] buffer2;
            if (this.mode != OpenMode.Write)
            {
                throw new NotImplementedException("Stream open for reading only");
            }
            if (offset == 0)
            {
                buffer2 = buffer;
            }
            else
            {
                buffer2 = new byte[count];
                Buffer.BlockCopy(buffer, offset, buffer2, 0, count);
            }
            uint len = (uint) count;
            MobileDevice.AFCFileRefWrite(this.phone.AFCHandle, this.handle, buffer2, len);
            */
        }

        public override bool CanRead
        {
            get
            {
                return (this.mode == OpenMode.Read);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this.mode == OpenMode.Write);
            }
        }

        public override long Length
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public unsafe override long Position
        {
            get
            {
                uint position = 0;
                throw new NotImplementedException();
                //MobileDevice.AFCFileRefTell(this.phone.AFCHandle, this.handle, ref position);
                return (long) position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        private enum OpenMode
        {
            None = 0,
            Read = 2,
            Write = 3
        }
    }
}

