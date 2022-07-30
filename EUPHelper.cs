using System;
using System.Collections.Generic;
using Rage;
using Rage.Native;
using System.Reflection;

namespace YobbinCallouts
{

    /// <summary>
    /// MP ped facial randomization 
    /// Developed by Alex Braun, tyyy <3
    /// </summary>
    [Obfuscation(Exclude = true)]
    internal class RandomCharacter
    {

        public enum AppearanceIndex
        {
            Hairstyle,
            Haircolor,
            Eyebrows,
            Complexion,
            MolesFreckles,
            Aging,
            Eyecolor,
            FacialHair
        }
        public enum HeadOverlay
        {
            Blemishes,
            FacialHair,
            Eyebrows,
            Aging,
            Makeup,
            Blush,
            Complexion,
            SunDamage,
            Lipstick,
            MolesFreckles,
            ChestHair,
            BodyBlemishes,
            AddedBodyBlemishes
        }
        public enum HeritageIndex
        {
            Mother,
            Father,
            ShapeMix,
            SkinMix
        }

        private static Random Roll = new Random();

        public static void RandomizeCharacter(Ped ped)
        {
            List<dynamic> _heritage;
            List<dynamic> _facialFeatures;
            List<dynamic> _appearance;

            RandomizeCharacter(ped, out _heritage, out _facialFeatures, out _appearance);
        }

