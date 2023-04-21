using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation.PathFinding;

namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    [TestClass]
    public class ConversionMapTests
    {
        #region Test Data Setup

        private const string A = "A";
        private const string B = "B";
        private const string C = "C";
        private const string D = "D";
        private const string E = "E";
        private const string F = "F";
        private const string G = "G";
        private const string H = "H";
        private const string I = "I";
        private const string J = "J";
        private const string K = "K";
        private const string L = "L";
        private const string M = "M";
        private const string N = "N";
        private const string O = "O";
        private const string P = "P";

        private static readonly string[] Nodes = new[]
        {
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
        };

        private static readonly ISet<(string A, string B)> NodeRelations = new HashSet<(
            string A,
            string B
        )>
        {
            (A, I),
            (B, I),
            (C, J),
            (D, E),
            (D, F),
            (D, J),
            (E, D),
            (F, D),
            (F, G),
            (G, F),
            (G, H),
            (G, K),
            (H, G),
            (H, I),
            (I, A),
            (I, B),
            (I, J),
            (J, C),
            (J, D),
            (J, I),
            (J, K),
            (K, G),
            (K, J),
            // Area (A-K) and (L-P) are not connected
            (L, M),
            (L, N),
            (M, L),
            (M, N),
            (M, O),
            (N, L),
            (N, M),
            (N, P),
            (O, M),
            (P, N),
        };

        private readonly ConversionMap _map;

        public ConversionMapTests()
        {
            _map = ConversionMapBuilder.BuildMap(Nodes, NodeRelations);
        }

        [TestMethod]
        [DataRow(A, I, true)]
        [DataRow(A, C, true)]
        [DataRow(A, O, false)]
        [DataRow(E, L, false)]
        public void TestReachabilityOfPoints(string origin, string target, bool canReach)
        {
            DoReachTest(
                origin,
                target,
                (o, t) =>
                {
                    var res = _map.IsNodeReachable(o, t, out var path);
                    Console.WriteLine(path.ToString());
                    return res;
                },
                canReach
            );
        }

        private void DoReachTest(
            string origin,
            string target,
            Func<ConversionNode, ConversionNode, bool> resolutionFunc,
            bool expectReach
        )
        {
            ConversionNode originNode = _map.GetNode(origin),
                targetNode = _map.GetNode(target);

            Assert.AreEqual(expectReach, resolutionFunc(originNode, targetNode));
        }

        #endregion
    }
}
