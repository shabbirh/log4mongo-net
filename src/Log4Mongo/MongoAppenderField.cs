using log4net.Layout;

namespace Log4Mongo
{
    /// <summary>
    /// Mongo Appender Field
    /// </summary>
    public class MongoAppenderField
	{
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>
        /// The layout.
        /// </value>
        public IRawLayout Layout { get; set; }
	}
}