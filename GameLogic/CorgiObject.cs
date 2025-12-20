using Google.Protobuf;
using IdleCs.Utils;
using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network.NetLib;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class CorgiObject 
    {
        private string _objectId;
        private string _dbId = null;
        private ulong _uid;
        private bool _isLoaded;
        private bool _canReload = false;

        public string ObjectId
        {
            get { return _objectId; }
        }

        public string DBId
        {
            get { return _dbId; }
            protected set { _dbId = value; }
        }

        public virtual ulong Uid
        {
            get { return _uid; }
            protected set { _uid = value; }
        }

        public bool IsLoaded => _isLoaded;

        protected void SetReload()
        {
            _canReload = true;
            
        }

        protected EventManager EventManager;

        public CorgiObject(string oldId = null)
        {
            if (oldId == null)
            {
                _objectId = ObjectManager.Instance.GetId(this);
            }
            else
            {
                _objectId = oldId;
            }
        }

        /// <summary>
        /// Load 세개중 1개만 override해서 써야함
        /// Override한 Method 제외하고 호출하면 무조건 false가 되도록
        /// </summary>
        // load from datasheet(uid)
        public bool Load(ulong uid)
        {
            if (_canReload == false &&_isLoaded)
            {
                throw new CorgiException("Already loaded object {0}", _objectId);
            }

            if(LoadInternal(uid) == false)
            {
                return false;
            }
            
            _uid = uid;
            _isLoaded = true;
            return true;
        }

        public bool Load(CorgiSharedObject sObject)
        {
            if (_canReload == false &&_isLoaded)
            {
                throw new CorgiException("Already loaded object {0}", _objectId);
            }
            
            if(LoadInternal(sObject) == false)
            {
                return false;
            }
            
            _isLoaded = true;
            return true;
        }
        
        public bool Load(IMessage dbObject)
        {
            if (_canReload == false &&_isLoaded)
            {
                throw new CorgiException("Already loaded object {0}", _objectId);
            }
            
            if(LoadInternal(dbObject) == false)
            {
                return false;
            }
            
            _isLoaded = true;
            return true;
        }
        
        public bool Load(JObject json)
        {
            if (_canReload == false &&_isLoaded)
            {
                throw new CorgiException("Already loaded object {0}", _objectId);
            }
            
            if(LoadInternal(json) == false)
            {
                return false;
            }
            
            _isLoaded = true;
            return true;
        }
        
        
        /// <summary>
        /// load by datasheet uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        protected virtual bool LoadInternal(ulong uid)
        {
            _uid = uid;
            
            return true;
        }

        /// <summary>
        /// load by data
        /// </summary>
        /// <param name="sObject"></param>
        /// <returns></returns>
        protected virtual bool LoadInternal(CorgiSharedObject sObject)
        {
            _uid = sObject.uid;
            _dbId = sObject.dbId;
            
            return true;
        }
        
        protected virtual bool LoadInternal(IMessage dbObject)
        {
            return true;
        }

        protected virtual bool LoadInternal(JObject json)
        {
            return true;
        }
        
        public virtual void OnDestroy()
        {
        }

        public void RegisterEvent(CombatEventType eventType, EventManager.OnEventDelegate callback)
        {
            EventManager.Register(eventType, callback);
        }

        public virtual bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
        {
            if (EventManager == null)
            {
                return false;
            }

            return EventManager.OnEvent(eventType, eventParam, logNode);
        }

        public bool IsSame(string id)
        {
            if (_objectId == null)
            {
                throw new CorgiException("invalid object id instance : {0}", this.GetType().ToString());
            }
            return _objectId == id;
        }

        public virtual void Serialize(IPacketWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public virtual void DeSerialize(IPacketReader reader)
        {
            throw new System.NotImplementedException();
        }
    
//	public interface IGameObject 
//	{
//		bool InitObject(JSONObject json);
//
//		bool Load(JSONObject json);
//		bool Load(long code);
//		bool InitState(); 
//
//		string 	Id { get; }
//		long Code { get; }
//
//        void OnDestroy();
//	}
//
//    [System.Serializable]
//	public class GameObject<T> : IGameObject, IDeserializationCallback
//		where T: IStaticSpecData, new()
//	{
//		private static UInt64 ms_idGenerator = 0;
//        public GameObject()
//        {
//			if(UInt64.MaxValue<=ms_idGenerator)
//				ms_idGenerator = 0;
//			ms_idGenerator++;
//			_id = ms_idGenerator.ToString();
//            
//			InitGameObject ();
//        }
//
//		public bool InitObject(JSONObject json)
//		{
//			if(Load(json) == false)
//			{
//				return false;
//			}
//
//			if(InitState() == false)
//			{
//				return false;
//			}
//
//			return true;
//		}
//
//        public bool InitObject(string id, long code)
//        {
//            _id = id;
//            _code = code;
//            if (Load(code) == false)
//            {
//                return false;
//            }
//
//            if (InitState() == false)
//            {
//                return false;
//            }
//
//            return true;
//        }
//
//		public bool InitObject(long code)
//		{
//            _code = code;
//			if(Load(code) == false)
//			{
//				return false;
//			}
//
//			if(InitState() == false)
//			{
//				return false;
//			}
//
//			return true;
//		}
//
//        public virtual void OnDeserialization(object o)
//        {
//            if(_spec == null)
//            {
//				InitGameObject();
//                return;
//            }
//            //_spec.Init(CorgiStaticData.Instance.GetOrig(), CorgiStaticStringData.Instance.GetOrig());
//
//			InitGameObject ();
//        }
//
//		public virtual bool Load(JSONObject json) {return false;}
//		public virtual bool Load(long code) { return false; }
//		public virtual bool InitState() {return false;}
//        public virtual void ResetTemp() { }
//		public string Id { get { return _id; } }
//		public long Code { get { if (_spec == null) return _code; else return _spec.Code; } }
//
//		protected T _spec;
//
//        protected string _id;
//		protected long _code;
//
//		protected virtual void InitGameObject() {
//			if(_spec == null)
//			{
//				return;
//			}
//			//_spec.Init(CorgiStaticData.Instance.GetOrig(), CorgiStaticStringData.Instance.GetOrig());
//		}
//
//        public virtual void OnDestroy()
//        {
//            _id = null;
//            _spec = null;
//        }
        
    }
}