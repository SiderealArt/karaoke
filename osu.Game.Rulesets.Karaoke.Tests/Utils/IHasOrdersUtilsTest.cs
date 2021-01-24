﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Karaoke.Objects.Types;
using osu.Game.Rulesets.Karaoke.Utils;

namespace osu.Game.Rulesets.Karaoke.Tests.Utils
{
    public class IHasOrdersUtilsTest
    {
        [TestCase(new[] { 1, 2, 3, 4 }, false)]
        [TestCase(new[] { 1, 2, 3, 5 }, false)]
        [TestCase(new[] { 1, 2, 3, 3 }, true)]
        [TestCase(new[] { 1, 1, 1, 1 }, true)]
        [TestCase(new[] { -1, -2, -3, -4 }, false)] // should not include those ids but not check for now.
        [TestCase(new int[] { }, false)]
        public void TestContainDuplicatedId(int[] orders, bool containDuplicated)
        {
            var objects = orders.Select(x => new TestOrderObject { Order = x }).ToArray();
            var result = IHasOrdersUtils.ContainDuplicatedId(objects);
            Assert.AreEqual(result, containDuplicated);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, 1)]
        [TestCase(new[] { 4, 3, 2, 1 }, 1)]
        [TestCase(new int[] { }, 0)]
        public void TestGetMinOrderNumber(int[] orders, int number)
        {
            var objects = orders.Select(x => new TestOrderObject { Order = x }).ToArray();
            var result = IHasOrdersUtils.GetMinOrderNumber(objects);
            Assert.AreEqual(result, number);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, 4)]
        [TestCase(new[] { 4, 3, 2, 1 }, 4)]
        [TestCase(new int[] { }, 0)]
        public void TestGetMaxOrderNumber(int[] orders, int number)
        {
            var objects = orders.Select(x => new TestOrderObject { Order = x }).ToArray();
            var result = IHasOrdersUtils.GetMaxOrderNumber(objects);
            Assert.AreEqual(result, number);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 4, 3, 2, 1 }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 4, 4, 2, 2 }, new[] { 2, 2, 4, 4 })] // should not happen but still make a order.
        [TestCase(new int[] { }, new int[] { })]
        [TestCase(null, null)]
        public void TestSorted(int[] orders, int[] actualOrders)
        {
            var objects = orders?.Select(x => new TestOrderObject { Order = x });
            var orderedArray = IHasOrdersUtils.Sorted(objects);
            var result = orderedArray?.Select(x => x.Order).ToArray();
            Assert.AreEqual(result, actualOrders);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, 1, new int[] { }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 1, 2, 3, 4 }, -1, new[] { 1, 2, 3, 4 }, new[] { -1, 0, 1, 2 })]
        [TestCase(new[] { 1, 2, 3, 4 }, 3, new[] { 4, 3, 2, 1 }, new[] { 3, 4, 5, 6 })] // change id should consider will affect exist mapping.
        [TestCase(new[] { 4, 3, 2, 1 }, 3, new[] { 4, 3, 2, 1 }, new[] { 6, 5, 4, 3 })] // change id should consider will affect exist mapping.
        [TestCase(new[] { 1, 3, 5, 7 }, 1, new[] { 3, 5, 7 }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 1, 1, 1, 1 }, 1, new[] { 1, 1, 1 }, new[] { 1, 2, 3, 4 })] // invalid input might cause some of id mapping will be lost.
        public void TestResortOrder(int[] orders, int startFrom, int[] movingOrders, int[] newOrder)
        {
            var objects = orders?.Select(x => new TestOrderObject { Order = x }).ToArray();

            var movingStepResult = new List<int>();
            IHasOrdersUtils.ResortOrder(objects, startFrom, (obj, oldOrder, newOrder) =>
            {
                movingStepResult.Add(oldOrder);
            });

            // change order result.
            var result = objects?.Select(x => x.Order).ToArray();
            Assert.AreEqual(result, newOrder);

            // should check moving order step also.
            Assert.AreEqual(movingStepResult.ToArray(), movingOrders);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, 1, 2, new[] { 1, 2, -1 }, new[] { 2, 1, 3, 4 })]
        [TestCase(new[] { 1, 2, 3, 4 }, 2, 1, new[] { 2, 1, -1 }, new[] { 2, 1, 3, 4 })]
        [TestCase(new[] { 1, 2, 3, 4 }, 1, 4, new[] { 1, 2, 3, 4, -1 }, new[] { 4, 1, 2, 3 })]
        [TestCase(new[] { 1, 2, 3, 4 }, 4, 1, new[] { 4, 3, 2, 1, -1 }, new[] { 2, 3, 4, 1 })]
        public void TestChangeOrder(int[] orders, int oldOrder, int nowOrder, int[] movingOrders, int[] newOrder)
        {
            try
            {
                var objects = orders?.Select(x => new TestOrderObject { Order = x }).ToArray();

                // record order index change step.
                var movingStepResult = new List<int>();

                // This utils only change order property.
                IHasOrdersUtils.ChangeOrder(objects, oldOrder, nowOrder, (obj, oldOrder, newOrder) =>
                {
                    movingStepResult.Add(oldOrder);
                });

                // change order result.
                var result = objects?.Select(x => x.Order).ToArray();
                Assert.AreEqual(result, newOrder);

                // should check moving order step also.
                Assert.AreEqual(movingStepResult.ToArray(), movingOrders);
            }
            catch
            {
                Assert.IsNull(newOrder);
            }
        }

        internal class TestOrderObject : IHasOrder
        {
            public int Order { get; set; }
        }
    }
}
