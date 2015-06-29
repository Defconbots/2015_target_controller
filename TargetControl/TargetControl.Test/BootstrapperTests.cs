using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TargetControl.Test
{
    [TestFixture]
    class BootstrapperTests
    {
        [Test]
        public void Shell()
        {
            var bootstrapper = new TestBootstrapper();
        }

        private class TestBootstrapper : AppBootstrapper
        {
            protected override void StartRuntime()
            {
                this.Configure();
            }
        }
    }
}
