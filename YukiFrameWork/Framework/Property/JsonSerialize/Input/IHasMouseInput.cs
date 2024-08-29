using UnityEngine;

namespace YukiFrameWork.JsonInspector
{
	public interface IHasMouseInput
	{
		#region Position Properties
		Vector2 position { get; }

		Vector2 delta { get; }
		#endregion

		#region Button Properties
		int button { get; set; }

		bool isLeftDown { get; }

		bool isLeftUp { get; }

		bool isLeft { get; }

		bool isRightDown { get; }

		bool isRightUp { get; }

		bool isRight { get; }
		#endregion

		#region Event Properties
		bool isMoving { get; }

		bool isDragging { get; }
		#endregion

		#region Methods
		void Update();
		#endregion
	}
}