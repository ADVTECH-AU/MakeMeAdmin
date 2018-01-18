﻿// <copyright file="Program.cs" company="Sinclair Community College">
// Copyright (c) 2010-2017, Sinclair Community College. All rights reserved.
// </copyright>

namespace SinclairCC.MakeMeAdmin
{
    using System;
    using System.ServiceModel;
    using System.Security.Principal;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    /// <summary>
    /// This class defines the main entry point for the application.
    /// </summary>
    internal class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        internal static void Main()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString());
                Console.WriteLine("Desktops Detected");
                Console.WriteLine("=================");
                foreach (string desktop in Desktop.Get())
                {
                    Console.WriteLine(desktop);
                }
                System.Threading.Thread.Sleep(2000);
                Console.WriteLine();
            }


#if DEBUG
            Console.Write("\n\nPress <ENTER> to close this program.");
            Console.ReadLine();
#endif
        }
    }
}
