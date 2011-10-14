﻿using System;
using System.Linq;
using Moq;
using WebBackgrounder.EntityFramework;
using WebBackgrounder.EntityFramework.Entities;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class WorkItemCleanupJobFacts
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void DeletesItemsBeyondMaxCount()
            {
                var context = new Mock<WorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Verifiable();
                context.Object.WorkItems = new InMemoryDbSet<WorkItem>
                {
                    new WorkItem {Id = 101, Started = DateTime.UtcNow.AddMinutes(-1)}, 
                    new WorkItem {Id = 102, Started = DateTime.UtcNow.AddMinutes(-2)}, 
                    new WorkItem {Id = 103, Started = DateTime.UtcNow.AddMinutes(-3)}, 
                    new WorkItem {Id = 104, Started = DateTime.UtcNow}, 
                };
                var job = new WorkItemCleanupJob(2, TimeSpan.FromSeconds(1), context.Object);

                job.Execute();

                Assert.Equal(2, context.Object.WorkItems.Count());
                Assert.Equal(101, context.Object.WorkItems.First().Id);
                Assert.Equal(104, context.Object.WorkItems.ElementAt(1).Id);
                context.Verify();
            }

            [Fact]
            public void DoesNothingWhenRecordCountLessThanMaxCount()
            {
                var context = new Mock<WorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Throws(
                    new InvalidOperationException("Should not have tried to save changes"));
                context.Object.WorkItems = new InMemoryDbSet<WorkItem> { new WorkItem(), new WorkItem() };
                var job = new WorkItemCleanupJob(2, TimeSpan.FromSeconds(1), context.Object);

                job.Execute();
            }
        }
    }
}