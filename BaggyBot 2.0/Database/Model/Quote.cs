﻿using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "quote")]
	class Quote : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "author"), NotNull]
		public int AuthorId { get; set; }

		[Column(Name = "text"), NotNull]
		public string Text { get; set; }

		[Column(Name = "taken_at"), NotNull]
		public DateTime TakenAt { get; set; }
	}
}
