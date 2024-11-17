// YobbinCallouts by YobB1n
// * If you actually took the time to read this this, I hope you can learn something :D 
// * Just know that I'm a terrible programmer with just one high school course as experience, things may be very messy/inneficient.
// * And messaging me on discord would be way better lol.

using System;
using System.Net;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Linq;

namespace YobbinCallouts
{
    public class Main : Plugin
    {
        public static Version NewVersion = new Version();
        public static Version curVersion = new Version("1.7.2");
        public static bool STP; //if STP is installed by the user
        public static bool UB; //if UB is installed by the user
        public static bool CalloutInterface; //if Callout Interface is installed by the user
        public static bool UpToDate; //if the Plugin is updated.
        public static bool Beta = false;

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("YOBBINCALLOUTS: YobbinCallouts " + curVersion + " by YobB1n has been loaded.");
        }
        public override void Finally()
        {
            Game.LogTrivial("YOBBINCALLOUTS: YobbinCallouts has been cleaned up.");
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                int num = (int)Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Yobbin Callouts", "~y~v." + curVersion + " ~b~by YobB1n", " ~g~Loaded Successfully. ~b~Enjoy!");
                GameFiber.StartNew(delegate
                {
                    Game.LogTrivial("YOBBINCALLOUTS: Player Went on Duty. Checking for Updates.");
                    try
                    {
                        Thread FetchVersionThread = new Thread(() =>
                        {
                            using (WebClient client = new WebClient())
                            {
                                try
                                {
                                    string s = client.DownloadString("http://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=29467&textOnly=1");

                                    NewVersion = new Version(s);
                                }
                                catch (Exception) { Game.LogTrivial("YOBBINCALLOUTS: LSPDFR Update API down. Aborting checks."); }
                            }
                        });
                        FetchVersionThread.Start();
                        try
                        {
                            while (FetchVersionThread.ThreadState != System.Threading.ThreadState.Stopped)
                            {
                                GameFiber.Yield();
                            }
                            // compare the versions  
                            if (curVersion.CompareTo(NewVersion) < 0)
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Finished Checking Yobbin Callouts for Updates.");
                                Game.LogTrivial("YOBBINCALLOUTS: Update Available for Yobbin Callouts. Installed Version " + curVersion + " ,New Version " + NewVersion);
                                Game.DisplayNotification("~g~Update Available~w~ for ~b~YobbinCallouts! Download at ~y~lcpdfr.com.");
                                Game.DisplayNotification("It is ~y~Strongly Recommended~w~ to~g~ Update~b~ YobbinCallouts. ~w~Playing on an Old Version ~r~May Cause Issues!");
                                Game.LogTrivial("====================YOBBINCALLOUTS WARNING====================");
                                Game.LogTrivial("Outdated YobbinCallouts Version. Please Update if You Experience Issues!!");
                                Game.LogTrivial("I'm not a Dick so I don't use a killswitch, however I strongly encourage you to update for the best experience.");
                                Game.LogTrivial("====================YOBBINCALLOUTS WARNING====================");
                                UpToDate = false;
                            }
                            else if (curVersion.CompareTo(NewVersion) > 0)
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: DETECTED BETA RELEASE. DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                Game.DisplayNotification("YOBBINCALLOUTS: ~r~DETECTED BETA RELEASE. ~w~DO NOT REDISTRIBUTE. PLEASE REPORT ALL ISSUES.");
                                UpToDate = true;
                                Beta = true;
                            }
                            else
                            {
                                Game.LogTrivial("YOBBINCALLOUTS: Finished Checking Yobbin Callouts for Updates.");
                                Game.DisplayNotification("You are on the ~g~Latest Version~w~ of ~b~YobbinCallouts.");
                                Game.LogTrivial("YOBBINCALLOUTS: Yobbin Callouts is Up to Date.");
                                UpToDate = true;
                            }
                        }
                        catch (Exception)
                        {
                            Game.LogTrivial("YOBBINCALLOUTS: Error while Processing Thread to Check for Updates.");
                        }
                    }
                    catch (Exception)
                    {
                        Game.LogTrivial("YOBBINCALLOUTS: Error while checking Yobbin Callouts for updates.");
                    }
                });
                RegisterCallouts();
            }
        }
        private static void RegisterCallouts()
        {
            Game.LogTrivial("==========YOBBINCALLOUTS INFORMATION==========");
            Game.LogTrivial("YobbinCallouts by YobB1n");
            Game.LogTrivial("Version " + curVersion + "");
            Game.LogTrivial("https://yobbinmods.com");
            Game.LogTrivial("Please Join My Discord Server to Report Bugs/Improvements: https://discord.gg/Wj522qa5mT. Enjoy!");

            if (Config.INIFile.Exists()) Game.LogTrivial("YobbinCallouts Config is Installed by User.");
            else Game.LogTrivial("YobbinCallouts Config is NOT Installed by User.");

            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("StopThePed")) == true)
            {
                Game.LogTrivial("StopThePed is Installed by User.");
                STP = true;
            }
            else
            {
                Game.LogTrivial("StopThePed is NOT Installed by User.");
                STP = false;
            }

            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("UltimateBackup")) == true)
            {
                Game.LogTrivial("UltimateBackup is Installed by User.");
                UB = true;
            }
            else
            {
                Game.LogTrivial("UltimateBackup is NOT Installed by User.");
                UB = false;
            }

            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("CalloutInterface")) == true)
            {
                Game.LogTrivial("CalloutInterface is Installed by User.");
                CalloutInterface = true;
            }
            else
            {
                Game.LogTrivial("CalloutInterface is NOT Installed by User.");
                CalloutInterface = false;
            }
            //CALLOUTS
            Game.LogTrivial("Started Registering Callouts.");
            if (Config.BrokenDownVehicle || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.BrokenDownVehicle));
            if (Config.AssaultOnBus || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.AssaultOnBus));
            if (Config.TrafficBreak || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.TrafficBreak));
            if (Config.PhotographyOfPrivateProperty || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.PhotographyOfPrivateProperty));
            if (Config.PropertyCheck || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.PropertyCheck));
            if (Config.StolenPoliceHardware || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.StolenPoliceHardware));
            if (Config.Arson || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.Arson));
            if (Config.BarFight || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.BarFight));
            if (Config.BaitCar || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.BaitCar));
            if (Config.RoadRage || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.RoadRage));
            if (Config.StolenCellPhone || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.StolenCellPhone));
            if (Config.SovereignCitizen || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.SovereignCitizen));
            if (Config.ActiveShooter || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.ActiveShooter));
            if (Config.HumanTrafficking || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.HumanTrafficking));
            if (Config.WeaponFound || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.WeaponFound));
            if (Config.HospitalEmergency || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.HospitalEmergency));
            if (Config.LandlordTenantDispute || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.LandlordTenantDispute));
            Game.LogTrivial("Finished Registering Callouts.");

            //BETA CALLOUTS
            //if (Beta)
            //{
            Game.LogTrivial("Started Registering Beta Callouts.");
            if (Config.CitizenArrest || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.CitizenArrest));
            //if (Config.DUIReported || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.DUIReported));
            //if (Config.StolenMail || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.StolenMail));
            if (Config.PedestrianHitByVehicle || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.PedestrianHitByVehicle));
            //if (Config.PersonWithWeapon || !Config.INIFile.Exists()) Functions.RegisterCallout(typeof(Callouts.PersonWithWeapon));
            Game.LogTrivial("Finished Registering Beta Callouts.");
            //}

            //INVESTIGATIONS
            if (Config.RunInvestigations) //not for now sadge
            {
                Game.LogTrivial("Started Registering Investigations.");
                //Functions.RegisterCallout(typeof(Callouts.StolenPoliceHardwareInvestigation)); (uncomment this)
                //Functions.RegisterCallout(typeof(Callouts.MissingPersonsInvestigation));
                Game.LogTrivial("More to come soon!");
                Game.LogTrivial("Finished Registering Investigations.");
            }
            Game.LogTrivial("==========YOBBINCALLOUTS INFORMATION==========");
        }
    }
}
