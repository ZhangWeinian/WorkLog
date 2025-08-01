using SQLite;

using WorkLog.Models;

namespace WorkLog.Services
{
	public class WorkLogDatabase
	{
		private SQLiteAsyncConnection? _database;

		private WorkLogDatabase()
		{
		}

		private static WorkLogDatabase? _instance;

		private static readonly Lock _lock = new();

		/// <summary>
		/// 获取数据库服务的唯一实例。
		/// </summary>
		public static WorkLogDatabase Instance
		{
			get
			{
				if (_instance == null)
				{
					using (_lock.EnterScope())
					{
						_instance ??= new WorkLogDatabase();
					}
				}
				return _instance!;
			}
		}


		/// <summary>
		/// 异步初始化数据库。创建连接并建表。 应该在应用启动时调用一次。
		/// </summary>
		public async Task Init()
		{
			if (_database is not null)
				return;

			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "WorkLog.db3");

			_database = new SQLiteAsyncConnection(dbPath,
				SQLiteOpenFlags.ReadWrite |   // 读写模式
				SQLiteOpenFlags.Create |      // 如果数据库不存在则创建
				SQLiteOpenFlags.SharedCache); // 启用共享缓存以提高多线程性能

			await _database.CreateTableAsync<WorkEvent>();
		}

		/// <summary>
		/// 获取所有的工作日志记录。
		/// </summary>
		public Task<List<WorkEvent>> GetEventsAsync()
		{
			if (_database is null)
			{
				throw new InvalidOperationException("数据库尚未初始化，请先调用 Init 方法。");
			}

			return _database.Table<WorkEvent>().ToListAsync();
		}

		/// <summary>
		/// 根据ID获取单个工作日志记录。
		/// </summary>
		public Task<WorkEvent> GetEventAsync(int id)
		{
			if (_database is null)
			{
				throw new InvalidOperationException("数据库尚未初始化，请先调用 Init 方法。");
			}

			return _database.Table<WorkEvent>().Where(i => i.Id == id).FirstOrDefaultAsync();
		}

		/// <summary>
		/// 保存或更新一条工作日志记录。 如果记录的 Id 为 0 ，则插入新记录；否则，更新现有记录。
		/// </summary>
		public Task<int> SaveEventAsync(WorkEvent workEvent)
		{
			if (_database is null)
			{
				throw new InvalidOperationException("数据库尚未初始化，请先调用 Init 方法。");
			}

			if (workEvent.Id != 0)
			{
				return _database.UpdateAsync(workEvent);
			}
			else
			{
				return _database.InsertAsync(workEvent);
			}
		}

		/// <summary>
		/// 删除一条工作日志记录。
		/// </summary>
		public Task<int> DeleteEventAsync(WorkEvent workEvent)
		{
			if (_database is null)
			{
				throw new InvalidOperationException("数据库尚未初始化，请先调用 Init 方法。");
			}

			if (workEvent.Id == 0)
			{
				throw new ArgumentException("无法删除未保存的工作日志记录。请先保存记录。");
			}

			return _database.DeleteAsync(workEvent);
		}
	}
}
