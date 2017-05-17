// All code copyright (c) 2016 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 22 07 2016 at 19:21 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	public interface IHUDObject : IDisposable {
		ViewportAnchoring Anchoring { get; set; }
		Vector2 AnchorOffset { get; set; }
		Vector4 Color { get; set; }
		Vector2 Scale { get; set; }
		float Rotation { get; set; }
		int ZIndex { get; set; }
		object GetValueAsObject();
		void AdjustAlpha(float alpha);
		void SetAlpha(float alpha);
		IHUDObject Clone();
		void CopyTo(IHUDObject targetObj);
	}
}