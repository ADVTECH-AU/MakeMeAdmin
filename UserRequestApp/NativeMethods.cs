﻿// 
// Copyright © 2010-2019, Sinclair Community College
// Licensed under the GNU General Public License, version 3.
// See the LICENSE file in the project root for full license information.  
//
// This file is part of Make Me Admin.
//
// Make Me Admin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3.
//
// Make Me Admin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Make Me Admin. If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SinclairCC.MakeMeAdmin
{
    internal class NativeMethods
    {
        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr ptr);

        [DllImport("Kernel32.dll", EntryPoint = "RtlSecureZeroMemory", SetLastError = false)]
        private static extern void SecureZeroMemory(IntPtr dest, UIntPtr size);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }


        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                   IntPtr pAuthBuffer,
                                                                   uint cbAuthBuffer,
                                                                   StringBuilder pszUserName,
                                                                   ref int pcchMaxUserName,
                                                                   StringBuilder pszDomainName,
                                                                   ref int pcchMaxDomainame,
                                                                   StringBuilder pszPassword,
                                                                   ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);


        // Define the Windows LogonUser and CloseHandle functions.
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String username, String domain, IntPtr password, int logonType, int logonProvider, ref IntPtr token);



        /*
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LogonUser(
            [MarshalAs(UnmanagedType.LPWStr)] string username,
            [MarshalAs(UnmanagedType.LPWStr)] string domain,
            [MarshalAs(UnmanagedType.LPWStr)] string password,
            int logonType,
            int logonProvider,
            ref IntPtr token);
        */

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        // Define the required LogonUser enumerations.
        private const int LOGON32_PROVIDER_DEFAULT = 0;

        /*
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        */
        private const int LOGON32_LOGON_NETWORK = 3;
        /*
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        */


        internal static System.Net.NetworkCredential GetCredentials()
        {
            CREDUI_INFO credui = new CREDUI_INFO();
            credui.pszCaptionText = "Please enter your credentials.";
            credui.pszMessageText = "Message Displayed Here";
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            int result = CredUIPromptForWindowsCredentials(ref credui,
                                                           0,
                                                           ref authPackage,
                                                           IntPtr.Zero,
                                                           0,
                                                           out outCredBuffer,
                                                           out outCredSize,
                                                           ref save,
                                                           0 /* 1:  Generic */);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;
            if (result == 0)
            {
                if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                   domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    //TODO: ms documentation says we should call this but i can't get it to work
                    //SecureZeroMemory(outCredBuffer, new UIntPtr(outCredSize));
                    Marshal.Copy(new byte[outCredSize], 0, outCredBuffer, (int)outCredSize);

                    //clear the memory allocated by CredUIPromptForWindowsCredentials 
                    CoTaskMemFree(outCredBuffer);
                    System.Net.NetworkCredential returnCreds = new System.Net.NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };

                    if ((string.IsNullOrEmpty(returnCreds.Domain)) && (returnCreds.UserName.IndexOf('\\') >= 0))
                    {
                        int slashIndex = returnCreds.UserName.IndexOf('\\');
                        returnCreds.Domain = returnCreds.UserName.Substring(0, slashIndex);
                        returnCreds.UserName = returnCreds.UserName.Substring(slashIndex + 1);
                    }

                    return returnCreds;
                }
            }

            return null;
        }


        internal static bool ValidateCredentials(System.Net.NetworkCredential credentials)
        {
            if (null == credentials) { return false; }

            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr passwordPtr = IntPtr.Zero;
            bool returnValue = false;
            int error = 0;

            // Marshal the SecureString to unmanaged memory.
            passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(credentials.SecurePassword);

            // Pass LogonUser the unmanaged (and decrypted) copy of the password.
            returnValue = LogonUser(credentials.UserName, credentials.Domain, passwordPtr,
                                    LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT,
                                    ref tokenHandle);

            if (!returnValue && tokenHandle == IntPtr.Zero)
            {
                error = Marshal.GetLastWin32Error();
            }

            // Perform cleanup whether or not the call succeeded.
            // Zero-out and free the unmanaged string reference.
            Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);

            // Close the token handle.
            CloseHandle(tokenHandle);

            // Throw an exception if an error occurred.
            if (error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return returnValue;
        }
    }
}
