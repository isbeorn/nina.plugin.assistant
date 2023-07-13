﻿using Assistant.NINAPlugin.Astrometry;
using Assistant.NINAPlugin.Database.Schema;
using NINA.Astrometry;
using NINA.Core.Model.Equipment;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;

namespace Assistant.NINAPlugin.Plan {

    /// <summary>
    /// PlannerEmulator isolates the NINA Scheduler sequence instructions from the Planner, allowing comprehensive
    /// testing of the sequencer operation without having to have an working database or running planner that relies
    /// on the current time, nighttime circumstances, etc.
    /// </summary>
    public class PlannerEmulator {

        private static int CallNumber = 0; // per NINA invocation

        private DateTime atTime;
        private IProfile profileService;

        public PlannerEmulator(DateTime atTime, IProfile profileService) {
            this.atTime = atTime;
            this.profileService = profileService;
        }

        public SchedulerPlan GetPlan(IPlanTarget previousPlanTarget) {
            CallNumber++;
            SchedulerPlan plan;

            switch (CallNumber) {
                case 1: plan = WaitForTime(DateTime.Now.AddSeconds(80)); break;
                case 2: plan = Plan1(); break;
                case 3: plan = Plan2(); break;
                default:
                    CallNumber = 0;
                    return null;
            }

            plan.IsEmulator = true;
            return plan;
        }

        private SchedulerPlan WaitForTime(DateTime waitFor) {
            return new SchedulerPlan(waitFor);
        }

