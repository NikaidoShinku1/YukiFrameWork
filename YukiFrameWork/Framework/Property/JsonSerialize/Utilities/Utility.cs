using System.Collections.Generic;

namespace YukiFrameWork.JsonInspector {
	public class Utility {
		#region Swap Methods
		public static void Swap<T>(IList<T> list, int indexA, int indexB) {
			T tmp = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = tmp;
		}
		#endregion
	}
}
