namespace ZK.Mapper.Core
{
    public class MapContext
    {
        public MapContext(bool deepCopy = false)
        {
            DeepCopy = deepCopy;
        }

        public bool DeepCopy { get; }
    }
}
