//THIS CALLOUT IS NOT FINISHED (or even started lol)
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System;

namespace YobbinCallouts.Callouts
{
    [CalloutInfo("Explosion", CalloutProbability.High)]
    public class Explosion : Callout
    {
        private Vector3 MainSpawnPoint;

        private Ped Suspect;
        private Ped Suspect2;
        private Ped Witness;
        private Ped[] Victims;
        Ped player = Game.LocalPlayer.Character;
        private string[] Suspects;

        private Blip SuspectBlip;
        private Blip Suspect2Blip;
        private Blip VictimBlip;
        private Blip AreaBlip;

        private int MainScenario;
        private string Zone;
        private bool CalloutRunning;

        private LHandle SuspectPursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS: Explosion Callout Start==========");
            System.Random r = new System.Random();
            int Scenario = r.Next(0, 0);    //change
            MainScenario = Scenario;
            Game.LogTrivial("YOBBINCALLOUTS: Scenario is " + MainScenario + "");

            if (MainScenario >= 0)
            {
                Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
                Game.LogTrivial("YOBBINCALLOUTS: Zone is " + Zone);
                MainSpawnPoint = World.GetNextPositionOnStreet(player.Position.Around(600f));
            }
            ShowCalloutAreaBlipBeforeAccepting(MainSpawnPoint, 75f);    //Callout Blip Circle with radius 50m
            AddMinimumDistanceCheck(25f, MainSpawnPoint);   //Player must be 25m or further away
            Functions.PlayScannerAudio("CITIZENS_REPORT YC_EXPLOSION");
            CalloutMessage = "Explosion";
            CalloutPosition = MainSpawnPoint;
            CalloutAdvisory = "Citizens Report an ~r~Explosion~w~. Several Reported ~o~Injured.";
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Explosion Callout Accepted by User.");
            if (Main.CalloutInterface)
            {
                CalloutInterfaceHandler.SendCalloutDetails(this, "CODE 3", "");
            }
            else
            {
                Game.DisplayNotification("Respond ~r~Code 3.");
            }

            System.Random monke = new System.Random();
            int victims = monke.Next(1, 5);
            Ped[] Randos = World.GetAllPeds();
            for (int i = 0; i < 25; i++)
            {
                GameFiber.Yield();
                if (Randos[i].Exists())
                {
                    if (Randos[i] != player && Randos[i] != Suspect)
                    {
                        Randos[i] = Victims[i];
                        Victims[i].IsPersistent = true;
                        Suspect.RelationshipGroup.SetRelationshipWith(Victims[i].RelationshipGroup, Relationship.Hate);
                        if(i <= victims) //victim
                        {
                            Victims[i].Position = MainSpawnPoint.Around(5);
                            Victims[i].BlockPermanentEvents = true;
                        }
                        else if(i <= victims + 1) //witnesses
                        {
                            Witness = Victims[i];
                            Witness.IsPersistent = true;
                            Witness.BlockPermanentEvents = true;
                        }
                        else //randos
                        {
                            Victims[i].Tasks.ReactAndFlee(Suspect);
                        }
                    }
                }
            }

            Suspects = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
            if (MainScenario == 0) //suspect still on scene
            {
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, Suspects.Length); //KEEP THIS ARRAY INDEXER LIKE THIS
                NativeFunction.Natives.xA0F8A7517A273C05<bool>(World.GetNextPositionOnStreet(MainSpawnPoint), 0, out Vector3 outPosition);
                Suspect = new Ped(Suspects[SuspectModel], outPosition, 69);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
            }

            AreaBlip = new Blip(MainSpawnPoint, 25);
            AreaBlip.Color = Color.Orange;
            AreaBlip.Alpha = 0.67f;
            AreaBlip.IsRouteEnabled = true;
            AreaBlip.Name = "Scene";

            if (CalloutRunning == false) Callout();
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("YOBBINCALLOUTS: Bar Fight Callout Not Accepted by User.");
            //Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL_01");  //this gets annoying after a while
            base.OnCalloutNotAccepted();
        }
        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning)
                    {
                        while (player.DistanceTo(Suspect) >= 25 && !Game.IsKeyDown(Config.CalloutEndKey)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Config.CalloutEndKey)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        Game.LogTrivial("YOBBINCALLOUTS: Player has arrived on scene.");

                        break;
                    }
                    Game.LogTrivial("YOBBINCALLOUTS: Callout Finished, Ending...");
                    EndCalloutHandler.EndCallout();
                    End();
                }
                catch (Exception e)
                {
                    if (CalloutRunning)
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                        Game.LogTrivial("IN: " + this);
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.DisplayNotification("There was an ~r~Error~w~ Caught with ~b~YobbinCallouts. ~w~Please Check Your ~g~Log File.~w~ Sorry for the Inconvenience!");
                        Game.DisplayNotification("Error: ~r~" + error);
                        Game.LogTrivial("If You Believe this is a Bug, Please Report it on my Discord Server. Thanks!");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT==========");
                    }
                    else
                    {
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                        Game.LogTrivial("No Need to Report This Error if it Did not Result in an LSPDFR Crash.");
                        Game.LogTrivial("==========YOBBINCALLOUTS: ERROR CAUGHT - CALLOUT NO LONGER RUNNING==========");
                    }
                    End();
                }
            }
            );
        }
        public override void End()
        {
            base.End();

            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            CalloutRunning = false;
            //if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            //if (Suspect2.Exists()) { Suspect2.Dismiss(); }
            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (AreaBlip.Exists()) { AreaBlip.Delete(); }
            Game.LogTrivial("YOBBINCALLOUTS: Bar Fight Callout Finished Cleaning Up.");
        }
        public override void Process()
        {
            base.Process();
        }
    }
}