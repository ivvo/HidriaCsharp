using System;
using System.Dynamic;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace XMLSettings
{
    /// <summary>
    /// Represents XML settings manager.
    /// </summary>
    public class XMLSettingsManager
    {
        #region Private fields
        private XElement XMLRoot;
        private string XMLPath;
        private object ObjLock;
        private string FileName;
        #endregion

        /// <summary>
        /// Constructs an object of type XMLSettingsManager.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <exception cref="XmlException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public XMLSettingsManager(string folderPath, string fileName)
        {
            // Check if folder path is null or empty
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if file name is null or empty
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name is null or empty.");

            // Parse an XML file and initialize lock
            XMLRoot = XElement.Load(Path.GetFullPath(string.Format(@"{0}\{1}", folderPath, fileName)));
            FileName = fileName;
            XMLPath = folderPath;
            ObjLock = new object();
        }

        #region Public methods
        /// <summary>
        /// Gets the specified element.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="attribute">Segment attribute.</param>
        /// <param name="name">Element name.</param>
        /// <returns>Returns value of a specified element.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public object GetElement(string segmentName, string segmentAttribute, string name) 
        {
            // Check if segment name is null or empty
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name is null or empty.");

            // Check if segment attribute is null or empty
            if (string.IsNullOrEmpty(segmentAttribute))
                throw new ArgumentException("Segment attribute is null or empty.");

            // Check if name is null or empty
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name is null or empty.");

            // Lock and return element's value
            lock (ObjLock)
            {
                IEnumerable<XElement> SegmentElements;
                XElement SegmentElement;
                Type ElementType;

                // Get segment elements
                SegmentElements = GetSegmentElements(segmentName, segmentAttribute);

                // Check that only one element with a given name exists
                if (SegmentElements.Where(x => x.Name == name).Count() != 1)
                    throw new InvalidOperationException("There are no elements or there are multiple elements with the same name.");

                // Get segment element
                SegmentElement = GetSegmentElement(SegmentElements, name);

                // Check if type has been specified inside the element
                if (SegmentElement.Attribute("Type") == null)
                    throw new InvalidOperationException("Type not specified.");

                // Get element type
                ElementType = GetElementType(SegmentElement);

                // Return element's value
                return Convert.ChangeType(SegmentElement.Value, ElementType, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Sets the specified element.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="attribute">Segment attribute.</param>
        /// <param name="name">Element name.</param>
        /// <param name="value">Element value.</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SetElement(string segmentName, string segmentAttribute, string name, object value)
        {
            // Check if segment name is null or empty
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name is null or empty.");

            // Check if segment attribute is null or empty
            if (string.IsNullOrEmpty(segmentAttribute))
                throw new ArgumentException("Segment attribute is null or empty.");

            // Check if name is null or empty
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name is null or empty.");

            // Check if value is null
            if (value == null)
                throw new ArgumentException("Value is null.");

            // Lock and set element's value
            lock (ObjLock)
            {
                IEnumerable<XElement> SegmentElements;
                XElement SegmentElement;
                Type ElementType;

                // Get segment elements
                SegmentElements = GetSegmentElements(segmentName, segmentAttribute);

                // Check that only one element with a given name exists
                if (SegmentElements.Where(x => x.Name == name).Count() != 1)
                    throw new InvalidOperationException("There are no elements or there are multiple elements with the same name.");

                // Get segment element
                SegmentElement = GetSegmentElement(SegmentElements, name);

                // Check if type has been specified inside the element
                if (SegmentElement.Attribute("Type") == null)
                    throw new InvalidOperationException("Type not specified.");

                // Get element type
                ElementType = GetElementType(SegmentElement);

                // Check if value type is valid
                if (value.GetType() != ElementType)
                    throw new InvalidOperationException("Value type is invalid.");

                // Convert value to string 
                SegmentElement.Value = Convert.ToString(value, CultureInfo.InvariantCulture);

                // Save the xml
                XMLRoot.Save(Path.Combine(XMLPath, FileName));
            }
        }

        /// <summary>
        /// Gets the specified segment.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="segmentAttribute">Segment attribute.</param>
        /// <returns>Returns the specified segment.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public object GetSegment(string segmentName, string segmentAttribute)
        {
            // Check if segment name is null or empty
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name is null or empty.");

            // Check if segment attribute is null or empty
            if (string.IsNullOrEmpty(segmentAttribute))
                throw new ArgumentException("Segment attribute is null or empty.");

            // Lock and get segment
            lock (ObjLock)
            {
                dynamic Segment = new ExpandoObject();
                IEnumerable<XElement> SegmentElements;
                IDictionary<string, object> ExpandoDict = Segment as IDictionary<string, object>;

                // Get segment elements
                SegmentElements = GetSegmentElements(segmentName, segmentAttribute);

                // Go through the elements
                foreach (XElement segmentElement in SegmentElements)
                {
                    Type ElementType;

                    // Check that only one element with a given name exists
                    if (SegmentElements.Where(x => x.Name == segmentElement.Name).Count() != 1)
                        throw new InvalidOperationException("There are no elements or there are multiple elements with the same name.");

                    // Check that the element has no other elements
                    if (segmentElement.Elements().Count() != 0)
                        throw new InvalidOperationException("Cannot get the element which has other elements.");

                    // Check if type has been specified inside the element
                    if (segmentElement.Attribute("Type") == null)
                        throw new InvalidOperationException("Type not specified.");

                    // Get element type
                    ElementType = GetElementType(segmentElement);

                    // Add value to the expando object
                    ExpandoDict.Add(segmentElement.Name.LocalName, Convert.ChangeType(segmentElement.Value, ElementType, CultureInfo.InvariantCulture));
                }

                return Segment;
            }
        }

        /// <summary>
        /// Sets the specified segment.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="segmentAttribute">Segment attribute.</param>
        /// <param name="segment">Segment data.</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SetSegment(string segmentName, string segmentAttribute, object segment)
        {
            // Check if segment name is null or empty
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name is null or empty.");

            // Check if segment attribute is null or empty
            if (string.IsNullOrEmpty(segmentAttribute))
                throw new ArgumentException("Segment attribute is null or empty.");

            // Check if segment is null
            if (segment == null)
                throw new ArgumentException("Value is null.");

            // Lock object and set segment
            lock (ObjLock)
            {
                IEnumerable<XElement> SegmentElements;
                IDictionary<string, object> ExpandoDict = segment as IDictionary<string, object>;

                // Get segment elements
                SegmentElements = GetSegmentElements(segmentName, segmentAttribute);

                // Go through the properties of the expando object
                foreach (KeyValuePair<string, object> prop in ExpandoDict)
                {
                    XElement SegmentElement;
                    Type ElementType;

                    // Check that only one element with a given name exists
                    if (SegmentElements.Where(x => x.Name == prop.Key).Count() != 1)
                        throw new InvalidOperationException("There are no elements or there are multiple elements with the same name.");

                    // Get segment element
                    SegmentElement = GetSegmentElement(SegmentElements, prop.Key);

                    // Check if type has been specified inside the element
                    if (SegmentElement.Attribute("Type") == null)
                        throw new InvalidOperationException("Type not specified.");

                    // Get element type
                    ElementType = GetElementType(SegmentElement);

                    // Check if value type is valid
                    if (prop.Value.GetType() != ElementType)
                        throw new InvalidOperationException("Value type is invalid.");

                    // Update element with new value
                    SegmentElement.Value = Convert.ToString(prop.Value, CultureInfo.InvariantCulture);
                }

                // Save the xml
                XMLRoot.Save(Path.Combine(XMLPath, FileName));
            }
        }

        /// <summary>
        /// Get all available segments of a given name.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="segmentAttribute">Segment attribute.</param>
        /// <param name="name">Specified segment.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public List<object> GetSegments(string segmentName, string segmentAttribute, string name)
        {
            List<object> AvailableSegments = new List<object>();

            // Check if segment name is null or empty
            if (string.IsNullOrEmpty(segmentName))
                throw new ArgumentException("Segment name is null or empty.");

            // Check if segment attribute is null or empty
            if (string.IsNullOrEmpty(segmentAttribute))
                throw new ArgumentException("Segment attribute is null or empty.");

            // Check if name is null or empty
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name is null or empty.");

            // Lock and get segment
            lock (ObjLock)
            {
                IEnumerable<XElement> MainSegments;

                // Get main segments
                MainSegments = GetSegmentElements(segmentName, segmentAttribute);

                // Go through the segments
                foreach(XElement mainSegment in MainSegments)
                {
                    // Segment must have specified name
                    if(mainSegment.Name == name)
                    {
                        dynamic Segment = new ExpandoObject();
                        IDictionary<string, object> ExpandoDict = Segment as IDictionary<string, object>;
                        IEnumerable<XElement> SegmentElements = mainSegment.Elements();

                        // Go through the elements of the main segment
                        foreach(XElement segmentElement in SegmentElements)
                        {
                            Type ElementType;

                            // Check that only one element with a given name exists
                            if (SegmentElements.Where(x => x.Name == segmentElement.Name).Count() != 1)
                                throw new InvalidOperationException("There are no elements or there are multiple elements with the same name.");

                            // Check that the element has no other elements
                            if (segmentElement.Elements().Count() != 0)
                                throw new InvalidOperationException("Cannot get the element which has other elements.");

                            // Check if type has been specified inside the element
                            if (segmentElement.Attribute("Type") == null)
                                throw new InvalidOperationException("Type not specified.");

                            // Get element type
                            ElementType = GetElementType(segmentElement);

                            // Add value to the expando object
                            ExpandoDict.Add(segmentElement.Name.LocalName, Convert.ChangeType(segmentElement.Value, ElementType, CultureInfo.InvariantCulture));
                        }

                        // Add expando object to the list
                        AvailableSegments.Add(Segment);
                    }
                }
            }

            return AvailableSegments;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the segment elements.
        /// </summary>
        /// <param name="segmentName">Segment name.</param>
        /// <param name="segmentAttribute">Segment attribute.</param>
        /// <returns>Segment elements.</returns>
        private IEnumerable<XElement> GetSegmentElements(string segmentName, string segmentAttribute)
        {
            IEnumerable<XElement> Segments;
            IEnumerable<XElement> SegmentElements;

            // Try to get the segments. Only one must exist
            Segments = XMLRoot.Elements(segmentName).Where(x => x.Attribute("Label")?.Value == segmentAttribute);

            if (Segments.Count() != 1)
                throw new InvalidOperationException("There is no segment or there are multiple segments with the same name and attribute.");

            // Get elements of a specified segment. Check if it has some
            SegmentElements = Segments.Elements();

            if (SegmentElements.Count() == 0)
                throw new InvalidOperationException("Cannot get the segment which does not have elements.");

            return SegmentElements;
        }

        /// <summary>
        /// Gets the segment element.
        /// </summary>
        /// <param name="segmentElements">Segment elements.</param>
        /// <param name="name">Element name.</param>
        /// <returns>Segment element.</returns>
        private XElement GetSegmentElement(IEnumerable<XElement> segmentElements, string name)
        {
            XElement SegmentElement;

            // Get the element
            SegmentElement = segmentElements.Where(x => x.Name == name).First();

            // Check that the element has no other elements
            if (SegmentElement.Elements().Count() != 0)
                throw new InvalidOperationException("Cannot get the element which has other elements.");

            return SegmentElement;
        }

        /// <summary>
        /// Gets the segment element type.
        /// </summary>
        /// <param name="segmentElement">Segment element.</param>
        /// <returns>Segment element type.</returns>
        private Type GetElementType(XElement segmentElement)
        {
            Type ElementType;

            // Create actual type from the element. Check if type is valid
            ElementType = Type.GetType(segmentElement.Attribute("Type").Value);

            if (ElementType == null)
                throw new InvalidOperationException("Element type is invalid.");

            return ElementType;
        }
        #endregion
    }
}
