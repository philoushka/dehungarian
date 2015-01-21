using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace dehungarian.Test
{
    [TestClass]
    public class HungarianUnitTests
    {
        const string CamelCasedVariableName = "CustomerName";
        const string CamelCasedVariableNameLower = "customerName";
        const string NonCamelCasedVariableName = "longitude";

        public IEnumerable<string> Prefixes { get { return dehungarian.DehungarianAnalyzer.HungarianPrefixes; } }


        [TestMethod]
        public void FindsHungarianVariablesWithCamelCasedName()
        {
            //strCustomer should find str
            var hungarianedVariables = Prefixes.Select(x => x + CamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreNotEqual(DehungarianAnalyzer.FindHungarianPrefix(variableName) , "");
            }
        }

        [TestMethod]
        public void FindHungarianPrefixHandlesBadUnexpectedInput()
        {
            Assert.AreEqual(DehungarianAnalyzer.FindHungarianPrefix(""), "");
            Assert.AreEqual(DehungarianAnalyzer.FindHungarianPrefix(" "), "");
            Assert.AreEqual(DehungarianAnalyzer.FindHungarianPrefix(null), "");
            Assert.AreEqual(DehungarianAnalyzer.FindHungarianPrefix(System.IO.Path.GetRandomFileName()), "");
        }

        [TestMethod]
        public void FindsHungarianVariablesWithCamelCasedNameAndLeadingUnderscore()
        {
            //_strCustomer should find _str
            var hungarianedVariables = Prefixes.Select(x => "_" + x + CamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreNotEqual(DehungarianAnalyzer.FindHungarianPrefix(variableName), "");
            }
        }

        [TestMethod]
        public void SkipsHungarianVariablesWhenNoCamelCaseImmediatelyAfterPrefix()
        {
            // strcustomer should find no hungprefix
            var hungarianedVariables = Prefixes.Select(x => x + NonCamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreEqual(DehungarianAnalyzer.FindHungarianPrefix(variableName), "");
            }
        }

        [TestMethod]
        public void SuggestsHungarianRename()
        {
            //iCustomer should result in customer
            var hungarianedVariables = Prefixes.Select(x => x + CamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreEqual(DehungarianAnalyzer.SuggestDehungarianName(variableName) ,CamelCasedVariableNameLower);
            }
        }

        [TestMethod]
        public void SuggestsHungarianRenameWithUnderscore()
        {
            //_strCustomerName should result in _customerName
            var hungarianedVariables = Prefixes.Select(x => "_" + x + CamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreEqual(DehungarianAnalyzer.SuggestDehungarianName(variableName) ,"_" + CamelCasedVariableNameLower);
            }
        }

        [TestMethod]
        public void RenameSuggestsSameVariableForNonHungarianWithUnderscore()
        {
            //_customer should result in _customer
            var hungarianedVariables = Prefixes.Select(x => "_" + NonCamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                Assert.AreEqual(DehungarianAnalyzer.SuggestDehungarianName(variableName), variableName);
            }
        }

        [TestMethod]
        public void RenameSuggestsSameVariableForNonHungarian()
        {
            //customerName should result in customerName
            var hungarianedVariables = Prefixes.Select(x => NonCamelCasedVariableName);
            foreach (string variableName in hungarianedVariables)
            {
                //should suggest the same as was input. no change.
                Assert.AreEqual(DehungarianAnalyzer.SuggestDehungarianName(variableName), variableName);
            }
        }
    }
}