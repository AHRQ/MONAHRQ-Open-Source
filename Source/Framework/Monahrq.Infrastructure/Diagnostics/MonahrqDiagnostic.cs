using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualBasic.Devices;
using Monahrq.Infrastructure.Events;

namespace Monahrq.Infrastructure.Diagnostics
{
    public static class MonahrqDiagnostic
    {
        #region User Associated
        /// <summary>
        /// Gets the name of the get user.
        /// </summary>
        /// <value>
        /// The name of the get user.
        /// </value>
        public static string GetUserName
        {
            get { return new WindowsPrincipal(WindowsIdentity.GetCurrent()).Identity.Name; }
        }

        /// <summary>
        /// Determines whether this instance is administrator.
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
        #endregion

        #region Internet/Network Associated
        /// <summary>
        /// Internets the state of the connected.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="reservedValue">The reserved value.</param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int description, int reservedValue);

        /// <summary>
        /// Checks if connected to internet.
        /// </summary>
        /// <param name="throwEvent">if set to <c>true</c> [throw event].</param>
        /// <returns></returns>
        public static bool CheckIfConnectedToInternet(bool throwEvent = true)
        {
            int description;
            var result = InternetGetConnectedState(out description, 0); // Get internet connection state

            if (throwEvent)
            {
                var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<InternetConnectionEvent>()
                    .Publish(result ? ConnectionState.OnLine : ConnectionState.OffLine);
                    // Broadcast the if connected to internet or not.
            }

            return result; // Return result;
        }
        #endregion

        #region Hard Drive Management Associated

        public static DriveInfo GetHDInfo(string drivePath)
        {
            if (string.IsNullOrEmpty(drivePath)) return null;

            return new DriveInfo(drivePath);
        }

        public static DriveType GetHDType(string drivePath)
        {
            var hdInfo = GetHDInfo(drivePath);

            if (hdInfo == null) return DriveType.Unknown;

            return hdInfo.DriveType;
        }

        public static long? GetHDTotalSize(string drivePath)
        {
            var hdInfo = GetHDInfo(drivePath);

            return hdInfo == null ? (long?) null : hdInfo.TotalSize;
        }

        public static long? GetHDAvailableFreeSpace(string drivePath)
        {
            var hdInfo = GetHDInfo(drivePath);

            return hdInfo == null ? (long?)null : hdInfo.AvailableFreeSpace;
        }

        public static string GetHDAvailableFreeSpaceAsString(string drivePath)
        {
            var space = GetHDAvailableFreeSpace(drivePath);
            var formatedSpace = "N/A";

            if (space.HasValue)
            {
                var dSpace = Convert.ToDecimal(space.Value);
                if (dSpace > 1000000000)
                {
                    formatedSpace = string.Format("{0} GB", Math.Round((dSpace / (1024 * 1024 * 1024)), 2));
                }
                else if (dSpace < 1000000000 && dSpace > 1000000)
                {
                    formatedSpace = string.Format("{0} MB", Math.Round((dSpace / (1024 * 1024)), 2));
                }
                else if (dSpace < 1000000 && dSpace > 1000)
                {
                    formatedSpace = string.Format("{0} KB", Math.Round((dSpace / (1024 * 1024)), 2));
                }
            }

            return formatedSpace;
        }

        public static string GetHDAvailableFreeSpaceAsString()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var pathRoot = Path.GetPathRoot(path);
            var hdInfo = GetHDInfo(pathRoot);

           // return hdInfo == null ? (long?)null : hdInfo.AvailableFreeSpace;

            var space = GetHDAvailableFreeSpace(hdInfo.Name);
            var formatedSpace = "N/A";

            if (space.HasValue)
            {
                var dSpace = Convert.ToDecimal(space.Value);
                if (dSpace > 1000000000)
                {
                    formatedSpace = string.Format("{0} GB", Math.Round((dSpace / (1024 * 1024 * 1024)), 2));
                }
                else if (dSpace < 1000000000 && dSpace > 1000000)
                {
                    formatedSpace = string.Format("{0} MB", Math.Round((dSpace / (1024 * 1024)), 2));
                }
                else if (dSpace < 1000000 && dSpace > 1000)
                {
                    formatedSpace = string.Format("{0} KB", Math.Round((dSpace / (1024 * 1024)), 2));
                }
            }

            return formatedSpace;
        }
       

        //public static long? GetHDTotalFreeSpace(string drivePath)
        //{
        //    var hdInfo = GetHDInfo(drivePath);

        //    return hdInfo == null ? (long?)null : hdInfo.TotalFreeSpace;
        //}

        #endregion

        #region Memory Management Associated

        public static long GetTotalAmountOfFreeVirtualMemory()
        {
            //PerformanceCounter pc = new PerformanceCounter("PERF_COUNTER_RAWCOUNT", "Virtual Bytes");
            //return Convert.ToInt64(pc.NextValue());
            return Convert.ToInt64(new ComputerInfo().AvailablePhysicalMemory);
        }

        public static string GetTotalAmountOfFreeVirtualMemoryAsString()
        {
            var memory = GetTotalAmountOfFreeVirtualMemory();

            var dMemory = Convert.ToDecimal(memory);
            if (dMemory > 1000000000)
            {
                return string.Format("{0} GB", Math.Round(dMemory/(1024*1024*1024), 2));
            }
            if (dMemory < 1000000000 && dMemory > 1000000)
            {
                return string.Format("{0} MB", Math.Round(dMemory / (1024 * 1024), 2));
            }
            if (memory < 1000000 && memory > 1000)
            {
                return string.Format("{0} KB", Math.Round(dMemory / (1024 * 1024), 2));
            }
            return string.Format("{0} B", Math.Round((dMemory / 1024), 2));
        }

        #endregion

        #region Processor Logic Associated
        
        public static int PhysicalCPUsCount
        {
            get { return 0; }
        }

        public static int CPUCoreCount
        {
            get
            {
                int coreCount = 0;
                //foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get().ToList())
                //{
                //    coreCount += int.Parse(item["NumberOfCores"].ToString());
                //}
                return coreCount;
            }
        }

        public static int LogicalCPUsCount
        {
            get { return Environment.ProcessorCount; }
        }
        #endregion
    }
}
