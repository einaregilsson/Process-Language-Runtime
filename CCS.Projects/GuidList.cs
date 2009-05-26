/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿// Guids.cs
// MUST match guids.h
using System;

namespace CCS.Projects
{
    static class GuidList
    {
        public const string guidCCSProjectPkgString = "1c3d7978-be09-4fd6-96ff-6c2c0805ae72";
        public const string guidCCSProjectCmdSetString = "fd4dc346-306b-4260-ae5a-88a2b801c679";
        public const string guidCCSProjectFactoryString = "D39941DE-05FC-4bb4-B97F-0896FEB88C5A";

        public static readonly Guid guidCCSProjectPkg = new Guid(guidCCSProjectPkgString);
        public static readonly Guid guidCCSProjectCmdSet = new Guid(guidCCSProjectCmdSetString);
        public static readonly Guid guidCCSProjectFactory = new Guid(guidCCSProjectFactoryString);
    };
}