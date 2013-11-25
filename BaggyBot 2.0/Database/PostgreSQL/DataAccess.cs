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
using System;
using System.ComponentModel;
using System.Data;
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
			this.OnCreated();
		}

		public BaggyBoT(string connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			this.OnCreated();
		}

		public BaggyBoT(IDbConnection connection, MappingSource mappingSource) :
			base(connection, mappingSource)
		{
			this.OnCreated();
		}

		public Table<Emoticon> EmOtIcons
		{
			get
			{
				return this.GetTable<Emoticon>();
			}
		}

		public Table<IrcLog> IrcLog
		{
			get
			{
				return this.GetTable<IrcLog>();
			}
		}

		public Table<KeyValuePair> KeyValuePairs
		{
			get
			{
				return this.GetTable<KeyValuePair>();
			}
		}

		public Table<Name> Names
		{
			get
			{
				return this.GetTable<Name>();
			}
		}

		public Table<Quote> Quotes
		{
			get
			{
				return this.GetTable<Quote>();
			}
		}

		public Table<Url> URLS
		{
			get
			{
				return this.GetTable<Url>();
			}
		}

		public Table<UserCredentials> UserCReds
		{
			get
			{
				return this.GetTable<UserCredentials>();
			}
		}

		public Table<UserStatistics> UserStatistics
		{
			get
			{
				return this.GetTable<UserStatistics>();
			}
		}

		public Table<Word> Words
		{
			get
			{
				return this.GetTable<Word>();
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
			base(connection, new DbLinq.PostgreSql.PgsqlVendor())
		{
			this.OnCreated();
		}

		public BaggyBoT(IDbConnection connection, IVendor sqlDialect) :
			base(connection, sqlDialect)
		{
			this.OnCreated();
		}

		public BaggyBoT(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) :
			base(connection, mappingSource, sqlDialect)
		{
			this.OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion

	[Table(Name = "dbo.emoticons")]
	public partial class Emoticon : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		[Column(Storage = "_emOticOn", Name = "emoticon", DbType = "character varying(16)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Emoticon1
		{
			get
			{
				return this._emOticOn;
			}
			set
			{
				if (((_emOticOn == value)
							== false)) {
					this.OnEmOticOnChanging(value);
					this.SendPropertyChanging();
					this._emOticOn = value;
					this.SendPropertyChanged("EmOticOn");
					this.OnEmOticOnChanged();
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
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lastUsedBy", Name = "last_used_by", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int LastUsedBy
		{
			get
			{
				return this._lastUsedBy;
			}
			set
			{
				if ((_lastUsedBy != value)) {
					this.OnLastUsedByChanging(value);
					this.SendPropertyChanging();
					this._lastUsedBy = value;
					this.SendPropertyChanged("LastUsedBy");
					this.OnLastUsedByChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return this._uses;
			}
			set
			{
				if ((_uses != value)) {
					this.OnUsesChanging(value);
					this.SendPropertyChanging();
					this._uses = value;
					this.SendPropertyChanged("Uses");
					this.OnUsesChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.irclog")]
	public partial class IrcLog
	{
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

		private string _channel;

		private int _id;

		private string _message;

		private string _nick;

		private System.Nullable<int> _sender;

		private System.DateTime _time;

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

		partial void OnSenderChanging(System.Nullable<int> value);

		partial void OnTimeChanged();

		partial void OnTimeChanging(System.DateTime value);
		#endregion


		public IrcLog()
		{
			this.OnCreated();
		}

		[Column(Storage = "_channel", Name = "channel", DbType = "character varying(64)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Channel
		{
			get
			{
				return this._channel;
			}
			set
			{
				if (((_channel == value)
							== false)) {
					this.OnChannelChanging(value);
					this.SendPropertyChanging();
					this._channel = value;
					this.SendPropertyChanged("Channel");
					this.OnChannelChanged();
				}
			}
		}

		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'irclog_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this._id = value;
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_message", Name = "message", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Message
		{
			get
			{
				return this._message;
			}
			set
			{
				if (((_message == value)
							== false)) {
					this.OnMessageChanging(value);
					this._message = value;
					this.OnMessageChanged();
				}
			}
		}

		[Column(Storage = "_nick", Name = "nick", DbType = "character varying(64)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Nick
		{
			get
			{
				return this._nick;
			}
			set
			{
				if (((_nick == value)
							== false)) {
					this.OnNickChanging(value);
					this._nick = value;
					this.OnNickChanged();
				}
			}
		}

		[Column(Storage = "_sender", Name = "sender", DbType = "integer(32,0)", AutoSync = AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> Sender
		{
			get
			{
				return this._sender;
			}
			set
			{
				if ((_sender != value)) {
					this.OnSenderChanging(value);
					this._sender = value;
					this.OnSenderChanged();
				}
			}
		}

		[Column(Storage = "_time", Name = "time", DbType = "timestamp without time zone", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public System.DateTime Time
		{
			get
			{
				return this._time;
			}
			set
			{
				if ((_time != value)) {
					this.OnTimeChanging(value);
					this._time = value;
					this.OnTimeChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.keyvaluepairs")]
	public partial class KeyValuePair : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'keyvaluepairs_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_key", Name = "key", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Key
		{
			get
			{
				return this._key;
			}
			set
			{
				if (((_key == value)
							== false)) {
					this.OnKeyChanging(value);
					this.SendPropertyChanging();
					this._key = value;
					this.SendPropertyChanged("Key");
					this.OnKeyChanged();
				}
			}
		}

		[Column(Storage = "_value", Name = "value", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Value
		{
			get
			{
				return this._value;
			}
			set
			{
				if ((_value != value)) {
					this.OnValueChanging(value);
					this.SendPropertyChanging();
					this._value = value;
					this.SendPropertyChanged("Value");
					this.OnValueChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.names")]
	public partial class Name : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		public Name()
		{
			this.OnCreated();
		}

		[Column(Storage = "_name1", Name = "name1", DbType = "character varying(90)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Name1
		{
			get
			{
				return this._name1;
			}
			set
			{
				if (((_name1 == value)
							== false)) {
					this.OnName1Changing(value);
					this.SendPropertyChanging();
					this._name1 = value;
					this.SendPropertyChanged("Name1");
					this.OnName1Changed();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value)) {
					this.OnUserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("UserID");
					this.OnUserIDChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.quotes")]
	public partial class Quote : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

		private int _id;

		private string _quote1;

		private int _userID;

		#region Extensibility Method Declarations
		partial void OnCreated();

		partial void OnIDChanged();

		partial void OnIDChanging(int value);

		partial void OnQuote1Changed();

		partial void OnQuote1Changing(string value);

		partial void OnUserIDChanged();

		partial void OnUserIDChanging(int value);
		#endregion


		public Quote()
		{
			this.OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'quotes_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_quote1", Name = "quote1", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Quote1
		{
			get
			{
				return this._quote1;
			}
			set
			{
				if (((_quote1 == value)
							== false)) {
					this.OnQuote1Changing(value);
					this.SendPropertyChanging();
					this._quote1 = value;
					this.SendPropertyChanged("Quote1");
					this.OnQuote1Changed();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value)) {
					this.OnUserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("UserID");
					this.OnUserIDChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.urls")]
	public partial class Url : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'urls_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lastUsage", Name = "last_usage", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string LastUsage
		{
			get
			{
				return this._lastUsage;
			}
			set
			{
				if (((_lastUsage == value)
							== false)) {
					this.OnLastUsageChanging(value);
					this.SendPropertyChanging();
					this._lastUsage = value;
					this.SendPropertyChanged("LastUsage");
					this.OnLastUsageChanged();
				}
			}
		}

		[Column(Storage = "_lastUsedBy", Name = "last_used_by", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int LastUsedBy
		{
			get
			{
				return this._lastUsedBy;
			}
			set
			{
				if ((_lastUsedBy != value)) {
					this.OnLastUsedByChanging(value);
					this.SendPropertyChanging();
					this._lastUsedBy = value;
					this.SendPropertyChanged("LastUsedBy");
					this.OnLastUsedByChanged();
				}
			}
		}

		[Column(Storage = "_url", Name = "url", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Url1
		{
			get
			{
				return this._url;
			}
			set
			{
				if (((_url == value)
							== false)) {
					this.OnURLChanging(value);
					this.SendPropertyChanging();
					this._url = value;
					this.SendPropertyChanged("URL");
					this.OnURLChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return this._uses;
			}
			set
			{
				if ((_uses != value)) {
					this.OnUsesChanging(value);
					this.SendPropertyChanging();
					this._uses = value;
					this.SendPropertyChanged("Uses");
					this.OnUsesChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.usercreds")]
	public partial class UserCredentials : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		[Column(Storage = "_hostMask", Name = "hostmask", DbType = "character varying(128)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string HostMask
		{
			get
			{
				return this._hostMask;
			}
			set
			{
				if (((_hostMask == value)
							== false)) {
					this.OnHostMaskChanging(value);
					this.SendPropertyChanging();
					this._hostMask = value;
					this.SendPropertyChanged("HostMask");
					this.OnHostMaskChanged();
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
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_idEnt", Name = "ident", DbType = "character varying(45)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Ident
		{
			get
			{
				return this._idEnt;
			}
			set
			{
				if (((_idEnt == value)
							== false)) {
					this.OnIDentChanging(value);
					this.SendPropertyChanging();
					this._idEnt = value;
					this.SendPropertyChanged("IDent");
					this.OnIDentChanged();
				}
			}
		}

		[Column(Storage = "_nick", Name = "nick", DbType = "character varying(45)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Nick
		{
			get
			{
				return this._nick;
			}
			set
			{
				if (((_nick == value)
							== false)) {
					this.OnNickChanging(value);
					this.SendPropertyChanging();
					this._nick = value;
					this.SendPropertyChanged("Nick");
					this.OnNickChanged();
				}
			}
		}

		[Column(Storage = "_nslOgin", Name = "ns_login", DbType = "character varying(45)", AutoSync = AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string NsLogin
		{
			get
			{
				return this._nslOgin;
			}
			set
			{
				if (((_nslOgin == value)
							== false)) {
					this.OnNSLoginChanging(value);
					this.SendPropertyChanging();
					this._nslOgin = value;
					this.SendPropertyChanged("NSLogin");
					this.OnNSLoginChanged();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value)) {
					this.OnUserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("UserID");
					this.OnUserIDChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.userstatistics")]
	public partial class UserStatistics : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		[Column(Storage = "_actions", Name = "actions", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Actions
		{
			get
			{
				return this._actions;
			}
			set
			{
				if ((_actions != value)) {
					this.OnActionsChanging(value);
					this.SendPropertyChanging();
					this._actions = value;
					this.SendPropertyChanged("Actions");
					this.OnActionsChanged();
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
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_lines", Name = "lines", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Lines
		{
			get
			{
				return this._lines;
			}
			set
			{
				if ((_lines != value)) {
					this.OnLinesChanging(value);
					this.SendPropertyChanging();
					this._lines = value;
					this.SendPropertyChanged("Lines");
					this.OnLinesChanged();
				}
			}
		}

		[Column(Storage = "_profAnItIes", Name = "profanities", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Profanities
		{
			get
			{
				return this._profAnItIes;
			}
			set
			{
				if ((_profAnItIes != value)) {
					this.OnProfAnITiesChanging(value);
					this.SendPropertyChanging();
					this._profAnItIes = value;
					this.SendPropertyChanged("Profanities");
					this.OnProfAnITiesChanged();
				}
			}
		}

		[Column(Storage = "_userID", Name = "user_id", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int UserId
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value)) {
					this.OnUserIDChanging(value);
					this._userID = value;
					this.OnUserIDChanged();
				}
			}
		}

		[Column(Storage = "_words", Name = "words", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Words
		{
			get
			{
				return this._words;
			}
			set
			{
				if ((_words != value)) {
					this.OnWordsChanging(value);
					this.SendPropertyChanging();
					this._words = value;
					this.SendPropertyChanged("Words");
					this.OnWordsChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name = "dbo.words")]
	public partial class Word : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

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
			this.OnCreated();
		}

		//[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		[Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false, Expression = "nextval(\'words_id_seq\')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value)) {
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}

		[Column(Storage = "_uses", Name = "uses", DbType = "integer(32,0)", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public int Uses
		{
			get
			{
				return this._uses;
			}
			set
			{
				if ((_uses != value)) {
					this.OnUsesChanging(value);
					this.SendPropertyChanging();
					this._uses = value;
					this.SendPropertyChanged("Uses");
					this.OnUsesChanged();
				}
			}
		}

		[Column(Storage = "_word", Name = "word", DbType = "text", AutoSync = AutoSync.Never, CanBeNull = false)]
		[DebuggerNonUserCode()]
		public string Word1
		{
			get
			{
				return this._word1;
			}
			set
			{
				if (((_word1 == value)
							== false)) {
					this.OnWordChanging(value);
					this.SendPropertyChanging();
					this._word1 = value;
					this.SendPropertyChanged("Word");
					this.OnWordChanged();
				}
			}
		}

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null)) {
				h(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null)) {
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
}