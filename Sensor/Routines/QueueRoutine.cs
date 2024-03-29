﻿using ElevatorModels;
using System.Collections.Generic;
using System.Linq;

namespace Sensor.Routines
{
    public class QueueRoutine : IQueueRoutine
    {
        private readonly object _inputQueueResource = new object();
        private readonly object _dequeueResource = new object();
        private Queue<Button> UpwardQueue = new Queue<Button>();
        private Queue<Button> DownwardQueue = new Queue<Button>();
        public bool DisallowFurtherEnqueuing { get; set; }

        private static QueueRoutine _instance = new QueueRoutine();

        public static QueueRoutine GetQueueRoutine()
        {
            if (_instance == null)
            {
                _instance = new QueueRoutine();
            }
            return _instance;
        }

        public bool ReviseUpwardQueueOrder(int currentFloor)
        {
            bool revised = false;
            var queueCopy = UpwardQueue.ToArray();
            var inOder = queueCopy.SequenceEqual(
                UpwardQueue.OrderBy(request => request.ButtonPress));

            if (!inOder)
            {
                revised = true;
                ResetUpwardQueueOrder();
            }

            return revised;
        }

        public bool ReviseDownwardQueueOrder(int currentFloor)
        {
            bool revised = false;
            var queueCopy = DownwardQueue.ToArray();
            var inOder = queueCopy.SequenceEqual(DownwardQueue
                .OrderByDescending(request => request.ButtonPress));

            if (!inOder)
            {
                revised = true;
                ResetDownwardQueueOrder();
            }

            return revised;
        }

        private void ResetDownwardQueueOrder()
        {
            var downwardElememts = new Queue<Button>(DownwardQueue.ToArray()
                                        .GroupBy(button => button.ButtonPress)
                                        .Select(group => group.First())
                                        .OrderByDescending(floor => floor.ButtonPress)
                                        .ToList());
            lock (_inputQueueResource)
            {
                DownwardQueue = downwardElememts;
            }
        }

        private void ResetUpwardQueueOrder()
        {
            var upwardElememts = new Queue<Button>(UpwardQueue
                                .ToArray().GroupBy(button => button.ButtonPress)
                                .Select(group => group.First())
                                .OrderBy(floor => floor.ButtonPress)
                                .ToList());

            lock (_inputQueueResource)
            {
                UpwardQueue = upwardElememts;
            }
        }

        public Queue<Button> CurrentUpwardQueue()
        {
            return UpwardQueue;
        }

        public Queue<Button> CurrentDownwardQueue()
        {
            return DownwardQueue;
        }

        public void EnqueueUpwardRequest(Button button)
        {
            lock (_inputQueueResource)
            {
                UpwardQueue.Enqueue(button);
            }
        }

        public Button DequeueUpwardRequest()
        {
            lock (_dequeueResource)
            {
                return UpwardQueue.Dequeue();
            }
        }

        public void EnqueueDownwardRequest(Button button)
        {
            lock (_inputQueueResource)
            {
                DownwardQueue.Enqueue(button);
            }
        }

        public Button DequeueDowanwardRequest()
        {
            lock (_dequeueResource)
            {
                return DownwardQueue.Dequeue();
            }
        }

        public bool ActiveRequest() => UpwardQueue.TryPeek(out var _)
            || DownwardQueue.TryPeek(out var _);


        public bool DownwardEmptyQueue() => DownwardQueue.Any();
        public bool UpwardEmptyQueue() => UpwardQueue.Any();

        public bool DownwardPeekQueue(out Button button)
        {
            if (DownwardQueue.Count >= 1)
            {
                DownwardQueue.TryPeek(out var el);
                button = el;
                return true;
            }

            button = null;
            return false;
        }
        public bool UpwardPeekQueue(out Button button)
        {
            if (UpwardQueue.Count >= 1)
            {
                UpwardQueue.TryPeek(out var el);
                button = el;
                return true;
            }

            button = null;
            return false;
        }
    }
}
