using System.IO;
using DOTS.Components;
using DOTS.Components.Tags;
using Newtonsoft.Json;
using Parameters;
using Structs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Bakers
{
    public class ScenarioParameters : MonoBehaviour
    {
        [SerializeField] private EntityParameters parameters;

        [SerializeField] private string flowMapFileName = "flow_field";
        [SerializeField] private float3 gridOrigin = Vector3.zero;
        [SerializeField] private float2 gridCellSize = new(100, 100);

        private static NativeArray2D<float2> ParseJsonFlowFieldMap(string inflowMapFileName)
        {
            //Read json file, that is stored in Assets/Resources folder.
            string path = Path.Combine(Application.streamingAssetsPath, inflowMapFileName + ".json");
            string json = File.ReadAllText(path);
            var flowFieldMap = JsonConvert.DeserializeObject<FlowFieldMap>(json);
            var flowMap = new NativeArray2D<float2>(flowFieldMap.width, flowFieldMap.height, Allocator.Domain);
            for (var x = 0; x < flowFieldMap.width; x++)
            {
                for (var y = 0; y < flowFieldMap.height; y++)
                {
                    flowMap[x, y] = flowFieldMap.flowMap[x + y * flowFieldMap.width];
                }
            }

            return flowMap;
        }

        public class ScenarioManagerBaker : Baker<ScenarioParameters>
        {
            public override void Bake(ScenarioParameters authoring)
            {
                if (World.DefaultGameObjectInjectionWorld.IsCreated == false)
                {
                    return;
                }

                NativeArray2D<float2> flowMapArray = ParseJsonFlowFieldMap(authoring.flowMapFileName);
                var flowMapComponent = new FlowMapComponent
                {
                    flowMap = flowMapArray
                };

                var gridParameters = new GridParametersComponent
                {
                    gridParameters = new GridParameters(authoring.gridOrigin, flowMapArray.Width, authoring.gridCellSize)
                };

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                entityManager.CreateSingleton(flowMapComponent);
                entityManager.CreateSingleton(gridParameters);
                entityManager.CreateSingleton(new EntitySpawnParametersComponent
                {
                    speed = authoring.parameters.speed,
                    entityCount = authoring.parameters.entityCount
                });

                Entity parametersEntity = entityManager.CreateEntity(typeof(SpawnEntityParametersTag));
                entityManager.AddSharedComponentManaged(parametersEntity,
                    new RenderMeshArray(new[] { authoring.parameters.material },
                        new[] { authoring.parameters.mesh }));
            }
        }
    }
}