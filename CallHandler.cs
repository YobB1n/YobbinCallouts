// YOBBINCALLOUTS CALL HANDLER
// This class provides a variety of important helper functions for Callouts. Feel free to use with credit!
// TO-DO: * MORE HOUSES; especially east vinewood
//        * Refactor Store Handler

using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;

namespace YobbinCallouts
{
    class CallHandler
    {
        public static Vector3 HousePoint; //returned location for houses, hospitals, stores, etc
        public static bool isHouse = true; //if a location (house, hospital, store, etc) was returnred
        private static int count; //random counter for arrays
        private static string[] VehicleModels; //array of vehicle models for vehicle spawner

        //These are the animations for the Idle Actions (ped just standing around)
        //“amb@world_human_cop_idles”
        private static string[,] FemaleCopAnim = new string[,] {
            {"amb@world_human_cop_idles@female@base", "base"},
            {"amb@world_human_cop_idles@female@idle_a", "idle_a" },
            {"amb@world_human_cop_idles@female@idle_a", "idle_b" },
            {"amb@world_human_cop_idles@female@idle_a", "idle_c" },
            {"amb@world_human_cop_idles@female@idle_b", "idle_d" },
            {"amb@world_human_cop_idles@female@idle_b", "idle_e" },
        };
        private static string[,] MaleCopAnim = new string[,] {
            {"amb@world_human_cop_idles@male@base", "base"},
            {"amb@world_human_cop_idles@male@idle_a", "idle_a" },
            {"amb@world_human_cop_idles@male@idle_a", "idle_b" },
            {"amb@world_human_cop_idles@male@idle_a", "idle_c" },
            {"amb@world_human_cop_idles@male@idle_b", "idle_d" },
            {"amb@world_human_cop_idles@male@idle_b", "idle_e" },
        };
        private static string[,] FemaleRandoAnim = new string[,] {
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a"},
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_b"},
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_c"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_b"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_c"},
        };
        private static string[,] MaleRandoAnim = new string[,] {
            {"amb@world_human_hang_out_street@male_a@base", "base"},
            {"amb@world_human_hang_out_street@male_b@base", "base"},
            {"amb@world_human_hang_out_street@male_c@base", "base"},
        };

        //ArrayList of houses all around the map
        private static ArrayList HouseList = new ArrayList() { new Vector3(240.7677f, -1687.701f, 29.6996f), new Vector3(100.6926f, -1914.058f, 21.03957f), new Vector3(288.6435f, -1792.515f, 28.08904f),
        new Vector3(1250.818f, -1734.568f, 52.03207f), new Vector3(1354.907f, -1694.046f, 60.49123f), new Vector3(1362.024f, -1568.026f, 56.34648f), new Vector3(1221.362f, -668.7222f, 63.49313f),
        new Vector3(1010.55f, -418.9665f, 64.95395f), new Vector3(-1101.879f, -1536.912f, 4.579572f), new Vector3(-977.2473f, -1091.995f, 4.222562f), new Vector3(-1064.605f, -1057.521f, 6.411661f),
        new Vector3(-1031.352f, -903.0417f, 3.691091f), new Vector3(-1950.582f, -544.11022f, 14.7255f), new Vector3(-1901.605f, -585.9387f, 11.86937f), new Vector3(-1777.128f, -701.4404f, 10.52536f),
        new Vector3(-817.6935f, 177.9567f, 72.22254f), new Vector3(-896.5508f, -5.058554f, 43.79892f), new Vector3(-1106.531f, 421.4244f, 75.68616f), new Vector3(-933.4481f, 472.059f, 85.12269f),
        new Vector3(-678.85f, 512.1063f, 113.526f), new Vector3(-565.5161f, 525.6989f, 110.2012f), new Vector3(-972.2734f, 752.2137f, 176.3808f), new Vector3(-305.1205f, 431.0618f, 110.4823f),
        new Vector3(260.885f, 22.27959f, 88.12721f), new Vector3(1975.007f, 3816.095f, 33.42553f), new Vector3(1862.327f, 3853.849f, 36.27155f), new Vector3(1808.881f, 3907.963f, 33.73134f),
        new Vector3(1544.591f, 3721.3f, 34.62653f), new Vector3(1725.54f, 4642.25f, 43.87547f), new Vector3(1966.98f, 4634.148f, 41.1016f), new Vector3(-218.4349f, 6453.148f, 31.19829f),
        new Vector3(-365.8479f, 6341.065f, 29.84357f), new Vector3(-374.104f, 6190.625f, 31.72954f)};

