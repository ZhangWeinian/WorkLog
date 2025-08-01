using System.ComponentModel;

namespace WorkLog.Models
{
	public enum EventStatus
	{
		[Description("待处理 🤪")]
		ToDo,

		[Description("进行中 🔄")]
		InProgress,

		[Description("已完成 ✌️")]
		Done,

		[Description("已取消 ❌")]
		Cancelled,

		[Description("其他 🙌")]
		Null,
	}
}
