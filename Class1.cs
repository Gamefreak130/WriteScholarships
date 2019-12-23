using System;
using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Abstracts;
using Sims3.UI;
using Gamefreak130.WriteScholarshipsSpace;
//TODO Refactor
namespace Gamefreak130.Common
{
    public static class Methods
    {
        public static void AddInteraction(GameObject gameObject, InteractionDefinition singleton)
        {
            foreach (InteractionObjectPair iop in gameObject.Interactions)
            {
                if (iop.InteractionDefinition.GetType() == singleton.GetType())
                {
                    return;
                }
            }
            if (gameObject.ItemComp != null && gameObject.ItemComp.InteractionsInventory != null)
            {
                foreach (InteractionObjectPair iop in gameObject.ItemComp.InteractionsInventory)
                {
                    if (iop.InteractionDefinition.GetType() == singleton.GetType())
                    {
                        return;
                    }
                }
            }
            gameObject.AddInteraction(singleton);
            gameObject.AddInventoryInteraction(singleton);
        }

        public static List<T> CloneList<T>(IEnumerable<T> old)
        {
            bool flag = old != null;
            List<T> result = flag ? new List<T>(old) : null;
            return result;
        }
    }
}

namespace Gamefreak130
{
    public static class WriteScholarships
    {
        [Tunable]
        private static bool kCJackB;

        static WriteScholarships()
        {
            World.OnWorldLoadFinishedEventHandler += new EventHandler(OnWorldLoadFinished);
            World.OnWorldQuitEventHandler += new EventHandler(OnWorldQuit);
        }

        private static void OnWorldQuit(object sender, EventArgs e)
        {
            TempDateSmall = SmallReuseDate;
            SmallReuseDate = DateAndTime.Invalid;
            TempDateMedium = MediumReuseDate;
            MediumReuseDate = DateAndTime.Invalid;
            TempDateLarge = LargeReuseDate;
            LargeReuseDate = DateAndTime.Invalid;
            TempProgressSmall = new Dictionary<ulong, float>(CurrentProgressSmall);
            CurrentProgressSmall.Clear();
            TempProgressMedium = new Dictionary<ulong, float>(CurrentProgressMedium);
            CurrentProgressMedium.Clear();
            TempProgressLarge = new Dictionary<ulong, float>(CurrentProgressLarge);
            CurrentProgressLarge.Clear();
        }

        private static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            if (Sims3.Gameplay.CAS.Household.ActiveHousehold != null)
            {
                InitInjection();
                return;
            }
            EventTracker.AddListener(EventTypeId.kEventSimSelected, new ProcessEventDelegate(OnSimSelected));
        }

        private static ListenerAction OnSimSelected(Event e)
        {
            if (Sims3.Gameplay.CAS.Household.ActiveHousehold != null)
            {
                InitInjection();
                return ListenerAction.Remove;
            }
            return ListenerAction.Keep;
        }

