// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created by Ben Bowen

#pragma once
#include "../CoreNative/LosgapCore.h"

namespace losgap {
	/*
	An enumeration of fullscreen state values for a window.
	*/
	enum WindowFullscreenState : uint8_t {
		NotFullscreen = 0,
		StandardFullscreen = 1,
		BorderlessFullscreen = 2
	};
}