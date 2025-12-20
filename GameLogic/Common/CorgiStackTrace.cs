namespace IdleCs.GameLogic
{
    public struct CorgiStackTrace
    {
        private Dungeon _dungeon;

        public CorgiStackTrace(Dungeon dungeon)
        {
            _dungeon = dungeon;
            _dungeon.StackCount++;
        }

        public bool IsValid()
        {
            if (_dungeon.StackCount > 1000)
            {
                return false;
            }

            return true;
        }

        public void Finish()
        {
            _dungeon.StackCount--;
        }
        
    }
}