        private static void InitInjection()
        {
            foreach (Computer computer in Sims3.Gameplay.Queries.GetObjects<Computer>())
            {
                Common.Methods.AddInteraction(computer, WriteScholarship.Singleton);
            }
            Commands.sGameCommands.Unregister("ClearScholarshipData");
            Commands.sGameCommands.Register("ClearScholarshipData", "Clears world-wide scholarship progress and cooldowns.", Commands.CommandType.General, new CommandHandler(OnClearData));
            World.OnObjectPlacedInLotEventHandler += new EventHandler(OnObjectPlacedInLot);
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kSimDied, new ProcessEventDelegate(OnSimDied));
            EventTracker.AddListener(EventTypeId.kSimEnteredVacationWorld, new ProcessEventDelegate(OnTravel));
            EventTracker.AddListener(EventTypeId.kSimReturnedFromVacationWorld, new ProcessEventDelegate(OnTravel));
        }

        private static ListenerAction OnTravel(Event e)
        {
            SmallReuseDate = TempDateSmall;
            MediumReuseDate = TempDateMedium;
            LargeReuseDate = TempDateLarge;
            CurrentProgressLarge = TempProgressLarge;
            CurrentProgressMedium = TempProgressMedium;
            CurrentProgressSmall = TempProgressSmall;
            return ListenerAction.Keep;
        }

        private static void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            if (e is World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs)
            {
                GameObject computer = GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId);
                if (computer != null && computer is Computer)
                {
                    Common.Methods.AddInteraction(computer as Computer, WriteScholarship.Singleton);
                }
            }
        }

        private static ListenerAction OnObjectChanged(Event e)
        {
            try
            {
                if (e.TargetObject is Computer computer)
                {
                    Common.Methods.AddInteraction(computer, WriteScholarship.Singleton);
                }
            }
            catch (Exception)
            { 
            }
            return ListenerAction.Keep;
        }

        private static ListenerAction OnSimDied(Event e)
        {
            if (e.Actor != null)
            {
                ulong id = e.Actor.SimDescription.SimDescriptionId;
                CurrentProgressSmall.Remove(id);
                CurrentProgressMedium.Remove(id);
                CurrentProgressLarge.Remove(id);
            }
            return ListenerAction.Keep;
        }

        private static int OnClearData(object[] parameters)
        {
            CurrentProgressSmall.Clear();
            CurrentProgressMedium.Clear();
            CurrentProgressLarge.Clear();
            SmallReuseDate = new DateAndTime(0);
            MediumReuseDate = new DateAndTime(0);
            LargeReuseDate = new DateAndTime(0);
            return 1;
        }

        [PersistableStatic]
        private static Dictionary<ulong, float> CurrentProgressSmall = new Dictionary<ulong, float>();

        private static Dictionary<ulong, float> TempProgressSmall;

        [PersistableStatic]
        private static Dictionary<ulong, float> CurrentProgressMedium = new Dictionary<ulong, float>();

        private static Dictionary<ulong, float> TempProgressMedium;

        [PersistableStatic]
        private static Dictionary<ulong, float> CurrentProgressLarge = new Dictionary<ulong, float>();

        private static Dictionary<ulong, float> TempProgressLarge;

        [PersistableStatic]
        private static DateAndTime SmallReuseDate = DateAndTime.Invalid;

        private static DateAndTime TempDateSmall;

        [PersistableStatic]
        private static DateAndTime MediumReuseDate = DateAndTime.Invalid;

        private static DateAndTime TempDateMedium;

        [PersistableStatic]
        private static DateAndTime LargeReuseDate = DateAndTime.Invalid;

        private static DateAndTime TempDateLarge;
    }
}

namespace Gamefreak130.WriteScholarshipsSpace
{
    public class WriteScholarship : Computer.ComputerInteraction
    {
        public enum ScholarshipSize
        {
            Small,
            Medium,
            Large
        }
        
        [Tunable, TunableComment("Base amount of time in sim days the scholarship needs to cool down after completion")]
        public static int kBaseLengthofTimeForCooldown = 7;

        [Tunable, TunableComment("Maximum deviation from base cooldown time in sim days; must be less than base cooldown time")]
        public static float kCooldownTimeJitter = 1f;

        [Tunable, TunableComment("Skill points per minute of writing scholarships gained in Writing skill")]
        public static float kWritingPointsPerMinute = 6f;

        [Tunable, TunableComment("Range 0-100: Base percent chance in decimals of getting scholarship; Format: Small, Medium, Large")]
        public static float[] kBaseScholarshipChance = new float[]
        {
            15f,
            10f,
            5f
        };
        
        [Tunable, TunableComment("Range 0-100: Increase in percent chance of getting scholarship per level in Writing skill; Format: Small, Medium, Large")]
        public static float[] kScholarshipChanceIncrement = new float[]
        {
            6f,
            4f,
            2f
        };

        [Tunable, TunableComment("Amount in Simoleons of scholarship; Format: Small, Medium, Large")]
        public static int[] kScholarshipAmount = new int[]
        {
            500,
            1750,
            3000
        };

        [Tunable, TunableComment("Amount of time in sim minutes it takes to write scholarship; Format: Small, Medium, Large")]
        public static float[] kScholarshipWriteTime = new float[]
        {
            60f,
            150f,
            240f
        };

        public class Definition : InteractionDefinition<Sim, Computer, WriteScholarship>
        {
            public ScholarshipSize mScholarshipSize;

            public Definition()
            { }

