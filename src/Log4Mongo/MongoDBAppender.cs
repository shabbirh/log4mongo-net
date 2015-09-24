﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net.Appender;
using log4net.Core;

namespace Log4Mongo
{
    /// <summary>
    /// MongoDb Appender for Log4Net
    /// </summary>
    public class MongoDBAppender : AppenderSkeleton
	{
		private readonly List<MongoAppenderField> _fields = new List<MongoAppenderField>();

		/// <summary>
		/// MongoDB database connection in the format:
		/// mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]
		/// See http://www.mongodb.org/display/DOCS/Connections
		/// If no database specified, default to "log4net"
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The connectionString name to use in the connectionStrings section of your *.config file
		/// If not specified or connectionString name does not exist will use ConnectionString value
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// Name of the collection in database
		/// Defaults to "logs"
		/// </summary>
		public string CollectionName { get; set; }

		#region Deprecated

		/// <summary>
		/// Hostname of MongoDB server
		/// Defaults to localhost
		/// </summary>
		[Obsolete("Use ConnectionString")]
		public string Host { get; set; }

		/// <summary>
		/// Port of MongoDB server
		/// Defaults to 27017
		/// </summary>
		[Obsolete("Use ConnectionString")]
		public int Port { get; set; }

		/// <summary>
		/// Name of the database on MongoDB
		/// Defaults to log4net_mongodb
		/// </summary>
		[Obsolete("Use ConnectionString")]
		public string DatabaseName { get; set; }

		/// <summary>
		/// MongoDB database user name
		/// </summary>
		[Obsolete("Use ConnectionString")]
		public string UserName { get; set; }

		/// <summary>
		/// MongoDB database password
		/// </summary>
		[Obsolete("Use ConnectionString")]
		public string Password { get; set; }

        #endregion

        /// <summary>
        /// Adds the field.
        /// </summary>
        /// <param name="fileld">The fileld.</param>
        public void AddField(MongoAppenderField fileld)
		{
			_fields.Add(fileld);
		}

        /// <summary>
        /// Appends the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event.</param>
        protected override async void Append(LoggingEvent loggingEvent)
		{
			var collection = GetCollection();
			await collection.InsertOneAsync(BuildBsonDocument(loggingEvent));
		}

        /// <summary>
        /// Appends the specified logging events.
        /// </summary>
        /// <param name="loggingEvents">The logging events.</param>
        protected override async void Append(LoggingEvent[] loggingEvents)
		{
			var collection = GetCollection();
			await collection.InsertManyAsync(loggingEvents.Select(BuildBsonDocument));
		}

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <returns></returns>
        private IMongoCollection<BsonDocument> GetCollection()
		{
			var db = GetDatabase();
			IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(CollectionName ?? "logs");
			return collection;
		}

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
		{
			ConnectionStringSettings connectionStringSetting = ConfigurationManager.ConnectionStrings[ConnectionStringName];
			return connectionStringSetting != null ? connectionStringSetting.ConnectionString : ConnectionString;
		}

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Must provide a valid connection string</exception>
        private IMongoDatabase GetDatabase()
		{
			string connStr = GetConnectionString();

			if (string.IsNullOrWhiteSpace(connStr))
			{
				throw new InvalidOperationException("Must provide a valid connection string");
			}

			MongoUrl url = MongoUrl.Create(connStr);
			MongoClient client = new MongoClient(url);
			IMongoDatabase db = client.GetDatabase(url.DatabaseName ?? "logStore");
			return db;
		}

        /// <summary>
        /// Builds the bson document.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        private BsonDocument BuildBsonDocument(LoggingEvent log)
		{
			if(_fields.Count == 0)
			{
				return BackwardCompatibility.BuildBsonDocument(log);
			}
			var doc = new BsonDocument();
			foreach(MongoAppenderField field in _fields)
			{
				object value = field.Layout.Format(log);
				var bsonValue = value as BsonValue ?? BsonValue.Create(value);
				doc.Add(field.Name, bsonValue);
			}
			return doc;
		}
	}
}