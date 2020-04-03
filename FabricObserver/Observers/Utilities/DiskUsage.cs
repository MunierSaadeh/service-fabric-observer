﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FabricObserver.Observers.Utilities
{
    internal class DiskUsage : IDisposable
    {
        private WindowsPerfCounters winPerfCounters;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskUsage"/> class.
        /// </summary>
        public DiskUsage()
        {
            var driveLetter = Environment.CurrentDirectory.Substring(0, 2);
            this.Drive = driveLetter;
            this.winPerfCounters = new WindowsPerfCounters();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskUsage"/> class.
        /// </summary>
        /// <param name="driveLetter">Drive letter.</param>
        public DiskUsage(string driveLetter)
        {
            this.Drive = driveLetter;
            this.winPerfCounters = new WindowsPerfCounters();
        }

        /// <summary>
        /// Gets the percent used space (as an integer value) of the current drive where this code is running from.
        /// Or from whatever drive letter you supplied to DiskUsage(string driveLetter) ctor.
        /// </summary>
        internal int PercentUsedSpace => GetCurrentDiskSpaceUsedPercent(this.Drive);

        internal string Drive { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
        }

        internal static double GetTotalDiskSpace(string driveLetter, SizeUnit sizeUnit = SizeUnit.Bytes)
        {
            var driveInfo = new DriveInfo(driveLetter);
            long total = driveInfo.TotalSize;

            return Math.Round(ConvertToSizeUnits(total, sizeUnit), 2);
        }

        internal static int GetCurrentDiskSpaceUsedPercent(string drive)
        {
            if (string.IsNullOrEmpty(drive))
            {
                return -1; // Don't throw here.
            }

            var driveInfo = new DriveInfo(drive);
            long availableMb = driveInfo.AvailableFreeSpace / 1024 / 1024;
            long totalMb = driveInfo.TotalSize / 1024 / 1024;
            double usedPct = ((double)(totalMb - availableMb)) / totalMb;

            return (int)(usedPct * 100);
        }

        internal List<(string DriveName, double DiskSize, int PercentConsumed)>
            GetCurrentDiskSpaceTotalAndUsedPercentAllDrives(SizeUnit sizeUnit = SizeUnit.Bytes)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            return (
                from t in allDrives
                where t.IsReady
                select t.Name into driveName
                let totalSize = GetTotalDiskSpace(driveName, sizeUnit)
                let pctUsed = GetCurrentDiskSpaceUsedPercent(driveName)
                select (driveName.Substring(0, 1), totalSize, pctUsed)).ToList();
        }

        internal double GetAvailableDiskSpace(string driveLetter, SizeUnit sizeUnit = SizeUnit.Bytes)
        {
            var driveInfo = new DriveInfo(driveLetter);
            long available = driveInfo.AvailableFreeSpace;

            return Math.Round(ConvertToSizeUnits(available, sizeUnit), 2);
        }

        internal double GetUsedDiskSpace(string driveLetter, SizeUnit sizeUnit = SizeUnit.Bytes)
        {
            var driveInfo = new DriveInfo(driveLetter);
            long used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;

            return Math.Round(ConvertToSizeUnits(used, sizeUnit), 2);
        }

        internal float GetAverageDiskQueueLength(string instance)
        {
            return this.winPerfCounters.PerfCounterGetAverageDiskQueueLength(instance);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.winPerfCounters != null)
                {
                    this.winPerfCounters.Dispose();
                    this.winPerfCounters = null;
                }
            }

            this.isDisposed = true;
        }

        private static double ConvertToSizeUnits(double amount, SizeUnit sizeUnit)
        {
            switch (sizeUnit)
            {
                case SizeUnit.Bytes:
                    return amount;

                case SizeUnit.Kilobytes:
                    return amount / 1024;

                case SizeUnit.Megabytes:
                    return amount / 1024 / 1024;

                case SizeUnit.Gigabytes:
                    return amount / 1024 / 1024 / 1024;

                case SizeUnit.Terabytes:
                    return amount / 1024 / 1024 / 1024 / 1024;

                default:
                    return amount;
            }
        }
    }

    public enum SizeUnit
    {
        Bytes,
        Kilobytes,
        Megabytes,
        Gigabytes,
        Terabytes,
    }
}
