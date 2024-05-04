using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

public class LimitedQueue<T>
{
    private Queue<T> queue;
    private int capacity;

    public LimitedQueue(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        }

        this.capacity = capacity;
        this.queue = new Queue<T>(capacity);
    }

    public void Enqueue(T item)
    {
        if (queue.Count == capacity)
        {
            queue.Dequeue(); // Remove the oldest item if the queue is full
        }
        queue.Enqueue(item);
    }

    public T Dequeue()
    {
        if (queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }
        return queue.Dequeue();
    }

    public T Peek()
    {
        return queue.Peek();
    }

    public bool TryDequeue(out T item) 
    {
        return queue.TryDequeue(out item);
    }

    public bool TryPeek(out T item)
    {
        return queue.TryPeek(out item);
    }
    public int Count => queue.Count;
    public int Capacity => capacity;
}


public class InputAction 
{
    public float axis;
    public Button button;
    private float timestamp;
    private float buffer;

    public InputAction(Button button, float timestamp, float buffer, float axis = 0) 
    {
        this.axis = axis;
        this.button = button;
        this.timestamp = timestamp;
        this.buffer = buffer;
    } 

    public bool Validate()
    {
        return timestamp + buffer >= Time.time;
    }
}