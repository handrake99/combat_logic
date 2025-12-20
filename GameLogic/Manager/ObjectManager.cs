
using System;
using System.Runtime.Serialization;
using System.Threading;
using IdleCs;
using IdleCs.Library;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	
	/// <summary>
	/// object id 관리.
	/// 현재는 단순 increment
	/// </summary>
	public class ObjectManager : Singleton<ObjectManager>
	{
        ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();
        private long _curId = 0;

		public ObjectManager() {}
		

		public string GetId(object thisObject)
		{
            bool firstTime;

			//long objectId = _objectIdGenerator.GetId(thisObject, out firstTime);
			var objectId = Interlocked.Increment(ref _curId);

			return CorgiString.Format("{0}-{1}", thisObject.GetType().ToString(), objectId);
		}
	}
}