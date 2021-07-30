using System;
using System.Collections.Generic;

namespace SortedStorage.Application.SymbolTable
{
    public class Node<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
        public int Size { get; set; }
        public Node<TKey, TValue> Left { get; set; }
        public Node<TKey, TValue> Right { get; set; }
        public bool IsRed { get; set; }

        public Node(TKey key, TValue value, int size, bool isRed)
        {
            Key = key;
            Value = value;
            Size = size;
            IsRed = isRed;
        }
    }

    internal static class ExtensionRedBlack
    {
        internal static Node<TKey, TValue> GetMin<TKey, TValue>(this Node<TKey, TValue> node)
        {
            if (node.Left == null) return node;
            return node.Left.GetMin();
        }

        internal static Node<TKey, TValue> GetMax<TKey, TValue>(this Node<TKey, TValue> node)
        {
            if (node.Right == null) return node;
            return node.Right.GetMax();
        }

        internal static Node<TKey, TValue> GetFloor<TKey, TValue>(this Node<TKey, TValue> node, TKey key)
            where TKey : IComparable
        {
            if (node == null) return null;

            int cmp = key.CompareTo(node.Key);

            if (cmp == 0) return node;
            if (cmp < 0) return node.Left.GetFloor(key);

            var floor = node.Right.GetFloor(key);

            if (floor != null) return floor;
            return node;
        }

        internal static Node<TKey, TValue> GetCeiling<TKey, TValue>(this Node<TKey, TValue> node, TKey key)
            where TKey : IComparable
        {
            if (node == null) return null;

            int cmp = key.CompareTo(node.Key);

            if (cmp == 0) return node;
            if (cmp > 0) return node.Right.GetCeiling(key);

            var ceiling = node.Left.GetCeiling(key);

            if (ceiling != null) return ceiling;
            return node;
        }

        internal static int GetSize<TKey, TValue>(this Node<TKey, TValue> x) => x?.Size ?? 0;

        internal static Node<TKey, TValue> Select<TKey, TValue>(this Node<TKey, TValue> node, int position)
        {
            if (node == null) return null;

            int t = node.Left.GetSize();

            if (t > position) return node.Left.Select(position);
            if (t < position) return node.Right.Select(position - t - 1);
            return node;
        }

        internal static int Rank<TKey, TValue>(this Node<TKey, TValue> node, TKey key)
            where TKey : IComparable
        {
            if (node == null) return -1;

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0) return node.Left.Rank(key);
            if (cmp > 0)
            {
                var result = node.Right.Rank(key);
                return result == -1
                    ? -1
                    : 1 + node.Left.GetSize() + result;
            }
            return node.Left.GetSize();
        }

        internal static TValue Get<TKey, TValue>(this Node<TKey, TValue> node, TKey key)
            where TKey : IComparable
        {
            if (node == null) return default;

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0) return node.Left.Get(key);
            if (cmp > 0) return node.Right.Get(key);
            return node.Value;
        }

        internal static Node<TKey, TValue> Add<TKey, TValue>(this Node<TKey, TValue> node, TKey key, TValue value)
            where TKey : IComparable
        {
            if (node == null)
                return new Node<TKey, TValue>(key, value, 1, true);

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0) node.Left = node.Left.Add(key, value);
            else if (cmp > 0) node.Right = node.Right.Add(key, value);
            else node.Value = value;

            if (IsRed(node?.Right) && !IsRed(node?.Left))
                node = node.RotateLeft();

            if (IsRed(node?.Left) && IsRed(node?.Left?.Left))
                node = node.RotateRight();

            if (IsRed(node?.Left) && IsRed(node?.Right))
                node.FlipColors();

            node.Size = 1 + node.Left.GetSize() + node.Right.GetSize();
            return node;
        }

        private static bool IsRed<TKey, TValue>(Node<TKey, TValue> node) => node?.IsRed ?? false;

        internal static bool RightNodeContainsLeftNodeInsideRange<TKey, TValue>(this Node<TKey, TValue> node, TKey start, TKey end)
            where TKey : IComparable
        {
            var rightNode = node.Right;
            if (rightNode == null) return false;

            var leftNode = rightNode.Left;
            if (leftNode == null) return false;

            return leftNode.InRange(start, end);
        }

        internal static bool LeftNodeContainsRightNodeInsideRange<TKey, TValue>(this Node<TKey, TValue> node, TKey start, TKey end)
            where TKey : IComparable
        {
            var leftNode = node.Left;
            if (leftNode == null) return false;

            var rightNode = leftNode.Right;
            if (rightNode == null) return false;

            return rightNode.InRange(start, end);
        }

        internal static bool LeftNodeIsHigherThanStartRange<TKey, TValue>(this Node<TKey, TValue> node, TKey start)
            where TKey : IComparable
        {
            var leftNode = node.Left;
            if (leftNode == null) return false;
            return start.CompareTo(leftNode.Key) <= 0;
        }

        internal static bool RightNodeIsLowerThanEndRange<TKey, TValue>(this Node<TKey, TValue> node, TKey end)
            where TKey : IComparable
        {
            var rightNode = node.Right;
            if (rightNode == null) return false;
            return end.CompareTo(rightNode.Key) >= 0;
        }

        internal static bool InRange<TKey, TValue>(this Node<TKey, TValue> node, TKey start, TKey end)
            where TKey : IComparable
        {
            return start.CompareTo(node.Key) <= 0 && end.CompareTo(node.Key) >= 0;
        }

        internal static Node<TKey, TValue> RotateLeft<TKey, TValue>(this Node<TKey, TValue> h)
        {
            Node<TKey, TValue> x = h.Right;
            h.Right = x.Left;
            x.Left = h;
            x.IsRed = h.IsRed;
            h.IsRed = true;
            x.Size = h.Size;
            h.Size = 1 + h.Left.GetSize() + h.Right.GetSize();

            return x;
        }

        internal static Node<TKey, TValue> RotateRight<TKey, TValue>(this Node<TKey, TValue> h)
        {
            Node<TKey, TValue> x = h.Left;
            h.Left = x.Right;
            x.Right = h;
            x.IsRed = h.IsRed;
            h.IsRed = true;
            x.Size = h.Size;
            h.Size = 1 + h.Left.GetSize() + h.Right.GetSize();

            return x;
        }

        internal static void FlipColors<TKey, TValue>(this Node<TKey, TValue> h)
        {
            h.IsRed = true;
            h.Left.IsRed = false;
            h.Right.IsRed = false;
        }
    }

    public class RedBlackTree<TKey, TValue>
        where TKey : class, IComparable
    {
        private Node<TKey, TValue> root;

        public int Size => root.GetSize();

        public TKey Min => root.GetMin()?.Key;

        public TKey Max => root.GetMax()?.Key;

        public TKey GetFloor(TKey key) => root.GetFloor(key)?.Key;

        public TKey GetCeiling(TKey key) => root.GetCeiling(key)?.Key;

        public TKey Select(int position) => root.Select(position)?.Key;

        public int Rank(TKey key) => root.Rank(key);

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        public TValue Get(TKey key) => root.Get(key);

        public void Add(TKey key, TValue value)
        {
            root = root.Add(key, value);
            root.IsRed = false;
        }

        public bool IsEmpty() => Size == 0;

        public void Clear() => root = null;

        public IEnumerable<Node<TKey, TValue>> GetAll()
        {
            foreach (var item in GetInRange(Min, Max))
            {
                yield return item;
            }
        }

        public IEnumerable<Node<TKey, TValue>> GetInRange(TKey start, TKey end)
        {
            if (root == null)
                yield break;

            var visited = new HashSet<TKey>();
            var stack = new Stack<Node<TKey, TValue>>();
            stack.Push(root);
            while (stack.Count != 0)
            {
                while (stack.Peek().Left != null && !visited.Contains(stack.Peek()?.Left?.Key))
                {
                    var peekNode = stack.Peek();
                    if (peekNode.LeftNodeIsHigherThanStartRange(start))
                        stack.Push(peekNode.Left);
                    else if (peekNode.LeftNodeContainsRightNodeInsideRange(start, end) && !visited.Contains(peekNode.Left.Right.Key))
                        stack.Push(peekNode.Left.Right);
                    else
                        break;
                }

                var node = stack.Pop();
                visited.Add(node.Key);

                if (node.RightNodeIsLowerThanEndRange(end) || node.RightNodeContainsLeftNodeInsideRange(start, end))
                    stack.Push(node.Right);

                if (node.InRange(start, end))
                    yield return node;
            }
        }
    }
}
