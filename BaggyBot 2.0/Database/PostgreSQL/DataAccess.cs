// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from baggybot on 2013-11-15 02:50:57Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//

// ReSharper disable All

using System;
using System.ComponentModel;
using System.Data;
using DbLinq.PostgreSql;
#if MONO_STRICT
	using System.Data.Linq;
#else   // MONO_STRICT
using DbLinq.Data.Linq;
using DbLinq.Vendor;
#endif  // MONO_STRICT
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace BaggyBot.Database.PostgreSQL
{

	public partial class BaggyBoT : DataContext
	{

		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion


		public BaggyBoT(string connectionString) :
			base(connectionString)
		{
			OnCreated();
		}

		public BaggyBoT(string connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public BaggyBoT(IDbConnection connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			OnCreated();
		}

		public Table<Emoticon> EmOtIcons
		{
			get
			{
				return GetTable<Emoticon>();
			}
		}

		public Table<IrcLog> IrcLog
		{
			get
			{
				return GetTable<IrcLog>();
			}
		}

		public Table<KeyValuePair> KeyValuePairs
		{
			get
			{
				return GetTable<KeyValuePair>();
			}
		}

		public Table<Name> Names
		{
			get
			{
				return GetTable<Name>();
			}
		}

		public Table<Quote> Quotes
		{
			get
			{
				return GetTable<Quote>();
			}
		}

		public Table<Url> URLS
		{
			get
			{
				return GetTable<Url>();
			}
		}

		public Table<UserCredentials> UserCReds
		{
			get
			{
				return GetTable<UserCredentials>();
			}
		}

		public Table<UserStatistics> UserStatistics
		{
			get
			{
				return GetTable<UserStatistics>();
			}
		}

		public Table<Word> Words
		{
			get
			{
				return GetTable<Word>();
			}
		}
	}

	#region Start MONO_STRICT
#if MONO_STRICT

public partial class BaggyBoT
{
	
	public BaggyBoT(IDbConnection connection) : 
			base(connection)
	{
		this.OnCreated();
	}
}
	#region End MONO_STRICT
#endregion
#else     // MONO_STRICT

	public partial class BaggyBoT
	{

		public BaggyBoT(IDbConnection connection) :
			base(connection, new PgsqlVendor())
		{
			OnCreated();
		}

		public BaggyBoT(IDbConnection connection, IVendor sqlDialect) :
			base(connection, sqlDialect)
		{
			OnCreated();
		}

		public BaggyBoT(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) :
			base(connection, mappingSource, sqlDialect)
		{
			OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion

	[Table(Name = "dbo.emoticons")]
	public partial class Emoticon : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private string _emOticOn;

		private int _id;

		private int _lastUsedBy;

		private int _uses;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnEmOticOnChanged();

		partial void OnEmOticOnChanging(string value);

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnLastUsedByChanged();

		partial void OnLastUsedByChanging(int value);

		partial void OnUsesChanged();

		partial void OnUsesChanging(int value);
		#endregion


		public Emoticon()
		{
			OnCreated();
		}

		[Column(Storage = "_emOticOn", Name = "emoticon", DbType = "character varying(16)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Emoticon1
		{
			get
			{
				return _emOticOn;
			}
			set
			{
				if (((_emOticOn == value)
							== false)) {
					OnEmOticOnChanging(value);
					SendPropertyChanging();
					_emOticOn = value;
					SendPropertyChanged("EmOticOn");
					OnEmOticOnChanged();
				}
			}
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'emoticons_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lastUsedBy", Name = "last_used_by", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int LastUsedBy
		{
			get
			{
				return _lastUsedBy;
			}
			set
			{
				if ((_lastUsedBy != value)) {
					OnLastUsedByChanging(value);
					SendPropertyChanging();
					_lastUsedBy = value;
					SendPropertyChanged("LastUsedBy");
					OnLastUsedByChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return _uses;
			}
			set
			{
				if ((_uses != value)) {
					OnUsesChanging(value);
					SendPropertyChanging();
					_uses = value;
					SendPropertyChanged("Uses");
					OnUsesChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.irclog")]
	public partial class IrcLog
	{
		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private string _channel;

		private int _id;

		private string _message;

		private string _nick;

		private Nullable<int> _sender;

		private DateTime _time;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnChannelChanged();

		partial void OnChannelChanging(string value);

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnMessageChanged();

		partial void OnMessageChanging(string value);

		partial void OnNickChanged();

		partial void OnNickChanging(string value);

		partial void OnSenderChanged();

		partial void OnSenderChanging(Nullable<int> value);

		partial void OnTimeChanged();

		partial void OnTimeChanging(DateTime value);
		#endregion


		public IrcLog()
		{
			OnCreated();
		}

		[Column(Storage = "_channel", Name = "channel", DbType = "character varying(64)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Channel
		{
			get
			{
				return _channel;
			}
			set
			{
				if (((_channel == value)
							== false)) {
					OnChannelChanging(value);
					SendPropertyChanging();
					_channel = value;
					SendPropertyChanged("Channel");
					OnChannelChanged();
				}
			}
		}

		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'irclog_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					_id = value;
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_message", Name = "message", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				if (((_message == value)
							== false)) {
					OnMessageChanging(value);
					_message = value;
					OnMessageChanged();
				}
			}
		}

		[Column(Storage = "_nick", Name = "nick", DbType = "character varying(64)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Nick
		{
			get
			{
				return _nick;
			}
			set
			{
				if (((_nick == value)
							== false)) {
					OnNickChanging(value);
					_nick = value;
					OnNickChanged();
				}
			}
		}

		[Column(Storage = "_sender", Name = "sender", DbType = "integer(32,0)", AutoSync = AutoSync.Never)]
		[DebuggerNonUserCode()]
		public Nullable<int> Sender
		{
			get
			{
				return _sender;
			}
			set
			{
				if ((_sender != value)) {
					OnSenderChanging(value);
					_sender = value;
					OnSenderChanged();
				}
			}
		}

		[Column(Storage = "_time", Name = "time", DbType = "timestamp without time zone", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public DateTime Time
		{
			get
			{
				return _time;
			}
			set
			{
				if ((_time != value)) {
					OnTimeChanging(value);
					_time = value;
					OnTimeChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.keyvaluepairs")]
	public partial class KeyValuePair : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _id;

		private string _key;

		private int _value;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnKeyChanged();

		partial void OnKeyChanging(string value);

		partial void OnValueChanged();

		partial void OnValueChanging(int value);
		#endregion


		public KeyValuePair()
		{
			OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'keyvaluepairs_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_key", Name = "key", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (((_key == value)
							== false)) {
					OnKeyChanging(value);
					SendPropertyChanging();
					_key = value;
					SendPropertyChanged("Key");
					OnKeyChanged();
				}
			}
		}

		[Column(Storage = "_value", Name = "value", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				if ((_value != value)) {
					OnValueChanging(value);
					SendPropertyChanging();
					_value = value;
					SendPropertyChanged("Value");
					OnValueChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.names")]
	public partial class Name : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _id;

		private string _name1;

		private int _userID;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnName1Changed();

		partial void OnName1Changing(string value);

		partial void OnUserIDChanged();

		partial void OnUserIDChanging(int value);
		#endregion

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'names_id_seq\')")]
		[DebuggerNonUserCode()]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		public Name()
		{
			OnCreated();
		}

		[Column(Storage = "_name1", Name = "name1", DbType = "character varying(90)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Name1
		{
			get
			{
				return _name1;
			}
			set
			{
				if (((_name1 == value)
							== false)) {
					OnName1Changing(value);
					SendPropertyChanging();
					_name1 = value;
					SendPropertyChanged("Name1");
					OnName1Changed();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return _userID;
			}
			set
			{
				if ((_userID != value)) {
					OnUserIDChanging(value);
					SendPropertyChanging();
					_userID = value;
					SendPropertyChanged("UserID");
					OnUserIDChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.quotes")]
	public partial class Quote : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _id;

		private string _quote1;

		private Nullable<DateTime> _snagGedAt;

		private int _userID;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnQuote1Changed();

		partial void OnQuote1Changing(string value);

		partial void OnSnagGedAtChanged();

		partial void OnSnagGedAtChanging(Nullable<DateTime> value);

		partial void OnUserIDChanged();

		partial void OnUserIDChanging(int value);
		#endregion


		public Quote()
		{
			OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'quotes_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_quote1", Name = "quote1", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Quote1
		{
			get
			{
				return _quote1;
			}
			set
			{
				if (((_quote1 == value)
							== false)) {
					OnQuote1Changing(value);
					SendPropertyChanging();
					_quote1 = value;
					SendPropertyChanged("Quote1");
					OnQuote1Changed();
				}
			}
		}
		[Column(Storage = "_snagGedAt", Name = "snagged_at", DbType = "timestamp without time zone", AutoSync = AutoSync.Never, CanBeNull = true)]
		[DebuggerNonUserCode()]
		public Nullable<DateTime> SnaggedAt
		{
			get
			{
				return _snagGedAt;
			}
			set
			{
				if ((_snagGedAt != value)) {
					OnSnagGedAtChanging(value);
					SendPropertyChanging();
					_snagGedAt = value;
					SendPropertyChanged("SnagGedAt");
					OnSnagGedAtChanged();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return _userID;
			}
			set
			{
				if ((_userID != value)) {
					OnUserIDChanging(value);
					SendPropertyChanging();
					_userID = value;
					SendPropertyChanged("UserID");
					OnUserIDChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.urls")]
	public partial class Url : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _id;

		private string _lastUsage;

		private int _lastUsedBy;

		private string _url;

		private int _uses;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnLastUsageChanged();

		partial void OnLastUsageChanging(string value);

		partial void OnLastUsedByChanged();

		partial void OnLastUsedByChanging(int value);

		partial void OnURLChanged();

		partial void OnURLChanging(string value);

		partial void OnUsesChanged();

		partial void OnUsesChanging(int value);
		#endregion


		public Url()
		{
			OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'urls_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lastUsage", Name = "last_usage", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string LastUsage
		{
			get
			{
				return _lastUsage;
			}
			set
			{
				if (((_lastUsage == value)
							== false)) {
					OnLastUsageChanging(value);
					SendPropertyChanging();
					_lastUsage = value;
					SendPropertyChanged("LastUsage");
					OnLastUsageChanged();
				}
			}
		}

		[Column(Storage = "_lastUsedBy", Name = "last_used_by", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int LastUsedBy
		{
			get
			{
				return _lastUsedBy;
			}
			set
			{
				if ((_lastUsedBy != value)) {
					OnLastUsedByChanging(value);
					SendPropertyChanging();
					_lastUsedBy = value;
					SendPropertyChanged("LastUsedBy");
					OnLastUsedByChanged();
				}
			}
		}

		[Column(Storage = "_url", Name = "url", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Url1
		{
			get
			{
				return _url;
			}
			set
			{
				if (((_url == value)
							== false)) {
					OnURLChanging(value);
					SendPropertyChanging();
					_url = value;
					SendPropertyChanged("URL");
					OnURLChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return _uses;
			}
			set
			{
				if ((_uses != value)) {
					OnUsesChanging(value);
					SendPropertyChanging();
					_uses = value;
					SendPropertyChanged("Uses");
					OnUsesChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.usercreds")]
	public partial class UserCredentials : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private string _hostMask;

		private int _id;

		private string _idEnt;

		private string _nick;

		private string _nslOgin;

		private int _userID;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnHostMaskChanged();

		partial void OnHostMaskChanging(string value);

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnIDentChanged();

		partial void OnIDentChanging(string value);

		partial void OnNickChanged();

		partial void OnNickChanging(string value);

		partial void OnNSLoginChanged();

		partial void OnNSLoginChanging(string value);

		partial void OnUserIDChanged();

		partial void OnUserIDChanging(int value);
		#endregion


		public UserCredentials()
		{
			OnCreated();
		}

		[Column(Storage = "_hostMask", Name = "hostmask", DbType = "character varying(128)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string HostMask
		{
			get
			{
				return _hostMask;
			}
			set
			{
				if (((_hostMask == value)
							== false)) {
					OnHostMaskChanging(value);
					SendPropertyChanging();
					_hostMask = value;
					SendPropertyChanged("HostMask");
					OnHostMaskChanged();
				}
			}
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.OnInsert, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'usercreds_id_seq\')")]
		[DebuggerNonUserCode()]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_idEnt", Name = "ident", DbType = "character varying(45)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Ident
		{
			get
			{
				return _idEnt;
			}
			set
			{
				if (((_idEnt == value)
							== false)) {
					OnIDentChanging(value);
					SendPropertyChanging();
					_idEnt = value;
					SendPropertyChanged("IDent");
					OnIDentChanged();
				}
			}
		}

		[Column(Storage = "_nick", Name = "nick", DbType = "character varying(45)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Nick
		{
			get
			{
				return _nick;
			}
			set
			{
				if (((_nick == value)
							== false)) {
					OnNickChanging(value);
					SendPropertyChanging();
					_nick = value;
					SendPropertyChanged("Nick");
					OnNickChanged();
				}
			}
		}

		[Column(Storage = "_nslOgin", Name = "ns_login", DbType = "character varying(45)", AutoSync = AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string NsLogin
		{
			get
			{
				return _nslOgin;
			}
			set
			{
				if (((_nslOgin == value)
							== false)) {
					OnNSLoginChanging(value);
					SendPropertyChanging();
					_nslOgin = value;
					SendPropertyChanged("NSLogin");
					OnNSLoginChanged();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return _userID;
			}
			set
			{
				if ((_userID != value)) {
					OnUserIDChanging(value);
					SendPropertyChanging();
					_userID = value;
					SendPropertyChanged("UserID");
					OnUserIDChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.userstatistics")]
	public partial class UserStatistics : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _actions;

		private int _id;

		private int _lines;

		private int _profAnItIes;

		private int _userID;

		private int _words;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnActionsChanged();

		partial void OnActionsChanging(int value);

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnLinesChanged();

		partial void OnLinesChanging(int value);

		partial void OnProfAnITiesChanged();

		partial void OnProfAnITiesChanging(int value);

		partial void OnUserIDChanged();

		partial void OnUserIDChanging(int value);

		partial void OnWordsChanged();

		partial void OnWordsChanging(int value);
		#endregion


		public UserStatistics()
		{
			OnCreated();
		}

		[Column(Storage = "_actions", Name = "actions", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Actions
		{
			get
			{
				return _actions;
			}
			set
			{
				if ((_actions != value)) {
					OnActionsChanging(value);
					SendPropertyChanging();
					_actions = value;
					SendPropertyChanged("Actions");
					OnActionsChanged();
				}
			}
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'userstatistics_id_seq\')")]
		[DebuggerNonUserCode()]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lines", Name = "lines", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Lines
		{
			get
			{
				return _lines;
			}
			set
			{
				if ((_lines != value)) {
					OnLinesChanging(value);
					SendPropertyChanging();
					_lines = value;
					SendPropertyChanged("Lines");
					OnLinesChanged();
				}
			}
		}

		[Column(Storage = "_profAnItIes", Name = "profanities", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Profanities
		{
			get
			{
				return _profAnItIes;
			}
			set
			{
				if ((_profAnItIes != value)) {
					OnProfAnITiesChanging(value);
					SendPropertyChanging();
					_profAnItIes = value;
					SendPropertyChanged("Profanities");
					OnProfAnITiesChanged();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return _userID;
			}
			set
			{
				if ((_userID != value)) {
					OnUserIDChanging(value);
					_userID = value;
					OnUserIDChanged();
				}
			}
		}

		[Column(Storage = "_words", Name = "words", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Words
		{
			get
			{
				return _words;
			}
			set
			{
				if ((_words != value)) {
					OnWordsChanging(value);
					SendPropertyChanging();
					_words = value;
					SendPropertyChanged("Words");
					OnWordsChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.words")]
	public partial class Word : INotifyPropertyChanging, INotifyPropertyChanged
	{

		private static readonly PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");

		private int _id;

		private int _uses;

		private string _word1;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnUsesChanged();

		partial void OnUsesChanging(int value);

		partial void OnWordChanged();

		partial void OnWordChanging(string value);
		#endregion


		public Word()
		{
			OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'words_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if ((_id != value)) {
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return _uses;
			}
			set
			{
				if ((_uses != value)) {
					OnUsesChanging(value);
					SendPropertyChanging();
					_uses = value;
					SendPropertyChanged("Uses");
					OnUsesChanged();
				}
			}
		}

		[Column(Storage = "_word", Name = "word", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Word1
		{
			get
			{
				return _word1;
			}
			set
			{
				if (((_word1 == value)
							== false)) {
					OnWordChanging(value);
					SendPropertyChanging();
					_word1 = value;
					SendPropertyChanged("Word");
					OnWordChanged();
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			var h = PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if ((h != null)) {
				h(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}