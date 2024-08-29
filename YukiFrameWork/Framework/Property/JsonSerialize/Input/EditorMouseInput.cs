using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.JsonInspector
{
	public class EditorMouseInput : IHasMouseInput
	{
		#region Position Properties
		public Vector2 position
		{
			get { return Event.current.mousePosition; }
		}

		public Vector2 delta
		{
			get { return Event.current.delta; }
		}
		#endregion

		#region Button Properties
		private int _button;
		public int button
		{
			get { return _button; }
			set { _button = value; }
		}

		public bool isLeftDown
		{
			get
			{
				return Event.current.type == EventType.MouseDown &&
					   Event.current.button == 0;
			}
		}

		public bool isLeftUp
		{
			get
			{
				return Event.current.type == EventType.MouseUp &&
					   Event.current.button == 0;
			}
		}

		public bool isLeft
		{
			get { return (button & (1 << 0)) > 0; }
		}

		public bool isRightDown
		{
			get
			{
				return Event.current.type == EventType.MouseDown &&
					   Event.current.button == 1;
			}
		}

		public bool isRightUp
		{
			get
			{
				return Event.current.type == EventType.MouseUp &&
					   Event.current.button == 1;
			}
		}

		public bool isRight
		{
			get { return (button & (1 << 1)) > 0; }
		}
		#endregion

		#region Event Properties
		public bool isMoving
		{
			get { return Event.current.type == EventType.MouseMove; }
		}

		public bool isDragging
		{
			get { return Event.current.type == EventType.MouseDrag; }
		}
		#endregion

		#region Methods
		public void Update()
		{
			if (isLeftDown)
			{
				button |= 1 << 0;
			}
			if (isLeftUp)
			{
				button &= ~(1 >> 0);
			}
		}
		#endregion
	}
}
