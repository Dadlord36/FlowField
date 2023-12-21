using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Structs
{
    public struct NativeArray2D<T> : IDisposable where T : struct
    {
        private NativeArray<T> _array;

        public ushort Height { get; private set; }

        public ushort Width { get; private set; }

        public int Length => _array.Length;

        public NativeArray2D(ushort width, ushort height, Allocator allocator) : this()
        {
            _array = new NativeArray<T>(width * height, allocator);
            Width = width;
            Height = height;
        }

        public NativeArray2D(in NativeArray2D<T> inArray, Allocator allocator) : this()
        {
            _array = new NativeArray<T>(inArray._array, allocator);
        }

        public T this[ushort x, ushort y]
        {
            get => _array[x + y * Width];
            set => _array[x + y * Width] = value;
        }

        public T this[int x, int y]
        {
            get => _array[x + y * Width];
            set => _array[x + y * Width] = value;
        }

        public T this[ushort index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public T this[uint2 index2D]
        {
            get => _array[(int)(index2D.x + index2D.y * Width)];
            set => _array[(int)(index2D.x + index2D.y * Width)] = value;
        }

        public void Dispose()
        {
            _array.Dispose();
        }
    }

    public struct BlobArray2D<T> where T : struct
    {
        public BlobArray<T> _array;
        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        public int Length => _array.Length;
        
        public void Initialize(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }

        public T this[ushort x, ushort y] => _array[x + y * Width];

        public T this[int x, int y] => _array[x + y * Width];

        public T this[ushort index] => _array[index];

        public T this[int index] => _array[index];

        public T this[uint2 index2D] => _array[(int)(index2D.x + index2D.y * Width)];


    }
}