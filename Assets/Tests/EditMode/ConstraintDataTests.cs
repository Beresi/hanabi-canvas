// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class ConstraintDataTests
    {
        // ---- Tests ----
        [Test]
        public void Constructor_ColorLimit_SetsTypeAndIntValue()
        {
            ConstraintData constraint = new ConstraintData(ConstraintType.ColorLimit, intValue: 3);

            Assert.AreEqual(ConstraintType.ColorLimit, constraint.Type);
            Assert.AreEqual(3, constraint.IntValue);
        }

        [Test]
        public void Constructor_TimeLimit_SetsTypeAndFloatValue()
        {
            ConstraintData constraint = new ConstraintData(ConstraintType.TimeLimit, floatValue: 30f);

            Assert.AreEqual(ConstraintType.TimeLimit, constraint.Type);
            Assert.AreEqual(30f, constraint.FloatValue);
        }

        [Test]
        public void Constructor_SymmetryRequired_SetsTypeAndBoolValue()
        {
            ConstraintData constraint = new ConstraintData(ConstraintType.SymmetryRequired, boolValue: true);

            Assert.AreEqual(ConstraintType.SymmetryRequired, constraint.Type);
            Assert.IsTrue(constraint.BoolValue);
        }

        [Test]
        public void Constructor_DefaultValues_AllZero()
        {
            ConstraintData constraint = new ConstraintData(ConstraintType.PixelLimit);

            Assert.AreEqual(0, constraint.IntValue);
            Assert.AreEqual(0f, constraint.FloatValue);
            Assert.IsFalse(constraint.BoolValue);
        }

        [Test]
        public void Type_AllConstraintTypes_Accessible()
        {
            ConstraintData colorLimit = new ConstraintData(ConstraintType.ColorLimit);
            ConstraintData timeLimit = new ConstraintData(ConstraintType.TimeLimit);
            ConstraintData symmetry = new ConstraintData(ConstraintType.SymmetryRequired);
            ConstraintData pixelLimit = new ConstraintData(ConstraintType.PixelLimit);
            ConstraintData palette = new ConstraintData(ConstraintType.PaletteRestriction);

            Assert.AreEqual(ConstraintType.ColorLimit, colorLimit.Type);
            Assert.AreEqual(ConstraintType.TimeLimit, timeLimit.Type);
            Assert.AreEqual(ConstraintType.SymmetryRequired, symmetry.Type);
            Assert.AreEqual(ConstraintType.PixelLimit, pixelLimit.Type);
            Assert.AreEqual(ConstraintType.PaletteRestriction, palette.Type);
        }
    }
}
