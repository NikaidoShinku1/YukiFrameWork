
namespace YukiFrameWork.DiaLogue
{
    public abstract class SingleNode : Node
    {
        [UnityEngine.SerializeField]
        internal Node child;
       
        public override bool IsDefaultMoveNext => true;
    }
}