            public Definition(ScholarshipSize size)
            {
                mScholarshipSize = size;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Computer target, List<InteractionObjectPair> results)
            {
                foreach (int size in Enum.GetValues(typeof(ScholarshipSize)))
                {
                    results.Add(new InteractionObjectPair(new Definition((ScholarshipSize)Enum.ToObject(typeof(ScholarshipSize), size)), iop.Target));
                }
                return;
            }

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
            {
                ulong simDescriptionId = actor.SimDescription.SimDescriptionId;
                switch (mScholarshipSize)
                {
                    case ScholarshipSize.Small:
                        return WriteScholarships.CurrentProgressSmall.ContainsKey(simDescriptionId) && (WriteScholarships.CurrentProgressSmall[simDescriptionId] > 0f)
                            ? LocalizeString("SmallInteractionContinuationName", new object[0])
                            : LocalizeString("SmallInteractionName", new object[0]);
                    case ScholarshipSize.Medium:
                        return WriteScholarships.CurrentProgressMedium.ContainsKey(simDescriptionId) && (WriteScholarships.CurrentProgressMedium[simDescriptionId] > 0f)
                            ? LocalizeString("MediumInteractionContinuationName", new object[0])
                            : LocalizeString("MediumInteractionName", new object[0]);
                    case ScholarshipSize.Large:
                        return WriteScholarships.CurrentProgressLarge.ContainsKey(simDescriptionId) && (WriteScholarships.CurrentProgressLarge[simDescriptionId] > 0f)
                            ? LocalizeString("LargeInteractionContinuationName", new object[0])
                            : LocalizeString("LargeInteractionName", new object[0]);
                    default:
                        return string.Empty;
                }
            }

