using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace UseMinifiedBuildings.Patches
{
    // Intercept building blueprints and replace with install blueprints if there are minified options available
    [HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForBuild")]
    public class Patch_PlaceBlueprintForBuild
    {
        static bool Prefix(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff, ref Blueprint_Build __result)
        {
            if (faction?.IsPlayer != true) return true;
            var toInstall = GetClosestCandidate(sourceDef, center, map, faction, stuff);
            if (toInstall != null)
            {
                GenConstruct.PlaceBlueprintForInstall(toInstall, center, map, rotation, faction);

                // Rimworld 1.3 uses the result to set faction style for new buildings, but the style should already be set since it's an existing building.
                // Might miss style update if the player uninstalls unclaimed buildings, we will have to see when we can test with Ideology
                __result = CreateFakeBlueprintBuild(sourceDef, faction, stuff);
                return false;
            }
            return true;
        }

        private static Blueprint_Build CreateFakeBlueprintBuild(BuildableDef sourceDef, Faction faction, ThingDef stuff)
        {
            var blueprintBuild = (Blueprint_Build)ThingMaker.MakeThing(sourceDef.blueprintDef);
            blueprintBuild.SetFactionDirect(faction);
            blueprintBuild.stuffToUse = stuff;
            return blueprintBuild;
        }

        private static MinifiedThing GetClosestCandidate(BuildableDef sourceDef, IntVec3 center, Map map, Faction faction, ThingDef stuff)
        {
            var settings = UseMinifiedBuildings.Settings;
            var matches = map.listerThings.ThingsOfDef(ThingDefOf.MinifiedThing).OfType<MinifiedThing>()
                .Where(m => m.InnerThing.def == sourceDef && m.InnerThing.Stuff == stuff
                    && !m.IsForbidden(faction)
                    && InstallBlueprintUtility.ExistingBlueprintFor(m) == null
                    && (!m.TryGetQuality(out QualityCategory qc) || (settings.EnableForQualityBuildings && qc >= settings.GetMinQuality(sourceDef.frameDef, map))))
                .ToList();

            var minDist = int.MaxValue;
            MinifiedThing closest = null;
            foreach(var t in matches)
            {
                var dist = IntVec3Utility.ManhattanDistanceFlat(center, t.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = t;
                }
            }

            return settings.IsTooFar(minDist) ? null : closest;
        }
    }
}
