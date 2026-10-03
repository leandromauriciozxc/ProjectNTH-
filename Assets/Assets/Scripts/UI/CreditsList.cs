using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectNTH.UI
{
    [CreateAssetMenu(menuName = "Project NTH/Credits List", fileName = "Credits List")]
    public sealed class CreditsList : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string modelName;
            public string creatorName;
            public string modelUrl;
            public string creatorUrl;
        }

        public List<Entry> entries = new List<Entry>();
    }
}
