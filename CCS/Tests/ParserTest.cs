using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using CCS.Parsing;
using CCS.Nodes;

namespace CCS.Tests {
    [TestFixture]
    public class ParserTest {

        [Test]
        public void TestNilProcess() {
            CCSSystem sys = Parse("P = 0");
            Assert.AreEqual(1, sys.Count);
            Process p = sys[0].Process;
            Assert.IsInstanceOfType(typeof(NilProcess), p);
            AssertNoRelabelling(p);
            AssertNoRestrictions(p);
            AssertParenCount(p, 0);
        }

        [Test]
        public void TestActionPrefixSingleAction() {
            CCSSystem sys = Parse("P = john.0");
            Assert.AreEqual(1, sys.Count);
            Process p = sys[0].Process;
            Assert.IsInstanceOfType(typeof(ActionPrefix), p);
            ActionPrefix ap = (ActionPrefix)p;
            Assert.IsInstanceOfType(typeof(NilProcess), ap.Process);
            Assert.AreEqual("john", ap.Action.Name);
            Assert.IsInstanceOfType(typeof(InAction), ap.Action);
            AssertNoRelabelling(ap);
            AssertNoRestrictions(ap);
            AssertParenCount(ap, 0);
        }

        [Test]
        public void TestActionPrefixMultipleActions() {
            CCSSystem sys = Parse("P = john._jane_.(  foo .0)");
            Assert.AreEqual(1, sys.Count);
            Process p = sys[0].Process;
            Assert.IsInstanceOfType(typeof(ActionPrefix), p);
            ActionPrefix ap = (ActionPrefix)p;

            //First action
            Assert.IsInstanceOfType(typeof(ActionPrefix), ap.Process);
            Assert.AreEqual("john", ap.Action.Name);
            Assert.IsInstanceOfType(typeof(InAction), ap.Action);
            AssertNoRelabelling(ap);
            AssertNoRestrictions(ap);
            AssertParenCount(ap, 0);

            //Second action
            ap = (ActionPrefix)ap.Process;
            Assert.IsInstanceOfType(typeof(ActionPrefix), ap.Process);
            Assert.AreEqual("jane", ap.Action.Name);
            Assert.IsInstanceOfType(typeof(OutAction), ap.Action);
            AssertNoRelabelling(ap);
            AssertNoRestrictions(ap);
            AssertParenCount(ap, 0);

            //Third action
            ap = (ActionPrefix)ap.Process;
            Assert.IsInstanceOfType(typeof(NilProcess), ap.Process);
            Assert.AreEqual("foo", ap.Action.Name);
            Assert.IsInstanceOfType(typeof(InAction), ap.Action);
            AssertNoRelabelling(ap);
            AssertNoRestrictions(ap);
            AssertParenCount(ap, 1);
        }

        private void AssertParenCount(Process p, int parens) {
            Assert.AreEqual(parens, p.ParenCount);
        }

        private void AssertNoRestrictions(Process p) {
            Assert.AreEqual(0, p.Restrictions.Count);
        }
        private void AssertNoRelabelling(Process p) {
            Assert.AreEqual(0, p.Relabelling.Count);
        }
        private CCSSystem Parse(string source) {
            MemoryStream ms = new MemoryStream();
            StreamWriter w = new StreamWriter(ms);
            w.Write(source);
            w.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Parser p = new Parser(new Scanner(ms));
            p.Parse();
            return p.System;
        }

        [Test]
        public void TestArithmeticExpression() {
            TestAExp("3+4", 7);
            TestAExp("3*4", 12);
            TestAExp("3/4", 0);
            TestAExp("3-1", 2);
            TestAExp("5+3+(3*4/2+4%2)-23*2-2-(3*4+1)", -47);
            TestAExp("56*(65+2*26*34+2*356-(23*2-5))", 140224);
        }

        private void TestAExp(string exp, int expectedValue) {
            CCSSystem sys = Parse(@"P = X_{" + exp + "}");
            Assert.AreEqual(1, sys.Count);
            ProcessConstant p = (ProcessConstant)sys[0].Process;
            ArithmeticExpression expr = (ArithmeticExpression)p.Subscript[0];
            Assert.AreEqual(expectedValue, expr.Value);
        }

    }
}
