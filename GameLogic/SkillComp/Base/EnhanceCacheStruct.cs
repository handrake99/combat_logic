namespace IdleCs.GameLogic
{
    public class EnhanceCacheStruct
    {
        public float AbsoloteValue = 0 ;
        public float PercentPlusValue = 1f;
        public float PercentMinusValue = 1f;

        public void AddEnhance(float absoluteValue, float percentPlusValue, float percentMinusValue)
        {
            AbsoloteValue += absoluteValue;
            PercentPlusValue += percentPlusValue;
            PercentMinusValue *= percentMinusValue;

        }
    }
}