﻿namespace Wacton.Colonies.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Wacton.Colonies.Interfaces;
    using Wacton.Colonies.Logic;
    using Wacton.Colonies.Models;
    using Wacton.Colonies.Tests.Mocks;

    [TestFixture]
    public class RandomDecisionTests
    {
        private List<TestMeasurableItem> items;

        [SetUp]
        public void SetupTest()
        {
            this.items = new List<TestMeasurableItem>();

            var itemIdentifiers = new List<string> { "A", "B", "C", "D", "W", "X", "Y", "Z" };
            foreach (var itemIdentifier in itemIdentifiers)
            {
                this.items.Add(new TestMeasurableItem(itemIdentifier));
            }
        }

        [Test]
        public void EqualItemsSingleMeasure()
        {
            // override the random number generator used by the decision logic so it can be manipulated
            var mockRandom = new MockRandom();
            DecisionLogic.SetRandomNumberGenerator(mockRandom);

            var biasedItem = new TestBiasedItem(1.0, 0);
            var chosenItems = new List<TestMeasurableItem>();

            // the numbers generated are distributed evenly based on the number of organisms
            // therefore there should be each organism should be chosen once by the decision logic
            var nextDouble = 0.0;
            for (var i = 0; i < this.items.Count; i++)
            {
                mockRandom.SetNextDouble(nextDouble);
                chosenItems.Add(DecisionLogic.MakeDecision(this.items, biasedItem));
                nextDouble += 1.0 / this.items.Count;
            }

            // expecting each item to be chosen once, so expecting the same as the original list
            var expectedItems = this.items;
            var actualItems = chosenItems;
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public void EqualItemsMultipleMeasures()
        {
            // override the random number generator used by the decision logic so it can be manipulated
            var mockRandom = new MockRandom();
            DecisionLogic.SetRandomNumberGenerator(mockRandom);

            // bias in favour of health 2x more than of pheromone
            var biasedItem = new TestBiasedItem(1.0, 2.0);
            var chosenItems = new List<TestMeasurableItem>();

            // the numbers generated are distributed evenly based on the number of organisms
            // therefore there should be each organism should be chosen once by the decision logic
            var nextDouble = 0.0;
            for (var i = 0; i < this.items.Count; i++)
            {
                mockRandom.SetNextDouble(nextDouble);
                chosenItems.Add(DecisionLogic.MakeDecision(this.items, biasedItem));
                nextDouble += 1.0 / this.items.Count;
            }

            // all items are equal, so the bias should not affect the frequency with which they are selected
            // expecting each item to be chosen once, so expecting the same as the original list
            var expectedItems = this.items;
            var actualItems = chosenItems;
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public void InequalItemsSingleMeasure()
        {
            // all items will only have one measurement but the levels will all be different
            // by an even spread based on how many items there are
            var measurementIncrement = 1.0 / this.items.Count;
            for (var i = 0; i < this.items.Count; i++)
            {
                var measurementLevel = (i + 1) * measurementIncrement;
                this.items[i].SetPheromoneLevel(measurementLevel);
            }

            // override the random number generator used by the decision logic so it can be manipulated
            // and set the base weighting to 0 (so that chance of being chosen is based directly on measurment level * bias)
            var mockRandom = new MockRandom();
            DecisionLogic.SetRandomNumberGenerator(mockRandom);
            DecisionLogic.SetBaseWeighting(0.0);

            var biasedItem = new TestBiasedItem(1.0, 0.0);
            var chosenItems = new List<TestMeasurableItem>();

            // the numbers generated need to reflect the range of measurement levels in the items
            // if there are 8 items...
            // - 8/8 pheromone -> 8 results
            // - 7/8 pheromone -> 7 results
            // - 6/8 pheromone -> 6 results
            // - ...
            // - 1/8 pheromone -> 1 result
            // total number of results: 1 + 2 + 3 + 4 + ... = n(n + 1)/2 [sum of integers from 1 - n]
            var numberOfResults = this.items.Count * (this.items.Count + 1) / 2.0;
            var nextDouble = 0.0;
            for (var i = 0; i < numberOfResults; i++)
            {
                mockRandom.SetNextDouble(nextDouble);
                chosenItems.Add(DecisionLogic.MakeDecision(this.items, biasedItem));
                nextDouble += 1.0 / numberOfResults;
            }

            // expecting each organism to be chosen a number of times proportional to the single measurement level used
            var expectedItemCounts = new Dictionary<TestMeasurableItem, int>();
            for (var i = 0; i < this.items.Count; i++)
            {
                expectedItemCounts.Add(this.items.ElementAt(i), i + 1);
            }

            var actualItemCounts = new Dictionary<TestMeasurableItem, int>();
            foreach (var item in this.items)
            {
                var numberOfTimesChosen = chosenItems.Count(chosenItem => chosenItem.Equals(item));
                actualItemCounts.Add(item, numberOfTimesChosen);
            }

            Assert.That(actualItemCounts, Is.EqualTo(expectedItemCounts));
        }

        [Test]
        public void InequalItemsMultipleBalancingMeasures()
        {
            // all items will have two measures, the second being the inverse of the first
            // this will balance the overall weighting and give all items a perfectly even chance
            var measurementIncrement = 1.0 / this.items.Count;
            for (var i = 0; i < this.items.Count; i++)
            {
                var measurementLevel = (i + 1) * measurementIncrement;
                this.items[i].SetPheromoneLevel(measurementLevel);
                this.items[i].SetHealthLevel(1.0 - measurementLevel);
            }

            // override the random number generator used by the decision logic so it can be manipulated
            // and set the base weighting to 0 (so that chance of being chosen is based directly on measurment level * bias)
            var mockRandom = new MockRandom();
            DecisionLogic.SetRandomNumberGenerator(mockRandom);
            DecisionLogic.SetBaseWeighting(0.0);

            // bias of both measurements are the same in order for them to balance
            var biasedItem = new TestBiasedItem(1.0, 1.0);
            var chosenItems = new List<TestMeasurableItem>();

            var nextDouble = 0.0;
            for (var i = 0; i < this.items.Count; i++)
            {
                mockRandom.SetNextDouble(nextDouble);
                chosenItems.Add(DecisionLogic.MakeDecision(this.items, biasedItem));
                nextDouble += 1.0 / this.items.Count;
            }

            // items are not equal, but the bias should evenly distribute the selection of items
            var expectedItems = this.items;
            var actualItems = chosenItems;
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public void InequalItemsMultipleUnbalancingMeasures()
        {
            // all items will have two measures, the second being double the inverse of the first
            // this imbalance will be corrected by the bias, showing that the bias has the desired effect
            var measurementIncrement = 1.0 / this.items.Count;
            for (var i = 0; i < this.items.Count; i++)
            {
                var measurementLevel = (i + 1) * measurementIncrement;
                this.items[i].SetPheromoneLevel(measurementLevel / 2.0);
                this.items[i].SetHealthLevel(1 - measurementLevel);
            }

            // override the random number generator used by the decision logic so it can be manipulated
            // and set the base weighting to 0 (so that chance of being chosen is based directly on measurment level * bias)
            var mockRandom = new MockRandom();
            DecisionLogic.SetRandomNumberGenerator(mockRandom);
            DecisionLogic.SetBaseWeighting(0.0);

            // pheromone bias is double that of health bias to compensate for the halving of the measurement level
            var biasedItem = new TestBiasedItem(2.0, 1.0);
            var chosenItems = new List<TestMeasurableItem>();

            var nextDouble = 0.0;
            for (var i = 0; i < this.items.Count; i++)
            {
                mockRandom.SetNextDouble(nextDouble);
                chosenItems.Add(DecisionLogic.MakeDecision(this.items, biasedItem));
                nextDouble += 1.0 / this.items.Count;
            }

            // items are not equal, but the bias should evenly compensate for the disproportionate measurements
            // and evenly distribute the selection of items
            var expectedItems = this.items;
            var actualItems = chosenItems;
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        private class TestMeasurableItem : IMeasurable
        {
            private readonly string identifier;
            private readonly Condition pheromoneCondition;
            private readonly Condition healthCondition;

            public TestMeasurableItem(string identifier)
            {
                this.identifier = identifier;
                this.pheromoneCondition = new Condition(Measure.Pheromone, 1.0);
                this.healthCondition = new Condition(Measure.Health, 1.0);
            }

            public void SetPheromoneLevel(double pheromoneLevel)
            {
                this.pheromoneCondition.SetLevel(pheromoneLevel);
            }

            public void SetHealthLevel(double healthLevel)
            {
                this.healthCondition.SetLevel(healthLevel);
            }

            public Measurement GetMeasurement()
            {
                return new Measurement(new List<Condition> { this.pheromoneCondition, this.healthCondition });
            }

            public override string ToString()
            {
                return this.identifier;
            }
        }

        private class TestBiasedItem : IBiased
        {
            private readonly string identifier;
            private readonly double pheromoneBias;
            private readonly double healthBias;

            public TestBiasedItem(double pheromoneBias, double healthBias)
            {
                this.pheromoneBias = pheromoneBias;
                this.healthBias = healthBias;
            }

            public Dictionary<Measure, double> GetMeasureBiases()
            {
                // there needs to be a bias for each measure (will be applied to all items)
                // but will have no effect when there is only one measure
                return new Dictionary<Measure, double> { { Measure.Pheromone, this.pheromoneBias }, { Measure.Health, this.healthBias } };
            }
        }

    }
}


