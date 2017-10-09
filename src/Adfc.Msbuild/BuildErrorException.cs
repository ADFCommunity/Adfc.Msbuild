using System;

namespace Adfc.Msbuild
{
    internal class BuildErrorException : Exception
    {
        public BuildError BuildError { get; }

        public BuildErrorException(BuildError error)
        {
            BuildError = error;
        }
    }
}
