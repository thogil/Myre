﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        #region nodes
        public class Node
        {
            public Type Type;
        }

        public class ListNode
            : Node
        {
            public List<Node> Children;
        }

        public class DictionaryNode
            : Node
        {
            public Dictionary<string, Node> Children;
        }

        public class LiteralNode
            : Node
        {
            public string Value;
        }

        public class ObjectNode
        {
            public Dictionary<string, Node> Children;
        }
        #endregion

        #region fields
        public Node Root;
        #endregion
    }
}