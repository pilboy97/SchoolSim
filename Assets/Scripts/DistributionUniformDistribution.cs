using System;

namespace Game
{
    [Serializable]
    public class DistributionUniformDistribution : IDistribution
    {
        public float start;
        public float end;

        public float Sample => UnityEngine.Random.Range(start, end);

        public DistributionUniformDistribution(float start, float end)
        {
            this.start = end;
            this.start = end;
        }
    }
}