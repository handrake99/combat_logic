using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using IdleCs.Network.NetLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace IdleCs.Utils
{
    public struct CorgiPosition
    {
    
        [JsonIgnore]
        private Vector2 _pos;
        [JsonIgnore]
        private Vector2 _dir;

        [JsonIgnore] public Vector2 Pos => _pos;
        [JsonIgnore] public Vector2 Dir => _dir;

//        public Decimal X => _x;
//        public Decimal Y => _y;

        // position
        public float X;
        public float Y;
        
        // direction
        public float DirX;
        public float DirY;

        public CorgiPosition(float inX, float inY, float inDirX, float inDirY)
        {
            _pos = new Vector2(inX, inY);
            X = _pos.x;
            Y = _pos.y;
            
            _dir = new Vector2(inDirX, inDirY);
            DirX = _dir.x;
            DirY = _dir.y;
            
        }

        public CorgiPosition(Vector2 pos, Vector2 dir)
        {
            _pos = pos;
            X = _pos.x;
            Y = _pos.y;
            
            _dir = dir;
            DirX = _dir.x;
            DirY = _dir.y;
        }
        
        public CorgiPosition(CorgiPosition position)
        {
            _pos = new Vector2(position.X, position.Y);
            X = _pos.x;
            Y = _pos.y;
            
            _dir = new Vector2(position.DirX, position.DirY);
            DirX = _dir.x;
            DirY = _dir.y;
        }

        public void SetPos(float inX, float inY)
        {
            _pos.Set(inX, inY);
            X = _pos.x;
            Y = _pos.y;
        }
        
        public void SetDir(float inDirX, float inDirY)
        {
            _dir.Set(inDirX, inDirY);
            DirX = _dir.x;
            DirY = _dir.y;
        }

        public static float Distance(CorgiPosition a, CorgiPosition b)
        {
            return Vector2.Distance(a.Pos, b.Pos);
        }
        
        public static float Distance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public void MoveToPos(CorgiPosition target, float factor)
        {
		    var targetPos = target.Pos;
            var direction = targetPos - _pos;

            var distance = Distance(this, target);

            if (direction != Vector2.zero)
            {
                _dir = direction.normalized;
            }
            
            if (distance < factor)
            {
                _pos = targetPos;
            }
            else
            {
                var moveDis = (_dir * factor);
                
                _pos += moveDis;
            }
            
            X = _pos.x;
            Y = _pos.y;

            DirX = _dir.x;
            DirY = _dir.y;

            //CorgiLog.LogLine("[Class]Char0Position ({0}, {1}) / ({2}, {3})", X, Y, DirX, DirY);
        }

        public CorgiPosition GetArrivalPosition(CorgiPosition target, float distance)
        {
		    var targetPos = target.Pos;
            var direction = targetPos - _pos;

            return new CorgiPosition(_pos + direction.normalized * distance, direction);
        }
        
        public static CorgiPosition GetArrivalPosition(Vector2 actorPos, Vector2 targetPos, float distance)
        {
            var direction = targetPos - actorPos;

            return new CorgiPosition(actorPos + direction.normalized * distance, direction);
        }

        
        // 거리 유지용
        public void SetDistance(float distance)
        {
            _pos = _pos.normalized * distance;
            X = _pos.x;
            Y = _pos.y;
        }
        
        public static CorgiPosition operator +(CorgiPosition a, CorgiPosition b)
        {
            Vector2 result = a.ToVector() + b.ToVector();
            return new CorgiPosition(result, a.Dir);
        }
        public static CorgiPosition operator -(CorgiPosition a, CorgiPosition b)
        {
            Vector2 result = a.ToVector() - b.ToVector();
            return new CorgiPosition(result, a.Dir);
        }

        public Vector2 ToVector()
        {
            return _pos;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(X, 0f, Y);
        }
        
        public Vector3 ToDirVector3()
        {
            return new Vector3(DirX, 0f, DirY);
        }

        void OnDeserialized()
        {
            _pos = new Vector2(X, Y);
            _dir = new Vector2(DirX, DirY);
        }

        public void Serialize(IPacketWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            
            writer.Write(DirX);
            writer.Write(DirY);
        }
        
        public void DeSerialize(IPacketReader reader)
        {
            reader.Read(out X);
            reader.Read(out Y);
            reader.Read(out DirX);
            reader.Read(out DirY);
            
            _pos = new Vector2(X, Y);
            
            OnDeserialized();
        }

    }

}
