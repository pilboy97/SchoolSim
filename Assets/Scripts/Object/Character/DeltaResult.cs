namespace Game.Object.Character
{
    public struct DeltaResult
    {
        public CharacterStats Stats;
        public RelationFloatDict Relation;

        public void Reset()
        {
            Stats = default;
            Relation ??= new RelationFloatDict();
            Relation.Clear();
        }
    }
}