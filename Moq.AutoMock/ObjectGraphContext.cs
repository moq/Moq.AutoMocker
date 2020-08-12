using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.AutoMock
{
    /// <summary>
    /// Handles state while creating an object graph.
    /// </summary>
    public class ObjectGraphContext
    {
        /// <summary>
        /// Creates an instance with binding flags set according to `enablePrivate`.
        /// </summary>
        /// <param name="enablePrivate"></param>
        public ObjectGraphContext(bool enablePrivate)
        {
            BindingFlags = GetBindingFlags(enablePrivate);
            VisitedTypes = new List<Type>();
        }

        /// <summary>
        /// Flags passed to Mock constructor.
        /// </summary>
        public BindingFlags BindingFlags { get; }
        
        /// <summary>
        /// Used internally to track which types have been created inside a call graph,
        /// to detect cycles in the object graph.
        /// </summary>
        public List<Type> VisitedTypes { get; }
        
        private static BindingFlags GetBindingFlags(bool enablePrivate)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (enablePrivate) bindingFlags |= BindingFlags.NonPublic;
            return bindingFlags;
        }
    }
}