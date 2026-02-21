using System;

namespace Elementum.Project.Systems
{
    [Serializable]
    public sealed class SaveDataModel
    {
        public const int LatestVersion = 1;

        public int SaveVersion = LatestVersion;
        public int LastKnownMass;
        public int BestMass;
        public int TotalWins;
        public int StarDust;
        public string LastElementId = "H";
    }
}
