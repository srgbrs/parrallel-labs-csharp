namespace ParallelLab
{
    public class QueueNode<T>
    {
        public QueueNode<T> Next;
        public T Value { get; private set; }

        public QueueNode(T value, QueueNode<T> next)
        {
            Value = value;
            Next = next;
        }
    }
}