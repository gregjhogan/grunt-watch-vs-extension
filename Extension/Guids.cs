// Guids.cs
// MUST match guids.h
using System;

namespace activelow.GruntWatchPackage
{
    static class GuidList
    {
        public const string guidGruntWatchPackagePkgString = "0651eb16-e3fb-4fdf-ac43-1060a45fda4e";
        public const string guidGruntWatchPackageCmdSetString = "def32a09-7656-4391-bd74-76bd4652e53b";

        public static readonly Guid guidGruntWatchPackageCmdSet = new Guid(guidGruntWatchPackageCmdSetString);
    };
}