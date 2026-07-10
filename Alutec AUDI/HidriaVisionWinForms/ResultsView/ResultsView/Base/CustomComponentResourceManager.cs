using System;
using System.ComponentModel;

namespace ResultsView
{
    /// <summary>
    /// Custom resource manager class for assigning resources to the component.
    /// </summary>
    internal class CustomComponentResourceManager : ComponentResourceManager
    {
        /// <summary>
        /// Constructs new object of type CustomComponentResourceManager.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="resourceName">Resource name.</param>
        public CustomComponentResourceManager(Type type, string resourceName) : base(type)
        {
            this.BaseNameField = resourceName;
        }
    }
}
