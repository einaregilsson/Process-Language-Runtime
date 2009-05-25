// Guids.cs
// MUST match guids.h
using System;

namespace EinarEgilsson.CCSProjects
{
    static class GuidList
    {
        public const string guidCCSProjectsPkgString = "1c3d7978-be09-4fd6-96ff-6c2c0805ae72";
        public const string guidCCSProjectsCmdSetString = "fd4dc346-306b-4260-ae5a-88a2b801c679";

        public static readonly Guid guidCCSProjectsCmdSet = new Guid(guidCCSProjectsCmdSetString);
    };
}