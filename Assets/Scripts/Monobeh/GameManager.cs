using DOTS.Systems;
using Unity.Entities;
using UnityEngine;
using MainSpawnSystem = DOTS.Systems.Situational.MainSpawnSystem;

namespace Monobeh
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            {
                World world = entityManager.World;
                var systemHandle = world.GetOrCreateSystemManaged<MainSpawnSystem>();
                systemHandle.Update();
                entityManager.World.DestroySystemManaged(systemHandle);
                
               // SystemHandle  systemHandle2 = World.DefaultGameObjectInjectionWorld.CreateSystem<CylinderSurfacePositioningSystem>();
            }

            Destroy(gameObject);
        }
    }
}