            public override bool Test(Sim actor, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.IsComputerUsable(actor, true, false, isAutonomous) && (actor.SimDescription.Teen || (actor.OccupationAsAcademicCareer != null)))
                {
                    switch (mScholarshipSize)
                    {
                        case ScholarshipSize.Small:
                            if (SimClock.CurrentTicks < WriteScholarships.SmallReuseDate.Ticks)
                            {
                                greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString("OnCooldown", new object[] { Convert.ToInt32(SimClock.ConvertFromTicks(WriteScholarships.SmallReuseDate.Ticks - SimClock.CurrentTicks, TimeUnit.Days)) }));
                                return false;
                            }
                            break;
                        case ScholarshipSize.Medium:
                            if (SimClock.CurrentTicks < WriteScholarships.MediumReuseDate.Ticks)
                            {
                                greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString("OnCooldown", new object[] { Convert.ToInt32(SimClock.ConvertFromTicks(WriteScholarships.MediumReuseDate.Ticks - SimClock.CurrentTicks, TimeUnit.Days)) }));
                                return false;
                            }
                            break;
                        case ScholarshipSize.Large:
                            if (SimClock.CurrentTicks < WriteScholarships.LargeReuseDate.Ticks)
                            {
                                greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString("OnCooldown", new object[] { Convert.ToInt32(SimClock.ConvertFromTicks(WriteScholarships.LargeReuseDate.Ticks - SimClock.CurrentTicks, TimeUnit.Days)) }));
                                return false;
                            }
                            break;
                    }
                    return true;
                }
                return false;
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[] {
                    LocalizeString("Path", new object [0]) + Localization.Ellipsis
                };
            }
        }

        private DateAndTime mStartTime;

        private float mScholarshipWriteTime;

        private float mCurrentProgress;

        private static readonly InteractionDefinition Singleton = new Definition();

        public float GetCurrentProgress(Sim actor, Dictionary<ulong, float> progress)
        {
            ulong simDescriptionId = actor.SimDescription.SimDescriptionId;
            return progress.ContainsKey(simDescriptionId) ? progress[simDescriptionId] : 0f;
        }

        public void SetCurrentProgress(Sim actor, int value)
        {
            ulong simDescriptionId = actor.SimDescription.SimDescriptionId;
            switch (value)
            {
                case 0:
                    if (WriteScholarships.CurrentProgressSmall.ContainsKey(simDescriptionId))
                    {
                        if (mCurrentProgress != 0f)
                        {
                            WriteScholarships.CurrentProgressSmall[simDescriptionId] = mCurrentProgress;
                        }
                        else
                        {
                            WriteScholarships.CurrentProgressSmall.Remove(simDescriptionId);
                        }
                    }
                    else if (mCurrentProgress != 0f)
                    {
                        WriteScholarships.CurrentProgressSmall.Add(simDescriptionId, mCurrentProgress);
                    }
                    break;

                case 1:
                    if (WriteScholarships.CurrentProgressMedium.ContainsKey(simDescriptionId))
                    {
                        if (mCurrentProgress != 0f)
                        {
                            WriteScholarships.CurrentProgressMedium[simDescriptionId] = mCurrentProgress;
                        }
                        else
                        {
                            WriteScholarships.CurrentProgressMedium.Remove(simDescriptionId);
                        }
                    }
                    else if (mCurrentProgress != 0f)
                    {
                        WriteScholarships.CurrentProgressMedium.Add(simDescriptionId, mCurrentProgress);
                    }
                    break;

                case 2:
                    if (WriteScholarships.CurrentProgressLarge.ContainsKey(simDescriptionId))
                    {
                        if (mCurrentProgress != 0f)
                        {
                            WriteScholarships.CurrentProgressLarge[simDescriptionId] = mCurrentProgress;
                        }
                        else
                        {
                            WriteScholarships.CurrentProgressLarge.Remove(simDescriptionId);
                        }
                    }
                    else if (mCurrentProgress != 0f)
                    {
                        WriteScholarships.CurrentProgressLarge.Add(simDescriptionId, mCurrentProgress);
                    }
                    break;
            }
            
        }

        public static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString("Gamefreak130/LocalizedMod/WriteScholarship:" + name, parameters);
        }

        public static string LocalizeString(bool isFemale, string name, params object[] parameters)
        {
            return Localization.LocalizeString(isFemale, "Gamefreak130/LocalizedMod/WriteScholarship:" + name, parameters);
        }

        public override bool Run()
        {
            Definition definition = InteractionDefinition as Definition;
            float mScholarshipBaseChance = 15f;
            float mScholarshipChanceIncrement = 6f;
            int mScholarshipAmount = 500;
            switch (definition.mScholarshipSize)
            {
                case ScholarshipSize.Small:
                    mCurrentProgress = GetCurrentProgress(Actor, WriteScholarships.CurrentProgressSmall);
                    mScholarshipWriteTime = kScholarshipWriteTime[0];
                    mScholarshipBaseChance = kBaseScholarshipChance[0];
                    mScholarshipChanceIncrement = kScholarshipChanceIncrement[0];
                    mScholarshipAmount = kScholarshipAmount[0];
                    break;
                case ScholarshipSize.Medium:
                    mCurrentProgress = GetCurrentProgress(Actor, WriteScholarships.CurrentProgressMedium);
                    mScholarshipWriteTime = kScholarshipWriteTime[1];
                    mScholarshipBaseChance = kBaseScholarshipChance[1];
                    mScholarshipChanceIncrement = kScholarshipChanceIncrement[1];
                    mScholarshipAmount = kScholarshipAmount[1];
                    break;
                case ScholarshipSize.Large:
                    mCurrentProgress = GetCurrentProgress(Actor, WriteScholarships.CurrentProgressLarge);
                    mScholarshipWriteTime = kScholarshipWriteTime[2];
                    mScholarshipBaseChance = kBaseScholarshipChance[2];
                    mScholarshipChanceIncrement = kScholarshipChanceIncrement[2];
                    mScholarshipAmount = kScholarshipAmount[2];
                    break;
            }

            StandardEntry();
            if (!Target.StartComputing(this, Sims3.SimIFace.Enums.SurfaceHeight.Table, true))
            {
                StandardExit();
                return false;
            }
            Target.StartVideo(Computer.VideoType.WordProcessor);
            mStartTime = SimClock.CurrentTime();
            BeginCommodityUpdates();
            Skill element = Actor.SkillManager.AddElement(SkillNames.Writing);
            AnimateSim("WorkTyping");
            ProgressMeter.ShowProgressMeter(Actor, mCurrentProgress, ProgressMeter.GlowType.Weak);
            bool flag = DoLoop(ExitReason.Default, new InsideLoopFunction(LoopDel), null);
            ProgressMeter.HideProgressMeter(Actor, flag);
            if (flag)
            {
                mCurrentProgress = 0f;
                float @float = kBaseLengthofTimeForCooldown + RandomUtil.GetFloat(kCooldownTimeJitter * -1, kCooldownTimeJitter);
                string titleText = string.Empty;
                if (RandomUtil.RandomChance(mScholarshipBaseChance + (mScholarshipChanceIncrement * element.SkillLevel)))
                {
                    Audio.StartSound("sting_opp_success");
                    Actor.ModifyFunds(mScholarshipAmount);
                    Actor.BuffManager.AddElement(BuffNames.Winner, (Origin)ResourceUtils.HashString64("FromWinningScholarship"));
                    switch (definition.mScholarshipSize)
                    {
                        case ScholarshipSize.Small:
                            titleText = LocalizeString(Actor.IsFemale, "SuccessSmall", new object[] { Actor, mScholarshipAmount });
                            if (SimClock.CurrentTicks > WriteScholarships.SmallReuseDate.Ticks)
                            {
                                WriteScholarships.SmallReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                        case ScholarshipSize.Medium: 
                            titleText = LocalizeString(Actor.IsFemale, "SuccessMedium", new object[] { Actor, mScholarshipAmount });
                            if (SimClock.CurrentTicks > WriteScholarships.MediumReuseDate.Ticks)
                            {
                                WriteScholarships.MediumReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                        case ScholarshipSize.Large:
                            titleText = LocalizeString(Actor.IsFemale, "SuccessLarge", new object[] { Actor, mScholarshipAmount });
                            if (SimClock.CurrentTicks > WriteScholarships.LargeReuseDate.Ticks)
                            {
                                WriteScholarships.LargeReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                    }
                    Actor.ShowTNSIfSelectable(titleText, StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, Actor.ObjectId);
                }
                else
                {  
                    switch (definition.mScholarshipSize)
                    {
                        case ScholarshipSize.Small:
                            titleText = LocalizeString(Actor.IsFemale, "FailureSmall", new object[] { Actor });
                            if (SimClock.CurrentTicks > WriteScholarships.SmallReuseDate.Ticks)
                            {
                                WriteScholarships.SmallReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                        case ScholarshipSize.Medium:
                            titleText = LocalizeString(Actor.IsFemale, "FailureMedium", new object[] { Actor });
                            if (SimClock.CurrentTicks > WriteScholarships.MediumReuseDate.Ticks)
                            {
                                WriteScholarships.MediumReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                        case ScholarshipSize.Large:
                            titleText = LocalizeString(Actor.IsFemale, "FailureLarge", new object[] { Actor });
                            if (SimClock.CurrentTicks > WriteScholarships.LargeReuseDate.Ticks)
                            {
                                WriteScholarships.LargeReuseDate.Ticks = SimClock.CurrentTicks + SimClock.ConvertToTicks(@float, TimeUnit.Days);
                            }
                            break;
                    }
                    Actor.ShowTNSIfSelectable(titleText, StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, Actor.ObjectId);
                }
            }
            switch (definition.mScholarshipSize)
            {
                case ScholarshipSize.Small:
                    SetCurrentProgress(Actor, 0);
                    break;
                case ScholarshipSize.Medium:
                    SetCurrentProgress(Actor, 1);
                    break;
                case ScholarshipSize.Large:
                    SetCurrentProgress(Actor, 2);
                    break;
            }
            float points = SimClock.ElapsedTime(TimeUnit.Minutes, mStartTime) * kWritingPointsPerMinute;
            element.AddPoints(points);
            if (element.SkillPoints == 0f)
            {
                Actor.SkillManager.RemoveElement(SkillNames.Writing);
            }
            EndCommodityUpdates(flag);
            Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            StandardExit();
            return flag;
        }

        public void LoopDel(StateMachineClient smc, LoopData data)
        {
            mCurrentProgress += data.mDeltaTime / mScholarshipWriteTime;
            ProgressMeter.UpdateProgressMeter(Actor, mCurrentProgress, ProgressMeter.GlowType.Weak);
            if (mCurrentProgress >= 1f)
            {
                Actor.AddExitReason(ExitReason.Finished);
            }
        }
    }
}