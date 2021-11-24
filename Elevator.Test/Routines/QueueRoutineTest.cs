using ElevatorModels;
using FluentAssertions;
using Sensor.Routines;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static ElevatorModels.Button;

namespace Elevator.Test.Routines
{
    public class QueueRoutineTest
    {
        private readonly QueueRoutine queue = new QueueRoutine();

        [Fact]
        public void ReviseUpwardQueue_QueueInOrder_Success()
        {
            EnqueueSamples(queue, firstPressedValue: 1, secondPressedValue: 2);

            var neededToOrder = queue.ReviseUpwardQueueOrder(currentFloor: 0);

            neededToOrder.Should().BeFalse();
        }

        [Fact]
        public void ReviseUpwardQueue_QueueNotInOrder_Success()
        {
            EnqueueSamples(queue, firstPressedValue: 2, secondPressedValue: 1);

            var neededToOrder = queue.ReviseUpwardQueueOrder(currentFloor: 0);

            neededToOrder.Should().BeTrue();

            queue.CurrentUpwardQueue().ToArray().Select(buttonPressed 
                => buttonPressed.ButtonPress).Should().Equal(1, 2);
        }

        [Fact]
        public void Upward_PeekQueue_NotEmpty()
        {
            EnqueueSamples(queue, firstPressedValue: 1, secondPressedValue: 2);

            var valuePresent = queue.UpwardPeekQueue(out var button);

            valuePresent.Should().BeTrue();
            button.Should().NotBeNull();
        }


        [Fact]
        public void Upward_PeekQueue_Empty()
        {
            var valuePresent = queue.UpwardPeekQueue(out var button);

            valuePresent.Should().BeFalse();
            button.Should().BeNull();
        }

        [Fact]
        public void ReviseDownwardQueue_QueueInOrder_Success()
        {
            EnqueueSamples(queue, firstPressedValue: 2, secondPressedValue: 1, upwardQueue: false);

            var neededToOrder = queue.ReviseDownwardQueueOrder(currentFloor:3);

            neededToOrder.Should().BeFalse();
        }

        [Fact]
        public void ReviseDownwardQueue_QueueNotInOrder_Success()
        {
            EnqueueSamples(queue, firstPressedValue: 1, secondPressedValue: 2, upwardQueue: false);

            var neededToOrder = queue.ReviseDownwardQueueOrder(currentFloor: 3);

            neededToOrder.Should().BeTrue();

            queue.CurrentDownwardQueue().ToArray().Select(buttonPressed
                => buttonPressed.ButtonPress).Should().Equal(2, 1);
        }

        [Fact]
        public void Downward_PeekQueue_NotEmpty()
        {
            EnqueueSamples(queue, firstPressedValue: 1, secondPressedValue: 2, upwardQueue: false);

            var valuePresent = queue.DownwardPeekQueue(out var button);

            valuePresent.Should().BeTrue();
            button.Should().NotBeNull();
        }

        [Fact]
        public void Downward_PeekQueue_Empty()
        {
            var valuePresent = queue.DownwardPeekQueue(out var button);

            valuePresent.Should().BeFalse();
            button.Should().BeNull();
        }

        private void EnqueueSamples(QueueRoutine queue, int firstPressedValue,
            int secondPressedValue, bool upwardQueue = true)
        {
            var list = new List<Button> {
            new Button { Action = ButtonAction.Internal.ToString(), ButtonPress = firstPressedValue },
            new Button { Action = ButtonAction.Internal.ToString(), ButtonPress = secondPressedValue }
            };


            foreach (var button in list)
            {
                if (upwardQueue)
                    queue.EnqueueUpwardRequest(button);
                else
                    queue.EnqueueDownwardRequest(button);
            }
        }
    }
}
