using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnvueClustering.Data;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class MatrixTests
    {
        [Test]
        public void SimilarityMatrix_SimplePoints_Correct()
        {
            var simplePoints = new List<EuclideanPoint>()
            {
                new EuclideanPoint(0, 0, 0),
                new EuclideanPoint(1, 0, 0),
                new EuclideanPoint(2, 0, 0),
                new EuclideanPoint(3, 0, 0),
                new EuclideanPoint(4, 0, 0)
            };

            var expected = new []
            {
                new [] {0, 1, 2, 3, 4}, 
                new [] {1, 0, 1, 2, 3}, 
                new [] {2, 1, 0, 1, 2}, 
                new [] {3, 2, 1, 0, 1}, 
                new [] {4, 3, 2, 1, 0}
            };

            var m = Matrix.SimilarityMatrix(simplePoints,
                (p, o) => (float) Math.Sqrt(Math.Pow(p.X - o.X, 2) + Math.Pow(p.Y - o.Y, 2)));

            foreach (var (row, expectedRow) in m.Zip(expected, (r, e) => (r, e)))
                Assert.That(row, Is.EquivalentTo(expectedRow));
        }

        [Test]
        public void Matrix_NewMatrix_SameValues()
        {
            for (var i = -10; i < 10; i++)
            {
                var m = new Matrix(10, 10, i);
                foreach (var row in m)
                {
                    foreach (var v in row)
                    {
                        Assert.That(v, Is.EqualTo(i));
                    }
                }
            }
        }

        [Test]
        public void Matrix_NewMatrix_ShapeIsConsistent()
        {
            var m = new Matrix(10, 10);
            Assert.That(m.Shape, Is.EquivalentTo(new [] {10, 10}));
        }

        [Test]
        public void DeleteColumn_StaticMatrix_ShapeChanged()
        {
            var m = new Matrix(10, 10);
            m = m.DeleteColumn(3);
            Assert.That(m.Columns.Count(), Is.EqualTo(9));
        }

        [Test]
        public void DeleteColumn_ValuedMatrix_ValuesRemoved()
        {
            var m = new Matrix(new []
            {
                new [] {0f, 0, 0, 0, 0}, 
                new [] {1f, 0, 0, 0, 0}, 
                new [] {2f, 1, 0, 0, 0}, 
                new [] {3f, 2, 1, 0, 0}, 
                new [] {4f, 3, 2, 1, 0}
            });
            
            var expectedM = new Matrix(new []
            {
                new [] {0f, 0, 0, 0}, 
                new [] {0f, 0, 0, 0}, 
                new [] {1f, 0, 0, 0}, 
                new [] {2f, 1, 0, 0}, 
                new [] {3f, 2, 1, 0}
            });

            m = m.DeleteColumn(0);
            
            Assert.That(m.Columns.Count(), Is.EqualTo(expectedM.Columns.Count()));

            foreach (var (c1, c2) in m.Zip(expectedM, (m1, m2) => (m1, m2)))
            {
                Assert.That(c1, Is.EquivalentTo(c2));
            }
        }

        [Test]
        public void DeleteColumn_ValuedMatrix_ShapeChanged()
        {
            var m = new Matrix(new []
            {
                new [] {0f, 0, 0, 0, 0}, 
                new [] {1f, 0, 0, 0, 0}, 
                new [] {2f, 1, 0, 0, 0}, 
                new [] {3f, 2, 1, 0, 0}, 
                new [] {4f, 3, 2, 1, 0}
            });

            var oldShape = m.Shape;
            m = m.DeleteColumn(1);
            var newShape = m.Shape;
            var expectedShape = new[] {5, 4};
            
            Assert.That(oldShape, Is.Not.EquivalentTo(newShape));
            Assert.That(newShape, Is.EquivalentTo(expectedShape));
        }

        [Test]
        public void Matrix_NewMatrix_DifferentMethodsSameResult()
        {
            var m1 = new Matrix(5, 5);
            var m2 = new Matrix(new[] {5, 5});
            var m3 = new Matrix(new []
            {
                new [] {0f, 0, 0, 0, 0}, 
                new [] {0f, 0, 0, 0, 0}, 
                new [] {0f, 0, 0, 0, 0}, 
                new [] {0f, 0, 0, 0, 0}, 
                new [] {0f, 0, 0, 0, 0}
            });

            Assert.That(m1.Shape, Is.EquivalentTo(m2.Shape));
            Assert.That(m2.Shape, Is.EquivalentTo(m3.Shape));
            
            Assert.That(m1.Max(), Is.EqualTo(m2.Max()));
            Assert.That(m2.Max(), Is.EqualTo(m3.Max()));
            Assert.That(m1.Min(), Is.EqualTo(m2.Min()));
            Assert.That(m2.Min(), Is.EqualTo(m3.Min()));

            foreach (var m in new[] {m1, m2, m3})
            {
                foreach (var i in 5.Range())
                {
                    foreach (var j in 5.Range())
                    {
                        Assert.That(m[i, j], Is.EqualTo(0));
                    }
                }
            }
        }

        [Test]
        public void GetRow_StaticMatrix_GetCorrectRow()
        {
            var m = new Matrix(new []
            {
                new [] {0f, 0, 0, 0}, 
                new [] {0f, 0, 0, 0}, 
                new [] {1f, 0, 0, 0}, 
                new [] {2f, 1, 0, 0}, 
                new [] {3f, 2, 1, 0}
            });

            var expected = new[] {3f, 2, 1, 0};
            
            Assert.That(m[4], Is.EquivalentTo(expected));
        }

        [Test]
        public void GetCell_ValuedMatrix_CorrectValue()
        {
            var m = new Matrix(new []
            {
                new [] {0f, 0, 0, 0}, 
                new [] {0f, 0, 0, 0}, 
                new [] {1f, 0, 0, 0}, 
                new [] {2f, 1, 0, 0}, 
                new [] {3f, 2, 1, 0}
            });
            
            Assert.That(m[1, 2], Is.EqualTo(0));
            Assert.That(m[4, 0], Is.EqualTo(3));
        }

        [TestCase(new [] {3, 6}, new [] {6, 3}, new [] {3, 3})]
        [TestCase(new [] {1, 6}, new [] {6, 3}, new [] {1, 3})]
        [TestCase(new [] {1, 1}, new [] {1, 1}, new [] {1, 1})]
        [TestCase(new [] {19, 2}, new [] {2, 19}, new [] {19, 19})]
        [TestCase(new [] {3, 6}, new [] {6, 19}, new [] {3, 19})]
        public void MatMul_StaticMatrices_ExpectedShape(int[] nShape, int[] mShape, int[] expectedShape)
        {
            var m = new Matrix(nShape[0], nShape[1]);
            var n = new Matrix(mShape[0], mShape[1]);
            
            Assert.That((m * n).Shape, Is.EquivalentTo(expectedShape));
        }

        [Test]
        public void RowAccess_SimpleMatrix_ChangedRow()
        {
            var m = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });
            
            var expected = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {3f, 3, 3, 3}
            });

            m[2] = new[] {3f, 3, 3, 3};

            foreach (var (r1, r2) in m.Zip(expected))
            {
                Assert.That(r1, Is.EquivalentTo(r2));
            }
        }
        
        [Test]
        public void CellAccess_SimpleMatrix_ChangedCell()
        {
            var m = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });
            
            var expected = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {2f, 2, 2, 3}
            });

            m[2,3] = 3;

            foreach (var (r1, r2) in m.Zip(expected))
            {
                Assert.That(r1, Is.EquivalentTo(r2));
            }
        }

        [Test]
        public void DeleteColumns_FalsePredicate_NoneRemoved()
        {
            var m = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });
            
            var expected = new Matrix(new[,]
            {
                {0f, 0, 0, 0},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });

            m = m.DeleteColumns(c => c.Contains(9));
            foreach (var (r1, r2) in m.Zip(expected))
                Assert.That(r1, Is.EquivalentTo(r2));
        }

        [Test]
        public void DeleteColumns_GoodPredicate_SomeRemoved()
        {
            var m = new Matrix(new[,]
            {
                {0f, 1, 0, 2},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });
            
            var expected = new Matrix(new[,]
            {
                {0f, 0},
                {1f, 1},
                {2f, 2}
            });

            m = m.DeleteColumns(c => c.Sum() >= 4);
            foreach (var (r1, r2) in m.Zip(expected))
                Assert.That(r1, Is.EquivalentTo(r2));
        }

        [Test]
        public void DeleteColumns_AlwaysTruePredicate_RaisedException()
        {
            var m = new Matrix(new[,]
            {
                {0f, 1, 0, 2},
                {1f, 1, 1, 1},
                {2f, 2, 2, 2}
            });
            
            Assert.That(() => m.DeleteColumns(c => true), Throws.ArgumentException);
        }
    }
}