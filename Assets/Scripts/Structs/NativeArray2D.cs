using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Structs
{
    public struct NativeArray2D<T> : IDisposable where T : struct
    {
        private NativeArray<T> _array;
        private ushort _height;
        private ushort _width;

        public ushort Height
        {
            readonly get => _height;
            private set => _height = value;
        }

        public ushort Width
        {
            readonly get => _width;
            private set => _width = value;
        }

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
            get => _array[x + y * _width];
            set => _array[x + y * _width] = value;
        }
        
        public T this[int x, int y]
        {
            get => _array[x + y * _width];
            set => _array[x + y * _width] = value;
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
            get => _array[(int)(index2D.x + index2D.y * _width)];
            set => _array[(int)(index2D.x + index2D.y * _width)] = value;
        }

        public void Dispose()
        {
            _array.Dispose();
        }
    }
}