using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace UseMinifiedBuildings
{
    [StaticConstructorOnStartup]
    public class UseMinifiedBuildings : Mod
    {
        public static UseMinifiedBuildingsSettings Settings;

        static UseMinifiedBuildings()
        {
            new Harmony("UseMinifiedBuildings").PatchAll(Assembly.GetExecutingAssembly());
        }

        public UseMinifiedBuildings(ModContentPack content) : base(content)
        {
            Settings = GetSettings<UseMinifiedBuildingsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard
            {
                ColumnWidth = 400f
            };
            listing.Begin(inRect);

            string buffer = Settings.MaxHaulDistance.ToString();
            listing.Label("Limit how many tiles the pawns will walk to get minified buildings. 0 means unlimited.");
            listing.IntEntry(ref Settings.MaxHaulDistance, ref buffer);

            listing.Gap(40f);

            listing.CheckboxLabeled("Enable for buildings with quality.", ref Settings.EnableForQualityBuildings);

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Use Minified Buildings";
    }
}
