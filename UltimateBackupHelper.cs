using System.Collections.Generic;
using System.Linq;
using System.IO;
using Rage;
using System;

namespace YobbinCallouts
{
    internal class UltimateBackupHelper
    {
        internal static void CallPursuitBackup()
        {
            try
            {
                UltimateBackup.API.Functions.callPursuitBackup();
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void callCode3Backup()
        {
            try
            {
                UltimateBackup.API.Functions.callCode3Backup();
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void callCode3SwatBackup(bool radio = false, bool isnooseunit = false)
        {
            try
            {
                UltimateBackup.API.Functions.callCode3SwatBackup(radio, isnooseunit);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void callFireDepartment()
        {
            try
            {
                UltimateBackup.API.Functions.callFireDepartment();
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
    }
}