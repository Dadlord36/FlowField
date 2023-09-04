using DOTS.Systems;
using Unity.Entities;
using UnityEngine;

namespace Monobeh
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            {
                var systemHandle = entityManager.World.GetOrCreateSystemManaged<MainSpawnSystem>();
                systemHandle.Update();
                entityManager.World.DestroySystemManaged(systemHandle);
            }

            Destroy(gameObject);
        }
    }
}