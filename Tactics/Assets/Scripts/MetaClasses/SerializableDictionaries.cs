using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using UnityEngine;


[Serializable]
public class DictStringInt : SerializableDictionary<string, int> {
    public DictStringInt() : base() { }
    public DictStringInt(SerializationInfo info, StreamingContext context) : base(info, context) { }
}