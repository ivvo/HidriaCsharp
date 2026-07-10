using System;
using System.Reflection;
using System.Resources;
using System.Linq;
using System.Collections.Generic;

namespace ResultsView
{
    /// <summary>
    /// Represents custom attribute for naming fields inside results structure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        #region Private fields
        private readonly string ResourceAttribute;
        private ResourceManager ResManager;
        #endregion;

        /// <summary>
        /// Constructs new object of type ColumnAttribute.
        /// </summary>
        /// <param name="resourceName">Resource name.</param>
        /// <param name="resourceAttribute">Resource attribute.</param>
        public ColumnAttribute(string resourceName, string resourceAttribute, float fillWeight = 100.0f) : base()
        {
            List<string> AvailableResources;
            string FullResourceName;
            Assembly CurrentAssembly;

            ResourceAttribute = resourceAttribute;
            FillWeight = fillWeight;

            // Get current assembly and all available resources
            CurrentAssembly = Assembly.GetExecutingAssembly();

            AvailableResources = CurrentAssembly.GetManifestResourceNames().ToList();

            // Get the resource name and check if it exists
            FullResourceName = AvailableResources.FirstOrDefault(x => x.IndexOf(resourceName) != -1);

            if (FullResourceName == null)
                throw new InvalidOperationException("Resource not found.");

            // Remove .resources postfix from string and create an manager
            FullResourceName = FullResourceName.Replace(".resources", string.Empty);

            ResManager = new ResourceManager(FullResourceName, CurrentAssembly);
        }

        #region Properties
        /// <summary>
        /// Get localized attribute.
        /// </summary>
        public string Name
        {
            get
            {
                return ResManager.GetString(ResourceAttribute);
            }
        }

        /// <summary>
        /// Gets fill weight of the column.
        /// </summary>
        public float FillWeight { get; }
        #endregion
    }
}
