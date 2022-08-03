using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace YobbinCallouts
{
    class PedBackground
    {
        //PED RELATED
        private string fullName { get; }
        private DateTime Birthday { get;}
        private ArrayList MedicalProblems { get; set; }
        private string Gender { get; }
        Random monke = new Random();
        //GENERAL
        private ArrayList commonMedicalProblems;
        private ArrayList commonMentalHealthProblems;
        private Persona pedPersona;

        /// <summary>
        /// constructor..you know how those work catYes
        /// </summary>
        /// <param name="ped"></param>
        public PedBackground(Ped ped)
        {
            if (ped.Exists())
            {
                pedPersona = Functions.GetPersonaForPed(ped);
                fullName = pedPersona.FullName;
                Birthday = pedPersona.Birthday;
                Gender = pedPersona.Gender.ToString();
                MedicalProblems = new ArrayList();

            }
            
        }
        /// <summary>
        /// sets medical problems for escaped suspect using the commonMedicalProblems arraylist
        /// </summary>
        public void setMedicalProblemsForEscapedSuspect()
        {
            MedicalProblems.Clear();
            ArrayList CMP = (ArrayList)commonMedicalProblems.Clone();
            for (int i = 0; i < monke.Next(1, 4); i++)
            {
                int num = monke.Next(0, CMP.Count);
                MedicalProblems.Add(CMP[num]);
                CMP.Remove(num);
            }
        }
        public String toString()
        {
            return fullName;
        }

    }
}
