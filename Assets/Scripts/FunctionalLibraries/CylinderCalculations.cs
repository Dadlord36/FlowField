using System.Runtime.CompilerServices;
using DOTS.Components;
using Structs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FunctionalLibraries
{
    public static class CylinderCalculations
    {
        public const float TwoPi = 2 * math.PI;
        public const float HalfPi = math.PI / 2f;
        public const float NegativeHalfPi = -HalfPi;

        private static readonly float3 SingularScale = new(1f, 1f, 1f);

        /// <summary>
        /// Calculates a point on the surface of a cylinder at a given angle and height.
        /// </summary>
        /// <param name="cylinderCenter"> Cylinder center. </param>
        /// <param name="cylinderRadius"> Cylinder radius. </param>
        /// <param name="angleInRadians"> Angle in radians. </param>
        /// <param name="height"> Height of the point. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetPointOnCylinderSurfaceAt(in float3 cylinderCenter, float cylinderRadius, float angleInRadians, float height)
        {
            float x = cylinderCenter.x + cylinderRadius * math.cos(angleInRadians);
            float y = cylinderCenter.y + height;
            float z = cylinderCenter.z + cylinderRadius * math.sin(angleInRadians);
            return new float3(x, y, z);
        }

        /// <summary>
        /// Calculates a point on the surface of a cylinder at a given angle and height.
        /// </summary>
        /// <param name="cylinderParameters"> Cylinder parameters. </param>
        /// <param name="angle"> Angle in radians. </param>
        /// <param name="height"> Height of the point. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetPointOnCylinderSurfaceAt(in CylinderParameters cylinderParameters, float angle, float height)
        {
            //Asset that height is in range [0, cylinderParameters.height].
            height = math.clamp(height, 0, cylinderParameters.height);
            return GetPointOnCylinderSurfaceAt(cylinderParameters.cylinderOrigin, cylinderParameters.radius, angle, height);
        }

        /// <summary>
        ///  Calculates a point on the surface of a cylinder at given cell index.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="cylinderParameters"> Cylinder parameters. </param>
        /// <param name="cellIndex"> Cell index. </param>
        /// <returns> Point on the surface of a cylinder. </returns>
        public static float3 GetGridCenterProjectedOnCylinderSurfaceAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in uint2 cellIndex)
        {
            float angle = CalculateRadiansAngleOnCylinderAtGridIndex(gridParameters, cellIndex);
            float height = CalculateHeightOnCylinderAtGridIndex(gridParameters, cellIndex);
            return GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
        }

        // Make sure that the position is on the surface of the cylinder and not away from it (with some tolerance).
        public static bool IsOnCylinderSurface(in CylinderParameters cylinderParameters, in float3 position)
        {
            float3 cylinderCenter = cylinderParameters.center;
            float3 cylinderOrigin = cylinderParameters.cylinderOrigin;
            float3 cylinderCenterToPosition = position - cylinderCenter;
            float3 cylinderOriginToPosition = position - cylinderOrigin;
            float distanceFromCenter = math.length(cylinderCenterToPosition);
            float distanceFromOrigin = math.length(cylinderOriginToPosition);

            return math.abs(distanceFromCenter - cylinderParameters.radius) < 0.1f &&
                   math.abs(distanceFromOrigin - cylinderParameters.radius) < 0.1f;
        }

        public static uint2 GetCellIndex2DOnCylinderSurfaceAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            float surfacePositioningHeight, float surfacePositioningAngleInRadians)
        {
            // Normalize the angle to be within [0, 2 * PI]
            surfacePositioningAngleInRadians = (surfacePositioningAngleInRadians + TwoPi) % TwoPi;

            return math.uint2(
                (uint)math.round(surfacePositioningAngleInRadians * cylinderParameters.radius / gridParameters.cellSize.x),
                (uint)math.round(surfacePositioningHeight / gridParameters.cellSize.y));
        }

        public static LocalToWorld CalculateLocalToWorldMatrixAt(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            in uint2 cellIndex, in float3 forwardVector)
        {
            float3 pointOnCylinderSurface = GetGridCenterProjectedOnCylinderSurfaceAt(gridParameters, cylinderParameters, cellIndex);
            return CalculateLocalToWorld(pointOnCylinderSurface, forwardVector);
        }

        public static LocalToWorld GetCellCenterAtReversed(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            uint index1D, float3 forwardVector)
        {
            uint2 cellIndex2D = IndexesCalculations.CalculateReversedIndex2DFrom1D(index1D, gridParameters.rowNumber);
            return CalculateLocalToWorldMatrixAt(gridParameters, cylinderParameters, cellIndex2D, forwardVector);
        }

        public static quaternion CalculateRotationParallelToCylinderSurfaceFromAngle(in CylinderParameters cylinderParameters, float angleOnCylinder,
            float angleInDegrees)
        {
            quaternion rotation = CalculateCylinderSurfaceNormalAtAngle(cylinderParameters, angleOnCylinder);
            //Rotate the rotation by 180 degrees, because the cylinder is rotated by 90 degrees.
            rotation = math.mul(rotation, quaternion.Euler(math.radians(angleInDegrees), math.radians(90f), 0f));
            return rotation;
        }

        public static float2 Calculate2DVelocityFromDirectionAngle(float angleInDegrees)
        {
            float angleInRadians = math.radians(angleInDegrees);
            return new float2(math.cos(angleInRadians), math.sin(angleInRadians));
        }

        /*public static float3 ConvertToCylinderVelocity(in float2 flatVelocity, float angle)
        {
            // Calculate the tangent vector based on some angle (this could be dynamically determined)
            var tangentVector = new float3(-math.sin(angle), 0, math.cos(angle));

            // Calculate axial and tangential components based on flatVelocity
            float3 axialComponent = flatVelocity.y * Vector3.up; // Axial movement
            float3 tangentialVelocity = tangentVector * flatVelocity.x; // Rotational movement

            // Combine them to get the final velocity
            float3 finalVelocity = axialComponent + tangentialVelocity;

            // Scale by the original speed
            float originalSpeed = math.length(flatVelocity);
            finalVelocity *= originalSpeed;

            return finalVelocity;
        }*/


        /*public static float3 ConvertToCylinderVelocity(float2 flatVelocity, float angleOnCylinder)
        {
            // Calculate the tangent vector based on some angle (this could be dynamically determined)
            var tangentVector = new float3(-math.sin(angleOnCylinder), 0, math.cos(angleOnCylinder));

            // Calculate axial and tangential components based on flatVelocity
            float3 axialComponent = flatVelocity.y * Vector3.up; // Axial movement
            float3 tangentialVelocity = tangentVector * flatVelocity.x; // Rotational movement

            return axialComponent + tangentialVelocity;
        }*/

        public static float3 ConvertToCylinderVelocity(float2 flatVelocity, float angleInRadians)
        {
            // Calculate the tangent vector based on the angle
            var tangentVector = new float3(-math.sin(angleInRadians), math.cos(angleInRadians), 0.0f);

            // Axial movement is along the Z-axis
            var axialComponent = new float3(0.0f, 0.0f, flatVelocity.y);

            // Tangential movement (rotational)
            float3 tangentialVelocity = tangentVector * flatVelocity.x;

            // Combine the axial and tangential components to get the final velocity
            float3 finalVelocity = axialComponent + tangentialVelocity;

            // Scale by the original speed to maintain the speed from the 2D velocity
            finalVelocity *= math.length(flatVelocity);

            return finalVelocity;
        }

        public static LocalToWorld CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(in CylinderParameters cylinderParameters, float height,
            float angle, float3 finalForwardVector)
        {
            float3 pointOnCylinderSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, height);
            quaternion rotation = quaternion.LookRotationSafe(finalForwardVector, Vector3.up);
            rotation = math.mul(rotation, quaternion.Euler(0f, 0f, math.radians(90f)));
            return new LocalToWorld
            {
                Value = float4x4.TRS(pointOnCylinderSurface, rotation, Vector3.one)
            };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateRadiansAngleOnCylinderAtPosition(in CylinderParameters cylinderParameters, float3 position)
        {
            return math.atan2(position.z - cylinderParameters.center.z, position.x - cylinderParameters.center.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetHeightAndAngleOnCylinderAt(in CylinderParameters cylinderParameters, float3 position,
            ref OrbitCoordinate orbitCoordinate)
        {
            orbitCoordinate.height = CalculateHeightOnCylinderAtPosition(cylinderParameters, position);
            orbitCoordinate.angle = CalculateRadiansAngleOnCylinderAtPosition(cylinderParameters, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrbitCoordinate ComputeNextOrbitCoordinate(OrbitCoordinate currentCoordinate, float2 velocity, float speed, float deltaTime)
        {
            // Convert the velocity vector to an angle and direction along the cylinder surface
            float angle = math.atan2(velocity.y, velocity.x);
            // Calculate the distance traveled based on the given speed
            float distanceTraveled = speed * deltaTime; // assuming a time interval of 0.1 seconds for the distance calculation
            // Update the height and angle of the OrbitCoordinate based on the calculated velocity and distance
            currentCoordinate.height += distanceTraveled * math.sin(angle);
            currentCoordinate.angle += distanceTraveled * math.cos(angle);

            return currentCoordinate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 OrbitToTransform(OrbitCoordinate orbitCoordinate, float radius)
        {
            float x = radius * math.cos(orbitCoordinate.angle);
            float y = orbitCoordinate.height;
            float z = radius * math.sin(orbitCoordinate.angle);
            var position = new float3(x, y, z);

            quaternion rotation = quaternion.Euler(0, orbitCoordinate.angle, 0); // Assuming the rotation is around the y-axis

            return float4x4.TRS(
                position,
                rotation,
                SingularScale // Assuming uniform scale of 1
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint2 CalculateCellIndexFromOrbitCoordinate(in GridParameters gridParameters, in CylinderParameters cylinderParameters,
            OrbitCoordinate orbitCoordinate)
        {
            float normalizedAngle = math.fmod(orbitCoordinate.angle, TwoPi);
            normalizedAngle = math.select(normalizedAngle, normalizedAngle + TwoPi, normalizedAngle < 0); // if angle is negative, add 2*PI

            float angleToGrid = normalizedAngle / TwoPi; // Convert angle to the range [0, 1]
            float heightToGrid = orbitCoordinate.height / cylinderParameters.height; // Convert height to the range [0, 1]

            // Calculate cell indices in the row and column by multiplying with the grid size and converting to integer.
            var columnIndex = (uint)math.floor(gridParameters.maxColumnIndex * angleToGrid);
            var rowIndex = (uint)math.floor(gridParameters.maxRowIndex * heightToGrid);

            return new uint2(columnIndex, rowIndex);
        }

        #region SurfaceCoordinate related

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 SurfaceCoordinateToLocalToWorld(SurfaceCoordinate coordinate, in CylinderParameters cylinder)
        {
            float x = cylinder.radius * math.sin(coordinate.latitude) * math.cos(coordinate.longitude);
            float y = cylinder.radius * math.sin(coordinate.latitude) * math.sin(coordinate.longitude);
            float z = cylinder.radius * math.cos(coordinate.latitude);

            var position = new float3(x, y, z);

            float3 forward = math.normalize(position);
            float3 up = math.cross(forward, new float3(0, 1, 0));
            float3 right = math.cross(forward, up);

            var rotation = new float3x3(right, up, forward);

            return new float4x4(rotation, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SurfaceCoordinate LocalToWorldToSurfaceCoordinate(in CylinderParameters cylinder, float3 position)
        {
            float longitude = math.atan2(position.x, position.z);
            float latitude = math.acos(position.y / cylinder.height);

            return new SurfaceCoordinate(latitude, longitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint2 CalculateCellIndexFromOrbitCoordinate(in GridParameters gridParameters, SurfaceCoordinate coordinate)
        {
            coordinate = NormalizeSurfaceCoordinate(coordinate);

            // Calculate cell indices in the row and column by multiplying with the grid size and converting to integer.
            var columnIndex = (uint)math.floor(gridParameters.maxColumnIndex * (coordinate.longitude + 1f) * 0.5f);
            var rowIndex = (uint)math.floor(gridParameters.maxRowIndex * (coordinate.latitude + 1f) * 0.5f);
            return new uint2(columnIndex, rowIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SurfaceCoordinate NormalizeSurfaceCoordinate(SurfaceCoordinate coordinate)
        {
            return new SurfaceCoordinate(math.clamp(coordinate.latitude, NegativeHalfPi, HalfPi),
                math.clamp(coordinate.longitude, -math.PI, math.PI));
        }

        /// <summary>
        /// Adjusts the given surface coordinate by adding an offset.
        /// </summary>
        /// <param name="coordinate">The surface coordinate to adjust.</param>
        /// <param name="offset">The offset to add to the coordinate.</param>
        /// <returns>The adjusted surface coordinate.</returns>
        public static SurfaceCoordinate AdjustCoordinate(SurfaceCoordinate coordinate, float2 offset)
        {
            // Assuming latitude ranges from -90 to 90 degrees and longitude from -180 to 180 degrees
            float newLatitude = WrapValue(coordinate.latitude + offset.y, -90f, 90f);
            float newLongitude = WrapValue(coordinate.longitude + offset.x, -180f, 180f);

            return new SurfaceCoordinate (newLatitude, newLongitude);
        }
        
        // Wraps the value within specified bounds
        private static float WrapValue(float value, float minValue, float maxValue)
        {
            float range = maxValue - minValue;
            value = Mathf.Repeat(value - minValue, range) + minValue;
            return value;
        }

        #endregion SurfaceCoordinate related

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrbitCoordinate CalculateOrbitFrom1DIndex(in GridParameters gridParameters, uint2 index2D)
        {
            return new OrbitCoordinate
            {
                angle = CalculateRadiansAngleOnCylinderAtGridIndex(gridParameters, index2D),
                height = CalculateHeightOnCylinderAtGridIndex(gridParameters, index2D)
            };
        }

        private static quaternion CalculateCylinderSurfaceNormalAtAngle(in CylinderParameters cylinderParameters, float angle)
        {
            float3 pointOnSurface = GetPointOnCylinderSurfaceAt(cylinderParameters, angle, 0);
            return quaternion.LookRotationSafe(pointOnSurface - cylinderParameters.center, Vector3.up);
        }

        private static LocalToWorld CalculateLocalToWorld(float3 pointOnCylinderSurface, float3 finalForwardVector)
        {
            quaternion rotation = quaternion.LookRotationSafe(finalForwardVector, Vector3.up);
            rotation = math.mul(rotation, quaternion.Euler(math.radians(90), 0, 0));
            return new LocalToWorld
            {
                Value = float4x4.TRS(pointOnCylinderSurface, rotation, Vector3.one)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateHeightOnCylinderAtPosition(in CylinderParameters cylinderParameters, float3 position)
        {
            return position.y - cylinderParameters.cylinderOrigin.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateRadiansAngleOnCylinderAtGridIndex(in GridParameters gridParameters, uint2 cellIndex)
        {
            return cellIndex.x * TwoPi / gridParameters.columnNumber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateHeightOnCylinderAtGridIndex(in GridParameters gridParameters, uint2 cellIndex)
        {
            return (float)cellIndex.y / gridParameters.maxRowIndex * gridParameters.gridSize.y;
        }

        public static SurfaceCoordinate CalculateSurfaceCoordinateFromIndex(in GridParameters gridParameters, uint2 index2d)
        {
            float normalizedRowIndex = (float)index2d.y / gridParameters.maxRowIndex;  // normalize to [0, 1]
            float normalizedColumnIndex = (float)index2d.x / gridParameters.maxColumnIndex;  // normalize to [0, 1]

            float latitude = (normalizedRowIndex * 2 - 1) * HalfPi;  // scale to [-HalfPi, HalfPi]
            float longitude = (normalizedColumnIndex * 2 - 1) * math.PI;  // scale to [-Pi, Pi]

            return new SurfaceCoordinate(latitude, longitude);
        }
    }
}