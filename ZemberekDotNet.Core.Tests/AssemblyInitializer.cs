using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Contracts;

namespace ZemberekDotNet.Core.Tests
{
    [TestClass]
    public class AssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            // avoid contract violation kill the process  
            Contract.ContractFailed += new EventHandler<ContractFailedEventArgs>(ContractFailed);
        }

        private static void ContractFailed(object sender, ContractFailedEventArgs e)
        {
            e.SetHandled();
            Assert.Fail($"{e.FailureKind}: {e.Message} {e.Condition}");
        }
    }
}
