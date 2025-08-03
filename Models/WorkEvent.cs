using SQLite;

namespace WorkLog.Models
{
	/// <summary>
	/// 代表一条工作日志记录的核心模型。 对应数据库中的 "WorkEvents" 表。
	/// </summary>
	[Table("WorkEvents")]
	public class WorkEvent
	{
		/// <summary>
		/// 主键，数据库会自动递增。
		/// </summary>
		[PrimaryKey, AutoIncrement]
		public int Id
		{
			get; set;
		}

		/// <summary>
		/// 事件的标题，通常是必需的。
		/// </summary>
		[NotNull]
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// 事件的描述，这是日志的主要内容。
		/// </summary>
		[NotNull]
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// 事件的备注信息，可选。
		/// </summary>
		public string? Remarks
		{
			get; set;
		}

		/// <summary>
		/// 事件发生的确切时间戳。
		/// </summary>
		public DateTime Timestamp
		{
			get; set;
		}

		/// <summary>
		/// 事件的类型（定位），如 BugFix, Feature 等。 将枚举存储为整数以提高效率。
		/// </summary>
		public EventType EventType
		{
			get; set;
		}

		/// <summary>
		/// 事件的当前状态，如 Done, InProgress 等。
		/// </summary>
		public EventStatus Status
		{
			get; set;
		}

		// 注意：下面这两个属性是未来扩展用的，暂时可以不实现功能，但模型先定义好

		/// <summary>
		/// 关联的项目ID（外键）。
		/// </summary>
		[Indexed]
		public int ProjectId
		{
			get; set;
		}

		/// <summary>
		/// 关联的任务ID（外键）。
		/// </summary>
		[Indexed]
		public int TaskId
		{
			get; set;
		}
	}
}
