using System;
using Unity.Collections;

namespace Structs
{
    public struct NativeArray2D<T> : IDisposable where T : struct
    {
        private NativeArray<T> _array;
        private int _height;
        private int _width;

        public int Height
        {
            readonly get => _height;
            private set => _height = value;
        }

        public int Width
        {
            readonly get => _width;
            private set => _width = value;
        }

        public int Length => _array.Length;

        public NativeArray2D(int width, int height, Allocator allocator) : this()
        {
            _array = new NativeArray<T>(width * height, allocator);
            Width = width;
            Height = height;
        }

        public T this[int x, int y]
        {
            get => _array[x + y * _width];
            set => _array[x + y * _width] = value;
        }

        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public void Dispose()
        {
            _array.Dispose();
        }
    }
}