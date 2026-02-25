using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class StrikeZoneDistribution : IDistribution
    {
        public float b = 1.37f;
        public float e = 0.8f;
        public float angle = 45f;
        
        private float F => b * e;
        private float A => b * Mathf.Sqrt(1 - e * e);
        
        public float Sample => CheckStrike() ? 1 : 0;

        public bool CheckStrike()
        {
            var focus = new Vector2(0, F);
            var focus2 = new Vector2(0, -F);
            var radius = new DistributionNormalDistribution(1, 0.1f).Sample;
            var point = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
            var dist = Vector2.Distance(focus, point);
            var dist2 = Vector2.Distance(focus2, point);

            return dist2 + dist <= 2 * b;
        }

        public float Priority {
            get
            {
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);
                var r = (A * b) / Mathf.Sqrt(b * b * cos * cos + A * A * sin * sin);

                return (float)MathNet.Numerics.Distributions.Normal.CDF(0,1,(r - 1) / 0.1);
            }
        }
    }
}