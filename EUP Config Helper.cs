using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YobbinCallouts
{
    using Rage;
    using Rage.Native;

    /// <summary>
    /// Developed by PNWParksFan
    /// Based on code from PieRGud
    /// </summary>
    public class EUPOutfit
    {
        public string Description { get; private set; }
        public IReadOnlyDictionary<EPedComponents, Component> Components { get; private set; } = new Dictionary<EPedComponents, Component>();
        public IReadOnlyDictionary<EProps, Component> Props { get; private set; } = new Dictionary<EProps, Component>();
        public EGender PedGender { get; private set; }

        /// <summary>
        /// Creates a configuration based on specifically configured settings
        /// </summary>
        /// <param name="description"></param>
        /// <param name="gender"></param>
        /// <param name="componentConfig"></param>
        /// <param name="propConfig"></param>
        public EUPOutfit(string description, EGender gender, IDictionary<EPedComponents, Component> componentConfig, IDictionary<EProps, Component> propConfig)
        {
            PedGender = gender;
            Components = componentConfig.ToDictionary(x => x.Key, x => x.Value);
            Props = propConfig.ToDictionary(x => x.Key, x => x.Value);
            Description = description;
        }

        /// <summary>Searches a specific INI file for an outfit name</summary>
        /// <param name="INI">The INI file to search in</param>
        /// <param name="sectionName">The outfit name to search for</param>
        /// <param name="success">Whether the outfit was found</param>
        public EUPOutfit(InitializationFile INI, string sectionName, out bool success)
        {
            this.Description = sectionName;

            success = _initializePedFromINI(INI, sectionName);
        }

        /// <summary>
        /// Looks in EUP config files for an outfit with the specified name. 
        /// Looks in wardrobe.ini first, then presetoutfits.ini if not found
        /// </summary>
        /// <param name="sectionName">The config name to search for</param>
        /// <param name="success">If a matching configuration was found</param>
        public EUPOutfit(string sectionName, out bool success)
        {
            this.Description = sectionName;

            InitializationFile wardrobeINI = new InitializationFile(@"Plugins\EUP\wardrobe.ini");
            InitializationFile presetsINI = new InitializationFile(@"Plugins\EUP\presetoutfits.ini");

            bool wardrobeSuccess = _initializePedFromINI(wardrobeINI, sectionName);
            Game.LogTrivial("Found " + sectionName + " in wardrobe: " + wardrobeSuccess);
            if (wardrobeSuccess)
            {
                success = true;
            }
            else
            {
                bool presetSuccess = _initializePedFromINI(presetsINI, sectionName);
                success = presetSuccess;
                Game.LogTrivial("Found " + sectionName + " in presets: " + presetSuccess);
            }
        }

        private bool _initializePedFromINI(InitializationFile INI, string sectionName)
        {
            Description = sectionName;
            Game.LogTrivial("Reading ped configuration for " + sectionName + " from INI file " + INI.FileName);

            PedGender = INI.ReadEnum<EGender>(sectionName, "Gender", EGender.UNKNOWN);

            Dictionary<EPedComponents, Component> newProps = new Dictionary<EPedComponents, Component>();
            Components = _readComponentSettingsFromINI<EPedComponents>(INI, sectionName).ToDictionary(x => (EPedComponents)x.Key, x => x.Value);
            Props = _readComponentSettingsFromINI<EProps>(INI, sectionName).ToDictionary(x => (EProps)x.Key, x => x.Value);

            return INI.DoesSectionExist(sectionName);
        }

        private bool _convertIniEntryToVariations(string part, string compSetting, out Tuple<int, int> variation)
        {
            // Game.LogDebug("Attempting to convert " + compSetting + " to component entry");
            variation = Tuple.Create(0, 0);
            string[] compSettings = compSetting.Split(':');
            if (compSettings.Length != 2)
            {
                Game.LogTrivial("Setting for " + part + " does not match expected format of num:num");
                return false;
            }

            int drawableVar = 0;
            int textureVar = 0;

            bool success = true;
            success = success && int.TryParse(compSettings[0], out drawableVar);
            success = success && int.TryParse(compSettings[1], out textureVar);

            if (!success)
            {
                Game.LogTrivial("Setting for " + part + " contained invalid non-int values");
                return false;
            }
            else
            {
                // Decrease by 1 because EUP INI uses 1-indexed but game used 0-indexed
                drawableVar--;
                textureVar--;
                Game.LogTrivial("Successfully parsed component " + part + " to variation " + drawableVar + ", texture " + textureVar);
                variation = Tuple.Create(drawableVar, textureVar);
                return true;
            }
        }

        private IReadOnlyDictionary<Enum, Component> _readComponentSettingsFromINI<T>(InitializationFile INI, string sectionName)
        {
            Dictionary<Enum, Component> newComponents = new Dictionary<Enum, Component>();

            foreach (Enum component in Enum.GetValues(typeof(T)))
            {
                string compSetting = INI.ReadString(sectionName, component.ToString(), null);
                if (compSetting == null)
                {
                    Game.LogTrivial("No setting found for " + component);
                    continue;
                }


                Tuple<int, int> variation;
                bool success = _convertIniEntryToVariations(component.ToString(), compSetting, out variation);

                newComponents.Add(component, new Component(component, variation.Item1, variation.Item2));
            }

            return newComponents;
        }

        private Model _getModelForGender()
        {
            switch (PedGender)
            {
                case EGender.Male:
                    return new Model("MP_M_FREEMODE_01");
                case EGender.Female:
                    return new Model("MP_F_FREEMODE_01");
                default:
                    return new Model("ERROR_INVALID_GENDER");
            }
        }

        /// <summary>
        /// Applies this outfit to the player's ped
        /// </summary>
        /// <param name="allowChangeModel">Whether to allow changing the player's model. 
        /// This will cause Game.LocalPlayer.Character to return a new ped, force the player out of any vehicle, and clear all tasks.</param>
        /// <param name="allowChangeModelInVehicle">If true and the model must be changed, the new player ped will be warped back into their previous 
        /// vehicle in the same seat as before. If false, configuration will fail if the model must be changed and the ped is in a vehicle.</param>
        /// <param name="restoreWeapons">If the model must be changed, restores the player's weapon inventory after changing the model.</param>
        /// <returns>True if the configuration is successful</returns>
        public bool ApplyOutfitToPlayer(bool allowChangeModel, bool allowChangeModelInVehicle = true, bool restoreWeapons = true, bool randomizeFace = false)
        {
            Model requiredModel = _getModelForGender();
            Vehicle prevVehicle = Game.LocalPlayer.Character.CurrentVehicle;
            int prevSeat = -1;
            var inventoryBefore = Game.LocalPlayer.Character.Inventory.Weapons.ToDictionary(w => w.Hash, w => w.Ammo);


            if (Game.LocalPlayer.Model != requiredModel)
            {
                Game.LogTrivial("Player model " + Game.LocalPlayer.Model.Name + " does not match outfit model of " + requiredModel.Name);
                if (allowChangeModel)
                {
                    if (prevVehicle && !allowChangeModelInVehicle)
                    {
                        Game.LogTrivial("Cannot change player model in vehicle");
                        return false;
                    }
                }
                else
                {
                    Game.LogTrivial("Cannot apply outfit because models do not match");
                    return false;
                }

                Game.LogTrivial("Changing ped model to " + requiredModel.Name);
                if (!requiredModel.IsValid)
                {
                    Game.LogTrivial("Unable to change ped model to " + requiredModel.Name + " - model may be invalid or gender may not have been specified");
                    return false;
                }
                Game.LocalPlayer.Model = requiredModel;

                GameFiber.StartNew(delegate
                {
                    if (prevVehicle)
                    {
                        Game.LocalPlayer.Character.WarpIntoVehicle(prevVehicle, prevSeat);
                    }

                    GameFiber.Sleep(300);
                    if (restoreWeapons)
                    {
                        foreach (var wpn in inventoryBefore)
                        {
                            Game.LogTrivial("Restoring weapon " + wpn.Key.ToString());
                            Game.LocalPlayer.Character.Inventory.GiveNewWeapon(wpn.Key, wpn.Value, false);
                            GameFiber.Yield();
                        }
                    }
                });

            }

            return ApplyOutfitToPed(Game.LocalPlayer.Character, randomizeFace);
        }

        public bool ApplyOutfitToPed(Ped ped, bool randomizeFace)
        {
            if (!ped)
            {
                Game.LogTrivial("Ped is invalid or does not exist");
                return false;
            }
            else if (ped.Model != _getModelForGender())
            {
                Game.LogTrivial("Ped model " + ped.Model.Name + " does not match requirement for specified gender " + PedGender.ToString());
                return false;
            }

            Game.LogTrivial("Applying outfit " + Description + " to ped");

            // Clear props before starting 
            NativeFunction.Natives.CLEAR_ALL_PED_PROPS(ped);

            // Assign all specified props
            foreach (var prop in Props)
            {
                NativeFunction.Natives.SET_PED_PROP_INDEX(ped, (int)prop.Key, prop.Value.DrawableVariation, prop.Value.TextureVariation, 0);
            }

            // Assign all known components
            foreach (var comp in Components)
            {
                Game.LogTrivial("Setting component " + (int)comp.Key + " (" + comp.Key.ToString() + ") to model " + comp.Value.DrawableVariation + ", texture " + comp.Value.TextureVariation);
                ped.SetVariation((int)comp.Key, comp.Value.DrawableVariation, comp.Value.TextureVariation);
            }

            if (randomizeFace)
            {
                Game.LogTrivial("Randomizing face");
                RandomCharacter.RandomizeCharacter(ped);
            }

            return true;
        }

        /// <summary>
        /// Spawns a new ped and configures with the loaded outfit
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <param name="heading"></param>
        /// <param name="success">Whether the ped was successfully spawned and configured. May return false if a ped was created but could not be configured.</param>
        /// <returns>The ped created</returns>
        public Ped Spawn(Vector3 spawnPosition, float heading, out bool success, bool randomizeFace = true)
        {
            Ped ped;
            Model pedModel = _getModelForGender();
            if (!pedModel.IsValid || !pedModel.IsPed)
            {
                Game.LogTrivial("No valid ped model found for specified gender");
                success = false;
                return null;
            }
            else
            {
                Game.LogTrivial("Spawning " + pedModel.Name + " at " + spawnPosition.ToString());
                ped = new Ped(pedModel, spawnPosition, heading);
                success = ApplyOutfitToPed(ped, randomizeFace);
                return ped;
            }
        }
    }

    public struct Component
    {
        public int DrawableVariation { get; private set; }
        public int TextureVariation { get; private set; }

        public Component(Enum component, int drawableVariation, int textureVariation)
        {
            this.DrawableVariation = drawableVariation;
            this.TextureVariation = textureVariation;
        }
    }

    public enum EGender
    {
        Male,
        Female,
        UNKNOWN
    }

    public enum EProps
    {
        Hat,
        Glasses,
        Ear,
        Watch = 6,
    }

    public enum EPedComponents
    {
        Head,
        Mask,
        Hair,
        UpperSkin,
        Pants,
        Parachute,
        Shoes,
        Accessories,
        UnderCoat,
        Armor,
        Decal,
        Top
    }
}