        //Arraylist of all the hospitals all around the map
        private static ArrayList HospitalList = new ArrayList() { new Vector3(361.0359f, -585.4946f, 28.8267f), new Vector3(356.689f, -597.6279f, 28.78184f), new Vector3(-449.401f, -347.7617f, 34.50174f),
        new Vector3(-447.8303f, -334.3066f, 34.50184f), new Vector3(295.7652f, -1447.524f, 29.966f), new Vector3(341.2158f, -1398.245f, 32.50923f), new Vector3(1838.992f, 3673.217f, 34.27671f),
        new Vector3(1815.018f, 3679.552f, 34.27674f), new Vector3(-247.249f, 6330.457f, 32.42619f), new Vector3(1152.5f, -1526.501f, 34.84344f), new Vector3(1161.176f, -1536.283f, 39.39494f)};

        //Arraylist of stores/gas stations all around the map
        private static ArrayList StoreList = new ArrayList()
        {

        };
        public static ArrayList getHospitalList { get { return HospitalList; } }
        public static ArrayList getHouseList { get { return HouseList; } }



        //old and needs to be replaced.
        public static Vector3 GetStore()
        {
            isHouse = true;
            String Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            Game.LogTrivial("YOBBINCALLOUTS: STOREHANDLER: Attempting to Locate a Store in: " + Zone);
            if (Zone == "Davis" || Zone == "Stad" || Zone == "STRAW" || Zone == "Banning" || Zone == "RANCHO" || Zone == "ChamH" || Zone == "PBOX" || Zone == "LegSqu" || Zone == "SKID" || Zone == "TEXTI")
            {
                System.Random r3 = new System.Random(); //LSPD
                int House = r3.Next(1, 3);
                if (House == 1) { HousePoint = new Vector3(-47.29313f, -1758.671f, 29.42101f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }   //Davis
                if (House == 2) { HousePoint = new Vector3(289f, -1267f, 29.44f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }    //Straw
            }
            else if (Zone == "Cypre" || Zone == "Murri" || Zone == "EBuro" || Zone == "LMesa" || Zone == "Mirr" || Zone == "East_V")
            {
                System.Random r3 = new System.Random(); //La Mesa PD
                int House = r3.Next(1, 4);
                if (House == 1) { HousePoint = new Vector3(818f, -1039f, 26.75f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }    //LaMesa
                if (House == 2) { HousePoint = new Vector3(1211.76f, -1390f, 35.37f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }    //El Burro
                if (House == 3) { HousePoint = new Vector3(1164.94f, -324.3139f, 69.22092f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }    //Mirror Park

            }
            else if (Zone == "Vesp" || Zone == "VCana" || Zone == "Beach" || Zone == "DelSol" || Zone == "Koreat")
            {
                System.Random r3 = new System.Random(); //Vesupicci Beach PD
                int House = r3.Next(1, 3);
                if (House == 1) { HousePoint = new Vector3(-530f, -1220f, 18.45f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }   //both Koreat
                if (House == 2) { HousePoint = new Vector3(-711f, -917f, 19.21f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }
            }
            else if (Zone == "DeLBe" || Zone == "DelPe" || Zone == "Morn" || Zone == "PBluff" || Zone == "Movie")
            {
                HousePoint = new Vector3(-2073f, -327f, 13.32f);    //Pbluff gas
            }
            else if (Zone == "Rockf" || Zone == "Burton" || Zone == "Richm" || Zone == "Golf")
            {
                isHouse = false;
            }
            else if (Zone == "CHIL" || Zone == "Vine" || Zone == "DTVine" || Zone == "WVine" || Zone == "Alta" || Zone == "Hawick")
            {
                System.Random r3 = new System.Random(); //Vinewood PD
                int House = r3.Next(1, 3);
                if (House == 1) { HousePoint = new Vector3(527f, -151f, 57.46f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); } //Hawick
                if (House == 2) { HousePoint = new Vector3(643f, 264.4f, 103.3f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout."); }    //DT Vine
            }
            else if (Zone == "AirP") HousePoint = new Vector3(-1442f, -1993f, 13.164f); //airport LS customs
            else if (Zone == "Sandy" || Zone == "Alamo")
            {
                HousePoint = new Vector3(1959.956f, 3740.31f, 32.34f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout.");  //Sandy
            }
            else if (Zone == "GrapeS")
            {
                HousePoint = new Vector3(1696.867f, 4923.803f, 42.06f); Game.LogTrivial("YOBBINCALLOUTS: Found Store for Callout.");  //Grapessed

            }
            else if (Zone == "Harmo" || Zone == "Desrt")
            {
                HousePoint = new Vector3(); //Desert off Highway
            }
            else if (Zone == "Tatamo") HousePoint = new Vector3(2557.269f, 380.7113f, 108.6229f);
            else if (Zone == "ProcoB" || Zone == "PalFor" || Zone == "Paleto") HousePoint = new Vector3(-93f, 6410.87f, 31.65f);
            else if (Zone == "MTChil") HousePoint = new Vector3(1727.931f, 6415.5f, 35.037f);
            else if (Zone == "BhamCa" || Zone == "TongVaH" || Zone == "TongvaV" || Zone == "CHU") HousePoint = new Vector3(-3038f, 483.778f, 7.91f);   //banham
            else if (Zone == "NCHU" || Zone == "ArmyB" || Zone == "Lago") HousePoint = new Vector3(-2545.63f, 2316.986f, 33.21579f);   //Lago Zancudo
            else
            {
                HousePoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
                Game.LogTrivial("YOBBINCALLOUTS: STOREHANDLER: Player not near any Store.");
                isHouse = false;
            }
            Game.LogTrivial("YOBBINCALLOUTS: HOUSEHANDLER: Choosing Store at " + HousePoint + " in " + Zone);
            return HousePoint;
        }

        //plays a dialgoue in a List<string> format. Optionally, specify a ped and animation to use while the dialogue is playing.
        public static void Dialogue(List<string> dialogue, Ped animped = null, String animdict = "missfbi3_party_d", String animname = "stand_talk_loop_a_male1", float animspeed = -1, AnimationFlags animflag = AnimationFlags.Loop)
        {
            count = 0;
            while (count < dialogue.Count)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Config.MainInteractionKey))
                {
                    if (animped != null && animped.Exists())
                    {
                        try
                        {
                            animped.Tasks.PlayAnimation(animdict, animname, animspeed, animflag);
                        }
                        catch (Exception) { }
                    }
                    Game.DisplaySubtitle(dialogue[count]);
                    count++;
                }
            }
        }

        //Plays an idle animation, depending on if the Ped is a cop or not.
        public static void IdleAction(Ped ped, bool iscop)
        {
            if (ped != null && ped.Exists())
            {
                if (iscop)
                {
                    if (ped.IsFemale)
                    {
                        System.Random coco = new System.Random();
                        int animation = coco.Next(0, FemaleCopAnim.Length / 2);
                        ped.Tasks.PlayAnimation(FemaleCopAnim[animation, 0], FemaleCopAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                    else
                    {
                        System.Random coco = new System.Random();
                        int animation = coco.Next(0, MaleCopAnim.Length / 2);
                        //Game.LogTrivial("YOBBINCALLOUTS: There are "+MaleCopAnim.Length+"animations");
                        //Game.LogTrivial(MaleCopAnim[animation, 0]);
                        //Game.LogTrivial(MaleCopAnim[animation, 1]);
                        ped.Tasks.PlayAnimation(MaleCopAnim[animation, 0], MaleCopAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                }
                else
                {
                    if (ped.IsFemale)
                    {
                        System.Random coco = new System.Random();
                        int animation = coco.Next(0, FemaleRandoAnim.Length / 2);
                        ped.Tasks.PlayAnimation(FemaleRandoAnim[animation, 0], FemaleRandoAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                    else
                    {
                        System.Random coco = new System.Random();
                        int animation = coco.Next(0, MaleRandoAnim.Length / 2);
                        ped.Tasks.PlayAnimation(MaleRandoAnim[animation, 0], MaleRandoAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                }
            }
        }

        //spawns a vehicle at the position and heading.
        public static Vehicle SpawnVehicle(Vector3 SpawnPoint, float Heading)
        {
            VehicleModels = new string[63] {"asbo", "blista", "dilettante", "panto", "prairie", "cogcabrio", "exemplar", "f620", "felon", "felon2", "jackal", "oracle", "oracle2", "sentinel", "sentinel2",
            "zion", "zion2", "baller", "baller2", "baller3", "cavalcade", "fq2", "granger", "gresley", "habanero", "huntley", "mesa", "radi", "rebla", "rocoto", "seminole", "serrano", "xls", "asea", "asterope",
            "emporor", "fugitive", "ingot", "intruder", "premier", "primo", "primo2", "regina", "stanier", "stratum", "surge", "tailgater", "washington", "bestiagts", "blista2", "buffalo", "schafter2", "euros",
            "sadler", "bison", "bison2", "bison3", "burrito", "burrito2", "minivan", "minivan2", "paradise", "pony"};
            System.Random chez = new System.Random();
            int model = chez.Next(0, VehicleModels.Length);
            Game.LogTrivial("YOBBINCALLOUTS: VEHICLESPAWNER: Vehicle Model is " + VehicleModels[model]);
            var veh = new Vehicle(VehicleModels[model], SpawnPoint, Heading);
            veh.IsPersistent = true; //vehicle is marked as persistent by default
            return veh;
        }

        //knock on a door of a house and wait for response (with doorbell audio)
        public static void OpenDoor(Vector3 doorlocation, Ped resident = null, String residentmodel = "")
        {
            Game.DisplayHelp("Press ~y~" + Config.MainInteractionKey + "~w~ to ~b~Ring~w~ the Doorbell.");
            while (!Game.IsKeyDown(Config.MainInteractionKey)) GameFiber.Wait(0);
            Doorbell();
            GameFiber.Wait(2500);
            Game.LocalPlayer.HasControl = false;
            Game.FadeScreenOut(1500, true);
            if (resident != null) //if you want to spawn a resident
            {
                if (residentmodel != "") resident = new Ped(residentmodel, doorlocation, Game.LocalPlayer.Character.Heading - 180);
                else resident = new Ped(doorlocation, Game.LocalPlayer.Character.Heading - 180);
                resident.Heading = Game.LocalPlayer.Character.Heading - 180; //might not be needed
                IdleAction(resident, false);
            }
            GameFiber.Wait(1500);
            Game.FadeScreenIn(1500, true);
            Game.LocalPlayer.HasControl = true;
        }

        //plays a sound at any specific file location.
        public static void PlaySound(string SoundLocation)
        {
            try
            {
                Game.LogTrivial("YOBBINCALLOUTS: PLAYSOUNDHANDLER:" + SoundLocation + " - SOUND PLAY");
                System.Media.SoundPlayer sound = new System.Media.SoundPlayer();
                sound.SoundLocation = SoundLocation;
                GameFiber.StartNew(delegate
                {
                    try
                    {
                        sound.Load();
                        sound.Play();
                        GameFiber.Wait(4500);
                        sound.Stop();
                    }
                    catch (System.IO.FileNotFoundException) //most common error due to user error not installing sound file properly
                    {
                        Game.DisplayNotification("The ~b~Audio File~w~ for ~g~YobbinCallouts~w~ is ~r~not Installed Properly.~w~ Please ~b~Reinstall~w~ the Plugin Properly.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                        Game.LogTrivial("AUDIO FILE FOR YOBBINCALLOUTS NOT INSTALLED. PLEASE REINSTALL THE PLUGIN PROPERLY.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                    }
                });
            }
            catch (System.Threading.ThreadAbortException) { } //this error doesn't really matter in my experience so I don't log it
            catch (System.IO.FileNotFoundException)
            {
                Game.DisplayNotification("The ~b~Audio File~w~ for ~g~YobbinCallouts~w~ is ~r~not Installed Properly.~w~ Please ~b~Reinstall~w~ the Plugin Properly.");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                Game.LogTrivial("AUDIO FILE FOR YOBBINCALLOUTS NOT INSTALLED. PLEASE REINSTALL THE PLUGIN PROPERLY.");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
            }
            catch (Exception e) //any other error
            {
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                string error = e.ToString();
                Game.LogTrivial("ERROR: " + error);
                Game.LogTrivial("IN - YOBBINCALLOUTS SOUND PLAYER");
                Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                //Game.DisplayNotification("Error: ~r~" + error);
                Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
            }
        }
        //just for my callouts, plays a doorbell sound.
        public static void Doorbell()
        {
            System.Random chez = new System.Random();
            int model = chez.Next(0, 3);
            if (model == 0) PlaySound(@"lspdfr\audio\scanner\YobbinCallouts Audio\YC_DOORBELL1.wav");
            else if (model == 2) PlaySound(@"lspdfr\audio\scanner\YobbinCallouts Audio\YC_DOORBELL2.wav");
            else PlaySound(@"lspdfr\audio\scanner\YobbinCallouts Audio\YC_RINGDOORBELL.wav");
        }
        public static void SuspectWait(Ped Suspect) //test this
        {
            while (Suspect.Exists())
            {
                GameFiber.Yield();
                if (!Suspect.Exists() || Suspect.IsDead || Functions.IsPedArrested(Suspect)) break;
            }
            if (Suspect.IsAlive) //test all this (STP )
            {
                Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest.");
            }
            else
            {
                GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect is ~r~Dead.");
            }
            GameFiber.Wait(2000);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
            GameFiber.Wait(2000);
        }

        //finish this (use from hospital emergency callout)
        //~n~
        public static void PedInfo(Ped ped)
        {
            if (ped.Exists())
            {
                var name = Functions.GetPersonaForPed(ped).FullName;
                var birthday = Functions.GetPersonaForPed(ped).Birthday;
                var advisory = Functions.GetPersonaForPed(ped).AdvisoryText;
            }
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "", "", "");
        }

        //Assign blip to entity
        public static Blip AssignBlip(Entity entity, Color blipcolor, float scale = 1f, string name = "", bool route = false, float intensity = 1f)
        {
            try
            {
                Blip blip = entity.AttachBlip();
                blip.Color = blipcolor;
                blip.Scale = scale;
                if (name != "") blip.Name = name;
                blip.IsRouteEnabled = route;
                blip.Alpha = intensity;
                return blip;
            }
            catch (Exception e)
            {
                Game.LogTrivial("YOBBINCALLOUTS: Error assigning blip. Error: " + e);
                return null;
            }
        }
        public static Vector3 nearestLocationChooser(ArrayList list, float maxdistance = 1000, float mindistance = 25)
        {
            Vector3 closestLocation = (Vector3)list[0];
            float closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, (Vector3)list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                float distance = Vector3.Distance(Game.LocalPlayer.Character.Position, (Vector3)list[i]);
                if (distance <= maxdistance && distance >= mindistance)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, (Vector3)list[i]);
                        closestLocation = (Vector3)list[i];
                    }
                }
            }
            return closestLocation;
        }
    }
}