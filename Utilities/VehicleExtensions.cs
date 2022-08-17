using System;
using System.Collections.Generic;
using Rage;

namespace YobbinCallouts.Utilities
{
    public static class VehicleExtensions
    {
        private static readonly string[] DoorMappings =
            { "door_dside_f", "door_pside_f", "door_dside_r", "door_pside_r", "bonnet", "boot" };

        public static void OpenDoor(this Vehicle vehicle, Doors door, bool openInstantly = false)
        {
            var index = (int)door;
            if (vehicle.Doors[index].IsValid() &&
                vehicle.HasBone(
                    DoorMappings[
                        index])) // These checks may not be necessary. Need to clarify with a test at some point.
                vehicle.Doors[index].Open(false);
        }

        public static void OpenDoors(this Vehicle vehicle, Doors[] doors, bool openInstantly = false)
        {
            foreach (var door in doors)
            {
                var index = (int)door;
                if (vehicle.Doors[index].IsValid() && vehicle.HasBone(DoorMappings[index]))
                    vehicle.Doors[index].Open(false);
            }
        }

        public static void OpenAllDoors(this Vehicle vehicle, bool openInstantly = false)
        {
            for (int doorIndex = 0; doorIndex < 6; doorIndex++)
                if (vehicle.Doors[doorIndex].IsValid() && vehicle.HasBone(DoorMappings[doorIndex]))
                    vehicle.Doors[doorIndex].Open(false);
        }

        public static void OpenDoorsForSearch(this Vehicle vehicle, bool openInstantly = false)
        {
            for (int doorIndex = 0; doorIndex < 6; doorIndex++)
            {
                if (doorIndex == 4) continue;
                if (vehicle.Doors[doorIndex].IsValid() && vehicle.HasBone(DoorMappings[doorIndex]))
                    vehicle.Doors[doorIndex].Open(false);
            }
        }

        public static void CloseDoor(this Vehicle vehicle, Doors door, bool closeInstantly = false)
        {
            var index = (int)door;
            if (vehicle.Doors[index].IsValid() &&
                vehicle.HasBone(
                    DoorMappings[
                        index])) // These checks may not be necessary. Need to clarify with a test at some point.
                vehicle.Doors[index].Close(false);
        }

        public static void CloseDoors(this Vehicle vehicle, Doors[] doors, bool closeInstantly = false)
        {
            foreach (var door in doors)
            {
                var index = (int)door;
                if (vehicle.Doors[index].IsValid() && vehicle.HasBone(DoorMappings[index]))
                    vehicle.Doors[index].Close(false);
            }
        }

        public static void CloseAllDoors(this Vehicle vehicle, bool closeInstantly = false)
        {
            for (int doorIndex = 0; doorIndex < 6; doorIndex++)
                if (vehicle.Doors[doorIndex].IsValid() && vehicle.HasBone(DoorMappings[doorIndex]))
                    vehicle.Doors[doorIndex].Close(false);
        }

        public static void CloseDoorsForSearch(this Vehicle vehicle, bool closeInstantly = false)
        {
            for (int doorIndex = 0; doorIndex < 6; doorIndex++)
            {
                if (doorIndex == 4) continue;
                if (vehicle.Doors[doorIndex].IsValid() && vehicle.HasBone(DoorMappings[doorIndex]))
                    vehicle.Doors[doorIndex].Close(false);
            }
        }

        public enum Doors
        {
            FrontLeft = 0,
            FrontRight = 1,
            BackLeft = 2,
            BackRight = 3,
            Hood = 4,
            Trunk = 5
        }
    }
}