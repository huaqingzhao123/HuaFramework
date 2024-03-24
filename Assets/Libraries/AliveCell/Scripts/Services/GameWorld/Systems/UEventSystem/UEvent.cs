using System;
using XMLib;

namespace AliveCell
{
    public abstract class UEvent : IPoolItem<Type>
    {
        public GameWorld world { get; set; }

        public int ID { get; set; }

        public SuperLogHandler LogHandler => world.uevt.LogHandler;

        public abstract void Execute();

        public virtual void Reset()
        {
            ID = UObjectSystem.noneID;
            world = null;
        }

        #region pool

        public Type poolTag => this.GetType();

        public bool inPool { get; set; }

        public virtual void OnPopPool()
        {
        }

        public virtual void OnPushPool()
        {
            Reset();
        }

        #endregion pool
    }
}