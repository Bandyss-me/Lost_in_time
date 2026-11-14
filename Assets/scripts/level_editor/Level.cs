using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Level
{
    [Serializable]
    public class NewVec3
    {
        public NewVec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public float x, y, z;
    }
    [Serializable]
    public class NewQuaternion
    {
        public NewQuaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public float w, x, y, z;
    }
    [Serializable]
    public struct Object
    {
        public uint ID;
        public NewVec3 position, scale;
        public NewQuaternion rotation;
    }

    public List<Object> objects;
}
