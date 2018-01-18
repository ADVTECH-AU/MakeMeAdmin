﻿// <copyright file="EventID.cs" company="Sinclair Community College">
// Copyright (c) 2010-2017, Sinclair Community College. All rights reserved.
// </copyright>

namespace SinclairCC.MakeMeAdmin
{
    /// <summary>
    /// This enumeration contains all of the event IDs that are written to the log.
    /// </summary>
    public enum EventID : int
    {
        /// <summary>
        /// A user was added to the Administrators group.
        /// </summary>
        UserAddedToAdminsSuccess,

        /// <summary>
        /// A user was removed from the Administrators group.
        /// </summary>
        UserRemovedFromAdminsSuccess,

        /// <summary>
        /// The application failed to add a user to the Administrators group.
        /// </summary>
        UserAddedToAdminsFailure,

        /// <summary>
        /// The application failed to remove a user from the Administrators group.
        /// </summary>
        UserRemovedFromAdminsFailure,

        /// <summary>
        /// A principal has been removed from the Administrators group by some
        /// external (to Make Me Admin) process.
        /// </summary>
        PrincipalRemovedByExternalProcess,

        /// <summary>
        /// Information about a request for administrator rights that was received
        /// from a remote host.
        /// </summary>
        RemoteRequestInformation,

        /// <summary>
        /// Event ID for debug messages.
        /// </summary>
        DebugMessage = 9000
    }
}
