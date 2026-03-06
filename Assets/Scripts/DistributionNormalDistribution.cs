using System;
using MathNet.Numerics.Distributions;

namespace Game
{
    [Serializable]
    public struct DistributionNormalDistribution : IDistribution
    {
        public float mean;
        public float stddev;

        public float Sample => (float)MathNet.Numerics.Distributions.Normal.Sample(mean, stddev);

        public DistributionNormalDistribution(float mean, float stddev)
        {
            this.mean = mean;
            this.stddev = stddev;
        }
        
        public static float GetTruncatedNormal(float min, float max)
        {
            float pMin = (float)Normal.CDF(0, 1, min);
            float pMax = (float)Normal.CDF(0, 1, max);

            float randomProbability = UnityEngine.Random.Range(pMin, pMax);

            return (float)Normal.InvCDF(0, 1, randomProbability);
        }
    }
}