using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace UseMinifiedBuildings
{
    public class UseMinifiedBuildingsSettings : ModSettings
    {
        public int MaxHaulDistance = 0;

        public bool EnableForQualityBuildings = true;

        private readonly QualityBuilderCompatibility qualityBuilderCompatibility;

        public UseMinifiedBuildingsSettings()
        {
            qualityBuilderCompatibility = new QualityBuilderCompatibility();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MaxHaulDistance, "maxHaulDistance", 0);
            Scribe_Values.Look(ref EnableForQualityBuildings, "enableForQualityBuildings", true);
        }

        public bool IsTooFar(int distance)
        {
            return MaxHaulDistance != 0 && distance > MaxHaulDistance;
        }

        public QualityCategory GetMinQuality(ThingDef thingDef, Map map)
        {
            if (!qualityBuilderCompatibility.IsQualityBuilderBuilding(thingDef)) return QualityCategory.Awful;

            return qualityBuilderCompatibility.GetMinQuality(map);
        }
    }

    class QualityBuilderCompatibility
    {
        private readonly MethodInfo getDefaultMinQualitySettingMethod;
        public QualityBuilderCompatibility()
        {
            if (LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == "hatti.qualitybuilder"))
            {
                var ns = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "QualityBuilder");
                var typ = ns?.GetTypes().FirstOrDefault(t => t.Name == "QualityBuilderModSettings");
                getDefaultMinQualitySettingMethod = typ?.GetMethod("getDefaultMinQualitySetting", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (getDefaultMinQualitySettingMethod == null)
                {
                    Log.Warning("UseMinifiedBuildings - QualityBuilder integration failed: couldn't find the quality function. Defaulting to setting min quality to awful.");
                }
                else
                {
                    Log.Message("UseMinifiedBuildings - QualityBuilder integration activated");
                }
            }
        }

        public QualityCategory GetMinQuality(Map map)
        {
            if (getDefaultMinQualitySettingMethod == null) return QualityCategory.Awful;

            var cat = getDefaultMinQualitySettingMethod.Invoke(null, new[] { map });
            return cat is QualityCategory category ? category : QualityCategory.Awful;
        }

        public bool IsQualityBuilderBuilding(ThingDef thingDef)
        {
            return thingDef.comps.Any(c => c.compClass.Name == "CompQualityBuilder");
        }
    }
}
