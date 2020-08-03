using Verse;

namespace UseMinifiedBuildings
{
    public class UseMinifiedBuildingsSettings : ModSettings
    {
        public int MaxHaulDistance = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MaxHaulDistance, "maxHaulDistance");
        }

        public bool IsTooFar(int distance)
        {
            return MaxHaulDistance != 0 && distance > MaxHaulDistance;
        }
    }
}
