﻿using System;
using ParallelLab.Util;
using ParallelLab.Util.Interfaces;

namespace ParallelLab
{
    public class LockFreeLinkedList<TKey, TValue> 
        where TKey : IBorderline<TValue>, new()
        where TValue : IComparable<TValue>
    {
        public LinkedListNode<TValue> Head { get; }

        public LinkedListNode<TValue> Tail { get; }

        public LockFreeLinkedList()
        {
            var dummy = new TKey();
            Head = new LinkedListNode<TValue>(dummy.MinValue());
            Tail = new LinkedListNode<TValue>(dummy.MaxValue());

            while (!Head.Next.CompareAndExchange(Tail, false, default, false))
            {
            }
        }

        public bool Add(TValue value)
        {
            var node = new LinkedListNode<TValue>(value);

            while (true)
            {
                try
                {
                    return ForAdd(value, node);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        public bool Remove(TValue value)
        {
            while (true)
            {
                try
                {
                    return ForRemove(value);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        public LinkedListNode<TValue> Search(TValue searchValue, ref LinkedListNode<TValue> leftNode)
        {
            var isRetryNeeded = false;
            var marked = false;
            
            while (true)
            {
                var head = Head;
                var headNext = head.Next.Value;

                while (true)
                {
                    var succ = headNext.Next.Get(ref marked);
                    while (marked)
                    {
                        var snip = head.Next.CompareAndExchange(succ, false, headNext, false);
                        if (!snip)
                        {
                            isRetryNeeded = true;
                            break;
                        }

                        headNext = head.Next.Value;
                        succ = headNext.Next.Get(ref marked);
                    }

                    if (isRetryNeeded)
                    {
                        isRetryNeeded = false;
                        continue;
                    }

                    if (headNext.Value.CompareTo(searchValue) < 0)
                    {
                        head = headNext;
                        headNext = succ;
                    }
                    else
                    {
                        leftNode = head;
                        return headNext;
                    }
                }
            }
        }

        private bool ForAdd(TValue value, LinkedListNode<TValue> node)
        {
            LinkedListNode<TValue> left = null;
            var right = Search(value, ref left);
            if (right != Tail && right.Value.CompareTo(value) == 0)
            {
                return false;
            }

            node.Next = new MarkedReference<LinkedListNode<TValue>>(right, false);
            return left.Next.CompareAndExchange(node, false, right, false) || true;
        }

        private bool ForRemove(TValue value)
        {
            LinkedListNode<TValue> left = null;
            var right = Search(value, ref left);

            if (right.Value.CompareTo(value) != 0)
            {
                return false;
            }

            var rightNext = right.Next.Value;

            var snip = right.Next.AttemptMark(rightNext, true);
            if (snip)
            {
                throw new Exception();
            }

            left.Next.CompareAndExchange(rightNext, false, right, false);
            return true;
        }
    }
}