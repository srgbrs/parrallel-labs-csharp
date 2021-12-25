using System;
using ParallelLab.Util;

namespace ParallelLab 
{
    public class SkipList<T>
    {
        private Node<T> Head { get; } = new(int.MinValue); 

        private Node<T> Tail { get; } = new(int.MaxValue);

        public SkipList()
        {
            for (var i = 0; i < Head.Next.Length; ++i)
            {
                Head.Next[i] = new MarkedReference<Node<T>>(Tail, false);
            }
        }

        public bool Add(Node<T> node)
        {
            var predecessor = new Node<T>[SkipListSettings.MaxLevel + 1];
            var successor = new Node<T>[SkipListSettings.MaxLevel + 1];

            while (true)
            {
                try
                {
                    return ForAdd(node, predecessor, successor);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }
        public bool Remove(Node<T> node)
        {
            var predecessor = new Node<T>[SkipListSettings.MaxLevel + 1];
            var successor = new Node<T>[SkipListSettings.MaxLevel + 1];

            while (true)
            {
                try
                {
                    return ForRemove(node, predecessor, successor);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }
      
        private bool Find(Node<T> node, ref Node<T>[] predecessor, ref Node<T>[] successor)
        {
            var marked = false;
            var isRetryNeeded = false;
            Node<T> curr = null;

            while (true)
            {
                var pred = Head;
                for (var level = SkipListSettings.MaxLevel; level >= SkipListSettings.MinLevel; level--)
                {
                    curr = pred.Next[level].Value;
                    while (true)
                    {
                        var succ = curr.Next[level].Get(ref marked);
                        while (marked) 
                        {
                            var snip = pred.Next[level].CompareAndExchange(succ, false, curr, false);
                            if (!snip)
                            {
                                isRetryNeeded = true;
                                break;
                            }

                            curr = pred.Next[level].Value;
                            succ = curr.Next[level].Get(ref marked);
                        }

                        if (isRetryNeeded)
                        {
                            break;
                        }

                        if (curr.NodeKey < node.NodeKey)
                        {
                            pred = curr;
                            curr = succ;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (isRetryNeeded)
                    {
                        isRetryNeeded = false;
                        continue;
                    }

                    predecessor[level] = pred;
                    successor[level] = curr;
                }

                return curr != null && curr.NodeKey == node.NodeKey;
            }
        }

       
        private bool ForRemove(Node<T> node, Node<T>[] predecessor, Node<T>[] successor)
        {
            var found = Find(node, ref predecessor, ref successor);
            if (!found)
            {
                return false;
            }

            Node<T> succ;
            for (var level = node.TopLevel; level > SkipListSettings.MinLevel; level--)
            {
                var isMarked = false;
                succ = node.Next[level].Get(ref isMarked);

                while (!isMarked)
                {
                    node.Next[level].CompareAndExchange(succ, true, succ, false);
                    succ = node.Next[level].Get(ref isMarked);
                }
            }

            var marked = false;
            succ = node.Next[SkipListSettings.MinLevel].Get(ref marked);

            while (true)
            {
                var iMarkedIt = node.Next[SkipListSettings.MinLevel].CompareAndExchange(succ, true, succ, false);
                succ = successor[SkipListSettings.MinLevel].Next[SkipListSettings.MinLevel].Get(ref marked);

                if (iMarkedIt)
                {
                    Find(node, ref predecessor, ref successor);
                    return true;
                }

                if (marked)
                {
                    return false;
                }
            }
        }

     
        private bool ForAdd(Node<T> node, Node<T>[] predecessor, Node<T>[] successor)
        {
            if (Find(node, ref predecessor, ref successor)) 
            {
                return false;
            }
            var topLevel = node.TopLevel;

            for (var level = SkipListSettings.MinLevel; level <= topLevel; level++)
            {
                var tempSucc = successor[level];
                node.Next[level] = new MarkedReference<Node<T>>(tempSucc, false);
            }

            var pred = predecessor[SkipListSettings.MinLevel];
            var succ = successor[SkipListSettings.MinLevel];

            node.Next[SkipListSettings.MinLevel] = new MarkedReference<Node<T>>(succ, false);

            if (!pred.Next[SkipListSettings.MinLevel].CompareAndExchange(node, false, succ, false))
            {
                throw new Exception();
            }

            for (var level = 1; level <= topLevel; level++)
            {
                while (true)
                {
                    pred = predecessor[level];
                    succ = successor[level];

                    if (pred.Next[level].CompareAndExchange(node, false, succ, false))
                    {
                        break;
                    }

                    Find(node, ref predecessor, ref successor);
                }
            }

            return true;
        }
    }
}