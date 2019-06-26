﻿using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ThumbnailedBOM
{
    [ComVisible(true)]
    [Guid("C672CD28-517F-4426-891C-EC8D8EE51EA2")]
    public class AddInContext : SwAddin
    {
        internal static AppWindow ApplicationWindow { get; set; }

        private List<int> CommandIDs = new List<int>();

        /// <summary>
        /// Gets the SldWorks object.
        /// </summary>
        internal static SldWorks SOLIDWORKS { get; private set; }
        /// <summary>
        /// Gets the cookie session integer.
        /// </summary>
        internal static int SessionCookie { get; private set; }

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            try
            {
                SOLIDWORKS = ThisSW as SldWorks;
                SessionCookie = Cookie;
                SOLIDWORKS.SetAddinCallbackInfo(0, this, SessionCookie);
                // todo: Uncomment line below and add implementation 
                BuildUI();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private void BuildUI()
        {


 
             
            var ret = SOLIDWORKS.AddMenuPopupItem3(
              (int)(swDocumentTypes_e.swDocDRAWING | swDocumentTypes_e.swDocASSEMBLY),
              SessionCookie,
              (int)swSelectType_e.swSelANNOTATIONTABLES,
              "Export to Excel (with thumbnails)",
              "OpenShell",
              "",
              "Export to Excel (with thumbnails)",
              ""
              );
            if (ret != -1)
                CommandIDs.Add(ret);



        }
        
        internal static Application Application = null;
        public void OpenShell()
        {
            if (Application.Current != Application)
            {
                string name = string.Empty;
                if (Application.Current != null)
                    if (Application.Current.MainWindow != null)
                        name = $" [{Application.Current.MainWindow.Title}]";
                SOLIDWORKS.SendMsgToUser($"Existing WPF application in SOLIDWORKS domain{name}. {AddInName} attempted to launch but failed. It is very likely that an existing add-in is causing this error. Please disable add-in and restart SOLIDWORKS.");
                return;
            }

                if (Application == null)
                { 
                    Application = new Application
                    {
                        ShutdownMode = ShutdownMode.OnExplicitShutdown
                    };

                }
                 else
                {
                    Application.Restart();
                }

        }

        private void DestroyUI()
        {
            foreach (int CommandID in CommandIDs)
            {
                var ret = SOLIDWORKS.RemoveFromPopupMenu(
                                CommandID,
                                (int)swDocumentTypes_e.swDocASSEMBLY,
                                 (int)swSelectType_e.swSelCOMPONENTS,
                                 true
                                 );
                
            }

            // shut down application
            Application.Shutdown();

        }



        public bool DisconnectFromSW()
        {
            try
            {
             
                DestroyUI();
                return true;
            }
            catch (Exception ex)
            {
                return false; 
            }
        }



        #region COM registration
        internal static string AddInName { get; private set; } = "ThumbnailedBOM";
        internal static string AddInDescription { get; private set; } = "Exports a SOLIDWORKS Bill of Materials to Excel with thumbail.";
        [ComRegisterFunction]
        private static void RegisterAssembly(Type t)
        {
            try
            {
                string KeyPath = string.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);
                RegistryKey rk = Registry.LocalMachine.CreateSubKey(KeyPath);
                rk.SetValue(null, 1); // 1: Add-in will load at start-up
                rk.SetValue("Title", AddInName); // Title
                rk.SetValue("Description", AddInDescription); // Description
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [ComUnregisterFunction]
        private static void UnregisterAssembly(Type t)
        {
            try
            {
                bool Exist = false;
                string KeyPath = string.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);
                using (RegistryKey Key = Registry.LocalMachine.OpenSubKey(KeyPath))
                {
                    if (Key != null)
                        Exist = true;
                    else
                        Exist = false;
                }
                if (Exist)
                    Registry.LocalMachine.DeleteSubKeyTree(KeyPath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion


    }
}