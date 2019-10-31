﻿using System.Reflection;

namespace Mystique.Core.Contracts
{
    public interface IReferenceLoader
    {
        public void LoadStreamsIntoContext(CollectibleAssemblyLoadContext context, string moduleFolder, Assembly assembly);
    }
}
