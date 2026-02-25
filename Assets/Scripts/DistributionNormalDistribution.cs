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
            // 1. 범위의 확률(CDF) 구간 계산
            // NegativeInfinity의 CDF는 0, PositiveInfinity의 CDF는 1이므로 안전하게 동작함
            float pMin = (float)Normal.CDF(0, 1, min);
            float pMax = (float)Normal.CDF(0, 1, max);

            // 2. 해당 확률 구간 내에서 랜덤 값 추출
            float randomProbability = UnityEngine.Random.Range(pMin, pMax);

            // 3. 확률을 다시 값으로 변환 (Inverse CDF)
            return (float)Normal.InvCDF(0, 1, randomProbability);
        }
    }
}