        private SchedulerPlan Plan1() {
            DateTime endTime = atTime.AddMinutes(5);
            TimeInterval timeInterval = new TimeInterval(atTime, endTime);

            IPlanProject planProject = new PlanProjectEmulator();
            planProject.Name = "P01";
            planProject.UseCustomHorizon = false;
            planProject.MinimumAltitude = 10;
            planProject.DitherEvery = 0;
            planProject.EnableGrader = true;

            IPlanTarget planTarget = GetBasePlanTarget("T01", planProject, Cp5n5);
            planTarget.EndTime = endTime;

            IPlanExposure lum = GetExposurePlan("Lum", 4, null, null, 3, 14);
            lum.ReadoutMode = 0;
            IPlanExposure red = GetExposurePlan("R", 4, null, null, 3, 15);
            IPlanExposure grn = GetExposurePlan("G", 4, null, null, 3, 16);
            IPlanExposure blu = GetExposurePlan("B", 4, null, null, 3, 17);
            planTarget.ExposurePlans.Add(lum);
            planTarget.ExposurePlans.Add(red);
            planTarget.ExposurePlans.Add(grn);
            planTarget.ExposurePlans.Add(blu);

            List<IPlanInstruction> instructions = new List<IPlanInstruction>();
            instructions.Add(new PlanMessage("planner emulator: Plan1"));
            //instructions.Add(new PlanBeforeTargetContainer());
            instructions.Add(new PlanSetReadoutMode(lum));
            instructions.Add(new PlanSwitchFilter(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanDither());
            instructions.Add(new PlanSwitchFilter(red));
            instructions.Add(new PlanTakeExposure(red));
            instructions.Add(new PlanTakeExposure(red));
            instructions.Add(new PlanTakeExposure(red));
            instructions.Add(new PlanDither());
            /*
            instructions.Add(new PlanSwitchFilter(grn));
            instructions.Add(new PlanTakeExposure(grn));
            instructions.Add(new PlanTakeExposure(grn));
            instructions.Add(new PlanTakeExposure(grn));
            instructions.Add(new PlanSwitchFilter(blu));
            instructions.Add(new PlanTakeExposure(blu));
            instructions.Add(new PlanTakeExposure(blu));
            instructions.Add(new PlanTakeExposure(blu));
            instructions.Add(new PlanSwitchFilter(lum));
            instructions.Add(new PlanTakeExposure(lum));
            */

            return new SchedulerPlan(planTarget, timeInterval, instructions);
        }

        private SchedulerPlan Plan2() {
            DateTime endTime = atTime.AddMinutes(5);
            TimeInterval timeInterval = new TimeInterval(atTime, endTime);

            IPlanProject planProject = new PlanProjectEmulator();
            planProject.Name = "M31 Andromeda";
            planProject.UseCustomHorizon = false;
            planProject.MinimumAltitude = 10;
            planProject.DitherEvery = 0;
            planProject.EnableGrader = false;

            IPlanTarget planTarget = GetBasePlanTarget("M31 Andromeda Panel 1", planProject, Cp1525);
            planTarget.EndTime = endTime;

            IPlanExposure lum = GetExposurePlan("Lum", 4, null, null, 3, 101);
            lum.ReadoutMode = 0;
            IPlanExposure red = GetExposurePlan("R", 4, null, null, 3, 102);
            IPlanExposure grn = GetExposurePlan("G", 4, null, null, 3, 103);
            IPlanExposure blu = GetExposurePlan("B", 4, null, null, 3, 104);
            planTarget.ExposurePlans.Add(lum);
            planTarget.ExposurePlans.Add(red);
            planTarget.ExposurePlans.Add(grn);
            planTarget.ExposurePlans.Add(blu);

            List<IPlanInstruction> instructions = new List<IPlanInstruction>();
            instructions.Add(new PlanMessage("planner emulator: Plan2"));
            //instructions.Add(new PlanBeforeTargetContainer());
            instructions.Add(new PlanSetReadoutMode(lum));
            instructions.Add(new PlanSwitchFilter(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanSwitchFilter(red));
            instructions.Add(new PlanTakeExposure(red));
            instructions.Add(new PlanSwitchFilter(grn));
            instructions.Add(new PlanTakeExposure(grn));
            instructions.Add(new PlanSwitchFilter(blu));
            instructions.Add(new PlanTakeExposure(blu));
            instructions.Add(new PlanDither());
            instructions.Add(new PlanSwitchFilter(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanDither());

            return new SchedulerPlan(planTarget, timeInterval, instructions);
        }

        private SchedulerPlan Plan3() {

            DateTime endTime = atTime.AddMinutes(5);
            TimeInterval timeInterval = new TimeInterval(atTime, endTime);

            IPlanProject planProject = new PlanProjectEmulator();
            planProject.Name = "P03";
            planProject.UseCustomHorizon = false;
            planProject.MinimumAltitude = 10;
            planProject.DitherEvery = 0;
            planProject.EnableGrader = true;
            planProject.DatabaseId = 2;

            IPlanTarget planTarget = GetBasePlanTarget("T03", planProject, Cp1525);
            planTarget.EndTime = endTime;

            IPlanExposure lum = GetExposurePlan("Lum", 4, null, null, 3, 1);
            IPlanExposure red = GetExposurePlan("R", 4, null, null, 3, 2);
            //IPlanExposure grn = GetExposurePlan("G", 4, null, null, 3, 103);
            //IPlanExposure blu = GetExposurePlan("B", 4, null, null, 3, 104);
            planTarget.ExposurePlans.Add(lum);

            List<IPlanInstruction> instructions = new List<IPlanInstruction>();
            instructions.Add(new PlanMessage("planner emulator: Plan3"));
            instructions.Add(new PlanSwitchFilter(lum));
            instructions.Add(new PlanTakeExposure(lum));
            instructions.Add(new PlanTakeExposure(lum));
            //instructions.Add(new PlanTakeExposure(lum));
            //instructions.Add(new PlanTakeExposure(lum));

            instructions.Add(new PlanSwitchFilter(red));
            instructions.Add(new PlanTakeExposure(red));
            instructions.Add(new PlanTakeExposure(red));
            //instructions.Add(new PlanTakeExposure(red));
            //instructions.Add(new PlanTakeExposure(red));

            ///instructions.Add(new PlanSwitchFilter(grn));
            ///instructions.Add(new PlanTakeExposure(grn));
            ///instructions.Add(new PlanTakeExposure(grn));
            //instructions.Add(new PlanTakeExposure(grn));
            //instructions.Add(new PlanTakeExposure(grn));

            ///instructions.Add(new PlanSwitchFilter(blu));
            ///instructions.Add(new PlanTakeExposure(blu));
            ///instructions.Add(new PlanTakeExposure(blu));
            //instructions.Add(new PlanTakeExposure(blu));
            //instructions.Add(new PlanTakeExposure(blu));

            return new SchedulerPlan(planTarget, timeInterval, instructions);
        }

        private IPlanExposure GetExposurePlan(string name, int exposure, int? gain, int? offset, int desired, int databaseId) {
            PlanExposureEmulator planExposure = new PlanExposureEmulator();
            planExposure.DatabaseId = databaseId;
            planExposure.FilterName = name;
            planExposure.ExposureLength = exposure;
            planExposure.Gain = gain;
            planExposure.Offset = offset;
            planExposure.ReadoutMode = 0;
            planExposure.BinningMode = new BinningMode(1, 1);
            planExposure.Desired = desired;
            return planExposure;
        }

        private IPlanTarget GetBasePlanTarget(string name, IPlanProject planProject, Coordinates coordinates) {
            IPlanTarget planTarget = new PlanTargetEmulator();
            planTarget.Project = planProject;
            planTarget.Name = name;
            planTarget.Coordinates = coordinates;
            planTarget.Rotation = 16;
            return planTarget;
        }

        public static readonly Coordinates Cp5n5 = new Coordinates(AstroUtil.HMSToDegrees("5:0:0"), AstroUtil.DMSToDegrees("-5:0:0"), Epoch.J2000, Coordinates.RAType.Degrees);
        public static readonly Coordinates Cp1525 = new Coordinates(AstroUtil.HMSToDegrees("15:0:0"), AstroUtil.DMSToDegrees("25:0:0"), Epoch.J2000, Coordinates.RAType.Degrees);
    }

    class PlanProjectEmulator : IPlanProject {

        public string PlanId { get; set; }
        public int DatabaseId { get; set; }
        public string Name { get; set; }
        public bool UseCustomHorizon { get; set; }
        public double MinimumAltitude { get; set; }
        public int DitherEvery { get; set; }
        public bool EnableGrader { get; set; }

        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ProjectState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ProjectPriority Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? ActiveDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? InactiveDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? StartDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? EndDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IPlanTarget> Targets { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HorizonDefinition HorizonDefinition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Rejected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RejectedReason { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ProjectState IPlanProject.State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ProjectPriority IPlanProject.Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int MinimumTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double HorizonOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int MeridianWindow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int FilterSwitchFrequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, double> RuleWeights { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /*
            pp.SetDefaults();
            pp.MinimumAltitude = 10;
            pp.EnableGrader = enableGrader;
            pp.DitherEvery = ditherEvery;
         */

        public PlanProjectEmulator() {
            this.PlanId = Guid.NewGuid().ToString();
        }
    }

    class PlanTargetEmulator : IPlanTarget {

        public string PlanId { get; set; }
        public int DatabaseId { get; set; }
        public string Name { get; set; }
        public IPlanProject Project { get; set; }
        public Coordinates Coordinates { get; set; }
        public double Rotation { get; set; }
        public double ROI { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<IPlanExposure> ExposurePlans { get; set; }

        public Epoch Epoch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Rejected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RejectedReason { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CulminationTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeInterval MeridianWindow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PlanTargetEmulator() {
            this.PlanId = Guid.NewGuid().ToString();
            this.ExposurePlans = new List<IPlanExposure>();
        }

        public void SetCircumstances(bool isVisible, DateTime startTime, DateTime culminationTime, DateTime endTime) {
            throw new NotImplementedException();
        }
    }

    class PlanExposureEmulator : IPlanExposure {

        public string PlanId { get; set; }
        public string FilterName { get; set; }
        public double ExposureLength { get; set; }
        public int? Gain { get; set; }
        public int? Offset { get; set; }
        public BinningMode BinningMode { get; set; }
        public int? ReadoutMode { get; set; }
        public int Desired { get; set; }
        public int Acquired { get; set; }
        public int Accepted { get; set; }
        public IPlanTarget PlanTarget { get; set; }
        public int DatabaseId { get; set; }

        public bool Rejected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RejectedReason { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int PlannedExposures { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TwilightLevel TwilightLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool MoonAvoidanceEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double MoonAvoidanceSeparation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int MoonAvoidanceWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double MaximumHumidity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PlanExposureEmulator() {
            this.PlanId = Guid.NewGuid().ToString();
        }

        public bool IsIncomplete() {
            throw new NotImplementedException();
        }

        public int NeededExposures(double exposureThrottlePercentage) {
            throw new NotImplementedException();
        }
    }
}
