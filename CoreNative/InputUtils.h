// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 03 2015 at 18:26 by Raphael Rabl

#pragma once
#include "../CoreNative/LosgapCore.h"
#include <Windows.h>

namespace losgap {

	const int KEY_DOWN_BIT = 0x8000;
	int32_t* VirtualKeys = NULL;

	/*
	Enumeration of possible virtual key states
	*/
	enum VirtualKeyState : uint8_t {
		KeyUp = 1,
		KeyDown = 2
	};

	/*
	Static class containing utilities for getting user/peripheral input states.
	*/
	class InputUtils {
	public:
		static void PassVirtualKeys(int32_t* virtualKeys);
		static void FillVirtualKeyStateArray(VirtualKeyState* keyStateArr, int32_t arrLen);
		static void FetchCursorPosition(float_t& outX, float_t& outY);
		static void SetCursorPosition(float_t x, float_t y);
	};
}