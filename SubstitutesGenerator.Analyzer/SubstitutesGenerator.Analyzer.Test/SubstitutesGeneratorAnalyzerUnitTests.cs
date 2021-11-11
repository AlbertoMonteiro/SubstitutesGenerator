using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = SubstitutesGenerator.Analyzer.Test.CSharpCodeFixVerifier<
    SubstitutesGenerator.Analyzer.SubstitutesGeneratorAnalyzerAnalyzer,
    SubstitutesGenerator.Analyzer.SubstitutesGeneratorAnalyzerCodeFixProvider>;

namespace SubstitutesGenerator.Analyzer.Test
{
    [TestClass]
    public class SubstitutesGeneratorAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    public class Ola
    {
        public Ola(string oi)
        {
        }
    }

    public class OlaTest
    {
        private readonly Ola _sut;

        public OlaTest()
        {
            _sut = new Ola(default);
        }
    }
}";

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    public class Ola
    {
        public Ola(string oi)
        {
        }
    }

    public class OlaTest
    {
        private readonly Ola _sut;
        private readonly String _oi = ""oi"";
        public OlaTest()
        {
            _sut = new Ola(_oi);
        }
    }
}";

            var expected = VerifyCS.Diagnostic("SubstitutesGeneratorAnalyzer").WithSpan(24, 20, 24, 36);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