        public static void RandomizeCharacter(Ped ped, out List<dynamic> heritage, out List<dynamic> facialFeatures, out List<dynamic> appearance)
        {
            //Random Randomizer = new Random();

            heritage = new List<dynamic> { 0, 0, 0f, 0f };
            facialFeatures = new List<dynamic> { 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
            appearance = new List<dynamic> { 0, 0, 0, 0, 0, 0, 0, 0 };

            SetRandomFace(ped, ref heritage);
            SetRandomFacialFeatures(ped, ref facialFeatures);
            SetRandomAppearance(ped, ref appearance);
        }

        private static void SetRandomFace(Ped Ped, ref List<dynamic> Heritage)
        {

            if (Ped.Model == new Model("mp_m_freemode_01"))
                SetPedFace(Ped, Roll.Next(0), Roll.Next(20), (float)Roll.NextDouble(), (float)Roll.NextDouble(), ref Heritage);
            else if (Ped.Model == new Model("mp_f_freemode_01"))
                SetPedFace(Ped, Roll.Next(21, 41), Roll.Next(21, 41), (float)Roll.NextDouble(), (float)Roll.NextDouble(), ref Heritage);
        }

        private static void SetPedFace(Ped ped, int parent1, int parent2, float shapemix, float skinmix, ref List<dynamic> Heritage)
        {
            Heritage[0] = parent1;
            Heritage[1] = parent2;
            Heritage[2] = shapemix;
            Heritage[3] = skinmix;
            NativeFunction.Natives.x9414E18B9434C2FE(
                ped, parent1, parent2, 0, parent1, parent2, 0,
                shapemix, skinmix, 0f, false);
        }



        private static void SetRandomFacialFeatures(Ped Ped, ref List<dynamic> FacialFeatures)
        {
            float value;

            for (int i = 0; i < FacialFeatures.Count; i++)
            {
                int random = Roll.Next(2);

                if (random == 1)
                {
                    random = Roll.Next(2);
                    if (random == 0)
                        value = ((float)Roll.NextDouble() * -1f);
                    else value = (float)Roll.NextDouble();
                }
                else value = 0f;

                SetPedFacialFeature(Ped, i, value);
                FacialFeatures[i] = value;
            }
        }

        private static void SetPedFacialFeature(Ped ped, int facialFeature, float value)
        {
            NativeFunction.Natives.x71A5C1DBA060049E(ped, facialFeature, value);
        }


        private static void SetRandomAppearance(Ped ped, ref List<dynamic> Appearance)
        {
            SetRandomHair(ped, ref Appearance);

            if (Roll.Next(2) == 1)
            {
                Appearance[(int)AppearanceIndex.Complexion] = Roll.Next(5);
            }
            else Appearance[(int)AppearanceIndex.Complexion] = 255;
            NativeFunction.Natives.x48F44967FA05CC1E(ped, 6, Appearance[(int)AppearanceIndex.Complexion], 0.8f);

            if (Roll.Next(2) == 1)
            {
                Appearance[(int)AppearanceIndex.MolesFreckles] = Roll.Next(5);
            }
            else Appearance[(int)AppearanceIndex.MolesFreckles] = 255;
            NativeFunction.Natives.x48F44967FA05CC1E(ped, 9, Appearance[(int)AppearanceIndex.MolesFreckles], 0.8f);

            if (Roll.Next(2) == 1)
            {
                Appearance[(int)AppearanceIndex.Aging] = Roll.Next(5);
            }
            else Appearance[(int)AppearanceIndex.Aging] = 255;
            NativeFunction.Natives.x48F44967FA05CC1E(ped, 3, Appearance[(int)AppearanceIndex.Aging], 0.8f);

            // EyeColor
            Appearance[(int)AppearanceIndex.Eyecolor] = Roll.Next(7);
            NativeFunction.Natives.x50B56988B170AFDF(ped, Appearance[(int)AppearanceIndex.Eyecolor]);
        }

        private static void SetRandomHair(Ped ped, ref List<dynamic> Appearance)
        {
            int randomHair = 0; // Used for excluded items;

            // Banned Male Hairstyles
            List<int> excludedMaleHairItems = new List<int>
            {
                8,22,23,24,25,26,27,28,29,30
            };
            // Banned Female Hairstyles
            List<int> excludedFemaleHairItems = new List<int>
            {

            };

            int randomBrow = 0;
            int randomBeard = 0;
            bool bannedItemFound = false;

            if (ped.Model == new Model("mp_m_freemode_01"))
            {
                randomHair = Roll.Next(NativeFunction.CallByName<int>("GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS", ped, 2) - 1);
                randomBrow = Roll.Next(22);
                for (int i = 0; i < excludedMaleHairItems.Count; i++)
                {
                    if (randomHair == excludedMaleHairItems[i]) { bannedItemFound = true; }
                    if (randomHair != excludedMaleHairItems[i])
                    {
                        if (bannedItemFound)
                        {
                            bannedItemFound = true;
                        }
                        else bannedItemFound = false;
                    }
                }

                int beardChance = Roll.Next(10);

                if (beardChance <= 3) { randomBeard = Roll.Next(18); }
                else randomBeard = 255;


                if (bannedItemFound)
                    SetRandomHair(ped, ref Appearance);
                else
                {
                    Appearance[(int)AppearanceIndex.Hairstyle] = randomHair;
                    Appearance[(int)AppearanceIndex.Eyebrows] = randomBrow;
                    Appearance[(int)AppearanceIndex.FacialHair] = randomBeard;

                    NativeFunction.Natives.x262B14F48D29DE80(ped, 2, randomHair, 0, 2);
                    NativeFunction.Natives.x48F44967FA05CC1E(ped, 2, randomBrow, 1f);
                    NativeFunction.Natives.x48F44967FA05CC1E(ped, 1, randomBeard, 1f);
                    RandomHairColor(ped, ref Appearance);
                    return;
                }


            }
            else if (ped.Model == new Model("mp_f_freemode_01"))
            {
                randomHair = Roll.Next(1, NativeFunction.CallByName<int>("GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS", ped, 2) - 1);
                randomBrow = Roll.Next(22);
                for (int i = 0; i <= excludedFemaleHairItems.Count - 1; i++)
                {
                    if (randomHair == excludedFemaleHairItems[i]) { bannedItemFound = true; }
                    if (randomHair != excludedFemaleHairItems[i])
                    {
                        if (bannedItemFound) { bannedItemFound = true; }
                        else bannedItemFound = false;
                    }
                }

                if (bannedItemFound)
                    SetRandomHair(ped, ref Appearance);
                else
                {
                    Appearance[(int)AppearanceIndex.Hairstyle] = randomHair;
                    Appearance[(int)AppearanceIndex.Eyebrows] = randomBrow;
                    Appearance[(int)AppearanceIndex.FacialHair] = 255;

                    NativeFunction.Natives.x262B14F48D29DE80(ped, 2, randomHair, 0, 2);
                    NativeFunction.Natives.x48F44967FA05CC1E(ped, 2, randomBrow, 1f);
                    RandomHairColor(ped, ref Appearance);
                    return;
                }
            }
        }
        private static void RandomHairColor(Ped ped, ref List<dynamic> Appearance)
        {
            // Banned Haircolors:
            // List only goes until 27 to filter out 'unnatural'
            // hair colors likepink or green.
            List<int> excludedHairColors = new List<int>
            {
                20,21,22,23,24,25,26
            };
            bool bannedColor = false;

            int randomColor = Roll.Next(27);

            // Checks if the random haircolor is on the banned haircolor list.
            for (int i = 0; i < excludedHairColors.Count; i++)
            {
                if (randomColor == excludedHairColors[i])
                {
                    bannedColor = true;
                }
                if (randomColor != excludedHairColors[i])
                {
                    if (bannedColor)
                    {
                        bannedColor = true;
                    }
                    else bannedColor = false;
                }
            }
            // Checks if bannedColor was set to true, if so, repeat the randomization.
            if (bannedColor) { RandomHairColor(ped, ref Appearance); }
            else
            {
                Appearance[(int)AppearanceIndex.Haircolor] = randomColor;
                NativeFunction.Natives.x4CFFC65454C93A49(ped, Appearance[(int)AppearanceIndex.Haircolor], Appearance[(int)AppearanceIndex.Haircolor]);
                NativeFunction.Natives.x497BF74A7B9CB952(ped, (int)HeadOverlay.Eyebrows, 1, Appearance[(int)AppearanceIndex.Haircolor], Appearance[(int)AppearanceIndex.Haircolor]);
                NativeFunction.Natives.x497BF74A7B9CB952(ped, (int)HeadOverlay.FacialHair, 1, Appearance[(int)AppearanceIndex.Haircolor], Appearance[(int)AppearanceIndex.Haircolor]);
            }
        }



        /// <summary>
        /// !Helper Class! Can be ignored.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static String GetHashKey(String value)
        {
            return String.Format("0x{0:x2}", NativeFunction.CallByName<uint>("GET_HASH_KEY", value));
        }
    }
}