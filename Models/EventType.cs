using System.ComponentModel;

namespace WorkLog.Models
{
	public enum EventType
	{
		[Description("错误修复 ⚒️")]
		BugFix,

		[Description("功能开发 🔬")]
		Feature,

		[Description("代码重构 ♻️")]
		Refactor,

		[Description("技术学习 📚")]
		Learning,

		[Description("其他 🙌")]
		Null
	}
}
