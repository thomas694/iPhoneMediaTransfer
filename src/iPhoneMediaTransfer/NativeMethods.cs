//--------------------------------------------------------------------------------
// Helper class to access native functions.
//
// Version 1.0
// Copyright (c) Feb 2023  thomas694 (@GH 0CFD61744DA1A21C)
//     initial version
//
// This file is part of iPhoneMediaTransfer.
//
// iPhoneMediaTransfer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//--------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace iPhoneMediaTransfer
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
            string lpFileName,
            string lpExistingFileName,
            IntPtr lpSecurityAttributes
        );
    }
}
