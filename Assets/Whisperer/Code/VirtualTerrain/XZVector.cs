namespace Whisperer.VirtualTerrain
{
    [System.Serializable]
    public struct XZVector
    {
        public int x;
        public int z;

        public XZVector(int x, int z)
        {
            this.x = x;
            this.z = z;
        }
    }
}
