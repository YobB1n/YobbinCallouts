using System.Collections.Generic;
using System.Linq;
using System.IO;
using Rage;
using System;

namespace YobbinCallouts
{
    internal class StopThePedFunctions
    {
        /// <summary>
        /// Sets ped over the legal limit of alcohol in STP
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="underInfluence"></param>
        internal static void SetPedUnderAlcoholInfluence(Ped ped, bool underInfluence)
        {
            try
            {
                StopThePed.API.Functions.setPedAlcoholOverLimit(ped, underInfluence);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void SetPedUnderDrugInfluence(Ped ped, bool underInfluence)
        {
            try
            {
                StopThePed.API.Functions.setPedUnderDrugsInfluence(ped, underInfluence);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void InjectPedItems(Ped ped)
        {
            try
            {
                StopThePed.API.Functions.injectPedSearchItems(ped);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        internal static void InjectVehicleItems(Vehicle vehicle)
        {
            try
            {
                StopThePed.API.Functions.injectVehicleSearchItems(vehicle);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }
        //StopThePed.API.Functions.callTowService();
        internal static void callTowService()
        {
            try
            {
                StopThePed.API.Functions.callTowService();
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