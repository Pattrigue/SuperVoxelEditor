using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    public sealed class ChunkMeshBuilder : IDisposable
    {
        private static readonly Vector3Int Dimensions = new Vector3Int(Chunk.Width, Chunk.Height, Chunk.Depth);

        private readonly Chunk chunk;
        
        private readonly bool[] hasMerged;
        
        private MeshData meshData;

        private int vertexCount;
        
        public ChunkMeshBuilder(Chunk chunk)
        {
            this.chunk = chunk;
            
            int maxDimension1 = Math.Max(Dimensions.x, Math.Max(Dimensions.y, Dimensions.z));
            int maxDimension2 = Dimensions.x + Dimensions.y + Dimensions.z - maxDimension1 - Math.Min(Dimensions.x, Math.Min(Dimensions.y, Dimensions.z));
            
            hasMerged = new bool[maxDimension1 * maxDimension2];
            meshData = MeshData.Allocate();
        }

        public void Dispose() => meshData.Dispose();

        public ref MeshData GenerateMeshData(BorderVisibilityFlags borderVisibilityFlags)
        {
            // This loop goes through each face of a voxel cube (6 faces)
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                bool isBackFace = faceIndex % 2 == 1;
                int primaryAxis = faceIndex % 3;
                int secondaryAxis = (primaryAxis + 1) % 3;
                int tertiaryAxis = (primaryAxis + 2) % 3;

                Vector3Int startPos = new Vector3Int();

                // This loop iterates over each layer of the voxel chunk
                for (startPos[primaryAxis] = 0; startPos[primaryAxis] < Dimensions[primaryAxis]; startPos[primaryAxis]++)
                {
                    // Clear hasMerged array
                    for (int i = 0; i < hasMerged.Length; i++)
                    {
                        hasMerged[i] = false;
                    }

                    // Build the slices of the mesh.
                    for (startPos[secondaryAxis] = 0; startPos[secondaryAxis] < Dimensions[secondaryAxis]; startPos[secondaryAxis]++)
                    {
                        for (startPos[tertiaryAxis] = 0; startPos[tertiaryAxis] < Dimensions[tertiaryAxis]; startPos[tertiaryAxis]++)
                        {
                            uint voxelData = chunk.GetVoxelDataFromWorldPosition(startPos + chunk.ChunkPosition.VoxelPosition);
                            uint colorId = Voxel.GetColorId(voxelData);

                            Color32 voxelColor = chunk.Volume.ColorPicker.GetColorByIndex(colorId);

                            int index = startPos[secondaryAxis] * Dimensions[tertiaryAxis] + startPos[tertiaryAxis];

                            // If this voxel has already been merged, is air, or not visible, skip it.
                            if (hasMerged[index] || Voxel.IsAir(voxelData) || !IsVoxelFaceVisible(startPos, primaryAxis, isBackFace, borderVisibilityFlags))
                            {
                                continue;
                            }

                            // Determine the size of the quad to be added
                            Vector3Int quadSize = CalculateQuadSize(startPos, primaryAxis, secondaryAxis, tertiaryAxis, isBackFace, hasMerged, borderVisibilityFlags);

                            // Add the quad to the mesh
                            AddQuadToMesh(startPos, quadSize, primaryAxis, secondaryAxis, tertiaryAxis, isBackFace, voxelColor);

                            // Mark the voxels as merged
                            MarkVoxelsAsMerged(startPos, quadSize, hasMerged, secondaryAxis, tertiaryAxis);
                        }
                    }
                }
            }

            return ref meshData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3Int CalculateQuadSize(in Vector3Int startPos,
            int primaryAxis, int secondaryAxis, int tertiaryAxis,
            bool isBackFace, in bool[] hasMerged, BorderVisibilityFlags borderVisibilityFlags)
        {
            Vector3Int quadSize = new Vector3Int();
            Vector3Int currentPos = startPos;

            // Calculate the width of the quad
            currentPos[tertiaryAxis]++;
    
            while (currentPos[tertiaryAxis] < Dimensions[tertiaryAxis] 
                   && CompareStep(startPos, currentPos, primaryAxis, isBackFace, borderVisibilityFlags) 
                   && !hasMerged[currentPos[secondaryAxis] * Dimensions[tertiaryAxis] + currentPos[tertiaryAxis]]) 
            {
                currentPos[tertiaryAxis]++;
            }
    
            quadSize[tertiaryAxis] = currentPos[tertiaryAxis] - startPos[tertiaryAxis];

            // Calculate the height of the quad
            currentPos = startPos;
            currentPos[secondaryAxis]++;
    
            while (currentPos[secondaryAxis] < Dimensions[secondaryAxis] 
                   && CompareStep(startPos, currentPos, primaryAxis, isBackFace, borderVisibilityFlags) 
                   && !hasMerged[currentPos[secondaryAxis] * Dimensions[tertiaryAxis] + currentPos[tertiaryAxis]]) 
            {
                while (currentPos[tertiaryAxis] < Dimensions[tertiaryAxis] 
                       && CompareStep(startPos, currentPos, primaryAxis, isBackFace, borderVisibilityFlags) 
                       && !hasMerged[currentPos[secondaryAxis] * Dimensions[tertiaryAxis] + currentPos[tertiaryAxis]])
                {
                    currentPos[tertiaryAxis]++;
                }

                // If the quad doesn't span the full width, it's not a good add.
                if (currentPos[tertiaryAxis] - startPos[tertiaryAxis] < quadSize[tertiaryAxis])
                {
                    break;
                }

                currentPos[tertiaryAxis] = startPos[tertiaryAxis];
                currentPos[secondaryAxis]++;
            }
    
            quadSize[secondaryAxis] = currentPos[secondaryAxis] - startPos[secondaryAxis];

            return quadSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddQuadToMesh(in Vector3Int startPos, in Vector3Int quadSize, int primaryAxis, int secondaryAxis, int tertiaryAxis, bool isBackFace, in Color32 voxelColor)
        {
            // Create the vectors for the corners of the quad
            Vector3Int horizontalVector = new Vector3Int();
            horizontalVector[secondaryAxis] = quadSize[secondaryAxis];

            Vector3Int verticalVector = new Vector3Int();
            verticalVector[tertiaryAxis] = quadSize[tertiaryAxis];

            // Offset the position if we're dealing with a front face
            Vector3Int offsetPosition = startPos;
            offsetPosition[primaryAxis] += isBackFace ? 0 : 1;

            // Create the vertices of the face
            Vector3 a = offsetPosition;
            Vector3 b = offsetPosition + horizontalVector;
            Vector3 c = offsetPosition + horizontalVector + verticalVector;
            Vector3 d = offsetPosition + verticalVector;

            // Calculate the normal for the face
            Vector3 normal = Vector3.zero;
            normal[primaryAxis] = isBackFace ? -1 : 1;

            // Add the square face to the mesh
            AddSquareFace(a, b, c, d, normal, voxelColor, isBackFace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddSquareFace(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, in Vector3 normal, in Color32 color, bool isBackFace)
        {
            // Add the 4 vertices, and color for each vertex.
            meshData.Vertices.Add(new Vertex(a, normal, color));
            meshData.Vertices.Add(new Vertex(b, normal, color));
            meshData.Vertices.Add(new Vertex(c, normal, color));
            meshData.Vertices.Add(new Vertex(d, normal, color));

            int i = vertexCount;
            vertexCount += 4;

            if (!isBackFace)
            {
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 1));
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)(i + 3));
            }
            else
            {
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)(i + 1));
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 3));
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)i);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVoxelFaceVisible(Vector3Int voxelPosition, int axis, bool backFace, BorderVisibilityFlags borderVisibilityFlags)
        {
            voxelPosition[axis] += backFace ? -1 : 1;
            voxelPosition += chunk.ChunkPosition.VoxelPosition; // Note - this is now a world position! (not a local voxel/chunk position)

            if (chunk.Volume.TryGetChunk(voxelPosition, out Chunk chunkAtPosition))
            {
                uint voxelData = chunkAtPosition.GetVoxelDataFromWorldPosition(voxelPosition);

                return Voxel.IsAir(voxelData);
            }
            
            // Calculate the bit mask based on the axis and whether it's a back face
            int mask = 1 << (2 * axis + (backFace ? 0 : 1));
            
            return ((int)borderVisibilityFlags & mask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CompareStep(Vector3Int a, Vector3Int b, int direction, bool backFace, BorderVisibilityFlags borderVisibilityFlags)
        {
            uint voxelDataB = chunk.GetVoxelData(b);
            
            if (Voxel.IsAir(voxelDataB))
            {
                return false;
            }
            
            uint voxelColorIdA = Voxel.GetColorId(chunk.GetVoxelData(a));
            uint voxelColorIdB = Voxel.GetColorId(chunk.GetVoxelData(b));
            
            bool isSameColor = voxelColorIdA == voxelColorIdB;
            
            return isSameColor && IsVoxelFaceVisible(b, direction, backFace, borderVisibilityFlags);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MarkVoxelsAsMerged(Vector3Int startPos, Vector3Int quadSize, in bool[] hasMerged, int secondaryAxis, int tertiaryAxis)
        {
            for (int i = 0; i < quadSize[secondaryAxis]; i++)
            {
                for (int j = 0; j < quadSize[tertiaryAxis]; j++)
                {
                    hasMerged[(startPos[secondaryAxis] + i) * Dimensions[tertiaryAxis] + (startPos[tertiaryAxis] + j)] = true;
                }
            }
        }
    }
}