using System;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
//using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gitan.SortedDictionary;

public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
where TKey : struct, IComparable<TKey>
{
    const bool ColorRed = true;
    const bool ColorBlack = false;

    public enum TreeRotation : byte
    {
        Left,
        LeftRight,
        Right,
        RightLeft
    } 

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public int Compare(long x, long y)
    //{
    //    if (x == y) return 0;
    //    if (IsBids ? (x > y) : (x < y)) return -1;
    //    return 1;
    //}

    private Node? root;
    //public Node? GetRoot => root;

    private int count;
    private int version;

    public bool Reverse { get; }

    public SortedDictionary(bool reverse)
    {
        Reverse = reverse;
    }

    public SortedDictionary(System.Collections.Generic.IDictionary<TKey, TValue> dictionary, bool reverse)
    {
        Reverse = reverse;
        foreach(var item in dictionary)
        {
            AddOrChangeValue(item);
        }
    }

    public int Count
    {
        get
        {
            return count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public ICollection<TKey> Keys
    {
        get => throw new NotImplementedException("");
    }

    public ICollection<TValue> Values
    {
        get => throw new NotImplementedException("");
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
        get => throw new NotImplementedException("");
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
        get => throw new NotImplementedException("");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(TKey item1, TKey item2)
    {
        return Reverse ? item2.CompareTo(item1) : item1.CompareTo(item2);
    }

    public int TotalCount() { return Count; }

    public TValue this[TKey key]
    {
        get
        {
            return Find(key);
        }
        set
        {
            AddCore(key, value, true);
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (AddCore(key, value, false) == false)
        {
            throw new ArgumentException($"An item with the same key has already been added. Key:{key} ");
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        var result = AddCore(item.Key, item.Value, false);
        if(result == false)
        {
            throw new InvalidOperationException($"An item with the same key has already been added. Key:{item.Key} ");
        }
    }

    public bool TryAdd(TKey key, TValue value)
    {
        return AddCore(key, value, false);
    }

    public bool TryAdd(KeyValuePair<TKey, TValue> item)
    {
        return AddCore(item.Key, item.Value, false);
    }

    public void AddOrChangeValue(TKey key, TValue value)
    {
        AddCore(key, value, true);
    }

    public void AddOrChangeValue(KeyValuePair<TKey, TValue> item)
    {
        AddCore(item.Key, item.Value, true);
    }

    private bool AddCore(TKey key, TValue value, bool changeMode)
    {
        if (root == null)
        {
            // The tree is empty and this is the first item.
            root = new Node(key, value, ColorBlack);
            count = 1;
            version++;
            return true;
        }

        // Search for a node at bottom to insert the new node.
        // If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
        // We split 4-nodes along the search path.
        Node? current = root;
        Node? parent = null;
        Node? grandParent = null;
        Node? greatGrandParent = null;

        // Even if we don't actually add to the set, we may be altering its structure (by doing rotations and such).
        // So update `_version` to disable any instances of Enumerator/TreeSubSet from working on it.
        version++;

        int order = 0;
        while (current != null)
        {
            order = Compare(key, current.Key);
            if (order == 0)
            {
                // We could have changed root node to red during the search process.
                // We need to set it to black before we return.
                root.SetBlack();
                if (changeMode)
                {
                    current.Value = value;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Split a 4-node into two 2-nodes.
            if (current.Is4Node)
            {
                current.Split4Node();
                // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                if (Node.IsNonNullRed(parent))
                {
                    InsertionBalance(current, ref parent!, grandParent!, greatGrandParent!);
                }
            }

            greatGrandParent = grandParent;
            grandParent = parent;
            parent = current;
            current = (order < 0) ? current.Left : current.Right;
        }

        Debug.Assert(parent != null);
        // We're ready to insert the new node.
        Node node = new(key, value, ColorRed);
        if (order > 0)
        {
            parent.Right = node;
        }
        else
        {
            parent.Left = node;
        }

        // The new node will be red, so we will need to adjust colors if its parent is also red.
        if (parent.IsRed)
        {
            InsertionBalance(node, ref parent!, grandParent!, greatGrandParent!);
        }

        // The root node is always black.
        root.SetBlack();
        ++count;
        return true;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array) => CopyTo(array, 0, Count);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) => CopyTo(array, index, Count);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_NeedNonNegNum");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_NeedNonNegNum");
        }

        if (count > array.Length - index)
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        int finishCount = 0;
        foreach (var item in this)
        {
            array[index++] = item;
            finishCount++;

            if (finishCount >= count) { break; }
        }
    }

    public void RemoveOrUnder(TKey orUnder)
    {
        while (true)
        {
            Node? current = root;
            if (current == null) { return; }
            while (current.Left != null)
            {
                current = current.Left;
            }
            var firstKey = current.Key;

            int order = Compare(orUnder, firstKey);
            if (order < 0) { return; }

            Remove(firstKey);
        }
    }

    public bool Remove(TKey key)
    {
        if (root == null)
        {
            return false;
        }

        // Search for a node and then find its successor.
        // Then copy the item from the successor to the matching node, and delete the successor.
        // If a node doesn't have a successor, we can replace it with its left child (if not empty),
        // or delete the matching node.
        //
        // In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
        // Following code will make sure the node on the path is not a 2-node.

        // Even if we don't actually remove from the set, we may be altering its structure (by doing rotations
        // and such). So update our version to disable any enumerators/subsets working on it.
        version++;

        Node? current = root;
        Node? parent = null;
        Node? grandParent = null;
        Node? match = null;
        Node? parentOfMatch = null;
        bool foundMatch = false;
        while (current != null)
        {
            if (current.Is2Node)
            {
                // Fix up 2-node
                if (parent == null)
                {
                    // `current` is the root. Mark it red.
                    current.SetRed();
                }
                else
                {
                    Node sibling = parent.GetSibling(current);
                    if (sibling.IsRed)
                    {
                        // If parent is a 3-node, flip the orientation of the red link.
                        // We can achieve this by a single rotation.
                        // This case is converted to one of the other cases below.
                        Debug.Assert(parent.IsBlack);
                        if (parent.Right == sibling)
                        {
                            parent.RotateLeft();
                        }
                        else
                        {
                            parent.RotateRight();
                        }

                        parent.SetRed();
                        sibling.SetBlack(); // The red parent can't have black children.
                                            // `sibling` becomes the child of `grandParent` or `root` after rotation. Update the link from that node.
                        ReplaceChildOrRoot(grandParent, parent, sibling);
                        // `sibling` will become the grandparent of `current`.
                        grandParent = sibling;
                        if (parent == match)
                        {
                            parentOfMatch = sibling;
                        }

                        sibling = parent.GetSibling(current);
                    }

                    Debug.Assert(Node.IsNonNullBlack(sibling));

                    if (sibling.Is2Node)
                    {
                        parent.Merge2Nodes();
                    }
                    else
                    {
                        // `current` is a 2-node and `sibling` is either a 3-node or a 4-node.
                        // We can change the color of `current` to red by some rotation.
                        Node newGrandParent = parent.Rotate(parent.GetRotation(current, sibling))!;

                        newGrandParent.Color = parent.Color;
                        parent.SetBlack();
                        current.SetRed();

                        ReplaceChildOrRoot(grandParent, parent, newGrandParent);
                        if (parent == match)
                        {
                            parentOfMatch = newGrandParent;
                        }
                    }
                }
            }

            // We don't need to compare after we find the match.
            int order = foundMatch ? -1 : Compare(key, current.Key);
            if (order == 0)
            {
                // Save the matching node.
                foundMatch = true;
                match = current;
                parentOfMatch = parent;
            }

            grandParent = parent;
            parent = current;
            // If we found a match, continue the search in the right sub-tree.
            current = order < 0 ? current.Left : current.Right;
        }

        // Move successor to the matching node position and replace links.
        if (match != null)
        {
            ReplaceNode(match, parentOfMatch!, parent!, grandParent!);
            --count;
        }

        root?.SetBlack();
        return foundMatch;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public void Clear()
    {
        root = null;
        count = 0;
        ++version;
    }

    public bool Contains(KeyValuePair<TKey,TValue> item)
    {
        var findNode = FindNode(item.Key);
        if (findNode == null) { return false; }
        if (findNode.Value == null)
        {
            return item.Value == null;
        }
        else
        {
            return EqualityComparer<TValue>.Default.Equals(item.Value, findNode.Value);
        }
    }

    public bool ContainsKey(TKey key)
    {
        return FindNode(key) != null;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Node? node = FindNode(key);
        if (node == null)
        {
            value = default;
            return false;
        }
        value = node.Value;
        return true;
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
    {
        Debug.Assert(parent != null);
        Debug.Assert(grandParent != null);

        bool parentIsOnRight = grandParent.Right == parent;
        bool currentIsOnRight = parent.Right == current;

        Node newChildOfGreatGrandParent;
        if (parentIsOnRight == currentIsOnRight)
        {
            // Same orientation, single rotation
            newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeft() : grandParent.RotateRight();
        }
        else
        {
            // Different orientation, double rotation
            newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeftRight() : grandParent.RotateRightLeft();
            // Current node now becomes the child of `greatGrandParent`
            parent = greatGrandParent;
        }

        // `grandParent` will become a child of either `parent` of `current`.
        grandParent.SetRed();
        newChildOfGreatGrandParent.SetBlack();

        ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
    }

    private void ReplaceChildOrRoot(Node? parent, Node child, Node newChild)
    {
        if (parent != null)
        {
            parent.ReplaceChild(child, newChild);
        }
        else
        {
            root = newChild;
        }
    }

    private void ReplaceNode(Node match, Node parentOfMatch, Node successor, Node parentOfSuccessor)
    {
        Debug.Assert(match != null);

        if (successor == match)
        {
            // This node has no successor. This can only happen if the right child of the match is null.
            Debug.Assert(match.Right == null);
            successor = match.Left!;
        }
        else
        {
            Debug.Assert(parentOfSuccessor != null);
            Debug.Assert(successor.Left == null);
            Debug.Assert((successor.Right == null && successor.IsRed) || (successor.Right!.IsRed && successor.IsBlack));

            successor.Right?.SetBlack();

            if (parentOfSuccessor != match)
            {
                // Detach the successor from its parent and set its right child.
                parentOfSuccessor.Left = successor.Right;
                successor.Right = match.Right;
            }

            successor.Left = match.Left;
        }

        if (successor != null)
        {
            successor.Color = match.Color;
        }

        ReplaceChildOrRoot(parentOfMatch, match, successor!);
    }

    internal Node? FindNode(TKey key)
    {
        Node? current = root;
        while (current != null)
        {
            var order = Compare(key, current.Key);
            if (order == 0)
            {
                return current;
            }

            current = order < 0 ? current.Left : current.Right;
        }

        return null;
    }

    public bool Any() => root != null;

    public KeyValuePair<TKey,TValue> First()
    {
        if (root == null)
        {
            return default;
        }

        Node current = root;
        while (current.Left != null)
        {
            current = current.Left;
        }

        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
    }

    public KeyValuePair<TKey, TValue> Last()
    {
        if (root == null)
        {
            return default;
        }

        Node current = root;
        while (current.Right != null)
        {
            current = current.Right;
        }

        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
    }

    public TKey GetFirstKey()
    {
        if (root == null)
        {
            return default;
        }

        Node current = root;
        while (current.Left != null)
        {
            current = current.Left;
        }

        return current.Key;
    }

    public TKey GetLastKey()
    {
        if (root == null)
        {
            return default;
        }

        Node current = root;
        while (current.Right != null)
        {
            current = current.Right;
        }

        return current.Key;
    }

    public TValue Find(TKey key)
    {
        Node? node = FindNode(key);
        return node == null ? throw new KeyNotFoundException("Could not find Key") : node.Value;
    }

    internal static int Log2(int value) => BitOperations.Log2((uint)value);

#if DEBUG
    public void WriteDebug()
    {
        if (root == null)
        {
            System.Diagnostics.Debug.WriteLine("Root is null.");
            return;
        }
        var stb = new System.Text.StringBuilder();

        WriteDebugCore(stb, 0, root);

        System.Diagnostics.Debug.WriteLine(stb.ToString());
    }

    private void WriteDebugCore(System.Text.StringBuilder stb, int nestLevel, Node? currentNode)
    {
        if (currentNode == null) { return; }

        WriteDebugCore(stb, nestLevel + 1, currentNode.Left);

        for (int i = 0; i < nestLevel; i++)
        {
            stb.Append("  ");
        }
        stb.AppendLine($"{currentNode.Key:#,##0} : {currentNode.Value:0.000} ({(currentNode.IsBlack ? "B" : "R")})");

        WriteDebugCore(stb, nestLevel + 1, currentNode.Right);
    }

#endif

    internal sealed class Node
    {
        public Node(TKey key, TValue value, bool isRed)
        {
            Key = key;
            Value = value;
            Color = isRed;
        }
        public static bool IsNonNullBlack(Node? node) => node != null && node.IsBlack;

        public static bool IsNonNullRed(Node? node) => node != null && node.IsRed;

        public static bool IsNullOrBlack(Node? node) => node == null || node.IsBlack;

        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public Node? Left { get; set; }

        public Node? Right { get; set; }

        public bool Color { get; set; } // true:Red , false:Black

        public bool IsBlack => !Color;

        public bool IsRed => Color;

        public bool Is2Node => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);

        public bool Is4Node => IsNonNullRed(Left) && IsNonNullRed(Right);

        public void SetBlack() => Color = ColorBlack;

        public void SetRed() => Color = ColorRed;

        public TreeRotation GetRotation(Node current, Node sibling)
        {
            Debug.Assert(IsNonNullRed(sibling.Left) || IsNonNullRed(sibling.Right));
            Debug.Assert(HasChildren(current, sibling));

            bool currentIsLeftChild = Left == current;
            return IsNonNullRed(sibling.Left) ?
                (currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right) :
                (currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight);
        }

        public Node GetSibling(Node node)
        {
            Debug.Assert(node != null);
            Debug.Assert(node == Left ^ node == Right);

            return node == Left ? Right! : Left!;
        }
        public void Split4Node()
        {
            Debug.Assert(Left != null);
            Debug.Assert(Right != null);

            SetRed();
            Left.SetBlack();
            Right.SetBlack();
        }

        public Node? Rotate(TreeRotation rotation)
        {
            Node removeRed;
            switch (rotation)
            {
                case TreeRotation.Right:
                    removeRed = Left!.Left!;
                    Debug.Assert(removeRed.IsRed);
                    removeRed.SetBlack();
                    return RotateRight();
                case TreeRotation.Left:
                    removeRed = Right!.Right!;
                    Debug.Assert(removeRed.IsRed);
                    removeRed.SetBlack();
                    return RotateLeft();
                case TreeRotation.RightLeft:
                    Debug.Assert(Right!.Left!.IsRed);
                    return RotateRightLeft();
                case TreeRotation.LeftRight:
                    Debug.Assert(Left!.Right!.IsRed);
                    return RotateLeftRight();
                default:
                    Debug.Fail($"{nameof(rotation)}: {rotation} is not a defined {nameof(TreeRotation)} value.");
                    return null;
            }
        }
        public Node RotateLeft()
        {
            Node child = Right!;
            Right = child.Left;
            child.Left = this;
            return child;
        }

        public Node RotateLeftRight()
        {
            Node child = Left!;
            Node grandChild = child.Right!;

            Left = grandChild.Right;
            grandChild.Right = this;
            child.Right = grandChild.Left;
            grandChild.Left = child;
            return grandChild;
        }

        public Node RotateRight()
        {
            Node child = Left!;
            Left = child.Right;
            child.Right = this;
            return child;
        }

        public Node RotateRightLeft()
        {
            Node child = Right!;
            Node grandChild = child.Left!;

            Right = grandChild.Left;
            grandChild.Left = this;
            child.Left = grandChild.Right;
            grandChild.Right = child;
            return grandChild;
        }

        public void Merge2Nodes()
        {
            Debug.Assert(IsRed);
            Debug.Assert(Left!.Is2Node);
            Debug.Assert(Right!.Is2Node);

            // Combine two 2-nodes into a 4-node.
            SetBlack();
            Left.SetRed();
            Right.SetRed();
        }

        public void ReplaceChild(Node child, Node newChild)
        {
            Debug.Assert(HasChild(child));

            if (Left == child)
            {
                Left = newChild;
            }
            else
            {
                Right = newChild;
            }
        }

        private bool HasChild(Node child) => child == Left || child == Right;
        private bool HasChildren(Node child1, Node child2)
        {
            Debug.Assert(child1 != child2);

            return (Left == child1 && Right == child2)
                || (Left == child2 && Right == child1);
        }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly SortedDictionary<TKey, TValue> _tree;
        private readonly int _version;

        private readonly Stack<Node> _stack;
        private Node? _current;

        //private readonly bool _reverse;

        public Enumerator(SortedDictionary<TKey, TValue> sortedDictinary)
        {
            _tree = sortedDictinary;
            _version = sortedDictinary.version;

            // 2 log(n + 1) is the maximum height.
            _stack = new Stack<Node>(2 * (int)Log2(sortedDictinary.TotalCount() + 1));
            _current = null;

            Initialize();
        }

        private void Initialize()
        {
            _current = null;
            Node? node = _tree.root;
            while (node != null)
            {
                _stack.Push(node);
                node = node.Left;
            }
        }

        public bool MoveNext()
        {
            // Make sure that the underlying subset has not been changed since

            if (_version != _tree.version)
            {
                throw new InvalidOperationException("SR.InvalidOperation_EnumFailedVersion");
            }

            if (_stack.Count == 0)
            {
                _current = null;
                return false;
            }

            _current = _stack.Pop();
            Node? node = _current.Right;
            while (node != null)
            {
                _stack.Push(node);
                node = node.Left;
            }
            return true;
        }

        public readonly void Dispose() { }

        public readonly KeyValuePair<TKey, TValue> Current
        {
            get
            {
                if (_current != null)
                {
                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
                return default; // Should only happen when accessing Current is undefined behavior
            }
        }

        readonly object? IEnumerator.Current
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("SR.InvalidOperation_EnumOpCantHappen");
                }

                return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
            }
        }

        internal readonly bool NotStartedOrEnded => _current == null;

        internal void Reset()
        {
            if (_version != _tree.version)
            {
                throw new InvalidOperationException("SR.InvalidOperation_EnumFailedVersion");
            }

            _stack.Clear();
            Initialize();
        }

        void IEnumerator.Reset() => Reset();
    }
}
