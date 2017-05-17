using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophidian.Losgap.Audio;

namespace Egodystonic.EscapeLizards
{
	public class LoopingSound
	{
		private readonly object instanceMutationLock = new object();

		private List<int> soundInstanceIds = new List<int>();
		private string file;

		public List<int> SoundInstanceIds
		{
			get { lock (instanceMutationLock) return soundInstanceIds; }
			private set { lock (instanceMutationLock) soundInstanceIds = value; }
		}
		public string File
		{
			get { lock (instanceMutationLock) return file; }
			private set { lock (instanceMutationLock) file = value; }
		}

		public LoopingSound(string file)
		{
			this.File = file;
			this.SoundInstanceIds = new List<int>();
		}

		public void AddInstance(int id)
		{
			lock (instanceMutationLock)
				this.SoundInstanceIds.Add(id);
		}

		public void StopAllInstances()
		{
			lock (instanceMutationLock)
			{
				foreach (int id in SoundInstanceIds)
				{
					AudioModule.StopSoundInstance(id);
				}
				SoundInstanceIds.Clear();
			}
		}
	}
}
