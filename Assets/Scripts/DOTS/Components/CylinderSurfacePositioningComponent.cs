using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    
    
    
    [BurstCompile]
    public struct SurfaceCoordinate
    {
        public float latitude, longitude;

        public SurfaceCoordinate(float latitude, float longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }

    [BurstCompile]
    public struct OrbitCoordinate
    {
        public float angle;
        public float height;

        public OrbitCoordinate(float angle, float height)
        {
            this.angle = angle;
            this.height = height;
        }
    }

    [BurstCompile]
    public struct CylinderSurfacePositioningComponent : IComponentData
    {
        // public OrbitCoordinate orbitCoordinate;
        public SurfaceCoordinate surfaceCoordinate;
    }
}