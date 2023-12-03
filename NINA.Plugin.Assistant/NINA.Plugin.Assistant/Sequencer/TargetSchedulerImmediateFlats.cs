﻿using Assistant.NINAPlugin.Database.Schema;
using Assistant.NINAPlugin.Plan;
using Assistant.NINAPlugin.Util;
using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Plugin.Assistant.Shared.Utility;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.NINAPlugin.Sequencer {

    [ExportMetadata("Name", "Target Scheduler Immediate Flats")]
    [ExportMetadata("Description", "Flats automation for Target Scheduler")]
    [ExportMetadata("Icon", "Scheduler.SchedulerSVG")]
    [ExportMetadata("Category", "Target Scheduler")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class TargetSchedulerImmediateFlats : TargetSchedulerFlatsBase {

        [ImportingConstructor]
        public TargetSchedulerImmediateFlats(IProfileService profileService,
                                    ICameraMediator cameraMediator,
                                    IImagingMediator imagingMediator,
                                    IImageSaveMediator imageSaveMediator,
                                    IImageHistoryVM imageHistoryVM,
                                    IFilterWheelMediator filterWheelMediator,
                                    IRotatorMediator rotatorMediator,
                                    IFlatDeviceMediator flatDeviceMediator) :
            base(profileService,
                 cameraMediator,
                 imagingMediator,
                 imageSaveMediator,
                 imageHistoryVM,
                 filterWheelMediator,
                 rotatorMediator,
                 flatDeviceMediator) { }

        public TargetSchedulerImmediateFlats(TargetSchedulerImmediateFlats cloneMe) : this(
            cloneMe.profileService,
            cloneMe.cameraMediator,
            cloneMe.imagingMediator,
            cloneMe.imageSaveMediator,
            cloneMe.imageHistoryVM,
            cloneMe.filterWheelMediator,
            cloneMe.rotatorMediator,
            cloneMe.flatDeviceMediator) {
            CopyMetaData(cloneMe);
        }

        private bool alwaysRepeatFlatSet = true;

        [JsonProperty]
        public bool AlwaysRepeatFlatSet {
            get => alwaysRepeatFlatSet;
            set {
                alwaysRepeatFlatSet = value;
                RaisePropertyChanged(nameof(AlwaysRepeatFlatSet));
            }
        }

        public override object Clone() {
            return new TargetSchedulerImmediateFlats(this);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {

            /* TODO: how are we going to handle a wall panel?  They can insert explicit slew instruction before this but
             * need to get back to target.  If they insert an implicit slew/center then the Target on parent may not be correct
             * since we're running this based on the previous plan?
             * 
             * ACTUALLY, it looks like we don't set the new target until AFTER this would have run so should be good but TEST
             */

            try {
                DisplayText = "Determining needed flats";
                List<LightSession> neededFlats = GetNeededFlats();
                if (neededFlats == null) {
                    DisplayText = "";
                    return;
                }

                LogTrainedFlatDetails();

                // Prep the flat device
                DisplayText = "Preparing flat device";
                await CloseCover(progress, token);
                await ToggleLight(true, progress, token);

                List<FlatSpec> takenFlats = new List<FlatSpec>();
                foreach (LightSession neededFlat in neededFlats) {
                    bool success = true;
                    if (!takenFlats.Contains(neededFlat.FlatSpec)) {
                        success = await TakeFlatSet(neededFlat.FlatSpec, false, progress, token);
                        if (success) {
                            takenFlats.Add(neededFlat.FlatSpec);
                        }
                    }
                    else {
                        TSLogger.Info($"TS Immediate Flats: flat already taken, skipping: {neededFlat}");
                    }

                    if (success) {
                        SaveFlatHistory(neededFlat);
                    }
                }

                DisplayText = "";
                Iterations = 0;
                CompletedIterations = 0;

                await ToggleLight(false, progress, token);
            }
            catch (Exception ex) {
                DisplayText = "";

                if (Utils.IsCancelException(ex)) {
                    TSLogger.Warning("TS Immediate Flats: sequence was canceled/interrupted");
                    Status = SequenceEntityStatus.CREATED;
                    token.ThrowIfCancellationRequested();
                }
                else {
                    TSLogger.Error($"Exception taking immediate flats: {ex.Message}:\n{ex.StackTrace}");
                }

                if (ex is SequenceEntityFailedException) {
                    throw;
                }

                throw new SequenceEntityFailedException($"exception taking immediate flats: {ex.Message}", ex);
            }

            return;
        }

        private List<LightSession> GetNeededFlats() {

            // Find parent TargetSchedulerContainer which should have the scheduler plan we need
            ISequenceContainer parent = Parent;
            while (parent != null) {
                if (parent is TargetSchedulerContainer) {
                    break;
                }

                parent = parent.Parent;
            }

            if (parent == null) {
                TSLogger.Error("TS Immediate Flats: failed to find TargetSchedulerContainer parent, instruction not placed correctly in sequence?");
                return null;
            }

            SchedulerPlan plan = (parent as TargetSchedulerContainer).previousSchedulerPlan;
            if (plan == null) {
                TSLogger.Error("TS Immediate Flats: failed to find previous plan on TargetSchedulerContainer parent, aborting flats");
                return null;
            }

            if (plan.WaitForNextTargetTime != null) {
                TSLogger.Error("TS Immediate Flats: previous plan is unexpectedly a wait, aborting flats");
                return null;
            }

            if (plan.PlanTarget.Project.FlatsHandling != Project.FLATS_HANDLING_IMMEDIATE) {
                string name = $"{plan.PlanTarget.Project.Name}/{plan.PlanTarget.Name}";
                Notification.ShowInformation($"TS Immediate Flats: project is not configured for immediate flats: {name}, skipping");
                TSLogger.Warning($"TS Immediate Flats: project {name} is not configured for immediate flats, aborting flats");
                return null;
            }

            FlatsExpert flatsExpert = new FlatsExpert();
            List<FlatSpec> flatSpecs = new List<FlatSpec>();

            // For each take exposure instruction, add the associated flat spec to the list if not already present
            foreach (IPlanInstruction instruction in plan.PlanInstructions) {
                Plan.PlanTakeExposure takeExposure = instruction as Plan.PlanTakeExposure;
                if (takeExposure != null) {
                    IPlanExposure exp = takeExposure.planExposure;
                    FlatSpec flatSpec = new FlatSpec(exp.FilterName,
                                                     GetGain(exp.Gain),
                                                     GetOffset(exp.Offset),
                                                     exp.BinningMode,
                                                     GetReadoutMode(exp.ReadoutMode),
                                                     GetCurrentRotation(),
                                                     plan.PlanTarget.ROI);

                    if (!flatSpecs.Contains(flatSpec)) {
                        flatSpecs.Add(flatSpec);
                    }
                }
            }

            List<LightSession> neededFlats = new List<LightSession>();
            DateTime lightSessionDate = flatsExpert.GetLightSessionDate(DateTime.Now);
            foreach (FlatSpec flatSpec in flatSpecs) {
                neededFlats.Add(new LightSession(plan.PlanTarget.DatabaseId, lightSessionDate, flatSpec));
            }

            // If always repeat is false, then remove where we've already taken a flat during this same light session
            if (!AlwaysRepeatFlatSet && neededFlats.Count > 0) {
                List<FlatHistory> takenFlats;
                using (var context = database.GetContext()) {
                    takenFlats = context.GetFlatsHistory(lightSessionDate)
                       .Where(fh => fh.TargetId == plan.PlanTarget.DatabaseId)
                       .ToList();
                }

                neededFlats = flatsExpert.CullFlatsByHistory(neededFlats, takenFlats);
            }

            if (neededFlats.Count == 0) {
                TSLogger.Info("TS Immediate Flats: no flats needed");
                return null;
            }

            TSLogger.Info($"TS Immediate Flats: need {neededFlats.Count} flat sets for target: {plan.PlanTarget.Name}");
            return neededFlats;
        }

        private int GetGain(int? gain) {
            return (int)(gain == null ? cameraMediator.GetInfo().DefaultGain : gain);
        }

        private int GetOffset(int? offset) {
            return (int)((int)(offset == null ? cameraMediator.GetInfo().DefaultOffset : offset));
        }

        private int GetReadoutMode(int? readoutMode) {
            return (int)((int)(readoutMode == null ? cameraMediator.GetInfo().ReadoutMode : readoutMode));
        }

    }
}