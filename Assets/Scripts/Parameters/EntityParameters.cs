using UnityEngine;

namespace Parameters
{
    [CreateAssetMenu(fileName = nameof(EntityParameters), menuName = nameof(Parameters) + "/" + nameof(EntityParameters), order = 0)]
    public class EntityParameters : ScriptableObject
    {
        public Mesh mesh;
        public Material material;
        public int entityCount;
        [Min(0f)] public float speed;
        [Min(0f)] public float crowdAvoidanceDistance;
    }
}