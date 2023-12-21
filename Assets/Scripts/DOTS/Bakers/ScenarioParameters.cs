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
using Random = Unity.Mathematics.Random;

namespace DOTS.Bakers
{
    public class ScenarioParameters : MonoBehaviour
    {
        [SerializeField] private EntityParameters parameters;

        // [SerializeField] private string flowMapFileName = "flow_field";
        [SerializeField] private float2 gridCellSize = new(100, 100);
        [SerializeField] private uint2 gridDimensions;
        [SerializeField] private CylinderParameters spawnCylinderParameters;

        private static NativeArray2D<float2> ParseJsonFlowFieldMap(string inflowMapFileName)
        {
            //Read json file, that is stored in Assets/Resources folder.
            string path = Path.Combine(Application.streamingAssetsPath, inflowMapFileName + ".json");
            string json = File.ReadAllText(path);
            var flowFieldMap = JsonConvert.DeserializeObject<FlowFieldMap>(json);
            var flowMap = new NativeArray2D<float2>((ushort)flowFieldMap.width, (ushort)flowFieldMap.height, Allocator.Domain);
            for (var x = 0; x < flowFieldMap.width; x++)
            {
                for (var y = 0; y < flowFieldMap.height; y++)
                {
                    flowMap[x, y] = flowFieldMap.flowMap[x + y * flowFieldMap.width];
                }
            }

            return flowMap;
        }

        private static NativeArray2D<float2> FormFlowField(ushort gridCols, ushort gridRows)
        {
            const float xBias = -1.0f; // Strong leftward bias

            // Flatten the 2D array to a 1D array of shape (GridX * GridY, 2)
            var vectorField2D = new NativeArray2D<float2>(gridCols, gridRows, Allocator.Temp);

            var randomGenerator = Random.CreateFromIndex(0);
            randomGenerator.InitState();

            for (ushort c = 0; c < gridCols; ++c)
            {
                for (ushort r = 0; r < gridRows; ++r)
                {
                    float yVariation = randomGenerator.NextFloat(-0.2f, 0.2f); // Some random vertical variation
                    vectorField2D[c, r] = new float2(xBias, yVariation);
                }
            }

            // Dampen the upward/downward vectors near the top and bottom edges
            const int boundaryThickness = 3;
            for (ushort i = 0; i < boundaryThickness; ++i)
            {
                float damping = (float)i / boundaryThickness;
                for (ushort j = 0; j < gridRows; ++j)
                {
                    float2 firstElement = vectorField2D[i, j];
                    float2 secondElement = vectorField2D[(ushort)(gridCols - (i + 1)), j];

                    firstElement.y *= damping; // Dampen upward motion near top
                    secondElement.y *= -damping; // Dampen downward motion near bottom

                    vectorField2D[i, j] = firstElement;
                    vectorField2D[gridCols - (i + 1), j] = secondElement;
                }
            }

            // Normalize vectors
            for (ushort i = 0; i < gridCols; ++i)
            {
                for (ushort j = 0; j < gridRows; ++j)
                {
                    vectorField2D[i, j] = math.normalize(vectorField2D[i, j]);
                }
            }

            // Make it tile seamlessly by setting edge vectors on the left and right to be the same
            for (int i = 0; i < gridCols; ++i)
            {
                vectorField2D[i, 0] = vectorField2D[i, gridRows - 1];
            }

            return vectorField2D;
        }

        public class ScenarioManagerBaker : Baker<ScenarioParameters>
        {
            public override void Bake(ScenarioParameters authoring)
            {
                if (World.DefaultGameObjectInjectionWorld.IsCreated == false)
                {
                    return;
                }

                var flowFieldArray =  new NativeArray2D<float2>(FormFlowField((ushort)authoring.gridDimensions.x, (ushort)authoring.gridDimensions.y),
                    Allocator.Temp);
                var flowMapComponent = new FlowMapComponent(flowFieldArray);
                flowFieldArray.Dispose();

                var gridParameters = new GridParametersComponent
                {
                    gridParameters = new GridParameters((ushort)authoring.gridDimensions.x, (ushort)authoring.gridDimensions.y,
                        authoring.gridCellSize)
                };

                var cylinderParameters = new CylinderParametersComponent
                {
                    cylinderParameters = authoring.spawnCylinderParameters
                };
                cylinderParameters.cylinderParameters.cylinderOrigin = authoring.transform.position;

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                
                entityManager.CreateSingleton(new EntitySpawnParametersComponent
                {
                    speed = authoring.parameters.speed,
                    entityCount = authoring.parameters.entityCount
                });
                entityManager.CreateSingleton(
                    new MovementParametersComponent(authoring.parameters.speed, authoring.parameters.crowdAvoidanceDistance));

                Entity parametersEntity = entityManager.CreateEntity(typeof(WalkingSurfaceTag));
                entityManager.AddSharedComponent(parametersEntity, flowMapComponent);
                entityManager.AddSharedComponent(parametersEntity, gridParameters);
                entityManager.AddSharedComponent(parametersEntity, cylinderParameters);
                entityManager.AddSharedComponentManaged(parametersEntity,
                    new RenderMeshArray(new[] { authoring.parameters.material },
                        new[] { authoring.parameters.mesh }));
                
            }
        }
    }
}