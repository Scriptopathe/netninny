using System.Collections.Generic;

namespace NetNinny.Filtering
{
    /// <summary>
    /// Represents a collection of Filters.
    /// </summary>
    public class FilterCollection : List<string>
    {
        /// <summary>
        /// Returns a FilterCollection loaded from a file containing all the filters,
        /// one for each line.
        /// </summary>
        public static FilterCollection FromFile(string path)
        {
            FilterCollection collection = new FilterCollection();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach(string filter in lines)
            {
                collection.Add(filter);
            }
            return collection;
        }

        /// <summary>
        /// Returns a value indicating if the given text contains the given filters.
        /// </summary>
        /// <param name="text">The text on which to apply filtering.</param>
        /// <param name="filters">The collection of filters to use.</param>
        /// <returns></returns>
        public bool ContainsFilteredItems(string text)
        {
            foreach (string filter in this)
            {
                if (text.Contains(filter))
                    return true;
            }
            return false;
        }
    }
}
