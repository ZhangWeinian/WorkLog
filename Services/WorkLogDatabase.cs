using SQLite;

using WorkLog.Models;

namespace WorkLog.Services
{
	/// <summary>
	/// 负责所有数据库交互的单例服务。 采用延迟初始化模式，确保在使用前数据库已准备就绪。
	/// </summary>
	public sealed class WorkLogDatabase
	{
		private static WorkLogDatabase? _instance;

		private static readonly Lock _lock = new();

		private SQLiteAsyncConnection? _database;

		private Task? _initializationTask;

		private WorkLogDatabase()
		{
		}

		/// <summary>
		/// 获取数据库服务的唯一实例。
		/// </summary>
		public static WorkLogDatabase Instance
		{
			get
			{
				if (_instance is null)
				{
					using (_lock.EnterScope())
					{
						if (_instance is null)
						{
							_instance = new WorkLogDatabase();
							_instance._initializationTask = _instance.InitializeDatabaseAsync();
						}
					}
				}

				return _instance;
			}
		}

		/// <summary>
		/// 真正的数据库初始化逻辑。只应被调用一次。
		/// </summary>
		private async Task InitializeDatabaseAsync()
		{
			if (_database is not null)
			{
				return;
			}

			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "WorkLog.db3");
			_database = new SQLiteAsyncConnection(dbPath,
				SQLiteOpenFlags.ReadWrite |
				SQLiteOpenFlags.Create |
				SQLiteOpenFlags.SharedCache);

			await _database.CreateTableAsync<WorkEvent>();
		}

		/// <summary>
		/// 一个私有的辅助方法，确保在执行任何数据库操作前，初始化已完成。
		/// </summary>
		private async Task EnsureInitializedAsync()
		{
			if (_database is not null)
			{
				return;
			}

			await (_initializationTask ?? Task.CompletedTask);

			if (_database is null)
			{
				throw new InvalidOperationException("数据库初始化失败，连接对象为空。");
			}
		}

		/// <summary>
		/// 获取所有的工作日志记录。
		/// </summary>
		public async Task<List<WorkEvent>> GetEventsAsync()
		{
			await EnsureInitializedAsync();
			return await _database!.Table<WorkEvent>().ToListAsync();
		}

		/// <summary>
		/// 根据ID获取单个工作日志记录。
		/// </summary>
		public async Task<WorkEvent?> GetEventAsync(int id)
		{
			await EnsureInitializedAsync();
			return await _database!.Table<WorkEvent>().Where(i => i.Id == id).FirstOrDefaultAsync();
		}

		/// <summary>
		/// 保存或更新一条工作日志记录，并自动管理审计时间戳。
		/// </summary>
		public async Task<int> SaveEventAsync(WorkEvent workEvent)
		{
			await EnsureInitializedAsync();
			var now = DateTime.UtcNow;

			if (workEvent.Id == 0)
			{
				workEvent.CreatedAt = now;
				workEvent.LastModifiedAt = now;
				return await _database!.InsertAsync(workEvent);
			}
			else
			{
				workEvent.LastModifiedAt = now;
				return await _database!.UpdateAsync(workEvent);
			}
		}

		/// <summary>
		/// 删除一条工作日志记录。
		/// </summary>
		public async Task<int> DeleteEventAsync(WorkEvent workEvent)
		{
			await EnsureInitializedAsync();
			return await _database!.DeleteAsync(workEvent);
		}
	}
}
