//To Create your own INI file, pull a pro gamer move and simply copy/paste a random INI file and rename it to what you want to call it.

using Rage;
using System.Windows.Forms;

namespace YobbinCallouts
{
    internal static class Config
    {
        public static readonly InitializationFile INIFile = new InitializationFile(@"Plugins\LSPDFR\YobbinCallouts.ini");

        //All Callouts
        public static readonly bool BrokenDownVehicle = INIFile.ReadBoolean("Callouts", "Broken Down Vehicle", true);
        public static readonly bool AssaultOnBus = INIFile.ReadBoolean("Callouts", "Assault On Bus", true);
        public static readonly bool TrafficBreak = INIFile.ReadBoolean("Callouts", "Traffic Break", true);
        public static readonly bool PhotographyOfPrivateProperty = INIFile.ReadBoolean("Callouts", "Photography of Private Property", true);
        public static readonly bool PropertyCheck = INIFile.ReadBoolean("Callouts", "Property Checkup", true);
        public static readonly bool StolenPoliceHardware = INIFile.ReadBoolean("Callouts", "Stolen Police Hardware", true);
        public static readonly bool Arson = INIFile.ReadBoolean("Callouts", "Arson", true);
        public static readonly bool BarFight = INIFile.ReadBoolean("Callouts", "Bar Fight", true);
        public static readonly bool BaitCar = INIFile.ReadBoolean("Callouts", "Bait Car", true);
        public static readonly bool RoadRage = INIFile.ReadBoolean("Callouts", "Road Rage", true);
        public static readonly bool StolenCellPhone = INIFile.ReadBoolean("Callouts", "Stolen Cell Phone", true);
        public static readonly bool SovereignCitizen = INIFile.ReadBoolean("Callouts", "Sovereign Citizen", true);
        public static readonly bool ActiveShooter = INIFile.ReadBoolean("Callouts", "Active Shooter", true);
        public static readonly bool CitizenArrest = INIFile.ReadBoolean("Callouts", "Citizen's Arrest", true);
        public static readonly bool HumanTrafficking = INIFile.ReadBoolean("Callouts", "Human Trafficking", true);
        public static readonly bool WeaponFound = INIFile.ReadBoolean("Callouts", "Weapon Found", true);
        public static readonly bool HospitalEmergency = INIFile.ReadBoolean("Callouts", "Hospital Emergency", true);
        //public static readonly bool DUIReported = INIFile.ReadBoolean("Callouts", "DUI Reported", true);
        public static readonly bool LandlordTenantDispute = INIFile.ReadBoolean("Callouts", "Landlord-Tenant Dispute", true);
        //public static readonly bool StolenMail = INIFile.ReadBoolean("Callouts", "Stolen Mail", true);
        public static readonly bool PedestrianHitByVehicle = INIFile.ReadBoolean("Callouts", "Pedestrian Hit By Vehicle", true);
        //public static readonly bool PersonWithWeapon = INIFile.ReadBoolean("Callouts", "Person With a Weapon", true);

        //All keys
        public static readonly Keys MainInteractionKey = INIFile.ReadEnum<Keys>("Keys", "Main Key", Keys.Y);
        public static readonly Keys CalloutEndKey = INIFile.ReadEnum<Keys>("Keys", "Callout End Key", Keys.End);
        public static readonly Keys InvestigationEndKey = INIFile.ReadEnum<Keys>("Keys", "Investigation End Key", Keys.PageUp);
        public static readonly Keys Key1 = INIFile.ReadEnum<Keys>("Keys", "First Option", Keys.Z);
        public static readonly Keys Key2 = INIFile.ReadEnum<Keys>("Keys", "Second Option", Keys.X);

        //Callout-wide settings
        public static readonly bool DisplayHelp = INIFile.ReadBoolean("Miscellaneous", "Display Help Messages", true);
        public static readonly bool RunInvestigations = INIFile.ReadBoolean("Miscellaneous", "Run Investigations", true);
        public static readonly bool LeaveCalloutsRunning = INIFile.ReadBoolean("Miscellaneous", "Leave Callouts Running", false);

        //Settings for Individual Callouts
        public static readonly string PoliceVehicle = INIFile.ReadString("Vehicle", "Police Vehicle", "POLICE");

        public static readonly bool CallFD = INIFile.ReadBoolean("Arson", "Automatically Call Fire Department", true);

        public static readonly string BaitVehicle = INIFile.ReadString("Bait Car", "Bait Vehicle", "None");
    }
}