using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YukiFrameWork.JsonInspector
{
	[System.Serializable]
	public class DataFile : ScriptableObject, ISerializationCallbackReceiver
	{
		#region Properties
		[SerializeField]
		protected object _data;
		public virtual object data
		{
			get { return _data; }
			protected set { _data = value; }
		}
		[SerializeField]
		private FileInfo _info;
		public FileInfo info
		{
			get { return _info; }
			protected set { _info = value; }
		}
		#endregion

		#region State Properties
		public virtual bool isChanged { get { return false; } }
		#endregion

		#region Unity Methods
		public void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }
		public virtual void OnBeforeSerialize() { }
		public virtual void OnAfterDeserialize() { }
		#endregion

		#region File Methods
		public virtual DataFile Load(FileInfo info) { return null; }
		public virtual DataFile Save(FileInfo info = null) { return null; }
		#endregion

		#region Render Methods
		public virtual void Render() { }
		#endregion
	}
}