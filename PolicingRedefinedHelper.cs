using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YobbinCallouts
{
    internal class PolicingRedefinedFunctions
    {

        // PED IMPAIRMENT

        /// <summary>
        /// Gets the mood of the specified ped during questioning.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>The ped's mood, or default(EPedMood) if unavailable.</returns>
        internal static PolicingRedefined.Interaction.Assets.PedAttributes.EPedMood GetPedMood(Ped ped)
        {
            try
            {
                return PolicingRedefined.API.PedAPI.GetPedMood(ped);
            }
            catch (FileNotFoundException)
            {
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Gets whether the specified ped is drunk.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>True if drunk, false otherwise.</returns>
        internal static bool IsPedDrunk(Ped ped)
        {
            try
            {
                return PolicingRedefined.API.PedAPI.IsPedDrunk(ped);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the blood alcohol concentration (BAC) of the specified ped.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>The ped's BAC, or 0f if unavailable.</returns>
        internal static float GetPedBAC(Ped ped)
        {
            try
            {
                return PolicingRedefined.API.PedAPI.GetPedBAC(ped);
            }
            catch (FileNotFoundException)
            {
                return 0f;
            }
            catch (Exception)
            {
                return 0f;
            }
        }

        /// <summary>
        /// Gets whether the specified ped is high.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>True if high, false otherwise.</returns>
        internal static bool IsPedHigh(Ped ped)
        {
            try
            {
                return PolicingRedefined.API.PedAPI.IsPedHigh(ped);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the drugs the specified ped is on.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>Flags representing the drugs the ped is on, or default(EDrugType) if unavailable.</returns>
        internal static PolicingRedefined.Interaction.Assets.EDrugType GetDrugsPedIsOn(Ped ped)
        {
            try
            {
                return PolicingRedefined.API.PedAPI.GetDrugsPedIsOn(ped);
            }
            catch (FileNotFoundException)
            {
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Sets the mood of the specified ped during questioning.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="mood"></param>
        internal static void SetPedMood(Ped ped, PolicingRedefined.Interaction.Assets.PedAttributes.EPedMood mood)
        {
            try
            {
                PolicingRedefined.API.PedAPI.SetPedMood(ped, mood);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Makes the specified ped drunk.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="drunkLevel"></param>
        /// <param name="setWalkstyle">Whether to apply the drunk walkstyle.</param>
        internal static void SetPedDrunk(Ped ped, PolicingRedefined.Interaction.Assets.PedAttributes.EDrunkLevel drunkLevel, bool setWalkstyle = true)
        {
            try
            {
                PolicingRedefined.API.PedAPI.SetPedDrunk(ped, drunkLevel, setWalkstyle);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Makes the specified ped drunk by changing their BAC.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="amountInBac"></param>
        /// <param name="setWalkstyle">Whether to apply the drunk walkstyle.</param>
        internal static void SetPedDrunk(Ped ped, float amountInBac, bool setWalkstyle = true)
        {
            try
            {
                PolicingRedefined.API.PedAPI.SetPedDrunk(ped, amountInBac, setWalkstyle);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Makes the specified ped high on a random drug.
        /// </summary>
        /// <param name="ped"></param>
        internal static void SetPedHigh(Ped ped)
        {
            try
            {
                PolicingRedefined.API.PedAPI.SetPedHigh(ped);
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception)
            {

            }
        }

        //PED INVENTORY

        // ==============================
        // 🔍 SEARCH ITEM CONSTRUCTORS
        // ==============================

        /// <summary>
        /// Creates a SearchItem found on a ped.
        /// </summary>
        internal static PolicingRedefined.Interaction.Assets.SearchItem CreateSearchItem(string item, Ped foundOn, PolicingRedefined.Interaction.Assets.EItemChance chance = PolicingRedefined.Interaction.Assets.EItemChance.Normal)
        {
            try
            {
                return new PolicingRedefined.Interaction.Assets.SearchItem(item, foundOn, chance);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a SearchItem found in a vehicle.
        /// </summary>
        internal static PolicingRedefined.Interaction.Assets.SearchItem CreateSearchItem(string item, PolicingRedefined.Interaction.Assets.EItemLocation itemLocation, Vehicle foundIn, PolicingRedefined.Interaction.Assets.EItemChance chance = PolicingRedefined.Interaction.Assets.EItemChance.Normal)
        {
            try
            {
                return new PolicingRedefined.Interaction.Assets.SearchItem(item, itemLocation, foundIn, chance);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // ==============================
        // 💊 DRUG ITEM CONSTRUCTORS
        // ==============================

        /// <summary>
        /// Creates a DrugItem found on a ped.
        /// </summary>
        internal static PolicingRedefined.Interaction.Assets.DrugItem CreateDrugItem(string item, Ped foundOn, PolicingRedefined.Interaction.Assets.EDrugType drugType, PolicingRedefined.Interaction.Assets.EItemChance chance = PolicingRedefined.Interaction.Assets.EItemChance.Normal)
        {
            try
            {
                return new PolicingRedefined.Interaction.Assets.DrugItem(item, foundOn, drugType, chance);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a DrugItem found in a vehicle.
        /// </summary>
        internal static PolicingRedefined.Interaction.Assets.DrugItem CreateDrugItem(string item, PolicingRedefined.Interaction.Assets.EItemLocation itemLocation, Vehicle foundIn, PolicingRedefined.Interaction.Assets.EDrugType drugType, PolicingRedefined.Interaction.Assets.EItemChance chance = PolicingRedefined.Interaction.Assets.EItemChance.Normal)
        {
            try
            {
                return new PolicingRedefined.Interaction.Assets.DrugItem(item, itemLocation, foundIn, drugType, chance);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // ==============================
        // 🔫 WEAPON ITEM CONSTRUCTORS
        // ==============================

        /// <summary>
        /// Creates a WeaponItem found on a ped.
        /// </summary>
        //internal static PolicingRedefined.Interaction.Assets.WeaponItem CreateWeaponItem(string item, Ped foundOn, string weaponModelId, PolicingRedefined.Interaction.Assets.EItemChance chance)
        //{
        //    try
        //    {

        //    }
        //    catch (FileNotFoundException)
        //    {

        //    }
        //    catch (Exception)
        //    {

        //    }
        //}
    }
}

