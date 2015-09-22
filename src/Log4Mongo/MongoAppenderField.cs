using log4net.Layout;

namespace Log4Mongo
{
	public class MongoAppenderField
	{
		public string Name { get; set; }
		public IRawLayout Layout { get; set; }
	}
}