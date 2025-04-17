using System.Numerics;

namespace sauronsring
{
    public class Entity
    {
        public List<Vector3> bones { get; set; }
        public List<Vector2> bones2d { get; set; }  
        public IntPtr pawnAddress {  get; set; }
        public Vector3 position { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector2 position2D { get; set; }
        public Vector2 viewPosition2D { get; set; }
        public int health { get; set; }
        public int team { get; set; }
        public float distance { get; set; }
        public string name { get; set; }
    }

    public enum BonesIds
    {
        Waist = 0,
        Neck = 5,
        Head = 6,
        ShoulderLeft = 8,
        ForeLeft = 9,
        HandLeft = 11,
        ShoulderRight = 13,
        ForeRight = 14,
        HandRight = 16,
        KneeLeft = 23,
        FeetLeft = 24,
        KneeRight = 26,
        FeetRight = 27,
    }
}
