
using System.Collections.Generic;
namespace YukiFrameWork.DiaLog
{
    public abstract class CompositeNode : Node
    {
        public List<Node> children = new List<Node>();
    }
}