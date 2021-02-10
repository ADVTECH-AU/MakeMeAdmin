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


namespace SinclairCC.MakeMeAdmin
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = "MakeMeAdmin")]
    public sealed class LoggingProvider : EventSource
    {
        public static LoggingProvider Log = new LoggingProvider();

        [NonEvent]
        public void ElevatedProcessDetected(TokenElevationType elevationType, ProcessInformation elevatedProcess /*, ProcessInformation parentProcess */)
        {
            string elevationTypeString = "Unknown";
            switch (elevationType)
            {
                case TokenElevationType.Full:
                    elevationTypeString = "Full";
                    break;
                case TokenElevationType.Limited:
                    elevationTypeString = "Limited";
                    break;
                case TokenElevationType.Default:
                    elevationTypeString = "Default";
                    break;
            }

            ElevatedProcessDetected(elevatedProcess.ImageFileName, elevatedProcess.TimeStamp, elevatedProcess.UserSIDString, elevatedProcess.SessionID, elevationTypeString, elevatedProcess.CommandLine, elevatedProcess.ProcessID);
        }
        
        [Event(
            101,
            Message = "Process {0} created at {1} by user {2} in session {3} with an elevation type of {4}." + "\r\ncommand line: \"{5}\"" + "\r\nprocess ID: \"{6}\"",
            Channel = EventChannel.Operational,
            Level = EventLevel.Informational
            )]
        private void ElevatedProcessDetected(string imageFileName, DateTime creationTime, string userSIDString, int sessionId, string elevationType, string commandLine, int processId)
        {
            if (IsEnabled())
            {
                WriteEvent(101, imageFileName, creationTime, userSIDString, sessionId, elevationType, commandLine, processId);
            }
        }
        
    }
}
