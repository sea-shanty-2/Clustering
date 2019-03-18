using System;
using NUnit;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Asd()
        {
            Assert.That(new int[] {1, 2, 3}, Has.Exactly(3).Positive);
            Assert.That(new int[] {1, 2, 3}, Has.No.Negative);
        }
    }
}